using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DodgeGame.UI
{
    [RequireComponent(typeof(Selectable))]
    public class ButtonStateFX : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler,
        ISelectHandler, IDeselectHandler
    {
        [Header("Target References")]
        [SerializeField] private Image buttonImage;
        [SerializeField] private Image edgeGlowImage;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform scaleTarget;

        [Header("Visual Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color highlightColor = new Color(1.10f, 1.10f, 1.10f, 1.0f);
        [SerializeField] private Color edgeGlowColor = new Color(0.239f, 0.863f, 0.165f, 0.40f);

        private Selectable selectable;
        private bool isPointerInside = false;
        private bool isSelected = false;
        private Coroutine activeGlowRoutine;
        private Coroutine activeScaleRoutine;

        private void Awake()
        {
            selectable = GetComponent<Selectable>();
            if (scaleTarget == null) scaleTarget = GetComponent<RectTransform>();
            if (buttonImage == null) buttonImage = GetComponent<Image>();
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

            EnsureEdgeGlow();
            ApplyInitialState();
        }

        private void OnEnable()
        {
            ApplyInitialState();
        }

        private void Update()
        {
            if (selectable != null)
            {
                bool interactable = selectable.interactable;
                if (canvasGroup != null)
                {
                    float targetAlpha = interactable ? 1.0f : 0.4f;
                    if (!Mathf.Approximately(canvasGroup.alpha, targetAlpha))
                    {
                        canvasGroup.alpha = targetAlpha;
                        if (!interactable && edgeGlowImage != null)
                        {
                            SetGlowAlpha(0f);
                        }
                    }
                }
            }
        }

        private void EnsureEdgeGlow()
        {
            if (edgeGlowImage == null)
            {
                var glowTransform = transform.Find("EdgeGlow");
                if (glowTransform != null)
                {
                    edgeGlowImage = glowTransform.GetComponent<Image>();
                }
                else
                {
                    var glowObj = new GameObject("EdgeGlow");
                    glowObj.transform.SetParent(transform, false);
                    glowObj.transform.SetAsFirstSibling();

                    var rect = glowObj.AddComponent<RectTransform>();
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = new Vector2(-2f, -2f);
                    rect.offsetMax = new Vector2(2f, 2f);

                    edgeGlowImage = glowObj.AddComponent<Image>();
                    edgeGlowImage.color = new Color(edgeGlowColor.r, edgeGlowColor.g, edgeGlowColor.b, 0f);
                    edgeGlowImage.raycastTarget = false;
                }
            }
        }

        public void SetBaseColors(Color normal, Color highlight)
        {
            normalColor = normal;
            highlightColor = highlight;
            if (buttonImage == null) buttonImage = GetComponent<Image>();
            if (buttonImage != null) buttonImage.color = normalColor;
        }

        private void ApplyInitialState()
        {
            if (scaleTarget != null) scaleTarget.localScale = Vector3.one;
            if (buttonImage != null) buttonImage.color = normalColor;
            SetGlowAlpha(0f);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (selectable != null && !selectable.interactable) return;
            isPointerInside = true;
            UpdateHighlightState();
            DodgeGame.Core.SoundManager.Instance?.PlayButtonHover();
            DodgeGame.Core.AudioManager.Instance?.PlayUI("UI_HOVER");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerInside = false;
            UpdateHighlightState();
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (selectable != null && !selectable.interactable) return;
            isSelected = true;
            UpdateHighlightState();
            if (!isPointerInside)
            {
                DodgeGame.Core.SoundManager.Instance?.PlayButtonHover();
                DodgeGame.Core.AudioManager.Instance?.PlayUI("UI_HOVER");
            }
        }

        public void OnDeselect(BaseEventData eventData)
        {
            isSelected = false;
            UpdateHighlightState();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (selectable != null && !selectable.interactable)
            {
                DodgeGame.Core.AudioManager.Instance?.PlayUI("UI_ERROR");
                return;
            }
            StartScaleAnimation(new Vector3(0.97f, 0.97f, 1.0f), 0.05f);
            DodgeGame.Core.SoundManager.Instance?.PlayButtonPress();
            DodgeGame.Core.AudioManager.Instance?.PlayUI("UI_SELECT");
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            StartScaleAnimation(Vector3.one, 0.05f);
        }

        private void UpdateHighlightState()
        {
            if (selectable != null && !selectable.interactable) return;

            bool isHighlighted = isPointerInside || isSelected;

            if (buttonImage != null)
            {
                buttonImage.color = isHighlighted ? highlightColor : normalColor;
            }

            StartGlowAnimation(isHighlighted ? edgeGlowColor.a : 0f, 0.12f);
        }

        private void StartGlowAnimation(float targetAlpha, float duration)
        {
            if (edgeGlowImage == null) return;
            if (activeGlowRoutine != null) StopCoroutine(activeGlowRoutine);
            activeGlowRoutine = StartCoroutine(GlowRoutine(targetAlpha, duration));
        }

        private IEnumerator GlowRoutine(float targetAlpha, float duration)
        {
            float startAlpha = edgeGlowImage.color.a;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float a = Mathf.Lerp(startAlpha, targetAlpha, t);
                SetGlowAlpha(a);
                yield return null;
            }

            SetGlowAlpha(targetAlpha);
            activeGlowRoutine = null;
        }

        private void SetGlowAlpha(float alpha)
        {
            if (edgeGlowImage != null)
            {
                Color c = edgeGlowColor;
                c.a = alpha;
                edgeGlowImage.color = c;
            }
        }

        private void StartScaleAnimation(Vector3 targetScale, float duration)
        {
            if (scaleTarget == null) return;
            if (activeScaleRoutine != null) StopCoroutine(activeScaleRoutine);
            activeScaleRoutine = StartCoroutine(ScaleRoutine(targetScale, duration));
        }

        private IEnumerator ScaleRoutine(Vector3 targetScale, float duration)
        {
            Vector3 startScale = scaleTarget.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                scaleTarget.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            scaleTarget.localScale = targetScale;
            activeScaleRoutine = null;
        }
    }
}
