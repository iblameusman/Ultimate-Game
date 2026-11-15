using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using DodgeGame.Core;
using DodgeGame.Data;

namespace DodgeGame.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Menu Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button howToPlayButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button aboutButton;
        [SerializeField] private Button quitButton;

        [Header("Difficulty Selector")]
        [SerializeField] private Button easyButton;
        [SerializeField] private Button normalButton;
        [SerializeField] private Button hardButton;
        [SerializeField] private Button nightmareButton;
        [SerializeField] private TextMeshProUGUI difficultyNoteText;

        [Header("Displays")]
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private TextMeshProUGUI versionText;

        [Header("Modals & Transitions")]
        [SerializeField] private MenuPanelTransition howToPlayTransition;
        [SerializeField] private Button closeHowToPlayButton;
        [SerializeField] private Button howToPlayBackButton;
        [SerializeField] private MenuPanelTransition settingsTransition;
        [SerializeField] private MenuSettingsUI settingsPanelUI;
        [SerializeField] private SettingsModalController settingsModalController;
        [SerializeField] private Button closeSettingsButton;
        [SerializeField] private MenuPanelTransition aboutTransition;
        [SerializeField] private Button closeAboutButton;
        [SerializeField] private Button aboutBackButton;
        [SerializeField] private Button aboutLinkButton;
        [SerializeField] private string aboutLinkUrl = "https://github.com/iblameusman/Ultimate-Game";

        [Header("Navigation & Presentation")]
        [SerializeField] private InputHintsBar inputHintsBar;
        [SerializeField] private CanvasGroup mainColumnCanvasGroup;
        [SerializeField] private RectTransform logoTransform;
        [SerializeField] private CanvasGroup logoCanvasGroup;
        [SerializeField] private List<RectTransform> animatedButtonRows = new List<RectTransform>();

        [Header("Scene Loading Screen Fade")]
        [SerializeField] private CanvasGroup screenFadeCanvasGroup;
        [SerializeField] private GameObject loadingIndicator;
        [SerializeField] private string gameSceneName = "MainGame";

        [Header("Difficulty Modal")]
        [SerializeField] private DifficultyModalController difficultyModal;

        private GameObject lastFocusedElement;
        private bool isStartingGame = false;
        private Coroutine pulseRoutine;

        public Button PlayButton => playButton;
        public Button HowToPlayButton => howToPlayButton;
        public Button SettingsButton => settingsButton;
        public Button AboutButton => aboutButton;
        public Button QuitButton => quitButton;

        public Button EasyButton => easyButton;
        public Button NormalButton => normalButton;
        public Button HardButton => hardButton;
        public Button NightmareButton => nightmareButton;

        public MenuPanelTransition HowToPlayTransition => howToPlayTransition;
        public MenuPanelTransition SettingsTransition => settingsTransition;
        public MenuSettingsUI SettingsPanelUI => settingsPanelUI;
        public SettingsModalController SettingsModalController => settingsModalController;
        public Button CloseSettingsButton => closeSettingsButton;
        public MenuPanelTransition AboutTransition => aboutTransition;
        public Button CloseHowToPlayButton => closeHowToPlayButton;
        public Button CloseAboutButton => closeAboutButton;
        public DifficultyModalController DifficultyModal => difficultyModal;

        public static MainMenuUI Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
            if (playButton != null) DodgeGame.Audio.SoundEventHook.Attach(playButton.gameObject, DodgeGame.Audio.SoundEventId.MenuPlayHover);
            if (howToPlayButton != null) howToPlayButton.onClick.AddListener(OnHowToPlayClicked);
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
            if (aboutButton != null) aboutButton.onClick.AddListener(OnAboutClicked);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
#if UNITY_WEBGL && !UNITY_EDITOR
            // Browsers can't close the tab from Application.Quit, so the button would do nothing.
            if (quitButton != null) quitButton.gameObject.SetActive(false);
