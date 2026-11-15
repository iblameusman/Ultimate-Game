using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DodgeGame.Core;
using DodgeGame.Player;

namespace DodgeGame.UI
{
    public class DashUI : MonoBehaviour
    {
        [Header("UI Element References")]
        [Tooltip("Image component with Image.Type = Filled to visually indicate cooldown/active state.")]
        [SerializeField] private Image fillBarImage;

        [Tooltip("TextMeshPro component displaying status text (READY, ACTIVE, or seconds remaining).")]
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("Event Source")]
        [Tooltip("Player DashController component.")]
        [SerializeField] private DashController dashController;

        [Header("Visual Theme")]
        [SerializeField] private Color readyColor = new Color(0.3f, 1.0f, 0.5f, 1f);
        [SerializeField] private Color activeColor = new Color(0.25f, 0.85f, 1.0f, 1f);
        [SerializeField] private Color cooldownColor = new Color(0.9f, 0.75f, 0.2f, 1f);
        [SerializeField] private Color pulseGreenColor = new Color(0.4f, 1.0f, 0.6f, 1f);

        private Coroutine activeAnimationCoroutine;
        private Coroutine readyPulseCoroutine;
        private Vector3 defaultScale = Vector3.one;

        private void Awake()
        {
            defaultScale = transform.localScale;
            if (dashController == null)
            {
                dashController = FindAnyObjectByType<DashController>();
            }

            if (fillBarImage == null)
            {
                var fillTr = transform.Find("Fill");
                if (fillTr != null) fillBarImage = fillTr.GetComponent<Image>();
            }

            if (statusText == null)
            {
                var textTr = transform.Find("Text");
                if (textTr != null) statusText = textTr.GetComponent<TextMeshProUGUI>();
            }
        }

        private void OnEnable()
        {
            if (dashController == null)
            {
                dashController = FindAnyObjectByType<DashController>();
            }

            if (dashController != null)
            {
                dashController.OnDashStart += HandleDashStart;
                dashController.OnDashEnd += HandleDashEnd;
            }

            UpdateReadyDisplay();
        }

        private void Start()
        {
            if (dashController == null)
            {
                dashController = FindAnyObjectByType<DashController>();
            }

            if (dashController != null)
            {
                dashController.OnDashStart -= HandleDashStart;
                dashController.OnDashStart += HandleDashStart;
                dashController.OnDashEnd -= HandleDashEnd;
                dashController.OnDashEnd += HandleDashEnd;
            }

            UpdateReadyDisplay();
        }

        private void OnDisable()
        {
            if (dashController != null)
            {
                dashController.OnDashStart -= HandleDashStart;
                dashController.OnDashEnd -= HandleDashEnd;
            }

            if (activeAnimationCoroutine != null)
            {
                StopCoroutine(activeAnimationCoroutine);
                activeAnimationCoroutine = null;
            }

            StopPulse();
        }

        private void HandleDashStart()
        {
            StopPulse();
            if (activeAnimationCoroutine != null)
            {
                StopCoroutine(activeAnimationCoroutine);
            }
            float duration = dashController.Config != null ? dashController.Config.Duration : 0.15f;
            activeAnimationCoroutine = StartCoroutine(DashActiveRoutine(duration));
        }

        private void HandleDashEnd()
        {
            if (activeAnimationCoroutine != null)
            {
                StopCoroutine(activeAnimationCoroutine);
            }
            float cooldown = dashController.Config != null ? dashController.Config.Cooldown : 2.5f;
            activeAnimationCoroutine = StartCoroutine(CooldownRoutine(cooldown));
        }

        private IEnumerator DashActiveRoutine(float duration)
        {
            if (statusText != null)
            {
                statusText.text = "DASH: ACTIVE";
                statusText.color = activeColor;
            }

            if (fillBarImage != null)
            {
                fillBarImage.color = activeColor;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (PauseController.Instance == null || !PauseController.Instance.IsPaused)
                {
                    elapsed += Time.unscaledDeltaTime;
                }
                float progress = Mathf.Clamp01(1f - (elapsed / duration));
                if (fillBarImage != null)
                {
                    fillBarImage.fillAmount = progress;
                }
                yield return null;
            }

            if (fillBarImage != null)
            {
                fillBarImage.fillAmount = 0f;
            }
        }

        private IEnumerator CooldownRoutine(float cooldownDuration)
        {
            if (fillBarImage != null)
            {
                fillBarImage.color = cooldownColor;
            }

            float remaining = cooldownDuration;
            while (remaining > 0f)
            {
                if (PauseController.Instance == null || !PauseController.Instance.IsPaused)
                {
                    remaining -= Time.unscaledDeltaTime;
                }
                float progress = Mathf.Clamp01(1f - (remaining / cooldownDuration));
                if (fillBarImage != null)
                {
                    fillBarImage.fillAmount = progress;
                }

                if (statusText != null)
                {
                    statusText.text = $"DASH: {Mathf.Max(0f, remaining):F1}s";
                    statusText.color = Color.white;
                }

                yield return null;
            }

            AudioManager.Instance?.PlaySkillReady();
            UpdateReadyDisplay();
            activeAnimationCoroutine = null;
        }

        private void UpdateReadyDisplay()
        {
            if (statusText != null)
            {
                statusText.text = "DASH: READY [SHIFT]";
                statusText.color = readyColor;
            }

            if (fillBarImage != null)
            {
                fillBarImage.fillAmount = 1f;
                fillBarImage.color = readyColor;
            }

            StartPulse();
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
            while (dashController != null && dashController.CurrentState == DashState.Ready)
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
    }
}
