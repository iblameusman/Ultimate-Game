using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DodgeGame.Core;
using DodgeGame.Data;

namespace DodgeGame.Player
{
    public class PlayerAnimationBridge : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The Animator component controlling the Dodger model.")]
        [SerializeField] private Animator animator;

        [Tooltip("The SkinnedMeshRenderer for Dodger's face containing expression blend shapes.")]
        [SerializeField] private SkinnedMeshRenderer faceRenderer;

        [Tooltip("Root PlayerController component.")]
        [SerializeField] private PlayerController playerController;

        [Tooltip("DashController component.")]
        [SerializeField] private DashController dashController;

        [Tooltip("DimensionShifter component.")]
        [SerializeField] private DimensionShifter dimensionShifter;

        [Tooltip("PlayerHealth component.")]
        [SerializeField] private PlayerHealth playerHealth;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int IsDashingHash = Animator.StringToHash("IsDashing");
        private static readonly int IsShiftedHash = Animator.StringToHash("IsShifted");
        private static readonly int IsSlowMoHash = Animator.StringToHash("IsSlowMo");
        private static readonly int JumpTriggerHash = Animator.StringToHash("Jump");
        private static readonly int HitTriggerHash = Animator.StringToHash("Hit");
        private static readonly int DieTriggerHash = Animator.StringToHash("Die");
        private static readonly int VictoryTriggerHash = Animator.StringToHash("Victory");

        private readonly Dictionary<string, int> shapeIndices = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
        private float[] currentWeights;
        private float[] targetWeights;

        private Coroutine activeExpressionRoutine;
        private bool isDead = false;

        private void Awake()
        {
            FindReferences();
            CacheBlendShapes();
        }

        public void FindReferences()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (playerController == null) playerController = GetComponentInParent<PlayerController>();
            if (dashController == null) dashController = GetComponentInParent<DashController>();
            if (dimensionShifter == null) dimensionShifter = GetComponentInParent<DimensionShifter>();
            if (playerHealth == null) playerHealth = GetComponentInParent<PlayerHealth>();

