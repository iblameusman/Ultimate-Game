using System;
using UnityEngine;
using DodgeGame.Data;
using DodgeGame.Player;

namespace DodgeGame.Hazards
{
    [RequireComponent(typeof(BoxCollider))]
    public class FireTile : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Hazard parameters controlling lifetime, damage, and fade.")]
        [SerializeField] private HazardConfig config;

        [Header("Visual Components")]
        [Tooltip("MeshRenderer of the tile whose material color and emission will fade.")]
        [SerializeField] private MeshRenderer meshRenderer;

        private Action<FireTile> returnToPoolCallback;
        private float lifeTimer;
        private float nextDamageTime;
        private MaterialPropertyBlock propertyBlock;
        private Color originalBaseColor;
        private Color originalEmissionColor;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private static readonly int FadeAlphaId = Shader.PropertyToID("_FadeAlpha");

        private void Awake()
        {
            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }

            propertyBlock = new MaterialPropertyBlock();

            if (meshRenderer != null && meshRenderer.sharedMaterial != null)
            {
                originalBaseColor = meshRenderer.sharedMaterial.HasProperty(BaseColorId)
                    ? meshRenderer.sharedMaterial.GetColor(BaseColorId)
                    : new Color(1f, 0.35f, 0f, 0.85f);

                originalEmissionColor = meshRenderer.sharedMaterial.HasProperty(EmissionColorId)
                    ? meshRenderer.sharedMaterial.GetColor(EmissionColorId)
                    : new Color(1f, 0.25f, 0f) * 2f;
            }
            else
            {
                originalBaseColor = new Color(1f, 0.35f, 0f, 0.85f);
                originalEmissionColor = new Color(1f, 0.25f, 0f) * 2f;
            }
        }

        public void Initialize(Action<FireTile> onReturnToPool, HazardConfig hazardConfig = null)
        {
            returnToPoolCallback = onReturnToPool;
            if (hazardConfig != null)
            {
                config = hazardConfig;
            }

            lifeTimer = config != null ? config.FireTileDuration : 4f;
            nextDamageTime = 0f;
            ResetVisuals();
        }

        private void Update()
        {
            lifeTimer -= Time.deltaTime;

            UpdateFadeVisuals();

            if (lifeTimer <= 0f)
            {
                ReturnToPool();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            TryDamagePlayer(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryDamagePlayer(other);
        }

        private void TryDamagePlayer(Collider other)
        {
            if (Time.time < nextDamageTime) return;

            if (other.CompareTag("Player"))
            {
                var health = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
                if (health != null)
                {
                    int damageAmount = config != null ? config.Damage : 1;
                    if (health.TakeDamage(damageAmount))
                    {
                        float cooldown = config != null ? config.DamageCooldown : 1f;
                        nextDamageTime = Time.time + cooldown;
                    }
                }
            }
        }

        private void UpdateFadeVisuals()
        {
            if (meshRenderer == null) return;
            if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();

            float fadeDuration = 1.0f;
            if (config != null && config.FadeDuration > 0f) fadeDuration = config.FadeDuration;

            if (lifeTimer < fadeDuration)
            {
                float normalizedProgress = Mathf.Clamp01(lifeTimer / fadeDuration);

                float flicker = (Mathf.Sin(lifeTimer * 26f) * 0.5f + 0.5f);
                float flickerFactor = normalizedProgress > 0.35f 
                    ? Mathf.Lerp(0.4f, 1.0f, flicker) 
                    : normalizedProgress;

                Color currentBase = originalBaseColor;
                currentBase.a = originalBaseColor.a * (normalizedProgress * flickerFactor);

                Color currentEmission = originalEmissionColor * (normalizedProgress * flickerFactor);

                meshRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(BaseColorId, currentBase);
                propertyBlock.SetColor(EmissionColorId, currentEmission);
                propertyBlock.SetFloat(FadeAlphaId, normalizedProgress * flickerFactor);
                meshRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void ResetVisuals()
        {
            if (meshRenderer == null) return;
            if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();

            meshRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorId, originalBaseColor);
            propertyBlock.SetColor(EmissionColorId, originalEmissionColor);
            propertyBlock.SetFloat(FadeAlphaId, 1.0f);
            meshRenderer.SetPropertyBlock(propertyBlock);
        }

        private void ReturnToPool()
        {
            if (returnToPoolCallback != null)
            {
                returnToPoolCallback(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
