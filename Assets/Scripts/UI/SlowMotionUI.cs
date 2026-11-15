using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DodgeGame.Core;
using DodgeGame.Player;
using DodgeGame.Data;

namespace DodgeGame.UI
{
    public class SlowMotionUI : MonoBehaviour
    {
        [Header("UI Element References")]
        [Tooltip("Filled Image component representing meter progress.")]
        [SerializeField] private Image fillBarImage;

        [Tooltip("TextMeshPro component displaying charge percentage or READY status.")]
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("Event & State Sources")]
        [SerializeField] private SlowMotionMeter meter;
        [SerializeField] private SlowMotionController controller;

        [Header("Visual Theme")]
        [SerializeField] private Color chargingColor = new Color(0.2f, 0.6f, 1.0f, 1f);
        [SerializeField] private Color readyColor = new Color(0.3f, 1.0f, 0.5f, 1f);
        [SerializeField] private Color activeColor = new Color(0.25f, 0.9f, 1.0f, 1f);
        [SerializeField] private Color pulseGreenColor = new Color(0.4f, 1.0f, 0.6f, 1f);

        private TweenHandle fillTween;
        private TweenHandle pulseTween;
        private Coroutine activeCountdownCoroutine;
        private Coroutine readyPulseCoroutine;
        private Vector3 defaultScale = Vector3.one;

        private void Awake()
        {
            defaultScale = transform.localScale;
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeEvents();
            UpdateDisplayImmediate();
        }

        private void Start()
        {
            ResolveReferences();
            SubscribeEvents();
            UpdateDisplayImmediate();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
            StopAllRunningCoroutines();
        }

        private void ResolveReferences()
        {
            if (meter == null) meter = FindAnyObjectByType<SlowMotionMeter>();
            if (controller == null) controller = FindAnyObjectByType<SlowMotionController>();

            if (fillBarImage == null)
            {
                var fillTr = transform.Find("Fill") ?? transform.Find("FillBar");
                if (fillTr != null) fillBarImage = fillTr.GetComponent<Image>();
            }

            if (statusText == null)
            {
                var textTr = transform.Find("Text") ?? transform.Find("StatusText");
                if (textTr != null) statusText = textTr.GetComponent<TextMeshProUGUI>();
            }
        }

        private void SubscribeEvents()
        {
            if (meter != null)
            {
                meter.OnChanged -= HandleMeterChanged;
                meter.OnChanged += HandleMeterChanged;
                meter.OnMeterFull -= HandleMeterFull;
                meter.OnMeterFull += HandleMeterFull;
            }

            if (controller != null)
            {
                controller.OnSlowMoStart -= HandleSlowMoStart;
                controller.OnSlowMoStart += HandleSlowMoStart;
                controller.OnSlowMoEnd -= HandleSlowMoEnd;
                controller.OnSlowMoEnd += HandleSlowMoEnd;
            }
        }

        private void UnsubscribeEvents()
        {
            if (meter != null)
            {
                meter.OnChanged -= HandleMeterChanged;
                meter.OnMeterFull -= HandleMeterFull;
            }

            if (controller != null)
            {
                controller.OnSlowMoStart -= HandleSlowMoStart;
                controller.OnSlowMoEnd -= HandleSlowMoEnd;
            }
        }

        private void StopAllRunningCoroutines()
        {
            if (activeCountdownCoroutine != null)
            {
                StopCoroutine(activeCountdownCoroutine);
                activeCountdownCoroutine = null;
            }

            if (readyPulseCoroutine != null)
            {
                StopCoroutine(readyPulseCoroutine);
                readyPulseCoroutine = null;
            }

            fillTween?.Stop();
            fillTween = null;
            pulseTween?.Stop();
            pulseTween = null;

            transform.localScale = defaultScale;
        }

        private void HandleMeterChanged(float current, float max)
        {
            if (controller != null && controller.IsSlowMoActive) return;

            float targetProgress = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            AnimateFillTo(targetProgress);

            if (meter != null && meter.IsReady)
            {
                SetReadyState();
            }
            else
            {
                StopPulse();
                int pct = Mathf.RoundToInt(targetProgress * 100f);
                if (statusText != null)
                {
                    statusText.text = $"SLOW-MO: {pct}%";
                    statusText.color = Color.white;
                }
                if (fillBarImage != null)
                {
                    fillBarImage.color = chargingColor;
                }
            }
        }