            if (faceRenderer == null)
            {
                var allSmr = GetComponentsInChildren<SkinnedMeshRenderer>(true);
                foreach (var smr in allSmr)
                {
                    if (smr.name.Contains("Face", System.StringComparison.OrdinalIgnoreCase))
                    {
                        faceRenderer = smr;
                        break;
                    }
                }
            }
        }

        private void CacheBlendShapes()
        {
            if (faceRenderer == null || faceRenderer.sharedMesh == null) return;

            int count = faceRenderer.sharedMesh.blendShapeCount;
            currentWeights = new float[count];
            targetWeights = new float[count];

            shapeIndices.Clear();
            for (int i = 0; i < count; i++)
            {
                string shapeName = faceRenderer.sharedMesh.GetBlendShapeName(i);
                shapeIndices[shapeName] = i;
            }
        }

        private void OnEnable()
        {
            if (playerController != null)
            {
                playerController.OnJump -= HandleJump;
                playerController.OnJump += HandleJump;
            }

            if (playerHealth != null)
            {
                playerHealth.OnDeath -= HandleDeath;
                playerHealth.OnDeath += HandleDeath;
                playerHealth.OnLivesChanged -= HandleLivesChanged;
                playerHealth.OnLivesChanged += HandleLivesChanged;
            }

            GameEvents.OnPlayerHit -= HandlePlayerHit;
            GameEvents.OnPlayerHit += HandlePlayerHit;

            GameEvents.OnNearMiss -= HandleNearMiss;
            GameEvents.OnNearMiss += HandleNearMiss;

            GameEvents.OnGameOver -= HandleGameOver;
            GameEvents.OnGameOver += HandleGameOver;
        }

        private void OnDisable()
        {
            if (playerController != null)
            {
                playerController.OnJump -= HandleJump;
            }

            if (playerHealth != null)
            {
                playerHealth.OnDeath -= HandleDeath;
                playerHealth.OnLivesChanged -= HandleLivesChanged;
            }

            GameEvents.OnPlayerHit -= HandlePlayerHit;
            GameEvents.OnNearMiss -= HandleNearMiss;
            GameEvents.OnGameOver -= HandleGameOver;
        }

        private void Update()
        {
            UpdateAnimationParameters();
            UpdateFacialExpressions();
        }

        private void UpdateAnimationParameters()
        {
            if (animator == null) return;

            if (GetComponent<PlayerAnimator>() != null || GetComponentInParent<PlayerAnimator>() != null)
            {
                return;
            }

            if (playerController != null)
            {
                Vector3 vel = playerController.Velocity;
                float horizontalSpeed = new Vector2(vel.x, vel.z).magnitude;
                animator.SetFloat(SpeedHash, horizontalSpeed);
                animator.SetBool(IsGroundedHash, playerController.IsGrounded);
            }

            if (dashController != null)
            {
                animator.SetBool(IsDashingHash, dashController.IsDashing);
            }

            if (dimensionShifter != null)
            {
                animator.SetBool(IsShiftedHash, dimensionShifter.IsShifted);
            }

            bool isSlowMo = Time.timeScale < 0.9f && Time.timeScale > 0.01f;
            animator.SetBool(IsSlowMoHash, isSlowMo);
        }

        private void UpdateFacialExpressions()
        {
            if (faceRenderer == null || currentWeights == null) return;

            if (activeExpressionRoutine == null && !isDead)
            {
                if (dashController != null && dashController.IsDashing)
                {
                    SetTargetExpression("Determined");
                }
                else if (Time.timeScale < 0.9f)
                {
                    SetTargetExpression("Cool");
                }
                else if (playerController != null && new Vector2(playerController.Velocity.x, playerController.Velocity.z).magnitude > 0.5f)
                {
                    SetTargetExpression("Happy");
                }
                else
                {
                    SetTargetExpression("Neutral");
                }
            }

            float dt = Time.unscaledDeltaTime * 10f;
            for (int i = 0; i < currentWeights.Length; i++)
            {
                currentWeights[i] = Mathf.MoveTowards(currentWeights[i], targetWeights[i], dt * 100f);
                faceRenderer.SetBlendShapeWeight(i, currentWeights[i]);
            }
        }

        public void SetTargetExpression(string name, float weight = 100f)
        {
            if (targetWeights == null) return;

            for (int i = 0; i < targetWeights.Length; i++)
            {
                targetWeights[i] = 0f;
            }

            if (shapeIndices.TryGetValue(name, out int index))
            {
                targetWeights[index] = weight;
            }
        }

        public void TriggerTemporaryExpression(string name, float duration)
        {
            if (isDead) return;
            if (activeExpressionRoutine != null) StopCoroutine(activeExpressionRoutine);
            activeExpressionRoutine = StartCoroutine(TemporaryExpressionRoutine(name, duration));
        }

        private IEnumerator TemporaryExpressionRoutine(string name, float duration)
        {
            SetTargetExpression(name);
            yield return new WaitForSecondsRealtime(duration);
            activeExpressionRoutine = null;
        }

        private void HandleJump()
        {
            if (animator != null)
            {
                animator.SetTrigger(JumpTriggerHash);
            }
        }

        private void HandleLivesChanged(int livesRemaining)
        {
            if (livesRemaining < (playerHealth != null ? playerHealth.MaxLives : 3) && livesRemaining > 0)
            {
                TriggerHitFeedback();
            }
        }

        private void HandlePlayerHit(int livesRemaining)
        {
            TriggerHitFeedback();
        }

        private void TriggerHitFeedback()
        {
            if (isDead) return;

            if (animator != null)
            {
                animator.SetTrigger(HitTriggerHash);
            }

            TriggerTemporaryExpression("Dizzy", 0.6f);
        }

        private void HandleNearMiss(PlayerMovementState state)
        {
            if (!isDead && activeExpressionRoutine == null)
            {
                TriggerTemporaryExpression("Surprised", 0.45f);
            }
        }

        private void HandleDeath()
        {
            OnDead();
        }

        private void HandleGameOver(int score)
        {
            OnDead();
        }

        private void OnDead()
        {
            if (isDead) return;
            isDead = true;

            if (activeExpressionRoutine != null)
            {
                StopCoroutine(activeExpressionRoutine);
                activeExpressionRoutine = null;
            }

            if (animator != null)
            {
                animator.SetTrigger(DieTriggerHash);
            }

            SetTargetExpression("Defeated");
        }

        public void TriggerVictory()
        {
            if (animator != null)
            {
                animator.SetTrigger(VictoryTriggerHash);
            }
            SetTargetExpression("Happy");
        }
    }
}
