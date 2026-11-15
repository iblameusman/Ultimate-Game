using System.Collections;
using UnityEngine;
using DodgeGame.Core;
using DodgeGame.Data;

namespace DodgeGame.Balls
{
    public class BallJuice : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Juice configuration providing squash amount and spawn pop duration.")]
        [SerializeField] private JuiceConfig juiceConfig;

        private Vector3 originalScale = Vector3.one;
        private TweenHandle activeSquashTween;
        private TweenHandle activeSpawnTween;

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private Coroutine activeHomingFlashCoroutine;
        private MeshRenderer meshRenderer;
        private MaterialPropertyBlock propertyBlock;
        private Color baseEmissionColor = Color.black;
        private bool hasCachedBaseEmission = false;

        private void Awake()
        {
            if (transform.localScale != Vector3.zero)
            {
                originalScale = transform.localScale;
            }

            meshRenderer = GetComponent<MeshRenderer>();
            ResolveConfig();
        }

        private void Start()
        {
            PlaySpawnPop();
        }

        private void OnDisable()
        {
            activeSquashTween?.Stop();
            activeSpawnTween?.Stop();
            if (activeHomingFlashCoroutine != null)
            {
                StopCoroutine(activeHomingFlashCoroutine);
                activeHomingFlashCoroutine = null;
            }
            transform.localScale = originalScale;
            if (meshRenderer != null && hasCachedBaseEmission && propertyBlock != null)
            {
                meshRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(EmissionColorId, baseEmissionColor);
                meshRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void ResolveConfig()
        {
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

        public void PlaySpawnPop()
        {
            ResolveConfig();
            float popDuration = juiceConfig != null ? juiceConfig.BallSpawnPopDuration : 0.25f;

            transform.localScale = Vector3.zero;

            activeSpawnTween?.Stop();
            activeSpawnTween = Tween.Scale(
                transform,
                originalScale,
                popDuration,
                EaseType.EaseOutBack,
                useUnscaledTime: false,
                onComplete: () =>
                {
                    transform.localScale = originalScale;
                }
            );
        }

        public void PlayHomingFlash(float duration = 0.2f)
        {
            if (activeHomingFlashCoroutine != null)
            {
                StopCoroutine(activeHomingFlashCoroutine);
            }
            activeHomingFlashCoroutine = StartCoroutine(HomingFlashRoutine(duration));
        }

        private IEnumerator HomingFlashRoutine(float duration)
        {
            if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null) yield break;

            if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();

            if (!hasCachedBaseEmission && meshRenderer.sharedMaterial != null)
            {
                if (meshRenderer.sharedMaterial.HasProperty(EmissionColorId))
                {
                    baseEmissionColor = meshRenderer.sharedMaterial.GetColor(EmissionColorId);
                }
                hasCachedBaseEmission = true;
            }

            Color flashColor = (baseEmissionColor != Color.black)
                ? (baseEmissionColor * 3.5f + Color.white * 1.5f)
                : (Color.white * 2.5f);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(1f - (elapsed / duration));
                Color c = Color.Lerp(baseEmissionColor, flashColor, t);
                meshRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(EmissionColorId, c);
                meshRenderer.SetPropertyBlock(propertyBlock);
                yield return null;
            }

            meshRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(EmissionColorId, baseEmissionColor);
            meshRenderer.SetPropertyBlock(propertyBlock);
            activeHomingFlashCoroutine = null;
        }

        public void TriggerBounceSquash(Vector3 contactNormal)
        {
            if (SettingsService.Instance != null && SettingsService.Instance.CurrentSettings != null)
            {
                if (SettingsService.Instance.CurrentSettings.ReducedMotion)
                {
                    return;
                }
            }

            ResolveConfig();
            float amount = juiceConfig != null ? juiceConfig.SquashAndStretchAmount : 0.12f;

            Vector3 absNormal = new Vector3(Mathf.Abs(contactNormal.x), Mathf.Abs(contactNormal.y), Mathf.Abs(contactNormal.z));
            if (absNormal.sqrMagnitude < 0.001f) absNormal = Vector3.up;
            absNormal.Normalize();

            Vector3 squashScale = new Vector3(
                Mathf.Lerp(originalScale.x * (1f + amount * 0.5f), originalScale.x * (1f - amount), absNormal.x),
                Mathf.Lerp(originalScale.y * (1f + amount * 0.5f), originalScale.y * (1f - amount), absNormal.y),
                Mathf.Lerp(originalScale.z * (1f + amount * 0.5f), originalScale.z * (1f - amount), absNormal.z)
            );

            activeSquashTween?.Stop();
            transform.localScale = squashScale;

            activeSquashTween = Tween.Scale(
                transform,
                originalScale,
                0.15f,
                EaseType.EaseOutBack,
                useUnscaledTime: false,
                onComplete: () =>
                {
                    transform.localScale = originalScale;
                }
            );
        }
    }
}
