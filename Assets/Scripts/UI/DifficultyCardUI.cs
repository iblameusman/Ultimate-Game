using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DodgeGame.Data;
using DodgeGame.Core;

namespace DodgeGame.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class DifficultyCardUI : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        ISelectHandler, IDeselectHandler,
        IPointerDownHandler, IPointerUpHandler,
        ISubmitHandler, IPointerClickHandler
    {
        [Header("Difficulty Setting")]
        [SerializeField] private Difficulty difficulty;

        [Header("Visual References")]
        [SerializeField] private Image cardBackground;
        [SerializeField] private Image glowImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private RectTransform shakeTarget;

        [Header("Colors & Tuning")]
        [SerializeField] private Color normalColor = new Color(0.18f, 0.22f, 0.25f, 1f);
        [SerializeField] private Color brightenedColor = new Color(0.24f, 0.29f, 0.33f, 1f);
        [SerializeField] private Color glowColor = new Color(0.239f, 0.863f, 0.165f, 0.40f);

        [SerializeField] private float shakeDuration = 0.3f;
        [SerializeField] private float shakeAmplitude = 6f;
        [SerializeField] private float punchScale = 0.97f;
        [SerializeField] private float punchDuration = 0.05f;

        public Difficulty Difficulty => difficulty;

        public event Action<DifficultyCardUI> OnCardSelected;
        public event Action<DifficultyCardUI> OnCardHighlighted;

        private RectTransform rectTransform;
        private Vector2 originalAnchoredPosition;
        private Vector3 originalScale = Vector3.one;
        private Coroutine activeShakeRoutine;
        private Coroutine activePunchRoutine;
        private bool isHighlighted = false;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (shakeTarget == null) shakeTarget = rectTransform;
            originalAnchoredPosition = shakeTarget.anchoredPosition;
            originalScale = rectTransform.localScale;

            if (glowImage != null)
            {
                glowImage.color = glowColor;
                glowImage.gameObject.SetActive(false);
            }

            if (cardBackground != null)
            {
                cardBackground.color = normalColor;
            }
        }

        public void Setup(Difficulty diff, string displayName, string description, Sprite iconSprite)
        {
            difficulty = diff;
            if (nameText != null) nameText.text = displayName;
            if (descriptionText != null) descriptionText.text = description;
            if (iconImage != null && iconSprite != null) iconImage.sprite = iconSprite;
        }

        public void SetHighlight(bool state)
        {
            isHighlighted = state;
            if (glowImage != null)
            {
                glowImage.gameObject.SetActive(state);
            }

            if (cardBackground != null)
            {
                cardBackground.color = state ? brightenedColor : normalColor;
            }

            if (state)
            {
                OnCardHighlighted?.Invoke(this);
            }
        }

        public void PlayShakeAnimation()
        {
            if (activeShakeRoutine != null)
            {
                StopCoroutine(activeShakeRoutine);
                activeShakeRoutine = null;
            }
            activeShakeRoutine = StartCoroutine(ShakeRoutine());
        }

        private IEnumerator ShakeRoutine()
        {
            shakeTarget.anchoredPosition = originalAnchoredPosition;
            float elapsed = 0f;
            const float frequency = 25f;

            while (elapsed < shakeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / shakeDuration;
                float decay = 1f - progress;
                float offset = Mathf.Sin(elapsed * frequency * Mathf.PI * 2f) * shakeAmplitude * decay;
                shakeTarget.anchoredPosition = new Vector2(originalAnchoredPosition.x + offset, originalAnchoredPosition.y);
                yield return null;
            }

            shakeTarget.anchoredPosition = originalAnchoredPosition;
            activeShakeRoutine = null;
        }

        private void TriggerPunch()
        {
            if (activePunchRoutine != null) StopCoroutine(activePunchRoutine);
            activePunchRoutine = StartCoroutine(PunchRoutine());
        }

        private IEnumerator PunchRoutine()
        {
            float elapsed = 0f;
            Vector3 target = originalScale * punchScale;
            while (elapsed < punchDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                rectTransform.localScale = Vector3.Lerp(originalScale, target, elapsed / punchDuration);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < punchDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                rectTransform.localScale = Vector3.Lerp(target, originalScale, elapsed / punchDuration);
                yield return null;
            }
            rectTransform.localScale = originalScale;
            activePunchRoutine = null;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetHighlight(true);
            EventSystem.current?.SetSelectedGameObject(gameObject);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject != gameObject)
            {
                SetHighlight(false);
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            SetHighlight(true);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            SetHighlight(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            TriggerPunch();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
        }

        public void OnSubmit(BaseEventData eventData)
        {
            TriggerPunch();
            OnCardSelected?.Invoke(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnCardSelected?.Invoke(this);
        }
    }
}