#endif

            if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(OnCloseSettingsClicked);
            if (closeHowToPlayButton != null) closeHowToPlayButton.onClick.AddListener(OnCloseHowToPlayClicked);
            if (closeAboutButton != null) closeAboutButton.onClick.AddListener(OnCloseAboutClicked);
            if (aboutLinkButton != null) aboutLinkButton.onClick.AddListener(OnAboutLinkClicked);

            // Footer BACK buttons: fall back to the builder's "BackButton" child when not assigned.
            if (howToPlayBackButton == null) howToPlayBackButton = FindPanelBackButton(howToPlayTransition);
            if (aboutBackButton == null) aboutBackButton = FindPanelBackButton(aboutTransition);
            if (howToPlayBackButton != null) howToPlayBackButton.onClick.AddListener(OnCloseHowToPlayClicked);
            if (aboutBackButton != null) aboutBackButton.onClick.AddListener(OnCloseAboutClicked);

            if (easyButton != null) easyButton.onClick.AddListener(() => OnDifficultyClicked(Difficulty.Easy));
            if (normalButton != null) normalButton.onClick.AddListener(() => OnDifficultyClicked(Difficulty.Normal));
            if (hardButton != null) hardButton.onClick.AddListener(() => OnDifficultyClicked(Difficulty.Hard));
            if (nightmareButton != null) nightmareButton.onClick.AddListener(() => OnDifficultyClicked(Difficulty.Nightmare));

            if (settingsModalController != null)
            {
                settingsModalController.OnClosed += RestoreFocus;
            }

            if (settingsPanelUI != null)
            {
                settingsPanelUI.OnBackRequested += OnCloseSettingsClicked;
            }

            if (howToPlayTransition != null)
            {
                howToPlayTransition.SetMainColumn(mainColumnCanvasGroup);
                howToPlayTransition.gameObject.SetActive(false);
            }

            if (settingsTransition != null)
            {
                settingsTransition.SetMainColumn(mainColumnCanvasGroup);
                settingsTransition.gameObject.SetActive(false);
            }

            if (aboutTransition != null)
            {
                aboutTransition.SetMainColumn(mainColumnCanvasGroup);
                aboutTransition.gameObject.SetActive(false);
            }

            if (screenFadeCanvasGroup != null)
            {
                screenFadeCanvasGroup.alpha = 0f;
                screenFadeCanvasGroup.gameObject.SetActive(false);
            }

            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(false);
            }

            if (versionText != null)
            {
                versionText.text = $"v{Application.version}";
            }

            UpdateDifficultyVisuals();
            UpdateHighScoreDisplay();
        }

        private void Start()
        {
            StartCoroutine(SetInitialFocusRoutine());
            PlayEntranceAnimations();
        }

        private bool IsReducedMotionActive()
        {
            return SettingsService.Instance != null &&
                   SettingsService.Instance.CurrentSettings != null &&
                   SettingsService.Instance.CurrentSettings.ReducedMotion;
        }

        private void PlayEntranceAnimations()
        {
            bool reducedMotion = IsReducedMotionActive();

            if (logoTransform != null)
            {
                if (logoCanvasGroup == null) logoCanvasGroup = logoTransform.GetComponent<CanvasGroup>();
                if (logoCanvasGroup == null) logoCanvasGroup = logoTransform.gameObject.AddComponent<CanvasGroup>();

                logoCanvasGroup.alpha = 0f;
                Vector3 startScale = reducedMotion ? Vector3.one : Vector3.one * 0.95f;
                logoTransform.localScale = startScale;

                Tween.Value(0f, 1f, 0.4f, t =>
                {
                    if (logoCanvasGroup != null) logoCanvasGroup.alpha = t;
                    if (!reducedMotion && logoTransform != null)
                    {
                        logoTransform.localScale = Vector3.Lerp(startScale, Vector3.one, Tween.EvaluateEase(t, EaseType.EaseOutCubic));
                    }
                }, EaseType.Linear, true);
            }

            if (animatedButtonRows != null && animatedButtonRows.Count > 0)
            {
                for (int i = 0; i < animatedButtonRows.Count; i++)
                {
                    var row = animatedButtonRows[i];
                    if (row == null) continue;

                    var cg = row.GetComponent<CanvasGroup>();
                    if (cg == null)
                    {
                        cg = row.gameObject.AddComponent<CanvasGroup>();
                    }
                    cg.alpha = 0f;

                    float delay = i * 0.05f;
                    StartCoroutine(AnimateButtonRowEntrance(row, cg, delay, reducedMotion));
                }
            }

            if (!reducedMotion && playButton != null)
            {
                if (pulseRoutine != null) StopCoroutine(pulseRoutine);
                pulseRoutine = StartCoroutine(PrimaryButtonPulseRoutine());
            }
        }

        private IEnumerator AnimateButtonRowEntrance(RectTransform row, CanvasGroup cg, float delay, bool reducedMotion)
        {
            yield return new WaitForSecondsRealtime(delay);

            if (reducedMotion)
            {
                float duration = 0.25f;
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    if (cg != null) cg.alpha = Mathf.Clamp01(elapsed / duration);
                    yield return null;
                }
                if (cg != null) cg.alpha = 1f;
                yield break;
            }

            Vector2 startPos = new Vector2(-70f, 0f);
            Vector2 targetPos = Vector2.zero;
            if (row != null) row.anchoredPosition = startPos;

            float animDuration = 0.35f;
            float time = 0f;
            while (time < animDuration)
            {
                time += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(time / animDuration);
                float ease = Tween.EvaluateEase(t, EaseType.EaseOutCubic);

                if (row != null) row.anchoredPosition = Vector2.Lerp(startPos, targetPos, ease);
                if (cg != null) cg.alpha = t;
                yield return null;
            }

            if (row != null) row.anchoredPosition = targetPos;
            if (cg != null) cg.alpha = 1f;
        }

        private IEnumerator PrimaryButtonPulseRoutine()
        {
            yield return new WaitForSecondsRealtime(0.5f);
            if (playButton == null) yield break;

            RectTransform rt = playButton.GetComponent<RectTransform>();
            if (rt == null) yield break;

            while (!isStartingGame)
            {
                if (IsReducedMotionActive())
                {
                    rt.localScale = Vector3.one;
                    yield return new WaitForSecondsRealtime(1.0f);
                    continue;
                }

                float elapsed = 0f;
                while (elapsed < 1.0f)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / 1.0f);
                    float ease = Mathf.SmoothStep(0f, 1f, t);
                    rt.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.03f, ease);
                    yield return null;
                }

                elapsed = 0f;
                while (elapsed < 1.0f)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / 1.0f);
                    float ease = Mathf.SmoothStep(0f, 1f, t);
                    rt.localScale = Vector3.Lerp(Vector3.one * 1.03f, Vector3.one, ease);
                    yield return null;
                }
            }

            if (rt != null) rt.localScale = Vector3.one;
        }

        private EventSystem GetEventSystem()
        {
            return EventSystem.current != null ? EventSystem.current : FindAnyObjectByType<EventSystem>();
        }

        private IEnumerator SetInitialFocusRoutine()
        {
            yield return null;
            var es = GetEventSystem();
            if (es != null && playButton != null)
            {
                es.SetSelectedGameObject(playButton.gameObject);
            }
        }

        private void Update()
        {
            if (isStartingGame) return;

            bool backPressed = false;
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) backPressed = true;

            var gamepad = Gamepad.current;
            if (gamepad != null && gamepad.buttonEast.wasPressedThisFrame) backPressed = true;

            if (backPressed)
            {
                CloseActiveModal();
            }
        }

        public void CloseActiveModal()
        {
            if (settingsModalController != null && settingsModalController.IsOpen)
            {
                OnCloseSettingsClicked();
            }
            else if (settingsTransition != null && settingsTransition.IsOpen)
            {
                OnCloseSettingsClicked();
            }
            else if (howToPlayTransition != null && howToPlayTransition.IsOpen)
            {
                OnCloseHowToPlayClicked();
            }
            else if (aboutTransition != null && aboutTransition.IsOpen)
            {
                OnCloseAboutClicked();
            }
        }

        private static Button FindPanelBackButton(MenuPanelTransition panel)
        {
            if (panel == null) return null;
            Transform back = panel.transform.Find("BackButton");
            return back != null ? back.GetComponent<Button>() : null;
        }

        private void OnDestroy()
        {
            if (playButton != null) playButton.onClick.RemoveListener(OnPlayClicked);
            if (howToPlayButton != null) howToPlayButton.onClick.RemoveListener(OnHowToPlayClicked);
            if (settingsButton != null) settingsButton.onClick.RemoveListener(OnSettingsClicked);
            if (aboutButton != null) aboutButton.onClick.RemoveListener(OnAboutClicked);
            if (quitButton != null) quitButton.onClick.RemoveListener(OnQuitClicked);

            if (closeSettingsButton != null) closeSettingsButton.onClick.RemoveListener(OnCloseSettingsClicked);
            if (closeHowToPlayButton != null) closeHowToPlayButton.onClick.RemoveListener(OnCloseHowToPlayClicked);
            if (closeAboutButton != null) closeAboutButton.onClick.RemoveListener(OnCloseAboutClicked);
            if (aboutLinkButton != null) aboutLinkButton.onClick.RemoveListener(OnAboutLinkClicked);
            if (howToPlayBackButton != null) howToPlayBackButton.onClick.RemoveListener(OnCloseHowToPlayClicked);
            if (aboutBackButton != null) aboutBackButton.onClick.RemoveListener(OnCloseAboutClicked);

            if (settingsModalController != null)
            {
                settingsModalController.OnClosed -= RestoreFocus;
            }

            if (settingsPanelUI != null)
            {
                settingsPanelUI.OnBackRequested -= OnCloseSettingsClicked;
            }
        }

        public void UpdateHighScoreDisplay(Difficulty? highlightedDifficulty = null)
        {
            if (highScoreText != null)
            {
                Difficulty diff = highlightedDifficulty.HasValue ? highlightedDifficulty.Value : Difficulty.Normal;
                int best = GameManager.GetHighScore(diff);
                highScoreText.text = $"BEST ({diff}): {best}";
            }
        }

        public void UpdateHighScoreDisplay(string difficultyName)
        {
            if (Enum.TryParse<Difficulty>(difficultyName, true, out var diff))
            {
                UpdateHighScoreDisplay(diff);
            }
            else
            {
                UpdateHighScoreDisplay(Difficulty.Normal);
            }
        }

        public void OnDifficultyClicked(Difficulty difficulty)
        {
            SettingsService.Instance?.SetDifficulty(difficulty);
            UpdateDifficultyVisuals();
            UpdateHighScoreDisplay();
            SoundManager.Instance?.PlayDifficultySwitch();
            AudioManager.Instance?.PlayUI("UI_SELECT_RISE");
        }

        public void UpdateDifficultyVisuals()
        {
            Difficulty current = SettingsService.Instance != null && SettingsService.Instance.CurrentSettings != null
                ? SettingsService.Instance.CurrentSettings.CurrentDifficulty
                : Difficulty.Normal;

            StyleDifficultyButton(easyButton, current == Difficulty.Easy);
            StyleDifficultyButton(normalButton, current == Difficulty.Normal);
            StyleDifficultyButton(hardButton, current == Difficulty.Hard);
            StyleDifficultyButton(nightmareButton, current == Difficulty.Nightmare);
        }

        private void StyleDifficultyButton(Button btn, bool isSelected)
        {
            if (btn == null) return;

            var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                txt.fontStyle = isSelected ? FontStyles.Bold : FontStyles.Normal;
                txt.color = isSelected ? new Color(0.24f, 0.86f, 0.16f, 1f) : new Color(0.55f, 0.62f, 0.68f, 1f);
            }

            var img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.color = isSelected ? new Color(0.18f, 0.22f, 0.25f, 1f) : new Color(0.12f, 0.15f, 0.18f, 0.8f);
            }
        }

        private void OnPlayClicked()
        {
            if (isStartingGame) return;

            SoundManager.Instance?.PlayPlayButtonPress();

            if (difficultyModal != null)
            {
                difficultyModal.Open(() =>
                {
                    if (EventSystem.current != null && playButton != null)
                    {
                        EventSystem.current.SetSelectedGameObject(playButton.gameObject);
                    }
                });
                return;
            }

            StartGameWithFade();
        }

        public void StartGameWithFade()
        {
            if (isStartingGame) return;
            isStartingGame = true;
            StartCoroutine(PlayGameFlowRoutine());
        }

        private IEnumerator PlayGameFlowRoutine()
        {
            if (playButton != null)
            {
                RectTransform rt = playButton.GetComponent<RectTransform>();
                if (rt != null)
                {
                    Vector3 orig = Vector3.one;
                    Vector3 pulse = Vector3.one * 1.08f;
                    float elapsed = 0f;
                    while (elapsed < 0.08f)
                    {
                        elapsed += Time.unscaledDeltaTime;
                        rt.localScale = Vector3.Lerp(orig, pulse, elapsed / 0.08f);
                        yield return null;
                    }
                    elapsed = 0f;
                    while (elapsed < 0.08f)
                    {
                        elapsed += Time.unscaledDeltaTime;
                        rt.localScale = Vector3.Lerp(pulse, orig, elapsed / 0.08f);
                        yield return null;
                    }
                    rt.localScale = orig;
                }
            }

            if (screenFadeCanvasGroup != null)
            {
                screenFadeCanvasGroup.gameObject.SetActive(true);
                screenFadeCanvasGroup.alpha = 0f;
                float elapsed = 0f;
                while (elapsed < 0.4f)
                {
                    elapsed += Time.unscaledDeltaTime;
                    screenFadeCanvasGroup.alpha = Mathf.Clamp01(elapsed / 0.4f);
                    yield return null;
                }
                screenFadeCanvasGroup.alpha = 1f;
            }

            TimeScaleController.ResetTimeScale();
            AsyncOperation op = SceneManager.LoadSceneAsync(gameSceneName);
            if (op != null)
            {
                op.allowSceneActivation = true;

                float loadElapsed = 0f;
                while (!op.isDone)
                {
                    loadElapsed += Time.unscaledDeltaTime;
                    if (loadElapsed > 0.2f && loadingIndicator != null && !loadingIndicator.activeSelf)
                    {
                        loadingIndicator.SetActive(true);
                    }
                    yield return null;
                }
            }
        }

        private void OnHowToPlayClicked()
        {
            SoundManager.Instance?.PlayButtonPress();
            RememberFocus(howToPlayButton != null ? howToPlayButton.gameObject : null);

            if (howToPlayTransition != null)
            {
                howToPlayTransition.Open(() =>
                {
                    var es = GetEventSystem();
                    if (closeHowToPlayButton != null && es != null)
                    {
                        es.SetSelectedGameObject(closeHowToPlayButton.gameObject);
                    }
                });
            }

            if (inputHintsBar != null) inputHintsBar.SetPanelOpen(true);
        }

        private void OnCloseHowToPlayClicked()
        {
            SoundManager.Instance?.PlayButtonPress();
            AudioManager.Instance?.PlayUI("UI_BACK");

            if (howToPlayTransition != null)
            {
                howToPlayTransition.Close(RestoreFocus);
            }
            else
            {
                RestoreFocus();
            }

            if (inputHintsBar != null) inputHintsBar.SetPanelOpen(false);
        }

        private void OnSettingsClicked()
        {
            SoundManager.Instance?.PlayButtonPress();
            RememberFocus(settingsButton != null ? settingsButton.gameObject : null);

            if (settingsModalController != null)
            {
                settingsModalController.Open(RestoreFocus);
            }
            else if (settingsTransition != null)
            {
                settingsTransition.Open(() =>
                {
                    if (settingsPanelUI != null)
                    {
                        settingsPanelUI.RefreshUI();
                        var es = GetEventSystem();
                        if (es != null)
                        {
                            if (settingsPanelUI.MasterVolumeSlider != null)
                            {
                                es.SetSelectedGameObject(settingsPanelUI.MasterVolumeSlider.gameObject);
                            }
                            else if (settingsPanelUI.BackButton != null)
                            {
                                es.SetSelectedGameObject(settingsPanelUI.BackButton.gameObject);
                            }
                        }
                    }
                });
            }

            if (inputHintsBar != null) inputHintsBar.SetPanelOpen(true);
        }

        private void OnCloseSettingsClicked()
        {
            SoundManager.Instance?.PlayButtonPress();
            AudioManager.Instance?.PlayUI("UI_BACK");

            if (settingsModalController != null && settingsModalController.IsOpen)
            {
                settingsModalController.Close();
            }
            else if (settingsTransition != null)
            {
                settingsTransition.Close(RestoreFocus);
            }
            else
            {
                RestoreFocus();
            }

            if (inputHintsBar != null) inputHintsBar.SetPanelOpen(false);
        }

        private void OnAboutClicked()
        {
            SoundManager.Instance?.PlayButtonPress();
            RememberFocus(aboutButton != null ? aboutButton.gameObject : null);

            if (aboutTransition != null)
            {
                aboutTransition.Open(() =>
                {
                    var es = GetEventSystem();
                    if (closeAboutButton != null && es != null)
                    {
                        es.SetSelectedGameObject(closeAboutButton.gameObject);
                    }
                });
            }

            if (inputHintsBar != null) inputHintsBar.SetPanelOpen(true);
        }

        private void OnCloseAboutClicked()
        {
            SoundManager.Instance?.PlayButtonPress();
            AudioManager.Instance?.PlayUI("UI_BACK");

            if (aboutTransition != null)
            {
                aboutTransition.Close(RestoreFocus);
            }
            else
            {
                RestoreFocus();
            }

            if (inputHintsBar != null) inputHintsBar.SetPanelOpen(false);
        }
        
        private void OnAboutLinkClicked()
        {
            SoundManager.Instance?.PlayButtonPress();
            if (!string.IsNullOrEmpty(aboutLinkUrl))
            {
                Application.OpenURL(aboutLinkUrl);
            }
        }

        private void RememberFocus(GameObject opener)
        {
            var es = GetEventSystem();
            lastFocusedElement = opener != null ? opener : (es != null ? es.currentSelectedGameObject : null);
        }

        private void RestoreFocus()
        {
            var es = GetEventSystem();
            if (lastFocusedElement != null && es != null)
            {
                es.SetSelectedGameObject(lastFocusedElement);
            }
        }

        private void OnQuitClicked()
        {
            SoundManager.Instance?.PlayButtonPress();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
