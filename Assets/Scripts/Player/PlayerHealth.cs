using System;
using UnityEngine;
using DodgeGame.Core;
using DodgeGame.Data;

namespace DodgeGame.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Player configuration asset specifying max lives and invulnerability window.")]
        [SerializeField] private PlayerConfig config;

        [Header("Abilities")]
        [Tooltip("Reference to the player DimensionShifter component to verify shift immunity.")]
        [SerializeField] private DimensionShifter dimensionShifter;

        private int currentLives;
        private float invulnerabilityTimer;
        private bool isDead;

        private MeshRenderer playerRenderer;
        private Renderer[] playerRenderers;
        private MaterialPropertyBlock propBlock;
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private Coroutine activeFlinchRoutine;
        private Vector3 initialScale = Vector3.one;

        public event Action<int> OnLivesChanged;
        public event Action OnDeath;

        public int CurrentLives => currentLives;
        public int MaxLives => SettingsService.Instance != null ? SettingsService.Instance.GetCurrentDifficultyPreset().StartingLives : (config != null ? config.MaxLives : 3);
        public bool IsInvulnerable => invulnerabilityTimer > 0f;
        public bool IsDead => isDead;

        private void Awake()
        {
            if (dimensionShifter == null)
            {
                dimensionShifter = GetComponent<DimensionShifter>();
            }
            playerRenderer = GetComponentInChildren<MeshRenderer>();
            playerRenderers = GetComponentsInChildren<Renderer>(true);
            propBlock = new MaterialPropertyBlock();
            initialScale = transform.localScale;
            InitializeLives();
        }

        private void Update()
        {
            if (invulnerabilityTimer > 0f)
            {
                invulnerabilityTimer -= Time.deltaTime;
                bool visible = (Mathf.FloorToInt(invulnerabilityTimer * 24f) % 2) == 0 || invulnerabilityTimer <= 0f;

                if (playerRenderers != null && playerRenderers.Length > 0)
                {
                    for (int i = 0; i < playerRenderers.Length; i++)
                    {
                        var r = playerRenderers[i];
                        if (r != null && !(r is ParticleSystemRenderer || r is TrailRenderer))
                        {
                            r.enabled = visible;
                        }
                    }
                }
                else if (playerRenderer != null)
                {
                    playerRenderer.enabled = visible;
                }
            }
            else if (!isDead)
            {
                if (playerRenderers != null && playerRenderers.Length > 0)
                {
                    for (int i = 0; i < playerRenderers.Length; i++)
                    {
                        var r = playerRenderers[i];
                        if (r != null && !(r is ParticleSystemRenderer || r is TrailRenderer) && !r.enabled)
                        {
                            r.enabled = true;
                        }
                    }
                }
                else if (playerRenderer != null && !playerRenderer.enabled)
                {
                    playerRenderer.enabled = true;
                }
            }
        }

        public void Initialize(PlayerConfig playerConfig)
        {
            config = playerConfig;
            InitializeLives();
        }

        private void InitializeLives()
        {
            int starting = 3;
            if (SettingsService.Instance != null)
            {
                starting = SettingsService.Instance.GetCurrentDifficultyPreset().StartingLives;
            }
            else if (config != null)
            {
                starting = config.MaxLives;
            }

            currentLives = starting;
            invulnerabilityTimer = 0f;
            isDead = false;
            transform.localScale = initialScale;
            if (playerRenderer != null) playerRenderer.enabled = true;
        }

        public void KillInstant(string reason = "Hit by Ball")
        {
            if (isDead) return;
            if (dimensionShifter != null && dimensionShifter.IsShifted) return;

            int streakBefore = DodgeStreakTracker.Instance != null ? DodgeStreakTracker.Instance.CurrentStreak : 0;

            currentLives = 0;
            isDead = true;
            invulnerabilityTimer = 0f;

            AudioManager.Instance?.PlayPlayerHit();
            SoundManager.Instance?.PlayHitExplosion();
            TriggerDeathAnimation();

            OnLivesChanged?.Invoke(currentLives);
            
            var hitDetails = new PlayerHitDetails(
                livesRemaining: 0,
                isSticky: false,
                isShieldOrPhaseBreak: false,
                streakBeforeHit: streakBefore,
                isLastLife: true
            );
            GameEvents.TriggerPlayerHit(hitDetails);
            OnDeath?.Invoke();
        }

        public bool TakeDamage(int amount)
        {
            return TakeDamage(amount, null, false);
        }

        public bool TakeDamage(int amount, Balls.BaseBall sourceBall, bool isShieldOrPhaseBreak = false)
        {
            if (isDead || invulnerabilityTimer > 0f || amount <= 0)
            {
                return false;
            }

            if (dimensionShifter != null && dimensionShifter.IsShifted)
            {
                return false;
            }

            int streakBefore = DodgeStreakTracker.Instance != null ? DodgeStreakTracker.Instance.CurrentStreak : 0;
            bool isLastLife = (currentLives - amount) <= 0;
            bool isSticky = sourceBall is Balls.StickyBall;

            currentLives = Mathf.Max(0, currentLives - amount);
            invulnerabilityTimer = config != null ? config.InvulnerabilityDuration : 1.5f;

            AudioManager.Instance?.PlayPlayerHit();
            SoundManager.Instance?.PlayHitExplosion();
            TriggerDamageFeedback();

            OnLivesChanged?.Invoke(currentLives);

            var hitDetails = new PlayerHitDetails(
                livesRemaining: currentLives,
                isSticky: isSticky,
                isShieldOrPhaseBreak: isShieldOrPhaseBreak,
                streakBeforeHit: streakBefore,
                isLastLife: isLastLife
            );
            GameEvents.TriggerPlayerHit(hitDetails);

            if (currentLives <= 0)
            {
                isDead = true;
                TriggerDeathAnimation();
                OnDeath?.Invoke();
            }

            return true;
        }

        private void TriggerDamageFeedback()
        {
            if (activeFlinchRoutine != null) StopCoroutine(activeFlinchRoutine);
            activeFlinchRoutine = StartCoroutine(DamageFeedbackRoutine());
        }

        private System.Collections.IEnumerator DamageFeedbackRoutine()
        {
            SetRenderersEmission(new Color(2.5f, 0.4f, 0.4f));

            float duration = 0.16f;
            float elapsed = 0f;
            Vector3 squashScale = new Vector3(initialScale.x * 1.3f, initialScale.y * 0.7f, initialScale.z * 1.3f);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.localScale = Vector3.Lerp(squashScale, initialScale, t);
                yield return null;
            }

            transform.localScale = initialScale;
            SetRenderersEmission(Color.black);

            activeFlinchRoutine = null;
        }
        private void SetRenderersEmission(Color col)
        {
            if (playerRenderers != null && playerRenderers.Length > 0)
            {
                for (int i = 0; i < playerRenderers.Length; i++)
                {
                    var r = playerRenderers[i];
                    if (r != null && !(r is ParticleSystemRenderer || r is TrailRenderer))
                    {
                        r.GetPropertyBlock(propBlock);
                        propBlock.SetColor(EmissionColorId, col);
                        r.SetPropertyBlock(propBlock);
                    }
                }
            }
            else if (playerRenderer != null)
            {
                playerRenderer.GetPropertyBlock(propBlock);
                propBlock.SetColor(EmissionColorId, col);
                playerRenderer.SetPropertyBlock(propBlock);
            }
        }

        private void TriggerDeathAnimation()
        {
            if (activeFlinchRoutine != null) StopCoroutine(activeFlinchRoutine);
            StartCoroutine(DeathCollapseRoutine());
        }

        private System.Collections.IEnumerator DeathCollapseRoutine()
        {
            if (GetComponentInChildren<Animator>() != null)
            {
                yield break;
            }

            float elapsed = 0f;
            float duration = 0.35f;
            Vector3 startScale = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t * t);
                yield return null;
            }

            transform.localScale = Vector3.zero;
        }
    }
}
