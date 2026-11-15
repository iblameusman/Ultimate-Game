using UnityEngine;
using DodgeGame.Core;
using DodgeGame.Data;

namespace DodgeGame.Balls
{
    [RequireComponent(typeof(FireBall))]
    public class FireBallEffects : MonoBehaviour
    {
        [Header("Point Light")]
        [SerializeField] private Light fireLight;
        [SerializeField] private float minIntensity = 1.5f;
        [SerializeField] private float maxIntensity = 4.0f;
        [SerializeField] private float flickerSpeed = 7.85398f;

        [Header("Trail & Embers")]
        [SerializeField] private TrailRenderer fireTrail;
        [SerializeField] private ParticleSystem emberParticles;

        private Camera mainCamera;
        private Transform cameraTransform;

        private void Awake()
        {
            EnsureComponents();
        }

        private void OnEnable()
        {
            if (QualityManager.Instance != null)
            {
                QualityManager.Instance.OnQualityChanged += HandleQualityChanged;
            }

            ApplyQualitySettings();
        }

        private void Start()
        {
            EnsureComponents();
            ApplyQualitySettings();
        }

        private void OnDisable()
        {
            if (QualityManager.Instance != null)
            {
                QualityManager.Instance.OnQualityChanged -= HandleQualityChanged;
            }

            if (fireLight != null)
            {
                fireLight.enabled = false;
            }
        }

        public void EnsureComponents()
        {
            mainCamera = Camera.main;
            if (mainCamera != null) cameraTransform = mainCamera.transform;

            var particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit") 
                ?? Shader.Find("Particles/Standard Unlit") 
                ?? Shader.Find("Sprites/Default");

            if (fireLight == null)
            {
                var lightChild = transform.Find("FireLight");
                if (lightChild == null)
                {
                    var go = new GameObject("FireLight");
                    go.transform.SetParent(transform, false);
                    lightChild = go.transform;
                }

                fireLight = lightChild.GetComponent<Light>();
                if (fireLight == null) fireLight = lightChild.gameObject.AddComponent<Light>();

                fireLight.type = LightType.Point;
                fireLight.range = 2.5f;
                fireLight.color = new Color(1.0f, 0.45f, 0.08f, 1.0f);
                fireLight.intensity = minIntensity;
                fireLight.shadows = LightShadows.None;
            }

            if (fireTrail == null)
            {
                fireTrail = GetComponent<TrailRenderer>();
                if (fireTrail == null) fireTrail = gameObject.AddComponent<TrailRenderer>();

                fireTrail.time = 0.35f;
                fireTrail.startWidth = 0.75f;
                fireTrail.endWidth = 0.05f;

                if (particleShader != null)
                {
                    var trailMat = new Material(particleShader);
                    fireTrail.material = trailMat;
                }

                Gradient g = new Gradient();
                g.SetKeys(
                    new GradientColorKey[]
                    {
                        new GradientColorKey(new Color(1f, 0.55f, 0.1f), 0f),
                        new GradientColorKey(new Color(0.9f, 0.15f, 0.02f), 0.7f),
                        new GradientColorKey(new Color(0.3f, 0.02f, 0f), 1f)
                    },
                    new GradientAlphaKey[]
                    {
                        new GradientAlphaKey(0.9f, 0f),
                        new GradientAlphaKey(0.4f, 0.6f),
                        new GradientAlphaKey(0f, 1f)
                    }
                );
                fireTrail.colorGradient = g;
            }

            if (emberParticles == null)
            {
                var emberChild = transform.Find("EmberParticles");
                if (emberChild == null)
                {
                    var go = new GameObject("EmberParticles");
                    go.transform.SetParent(transform, false);
                    emberChild = go.transform;
                }

                emberParticles = emberChild.GetComponent<ParticleSystem>();
                if (emberParticles == null) emberParticles = emberChild.gameObject.AddComponent<ParticleSystem>();

                var main = emberParticles.main;
                main.playOnAwake = true;
                main.loop = true;
                main.duration = 1.0f;
                main.startLifetime = 0.65f;
                main.startSpeed = 1.4f;
                main.startSize = 0.14f;
                main.gravityModifier = -0.35f;
                main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.75f, 0.2f, 0.9f), new Color(1f, 0.35f, 0.05f, 0.8f));

                var emission = emberParticles.emission;
                emission.rateOverTime = 8f;

                var shape = emberParticles.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.45f;

                var renderer = emberChild.GetComponent<ParticleSystemRenderer>();
                if (renderer != null && particleShader != null)
                {
                    renderer.material = new Material(particleShader);
                }
            }
        }

        private void Update()
        {
            AnimatePointLight();
        }

        private void AnimatePointLight()
        {
            if (fireLight == null) return;

            bool qualityAllowsLight = QualityManager.Instance == null || QualityManager.Instance.EnablePointLight;
            if (!qualityAllowsLight)
            {
                if (fireLight.enabled) fireLight.enabled = false;
                return;
            }

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }

            if (cameraTransform != null)
            {
                float maxDist = QualityManager.Instance != null ? QualityManager.Instance.MaxLightDistance : 25f;
                float distSqr = (transform.position - cameraTransform.position).sqrMagnitude;
                if (distSqr > (maxDist * maxDist))
                {
                    if (fireLight.enabled) fireLight.enabled = false;
                    return;
                }
            }

            if (!fireLight.enabled) fireLight.enabled = true;

            float timeVal = Time.time * flickerSpeed;
            float pulse = Mathf.Sin(timeVal) * 0.5f + 0.5f;
            float flicker = (Mathf.Sin(timeVal * 3.73f + 1.25f) * 0.5f + 0.5f) * 0.25f;
            float combined = Mathf.Clamp01(pulse * 0.85f + flicker);

            fireLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, combined);
        }

        private void HandleQualityChanged(QualityTier tier)
        {
            ApplyQualitySettings();
        }

        public void ApplyQualitySettings()
        {
            EnsureComponents();

            var qm = QualityManager.Instance;
            float trailLife = qm != null ? qm.TrailLifetime : 0.35f;
            int emberRate = qm != null ? qm.EmberEmissionRate : 8;

            if (fireTrail != null)
            {
                fireTrail.time = trailLife;
            }

            if (emberParticles != null)
            {
                var emission = emberParticles.emission;
                emission.rateOverTime = emberRate;

                if (emberRate <= 0)
                {
                    emberParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
                else if (!emberParticles.isPlaying)
                {
                    emberParticles.Play();
                }
            }

            if (fireLight != null && qm != null && !qm.EnablePointLight)
            {
                fireLight.enabled = false;
            }
        }
    }
}
