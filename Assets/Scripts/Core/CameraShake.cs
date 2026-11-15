using System.Collections;
using UnityEngine;
using DodgeGame.Data;

namespace DodgeGame.Core
{
    [DefaultExecutionOrder(200)]
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        [Header("Configuration")]
        [Tooltip("Optional reference to JuiceConfig. If null, loads DefaultJuice.")]
        [SerializeField] private JuiceConfig juiceConfig;

        [Header("Target Camera")]
        [Tooltip("Target camera transform. If null, automatically resolves Camera.main.")]
        [SerializeField] private Transform targetCameraTransform;

        private Vector3 currentShakeOffset = Vector3.zero;
        private Coroutine activeShakeCoroutine;
        private bool isScreenShakeEnabled = true;

        public void SetScreenShakeEnabled(bool isEnabled)
        {
            isScreenShakeEnabled = isEnabled;
        }

        public void ShakeDash()
        {
            if (!isScreenShakeEnabled) return;
            if (SettingsService.Instance?.CurrentSettings != null && !SettingsService.Instance.CurrentSettings.ScreenShakeEnabled)
            {
                return;
            }

            ResolveReferences();
            if (targetCameraTransform == null) return;

            float amp = 0.12f;
            float dur = 0.08f;
            float freq = 25f;
            float punch = 0.1f;

            if (SettingsService.Instance?.CurrentSettings != null && SettingsService.Instance.CurrentSettings.ReducedMotion)
            {
                amp *= 0.5f;
                punch *= 0.5f;
            }

            if (activeShakeCoroutine != null)
            {
                StopCoroutine(activeShakeCoroutine);
            }

            activeShakeCoroutine = StartCoroutine(ShakeRoutine(amp, dur, freq, punch));
        }

        public void ShakeNuclearExplosion()
        {
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            ResolveReferences();
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerHit += HandlePlayerHit;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerHit -= HandlePlayerHit;
            currentShakeOffset = Vector3.zero;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void ResolveReferences()
        {
            if (targetCameraTransform == null && Camera.main != null)
            {
                targetCameraTransform = Camera.main.transform;
            }

            if (juiceConfig == null)
            {
                juiceConfig = Resources.Load<JuiceConfig>("SO_Juice") ?? Resources.Load<JuiceConfig>("DefaultJuice");
#if UNITY_EDITOR
                if (juiceConfig == null)
                {
                    juiceConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<JuiceConfig>("Assets/ScriptableObjects/SO_Juice.asset")
                               ?? UnityEditor.AssetDatabase.LoadAssetAtPath<JuiceConfig>("Assets/ScriptableObjects/DefaultJuice.asset");
                }
#endif
            }
        }

        private void HandlePlayerHit(int livesRemaining)
        {
            ShakePlayerHit();
        }

        public void ShakePlayerHit()
        {
            if (!isScreenShakeEnabled) return;

            if (SettingsService.Instance != null && SettingsService.Instance.CurrentSettings != null)
            {
                if (!SettingsService.Instance.CurrentSettings.ScreenShakeEnabled)
                {
                    return;
                }
            }

            ResolveReferences();
            if (targetCameraTransform == null) return;

            float amp = juiceConfig != null ? juiceConfig.ShakeAmplitude : 0.35f;
            float dur = juiceConfig != null ? juiceConfig.ShakeDuration : 0.25f;
            float freq = juiceConfig != null ? juiceConfig.ShakeFrequency : 22f;
            float punch = juiceConfig != null ? juiceConfig.CameraPunchStrength : 0.25f;

            if (SettingsService.Instance != null && SettingsService.Instance.CurrentSettings != null)
            {
                if (SettingsService.Instance.CurrentSettings.ReducedMotion)
                {
                    amp *= 0.5f;
                    punch *= 0.5f;
                }
            }

            if (activeShakeCoroutine != null)
            {
                StopCoroutine(activeShakeCoroutine);
            }

            activeShakeCoroutine = StartCoroutine(ShakeRoutine(amp, dur, freq, punch));
        }

        private IEnumerator ShakeRoutine(float amplitude, float duration, float frequency, float punchStrength)
        {
            float elapsed = 0f;
            float seed = Random.Range(10f, 1000f);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float decay = Mathf.Clamp01(1f - t);

                float timeSample = (Time.unscaledTime + seed) * frequency;
                float x = (Mathf.PerlinNoise(timeSample, 0f) * 2f - 1f) * amplitude * decay;
                float y = (Mathf.PerlinNoise(0f, timeSample) * 2f - 1f) * amplitude * decay;
                float z = (Mathf.PerlinNoise(timeSample, timeSample) * 2f - 1f) * (amplitude * 0.35f) * decay;

                Vector3 punchOffset = Vector3.zero;
                if (targetCameraTransform != null)
                {
                    punchOffset = -targetCameraTransform.forward * (punchStrength * decay);
                }

                currentShakeOffset = new Vector3(x, y, z) + punchOffset;
                yield return null;
            }

            currentShakeOffset = Vector3.zero;
            activeShakeCoroutine = null;
        }

        private void LateUpdate()
        {
            if (currentShakeOffset != Vector3.zero && targetCameraTransform != null)
            {
                targetCameraTransform.position += currentShakeOffset;
            }
        }
    }
}
