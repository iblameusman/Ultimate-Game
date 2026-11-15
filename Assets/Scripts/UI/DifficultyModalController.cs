using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using DodgeGame.Core;
using DodgeGame.Data;

namespace DodgeGame.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DifficultyModalController : MonoBehaviour
    {
        [Header("Modal Panel & Dimmer")]
        [SerializeField] private CanvasGroup dimmerCanvasGroup;
        [SerializeField] private Image dimmerImage;
        [SerializeField] private RectTransform modalPanelRect;
        [SerializeField] private CanvasGroup modalPanelCanvasGroup;

        [Header("Header Elements")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;

        [Header("Difficulty Cards")]
        [SerializeField] private DifficultyCardUI easyCard;
        [SerializeField] private DifficultyCardUI normalCard;
        [SerializeField] private DifficultyCardUI hardCard;
        [SerializeField] private DifficultyCardUI nightmareCard;

        [Header("Footer Controls")]
        [SerializeField] private Button backButton;

        [Header("Configuration")]
        [SerializeField] private DifficultyConfig difficultyConfig;

        [Header("Animation Tuning")]
        [SerializeField] private float dimmerFadeInDuration = 0.2f;
        [SerializeField] private float panelOpenDuration = 0.25f;
        [SerializeField] private float closeDuration = 0.15f;
        [SerializeField] private float dimmerTargetAlpha = 0.55f;

        public event Action<Difficulty> OnDifficultyHighlighted;

        private Action onCloseCallback;
        private Coroutine activeOpenRoutine;
        private Coroutine activeCloseRoutine;
        private bool isOpen = false;
        private bool isProcessingSelection = false;

        public bool IsOpen => isOpen;

        public void ResolveDifficultyConfig()
        {
            if (difficultyConfig == null)
            {
                difficultyConfig = Resources.Load<DifficultyConfig>("SO_DifficultyConfig") ??
                                   Resources.Load<DifficultyConfig>("DifficultyConfig");
#if UNITY_EDITOR
                if (difficultyConfig == null)
                {
                    difficultyConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<DifficultyConfig>("Assets/ScriptableObjects/SO_DifficultyConfig.asset")
                                    ?? UnityEditor.AssetDatabase.LoadAssetAtPath<DifficultyConfig>("Assets/ScriptableObjects/DifficultyConfig.asset");
                }
#endif
            }
        }

        private void Awake()
        {
            ResolveDifficultyConfig();

            if (backButton != null)
            {
                backButton.onClick.AddListener(Close);
            }

            RegisterCardEvents(easyCard);
            RegisterCardEvents(normalCard);
            RegisterCardEvents(hardCard);
            RegisterCardEvents(nightmareCard);

            if (modalPanelCanvasGroup != null) modalPanelCanvasGroup.alpha = 0f;
            if (dimmerCanvasGroup != null) dimmerCanvasGroup.alpha = 0f;
            if (!isOpen)
            {
                gameObject.SetActive(false);
            }
        }

        private void RegisterCardEvents(DifficultyCardUI card)
        {
            if (card == null) return;
            card.OnCardSelected += HandleCardSelected;
            card.OnCardHighlighted += HandleCardHighlighted;
        }

        private void OnDestroy()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveListener(Close);
            }

            UnregisterCardEvents(easyCard);
            UnregisterCardEvents(normalCard);
            UnregisterCardEvents(hardCard);
            UnregisterCardEvents(nightmareCard);
        }

        private void UnregisterCardEvents(DifficultyCardUI card)
        {
            if (card == null) return;
            card.OnCardSelected -= HandleCardSelected;
            card.OnCardHighlighted -= HandleCardHighlighted;
        }

        private void Update()
        {
            if (!isOpen || isProcessingSelection) return;

            bool cancelPressed = false;

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                cancelPressed = true;
            }

            if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
            {
                cancelPressed = true;
            }

            if (cancelPressed)
            {
                Close();
            }
        }

        public void Open(Action onClose = null)
        {
            onCloseCallback = onClose;
            isOpen = true;
            isProcessingSelection = false;
            gameObject.SetActive(true);

            if (activeCloseRoutine != null) StopCoroutine(activeCloseRoutine);
            if (activeOpenRoutine != null) StopCoroutine(activeOpenRoutine);

            activeOpenRoutine = StartCoroutine(OpenRoutine());
        }

        private IEnumerator OpenRoutine()
        {
            bool reducedMotion = IsReducedMotionActive();

            if (dimmerCanvasGroup != null)
            {
                dimmerCanvasGroup.alpha = 0f;
                dimmerCanvasGroup.blocksRaycasts = true;
                dimmerCanvasGroup.interactable = true;
            }

            if (modalPanelCanvasGroup != null)
            {
                modalPanelCanvasGroup.alpha = 0f;
                modalPanelCanvasGroup.blocksRaycasts = true;
                modalPanelCanvasGroup.interactable = true;
            }

            if (modalPanelRect != null)
            {
                modalPanelRect.localScale = reducedMotion ? Vector3.one : (Vector3.one * 0.95f);
            }

            float elapsed = 0f;
            float maxDuration = Mathf.Max(dimmerFadeInDuration, panelOpenDuration);

            while (elapsed < maxDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                if (dimmerCanvasGroup != null)
                {
                    float dimmerT = Mathf.Clamp01(elapsed / dimmerFadeInDuration);
                    dimmerCanvasGroup.alpha = Mathf.Lerp(0f, dimmerTargetAlpha, dimmerT);
                }

                if (modalPanelCanvasGroup != null && modalPanelRect != null)
                {
                    float panelT = Mathf.Clamp01(elapsed / panelOpenDuration);
                    modalPanelCanvasGroup.alpha = Mathf.Clamp01(panelT * 2f);

                    if (!reducedMotion)
                    {
                        const float c1 = 1.70158f;
                        const float c3 = c1 + 1f;
                        float eased = 1f + c3 * Mathf.Pow(panelT - 1f, 3f) + c1 * Mathf.Pow(panelT - 1f, 2f);
                        float scale = Mathf.LerpUnclamped(0.95f, 1.0f, eased);
                        modalPanelRect.localScale = Vector3.one * scale;
                    }
                }

                yield return null;
            }

            if (dimmerCanvasGroup != null) dimmerCanvasGroup.alpha = dimmerTargetAlpha;
            if (modalPanelCanvasGroup != null) modalPanelCanvasGroup.alpha = 1f;
            if (modalPanelRect != null) modalPanelRect.localScale = Vector3.one;

            FocusNormalCard();

            activeOpenRoutine = null;
        }

        private void FocusNormalCard()
        {
            var target = normalCard != null ? normalCard.gameObject : (easyCard != null ? easyCard.gameObject : null);
            if (target != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(target);
            }

            if (normalCard != null)
            {
                normalCard.SetHighlight(true);
                HandleCardHighlighted(normalCard);
            }
        }

        public void Close()
        {
            if (!isOpen) return;
            isOpen = false;

            if (activeOpenRoutine != null) StopCoroutine(activeOpenRoutine);
            if (activeCloseRoutine != null) StopCoroutine(activeCloseRoutine);

            activeCloseRoutine = StartCoroutine(CloseRoutine());
        }

        private IEnumerator CloseRoutine()
        {
            bool reducedMotion = IsReducedMotionActive();
            float elapsed = 0f;

            float startDimmerAlpha = dimmerCanvasGroup != null ? dimmerCanvasGroup.alpha : 0f;
            float startPanelAlpha = modalPanelCanvasGroup != null ? modalPanelCanvasGroup.alpha : 1f;
            Vector3 startScale = modalPanelRect != null ? modalPanelRect.localScale : Vector3.one;
            Vector3 targetScale = Vector3.one * 0.95f;

            if (dimmerCanvasGroup != null) dimmerCanvasGroup.blocksRaycasts = false;
            if (modalPanelCanvasGroup != null) modalPanelCanvasGroup.blocksRaycasts = false;

            while (elapsed < closeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / closeDuration);

                if (dimmerCanvasGroup != null)
                {
                    dimmerCanvasGroup.alpha = Mathf.Lerp(startDimmerAlpha, 0f, t);
                }

                if (modalPanelCanvasGroup != null)
                {
                    modalPanelCanvasGroup.alpha = Mathf.Lerp(startPanelAlpha, 0f, t);
                }

                if (!reducedMotion && modalPanelRect != null)
                {
                    modalPanelRect.localScale = Vector3.Lerp(startScale, targetScale, t);
                }

                yield return null;
            }

            if (dimmerCanvasGroup != null) dimmerCanvasGroup.alpha = 0f;
            if (modalPanelCanvasGroup != null) modalPanelCanvasGroup.alpha = 0f;
            gameObject.SetActive(false);

            onCloseCallback?.Invoke();
            activeCloseRoutine = null;
        }

        private void HandleCardHighlighted(DifficultyCardUI card)
        {
            if (card == null) return;
            OnDifficultyHighlighted?.Invoke(card.Difficulty);

            if (MainMenuUI.Instance != null)
            {
                MainMenuUI.Instance.UpdateHighScoreDisplay(card.Difficulty);
            }
        }

        private void HandleCardSelected(DifficultyCardUI card)
        {
            if (isProcessingSelection || !isOpen || card == null) return;

            Difficulty diff = card.Difficulty;
            ResolveDifficultyConfig();

            DifficultyMappingEntry mapping = difficultyConfig != null ? difficultyConfig.GetMapping(diff) : null;

            if (mapping == null)
            {
                Difficulty effective = diff == Difficulty.Hard ? Difficulty.Easy : (diff == Difficulty.Nightmare ? Difficulty.Normal : diff);
                bool blocks = diff == Difficulty.Easy || diff == Difficulty.Normal;
                string rejectClip = diff == Difficulty.Easy ? "UI_REJECT_EASY" : (diff == Difficulty.Normal ? "UI_REJECT_NORMAL" : "");
                mapping = new DifficultyMappingEntry(diff, effective, blocks, rejectClip);
            }

            if (mapping.BlocksStart)
            {
                bool hookPlayed = (diff == Difficulty.Easy || diff == Difficulty.Normal) &&
                    DodgeGame.Audio.SoundEventHook.Play(card.gameObject, diff == Difficulty.Easy ? DodgeGame.Audio.SoundEventId.EasySelected : DodgeGame.Audio.SoundEventId.MediumSelected);

                if (!hookPlayed && !string.IsNullOrEmpty(mapping.RejectVoiceClipId))
                {
                    if (AudioManager.Instance != null)
                    {
                        if (!AudioManager.Instance.IsClipPlaying(mapping.RejectVoiceClipId))
                        {
                            AudioManager.Instance.Play(mapping.RejectVoiceClipId);
                        }
                    }
                }

                card.PlayShakeAnimation();

                if (EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(card.gameObject);
                }
                return;
            }

            isProcessingSelection = true;

            if (SettingsService.Instance != null)
            {
                SettingsService.Instance.SetDifficulty(mapping.DisplayName, mapping.EffectiveTier);
            }

#if UNITY_EDITOR || DEBUG
            Debug.Log($"[Difficulty] Selected '{mapping.DisplayName}', Effective Tier '{mapping.EffectiveTier}'");
#endif

            Close();

            if (MainMenuUI.Instance != null)
            {
                MainMenuUI.Instance.StartGameWithFade();
            }
            else
            {
                TimeScaleController.ResetTimeScale();
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainGame");
            }
        }

        private bool IsReducedMotionActive()
        {
            return SettingsService.Instance != null &&
                   SettingsService.Instance.CurrentSettings != null &&
                   SettingsService.Instance.CurrentSettings.ReducedMotion;
        }
    }
}
