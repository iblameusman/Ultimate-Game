using System;
using System.Collections;
using UnityEngine;
using DodgeGame.Core;

namespace DodgeGame.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class MenuPanelTransition : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup panelCanvasGroup;
        [SerializeField] private RectTransform panelRectTransform;
        [SerializeField] private CanvasGroup mainColumnCanvasGroup;

        [Header("Transition Tuning")]
        [SerializeField] private float slideDistance = 24f;
        [SerializeField] private float openFadeDuration = 0.2f;
        [SerializeField] private float closeFadeDuration = 0.15f;
        [SerializeField] private float mainColumnDimOpacity = 0.6f;

        private Vector2 initialAnchoredPosition;
        private bool hasInitializedPosition = false;
        private Coroutine activeTransitionRoutine;
        private Coroutine activeDimRoutine;

        public bool IsOpen => gameObject.activeSelf && panelCanvasGroup != null && panelCanvasGroup.alpha > 0.05f;

        private void Awake()
        {
            EnsureComponents();
        }

        private void EnsureComponents()
        {
            if (panelCanvasGroup == null) panelCanvasGroup = GetComponent<CanvasGroup>();
            if (panelRectTransform == null) panelRectTransform = GetComponent<RectTransform>();

            if (!hasInitializedPosition && panelRectTransform != null)
            {
                initialAnchoredPosition = panelRectTransform.anchoredPosition;
                hasInitializedPosition = true;
            }
        }

        public void SetMainColumn(CanvasGroup mainColumn)
        {
            mainColumnCanvasGroup = mainColumn;
        }

        public void Open(Action onComplete = null)
        {
            EnsureComponents();
            gameObject.SetActive(true);

            if (activeTransitionRoutine != null) StopCoroutine(activeTransitionRoutine);
            activeTransitionRoutine = StartCoroutine(OpenRoutine(onComplete));

            if (mainColumnCanvasGroup != null)
            {
                if (activeDimRoutine != null) StopCoroutine(activeDimRoutine);
                activeDimRoutine = StartCoroutine(DimColumnRoutine(mainColumnDimOpacity, openFadeDuration));
            }
        }

        public void OpenPanel(Action onComplete = null) => Open(onComplete);

        public void Close(Action onComplete = null)
        {
            EnsureComponents();
            if (!gameObject.activeSelf) return;

            if (activeTransitionRoutine != null) StopCoroutine(activeTransitionRoutine);
            activeTransitionRoutine = StartCoroutine(CloseRoutine(onComplete));

            if (mainColumnCanvasGroup != null)
            {
                if (activeDimRoutine != null) StopCoroutine(activeDimRoutine);
                activeDimRoutine = StartCoroutine(DimColumnRoutine(1.0f, closeFadeDuration));
            }
        }

        public void ClosePanel(Action onComplete = null) => Close(onComplete);

        private bool IsReducedMotionActive()
        {
            return SettingsService.Instance != null &&
                   SettingsService.Instance.CurrentSettings != null &&
                   SettingsService.Instance.CurrentSettings.ReducedMotion;
        }

        private IEnumerator OpenRoutine(Action onComplete)
        {
            bool reducedMotion = IsReducedMotionActive();
            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;

            Vector2 startPos = reducedMotion
                ? initialAnchoredPosition
                : initialAnchoredPosition + new Vector2(slideDistance, 0f);

            panelRectTransform.anchoredPosition = startPos;

            float elapsed = 0f;
            Vector3 startScale = reducedMotion ? Vector3.one : Vector3.one * 0.95f;
            panelRectTransform.localScale = startScale;

            while (elapsed < openFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / openFadeDuration);
                float ease = Mathf.SmoothStep(0f, 1f, t);

                panelCanvasGroup.alpha = ease;
                if (!reducedMotion)
                {
                    panelRectTransform.anchoredPosition = Vector2.Lerp(startPos, initialAnchoredPosition, ease);
                    panelRectTransform.localScale = Vector3.Lerp(startScale, Vector3.one, ease);
                }

                yield return null;
            }

            panelCanvasGroup.alpha = 1f;
            panelRectTransform.anchoredPosition = initialAnchoredPosition;
            panelRectTransform.localScale = Vector3.one;
            panelCanvasGroup.interactable = true;
            panelCanvasGroup.blocksRaycasts = true;

            activeTransitionRoutine = null;
            onComplete?.Invoke();
        }

        private IEnumerator CloseRoutine(Action onComplete)
        {
            bool reducedMotion = IsReducedMotionActive();
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;

            Vector2 targetPos = reducedMotion
                ? initialAnchoredPosition
                : initialAnchoredPosition + new Vector2(-slideDistance, 0f);

            Vector2 startPos = panelRectTransform.anchoredPosition;
            float startAlpha = panelCanvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < closeFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / closeFadeDuration);
                float ease = Mathf.SmoothStep(0f, 1f, t);

                panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, ease);
                if (!reducedMotion)
                {
                    panelRectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, ease);
                }

                yield return null;
            }

            panelCanvasGroup.alpha = 0f;
            panelRectTransform.anchoredPosition = initialAnchoredPosition;
            gameObject.SetActive(false);

            activeTransitionRoutine = null;
            onComplete?.Invoke();
        }

        private IEnumerator DimColumnRoutine(float targetAlpha, float duration)
        {
            if (mainColumnCanvasGroup == null) yield break;

            float startAlpha = mainColumnCanvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                mainColumnCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            mainColumnCanvasGroup.alpha = targetAlpha;
            activeDimRoutine = null;
        }
    }
}
