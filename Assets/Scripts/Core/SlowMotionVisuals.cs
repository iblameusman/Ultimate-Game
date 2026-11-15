using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DodgeGame.Data;

namespace DodgeGame.Core
{
    [RequireComponent(typeof(CanvasGroup))]
    public class SlowMotionVisuals : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private SlowMotionConfig config;

        [Header("UI Element References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image tintImage;
        [SerializeField] private Image vignetteImage;

        private static Sprite cachedVignetteSprite;
        private Coroutine pulseCoroutine;
        private TweenHandle alphaTween;

        private void Awake()
        {
            ResolveReferences();
            EnsureVisualHierarchy();
        }

        private void OnEnable()
        {
            if (SlowMotionController.Instance != null)
            {
                SlowMotionController.Instance.OnSlowMoStart += HandleSlowMoStart;
                SlowMotionController.Instance.OnSlowMoEnd += HandleSlowMoEnd;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }

        private void Start()
        {
            if (SlowMotionController.Instance != null)
            {
                SlowMotionController.Instance.OnSlowMoStart -= HandleSlowMoStart;
                SlowMotionController.Instance.OnSlowMoStart += HandleSlowMoStart;
                SlowMotionController.Instance.OnSlowMoEnd -= HandleSlowMoEnd;
                SlowMotionController.Instance.OnSlowMoEnd += HandleSlowMoEnd;
            }
        }

        private void OnDisable()
        {
            if (SlowMotionController.Instance != null)
            {
                SlowMotionController.Instance.OnSlowMoStart -= HandleSlowMoStart;
                SlowMotionController.Instance.OnSlowMoEnd -= HandleSlowMoEnd;
            }

            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
                pulseCoroutine = null;
            }

            alphaTween?.Stop();
            alphaTween = null;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }

        private void ResolveReferences()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

            if (config == null)
            {
                config = Resources.Load<SlowMotionConfig>("SO_SlowMotionConfig") ?? Resources.Load<SlowMotionConfig>("DefaultSlowMotionConfig");
#if UNITY_EDITOR
                if (config == null)
                {
                    config = UnityEditor.AssetDatabase.LoadAssetAtPath<SlowMotionConfig>("Assets/ScriptableObjects/SO_SlowMotionConfig.asset")
                          ?? UnityEditor.AssetDatabase.LoadAssetAtPath<SlowMotionConfig>("Assets/ScriptableObjects/DefaultSlowMotionConfig.asset");
                }
#endif
            }
        }

        public void EnsureVisualHierarchy()
        {
            ResolveReferences();

            var rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            if (tintImage == null)
            {
                var tintChild = transform.Find("SlowMoTint");
                if (tintChild == null)
                {
                    var go = new GameObject("SlowMoTint", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    go.transform.SetParent(transform, false);
                    tintChild = go.transform;
                }
                tintImage = tintChild.GetComponent<Image>();
                var tintRect = tintChild.GetComponent<RectTransform>();
                tintRect.anchorMin = Vector2.zero;
                tintRect.anchorMax = Vector2.one;
                tintRect.offsetMin = Vector2.zero;
                tintRect.offsetMax = Vector2.zero;
            }

            Color tintCol = config != null ? config.TintColor : new Color(0.1f, 0.45f, 0.95f, 0.18f);
            tintImage.color = tintCol;
            tintImage.raycastTarget = false;

            if (vignetteImage == null)
            {
                var vigChild = transform.Find("SlowMoVignette");
                if (vigChild == null)
                {
                    var go = new GameObject("SlowMoVignette", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    go.transform.SetParent(transform, false);
                    vigChild = go.transform;
                }
                vignetteImage = vigChild.GetComponent<Image>();
                var vigRect = vigChild.GetComponent<RectTransform>();
                vigRect.anchorMin = Vector2.zero;
                vigRect.anchorMax = Vector2.one;
                vigRect.offsetMin = Vector2.zero;
                vigRect.offsetMax = Vector2.zero;
            }

            if (cachedVignetteSprite == null)
            {
                cachedVignetteSprite = CreateRadialVignetteSprite();
            }

            vignetteImage.sprite = cachedVignetteSprite;
            vignetteImage.type = Image.Type.Simple;
            Color vigCol = config != null ? config.VignetteColor : new Color(0.05f, 0.25f, 0.65f, 0.65f);
            vignetteImage.color = vigCol;
            vignetteImage.raycastTarget = false;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }

        private static Sprite CreateRadialVignetteSprite()
        {
            int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float maxDist = size * 0.5f * 1.4142f;

            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float normDist = Mathf.Clamp01(dist / maxDist);
                    float alpha = Mathf.SmoothStep(0.18f, 0.95f, normDist);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private void HandleSlowMoStart()
        {
            EnsureVisualHierarchy();

            alphaTween?.Stop();
            alphaTween = Tween.Alpha(canvasGroup, 1f, 0.2f, EaseType.EaseOutCubic, useUnscaledTime: true);

            bool reducedMotion = SettingsService.Instance?.CurrentSettings != null 
                && SettingsService.Instance.CurrentSettings.ReducedMotion;

            if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
            if (!reducedMotion)
            {
                pulseCoroutine = StartCoroutine(VignettePulseRoutine());
            }
            else if (vignetteImage != null)
            {
                Color baseColor = config != null ? config.VignetteColor : new Color(0.05f, 0.25f, 0.65f, 0.65f);
                vignetteImage.color = baseColor;
            }
        }

        private void HandleSlowMoEnd()
        {
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
                pulseCoroutine = null;
            }

            alphaTween?.Stop();
            alphaTween = Tween.Alpha(canvasGroup, 0f, 0.3f, EaseType.Linear, useUnscaledTime: true);
        }

        private IEnumerator VignettePulseRoutine()
        {
            Color baseColor = config != null ? config.VignetteColor : new Color(0.05f, 0.25f, 0.65f, 0.65f);
            float elapsed = 0f;

            while (true)
            {
                if (PauseController.Instance == null || !PauseController.Instance.IsPaused)
                {
                    elapsed += Time.unscaledDeltaTime;
                }
                float pulse = 0.9f + (0.1f * Mathf.Sin(elapsed * 5.236f));
                if (vignetteImage != null)
                {
                    vignetteImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * pulse);
                }
                yield return null;
            }
        }
    }
}
