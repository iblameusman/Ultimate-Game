using UnityEngine;
using DodgeGame.Core;
using DodgeGame.Data;
using DodgeGame.Hazards;

namespace DodgeGame.Balls
{
    public class NuclearBall : BaseBall
    {
        [Header("Nuclear Tuning")]
        [Tooltip("Configuration containing fuse time and shockwave parameters.")]
        [SerializeField] private NuclearBallConfig nuclearConfig;

        [Header("Explosion Hazard")]
        [Tooltip("Shockwave ring prefab instantiated on detonation.")]
        [SerializeField] private ShockwaveRing shockwavePrefab;

        private float fuseTimer;
        private float nextBeepTimer;
        private Vector3 initialBaseScale;
        private Color baseEmission;

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        protected override void Awake()
        {
            if (nuclearConfig != null && ballConfig == null)
            {
                ballConfig = nuclearConfig;
            }

            base.Awake();

            initialBaseScale = transform.localScale;

            if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
            baseEmission = new Color(0.8f, 1.0f, 0.1f) * 2f;
        }

        protected override void Start()
        {
            base.Start();
            fuseTimer = nuclearConfig != null ? nuclearConfig.FuseTime : 3f;
            nextBeepTimer = 0f;
        }

        protected override void Update()
        {
            base.Update();
            fuseTimer -= Time.deltaTime;
            nextBeepTimer -= Time.deltaTime;

            float totalFuse = nuclearConfig != null ? nuclearConfig.FuseTime : 3f;
            float normalizedElapsed = 1f - Mathf.Clamp01(fuseTimer / Mathf.Max(0.01f, totalFuse));

            if (nextBeepTimer <= 0f && fuseTimer > 0f)
            {
                float beepInterval = Mathf.Lerp(0.6f, 0.12f, normalizedElapsed);
                nextBeepTimer = beepInterval;
                float beepPitch = Mathf.Lerp(0.9f, 1.6f, normalizedElapsed);
                AudioManager.Instance?.PlayNuclearFuseBeep(transform.position, beepPitch);
            }

            UpdatePulsingVisuals(normalizedElapsed);

            if (fuseTimer <= 0f)
            {
                Detonate();
            }
        }

        private void UpdatePulsingVisuals(float normalizedElapsed)
        {
            float frequency = Mathf.Lerp(2f, 14f, normalizedElapsed);
            float pulseWave = (Mathf.Sin(Time.time * frequency * Mathf.PI * 2f) + 1f) * 0.5f;

            float scaleMultiplier = 1f + (pulseWave * 0.15f * normalizedElapsed);
            transform.localScale = initialBaseScale * scaleMultiplier;

            if (meshRenderer != null)
            {
                meshRenderer.GetPropertyBlock(propertyBlock);
                Color pulsedEmission = baseEmission * (1f + (pulseWave * 2.5f * (normalizedElapsed + 0.2f)));
                propertyBlock.SetColor(EmissionColorId, pulsedEmission);
                meshRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void Detonate()
        {
            AudioManager.Instance?.PlayNuclearExplosion(transform.position);
            CameraShake.Instance?.ShakeNuclearExplosion();
            VFXManager.Instance?.SpawnNuclearExplosion(transform.position);

            if (shockwavePrefab != null)
            {
                Vector3 spawnPos = transform.position;
                spawnPos.y = Mathf.Max(0.1f, spawnPos.y);

                ShockwaveRing ring = Instantiate(shockwavePrefab, spawnPos, Quaternion.identity);

                float maxRad = nuclearConfig != null ? nuclearConfig.ShockwaveMaxRadius : 15f;
                float duration = nuclearConfig != null ? nuclearConfig.ShockwaveDuration : 1.5f;
                int dmg = nuclearConfig != null ? nuclearConfig.ShockwaveDamage : 1;

                ring.Initialize(maxRad, duration, dmg);
            }

            Destroy(gameObject);
        }
    }
}
