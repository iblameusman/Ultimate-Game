using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace DodgeGame.UI
{
    public class SettingsStepSlider : Slider, IScrollHandler
    {
        [Header("Step Tuning")]
        [SerializeField] private float stepSize = 0.05f;
        [SerializeField] private TextMeshProUGUI valueLabel;
        [SerializeField] private bool isDirectPercent = false;

        public float StepSize
        {
            get => stepSize;
            set => stepSize = value;
        }

        public TextMeshProUGUI ValueLabel
        {
            get => valueLabel;
            set
            {
                valueLabel = value;
                UpdateValueLabel(this.value);
            }
        }

        public bool IsDirectPercent
        {
            get => isDirectPercent;
            set
            {
                isDirectPercent = value;
                UpdateValueLabel(this.value);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            onValueChanged.AddListener(UpdateValueLabel);
            UpdateValueLabel(value);
        }

        protected override void OnDestroy()
        {
            onValueChanged.RemoveListener(UpdateValueLabel);
            base.OnDestroy();
        }

        public void SetReferences(TextMeshProUGUI label, bool directPercent, float step = 0.05f)
        {
            valueLabel = label;
            isDirectPercent = directPercent;
            stepSize = step;
            UpdateValueLabel(value);
        }

        public void UpdateValueLabel(float val)
        {
            if (valueLabel == null) return;
            int pct = Mathf.RoundToInt(val * 100f);
            valueLabel.text = $"{pct}%";
        }

        public override void OnMove(AxisEventData eventData)
        {
            if (!IsActive() || !IsInteractable())
            {
                base.OnMove(eventData);
                return;
            }

            if (eventData.moveDir == MoveDirection.Left)
            {
                float newVal = Mathf.Clamp(value - stepSize, minValue, maxValue);
                Set(newVal);
                UpdateValueLabel(newVal);
                eventData.Use();
            }
            else if (eventData.moveDir == MoveDirection.Right)
            {
                float newVal = Mathf.Clamp(value + stepSize, minValue, maxValue);
                Set(newVal);
                UpdateValueLabel(newVal);
                eventData.Use();
            }
            else
            {
                base.OnMove(eventData);
            }
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (!IsActive() || !IsInteractable()) return;

            float scrollY = eventData.scrollDelta.y;
            if (Mathf.Abs(scrollY) < 0.01f) return;

            float delta = Mathf.Sign(scrollY) * stepSize;
            float newVal = Mathf.Clamp(value + delta, minValue, maxValue);
            Set(newVal);
            UpdateValueLabel(newVal);
            eventData.Use();
        }
    }
}
