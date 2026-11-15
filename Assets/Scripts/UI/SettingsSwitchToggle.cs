using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DodgeGame.UI
{
    public class SettingsSwitchToggle : Selectable, IPointerClickHandler, ISubmitHandler
    {
        [Header("Components")]
        [SerializeField] private Image trackImage;
        [SerializeField] private RectTransform knobRect;
        [SerializeField] private GameObject checkmarkObject;
        [SerializeField] private TMPro.TextMeshProUGUI valueLabel;

        [Header("Tuning")]
        [SerializeField] private bool isOn = false;
        [SerializeField] private float slideDuration = 0.15f;
        [SerializeField] private float leftPosition = -13f;
        [SerializeField] private float rightPosition = 13f;

        private static readonly Color OffTrackColor = new Color(0.788f, 0.820f, 0.843f, 1f);
        private static readonly Color OnTrackColor = new Color(0.239f, 0.863f, 0.165f, 1f);

        public event Action<bool> onValueChanged;

        private Coroutine activeSlideRoutine;

        public bool IsOn
        {
            get => isOn;
            set
            {
                SetIsOn(value, false);
            }
        }

        public TMPro.TextMeshProUGUI ValueLabel
        {
            get => valueLabel;
            set
            {
                valueLabel = value;
                if (valueLabel != null) valueLabel.text = isOn ? "On" : "Off";
            }
        }

        protected override void Awake()
        {
            base.Awake();
            ApplyStateInstant();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            ApplyStateInstant();
        }

        public void SetIsOn(bool value, bool animate = true)
        {
            if (isOn == value)
            {
                ApplyStateInstant();
                return;
            }

            isOn = value;

            if (valueLabel != null)
            {
                valueLabel.text = isOn ? "On" : "Off";
            }

            if (animate && gameObject.activeInHierarchy)
            {
                StartSlideAnimation();
            }
            else
            {
                ApplyStateInstant();
            }

            onValueChanged?.Invoke(isOn);
        }

        public void Toggle()
        {
            if (!IsInteractable()) return;
            SetIsOn(!isOn, true);
            DodgeGame.Core.SoundManager.Instance?.PlayButtonPress();
            DodgeGame.Core.AudioManager.Instance?.PlayUI("UI_SELECT");
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            Toggle();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            Toggle();
        }

        private void Update()
        {
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
            {
                if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    Toggle();
                }
            }
        }

        private void ApplyStateInstant()
        {
            if (activeSlideRoutine != null) StopCoroutine(activeSlideRoutine);
            activeSlideRoutine = null;

            if (knobRect != null)
            {
                knobRect.anchoredPosition = new Vector2(isOn ? rightPosition : leftPosition, 0f);
            }
            if (trackImage != null)
            {
                trackImage.color = isOn ? OnTrackColor : OffTrackColor;
            }
            if (checkmarkObject != null)
            {
                checkmarkObject.SetActive(isOn);
            }
            if (valueLabel != null)
            {
                valueLabel.text = isOn ? "On" : "Off";
            }
        }

        private void StartSlideAnimation()
        {
            if (activeSlideRoutine != null) StopCoroutine(activeSlideRoutine);
            activeSlideRoutine = StartCoroutine(SlideRoutine());
        }

        private IEnumerator SlideRoutine()
        {
            Vector2 startPos = knobRect != null ? knobRect.anchoredPosition : Vector2.zero;
            Vector2 targetPos = new Vector2(isOn ? rightPosition : leftPosition, 0f);

            Color startCol = trackImage != null ? trackImage.color : (isOn ? OffTrackColor : OnTrackColor);
            Color targetCol = isOn ? OnTrackColor : OffTrackColor;

            float elapsed = 0f;
            while (elapsed < slideDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / slideDuration);
                float ease = Mathf.SmoothStep(0f, 1f, t);

                if (knobRect != null) knobRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, ease);
                if (trackImage != null) trackImage.color = Color.Lerp(startCol, targetCol, ease);

                yield return null;
            }

            if (knobRect != null) knobRect.anchoredPosition = targetPos;
            if (trackImage != null) trackImage.color = targetCol;
            if (checkmarkObject != null) checkmarkObject.SetActive(isOn);
            if (valueLabel != null) valueLabel.text = isOn ? "On" : "Off";

            activeSlideRoutine = null;
        }

        public void SetReferences(Image track, RectTransform knob, GameObject checkmark, TMPro.TextMeshProUGUI label = null)
        {
            trackImage = track;
            knobRect = knob;
            checkmarkObject = checkmark;
            if (label != null) valueLabel = label;
            ApplyStateInstant();
        }
    }
}
