using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace DodgeGame.UI
{
    [RequireComponent(typeof(Slider))]
    public class SettingsSliderStepper : MonoBehaviour, IMoveHandler, IScrollHandler
    {
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI valueLabel;
        [SerializeField] private float stepPercent = 0.05f;
        [SerializeField] private bool isDirectPercent = false;

        private void Awake()
        {
            if (slider == null) slider = GetComponent<Slider>();
            if (slider != null)
            {
                slider.onValueChanged.AddListener(OnValueChanged);
                UpdateLabel(slider.value);
            }
        }

        private void OnDestroy()
        {
            if (slider != null)
            {
                slider.onValueChanged.RemoveListener(OnValueChanged);
            }
        }

        public void SetReferences(Slider s, TextMeshProUGUI label, bool directPercent = false)
        {
            slider = s;
            valueLabel = label;
            isDirectPercent = directPercent;
            if (slider != null)
            {
                slider.onValueChanged.RemoveListener(OnValueChanged);
                slider.onValueChanged.AddListener(OnValueChanged);
                UpdateLabel(slider.value);
            }
        }

        private void OnValueChanged(float val)
        {
            UpdateLabel(val);
        }

        public void UpdateLabel(float val)
        {
            if (valueLabel == null) return;
            if (isDirectPercent)
            {
                int pct = Mathf.RoundToInt(val * 100f);
                valueLabel.text = $"{pct}%";
            }
            else
            {
                int pct = Mathf.RoundToInt(val * 100f);
                valueLabel.text = $"{pct}%";
            }
        }

        public void OnMove(AxisEventData eventData)
        {
            if (slider == null || !slider.interactable) return;

            float step = stepPercent;
            if (isDirectPercent)
            {
                step = 0.05f;
            }

            if (eventData.moveDir == MoveDirection.Left)
            {
                slider.value = Mathf.Clamp(slider.value - step, slider.minValue, slider.maxValue);
                eventData.Use();
            }
            else if (eventData.moveDir == MoveDirection.Right)
            {
                slider.value = Mathf.Clamp(slider.value + step, slider.minValue, slider.maxValue);
                eventData.Use();
            }
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (slider == null || !slider.interactable) return;

            float scrollY = eventData.scrollDelta.y;
            if (Mathf.Abs(scrollY) < 0.01f) return;

            float step = stepPercent;
            if (isDirectPercent)
            {
                step = 0.05f;
            }

            float delta = Mathf.Sign(scrollY) * step;
            slider.value = Mathf.Clamp(slider.value + delta, slider.minValue, slider.maxValue);
            eventData.Use();
        }
    }
}