        private void HandleMeterFull()
        {
            AudioManager.Instance?.PlaySkillReady();
            SetReadyState();
        }

        private void SetReadyState()
        {
            if (statusText != null)
            {
                statusText.text = "SLOW-MO: READY [E]";
                statusText.color = readyColor;
            }

            if (fillBarImage != null)
            {
                fillBarImage.color = readyColor;
            }

            StartPulse();
        }

        private void AnimateFillTo(float targetProgress)
        {
            if (fillBarImage == null) return;

            fillTween?.Stop();
            float startVal = fillBarImage.fillAmount;

            fillTween = Tween.Value(
                startVal,
                targetProgress,
                0.22f,
                val =>
                {
                    if (fillBarImage != null) fillBarImage.fillAmount = val;
                },
                EaseType.EaseOutCubic,
                useUnscaledTime: true
            );
        }

        private void StartPulse()
        {
            bool reducedMotion = SettingsService.Instance?.CurrentSettings != null 
                && SettingsService.Instance.CurrentSettings.ReducedMotion;

            if (readyPulseCoroutine != null) StopCoroutine(readyPulseCoroutine);
            if (!reducedMotion)
            {
                readyPulseCoroutine = StartCoroutine(GreenPulseRoutine());
            }
        }

        private void StopPulse()
        {
            if (readyPulseCoroutine != null)
            {
                StopCoroutine(readyPulseCoroutine);
                readyPulseCoroutine = null;
            }
            transform.localScale = defaultScale;
        }

        private IEnumerator GreenPulseRoutine()
        {
            float elapsed = 0f;
            while (meter != null && meter.IsReady && (controller == null || !controller.IsSlowMoActive))
            {
                if (PauseController.Instance == null || !PauseController.Instance.IsPaused)
                {
                    elapsed += Time.unscaledDeltaTime;
                }

                float wave = (Mathf.Sin(elapsed * 4f) + 1f) * 0.5f;
                transform.localScale = defaultScale * (1f + 0.05f * wave);

                if (fillBarImage != null)
                {
                    fillBarImage.color = Color.Lerp(readyColor, pulseGreenColor, wave);
                }

                yield return null;
            }

            transform.localScale = defaultScale;
        }

        private void HandleSlowMoStart()
        {
            StopPulse();
            if (activeCountdownCoroutine != null) StopCoroutine(activeCountdownCoroutine);
            activeCountdownCoroutine = StartCoroutine(ActiveCountdownRoutine());
        }

        private void HandleSlowMoEnd()
        {
            if (activeCountdownCoroutine != null)
            {
                StopCoroutine(activeCountdownCoroutine);
                activeCountdownCoroutine = null;
            }

            UpdateDisplayImmediate();
        }

        private IEnumerator ActiveCountdownRoutine()
        {
            if (statusText != null)
            {
                statusText.text = "SLOW-MO: ACTIVE";
                statusText.color = activeColor;
            }

            if (fillBarImage != null)
            {
                fillBarImage.color = activeColor;
            }

            float duration = controller != null ? controller.Duration : 3.0f;

            while (controller != null && controller.IsSlowMoActive)
            {
                float remaining = controller.TimeRemaining;
                float progress = duration > 0f ? Mathf.Clamp01(remaining / duration) : 0f;

                if (fillBarImage != null)
                {
                    fillBarImage.fillAmount = progress;
                }

                if (statusText != null)
                {
                    statusText.text = $"SLOW-MO: {remaining:F1}s";
                    statusText.color = activeColor;
                }

                yield return null;
            }

            activeCountdownCoroutine = null;
            UpdateDisplayImmediate();
        }

        private void UpdateDisplayImmediate()
        {
            if (controller != null && controller.IsSlowMoActive)
            {
                HandleSlowMoStart();
                return;
            }

            if (meter == null)
            {
                if (statusText != null) statusText.text = "SLOW-MO: 0%";
                if (fillBarImage != null) fillBarImage.fillAmount = 0f;
                return;
            }

            float charge = meter.Charge;
            if (fillBarImage != null) fillBarImage.fillAmount = charge;

            if (meter.IsReady)
            {
                SetReadyState();
            }
            else
            {
                StopPulse();
                int pct = Mathf.RoundToInt(charge * 100f);
                if (statusText != null)
                {
                    statusText.text = $"SLOW-MO: {pct}%";
                    statusText.color = Color.white;
                }
                if (fillBarImage != null)
                {
                    fillBarImage.color = chargingColor;
                }
            }
        }
    }
}
