using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using DodgeGame.Core;

namespace DodgeGame.UI
{
    public class PauseUI : MonoBehaviour
    {
        [Header("Main Pause Panel References")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private MenuPanelTransition pauseTransition;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI scoreText;

        [Header("Menu Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button howToPlayButton;
        [SerializeField] private Button quitButton;

        [Header("Subpanels")]
        [SerializeField] private MenuPanelTransition settingsTransition;
        [SerializeField] private Button closeSettingsButton;
        [SerializeField] private MenuPanelTransition howToPlayTransition;
        [SerializeField] private Button closeHowToPlayButton;

        [Header("Main Pause Column")]
        [SerializeField] private CanvasGroup mainPauseColumn;

        private GameObject lastFocusedElement;

        private void Awake()
        {
            if (pausePanel == null)
            {
                var tr = transform.Find("PausePanel");
                pausePanel = tr != null ? tr.gameObject : gameObject;
            }

            if (pauseTransition == null && pausePanel != null)
            {
                pauseTransition = pausePanel.GetComponent<MenuPanelTransition>();
            }
            if (pauseTransition != null && mainPauseColumn != null)
            {
                pauseTransition.SetMainColumn(mainPauseColumn);
            }

            if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
            if (howToPlayButton != null) howToPlayButton.onClick.AddListener(OnHowToPlayClicked);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);

            if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(OnCloseSettingsClicked);
            if (closeHowToPlayButton != null) closeHowToPlayButton.onClick.AddListener(OnCloseHowToPlayClicked);

            if (settingsTransition != null)
            {
                settingsTransition.SetMainColumn(mainPauseColumn);
                settingsTransition.gameObject.SetActive(false);
            }

            if (howToPlayTransition != null)
            {
                howToPlayTransition.SetMainColumn(mainPauseColumn);
                howToPlayTransition.gameObject.SetActive(false);
            }

            pausePanel.SetActive(false);
        }

        private void OnEnable()
        {
            if (PauseController.Instance != null)
            {
                PauseController.Instance.OnPauseChanged -= HandlePauseChanged;
                PauseController.Instance.OnPauseChanged += HandlePauseChanged;
            }
            PauseController.OnGlobalPauseChanged -= HandlePauseChanged;
            PauseController.OnGlobalPauseChanged += HandlePauseChanged;
        }

        private void OnDisable()
        {
            if (PauseController.Instance != null)
            {
                PauseController.Instance.OnPauseChanged -= HandlePauseChanged;
            }
            PauseController.OnGlobalPauseChanged -= HandlePauseChanged;
        }

        private void OnDestroy()
        {
            if (resumeButton != null) resumeButton.onClick.RemoveListener(OnResumeClicked);
            if (settingsButton != null) settingsButton.onClick.RemoveListener(OnSettingsClicked);
            if (howToPlayButton != null) howToPlayButton.onClick.RemoveListener(OnHowToPlayClicked);
            if (quitButton != null) quitButton.onClick.RemoveListener(OnQuitClicked);

            if (closeSettingsButton != null) closeSettingsButton.onClick.RemoveListener(OnCloseSettingsClicked);
            if (closeHowToPlayButton != null) closeHowToPlayButton.onClick.RemoveListener(OnCloseHowToPlayClicked);
        }

        private void HandlePauseChanged(bool isPaused)
        {
            if (isPaused)
            {
                OpenPauseMenu();
            }
            else
            {
                ClosePauseMenu();
            }
        }

        public void OpenPauseMenu()
        {
            if (pausePanel == null) return;

            if (settingsTransition != null && settingsTransition.IsOpen)
            {
                settingsTransition.Close();
            }
            if (howToPlayTransition != null && howToPlayTransition.IsOpen)
            {
                howToPlayTransition.Close();
            }

            if (scoreText != null && GameManager.Instance != null)
            {
                scoreText.text = $"CURRENT SCORE: {GameManager.Instance.CurrentScore}";
            }

            pausePanel.SetActive(true);
            if (pauseTransition != null)
            {
                pauseTransition.Open(() =>
                {
                    FocusDefaultButton();
                });
            }
            else
            {
                FocusDefaultButton();
            }
        }

        public void ClosePauseMenu()
        {
            if (settingsTransition != null && settingsTransition.IsOpen)
            {
                settingsTransition.Close();
            }
            if (howToPlayTransition != null && howToPlayTransition.IsOpen)
            {
                howToPlayTransition.Close();
            }

            if (pauseTransition != null)
            {
                pauseTransition.Close(() =>
                {
                    if (pausePanel != null) pausePanel.SetActive(false);
                });
            }
            else
            {
                if (pausePanel != null) pausePanel.SetActive(false);
            }
        }

        private void FocusDefaultButton()
        {
            if (resumeButton != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
            }
        }

        private void OnResumeClicked()
        {
            SoundManager.Instance?.PlayButtonClick();
            if (PauseController.Instance != null)
            {
                PauseController.Instance.SetPaused(false);
            }
        }

        private void OnSettingsClicked()
        {
            SoundManager.Instance?.PlayButtonClick();
            lastFocusedElement = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;

            if (settingsTransition != null)
            {
                settingsTransition.Open(() =>
                {
                    if (closeSettingsButton != null && EventSystem.current != null)
                    {
                        EventSystem.current.SetSelectedGameObject(null);
                        EventSystem.current.SetSelectedGameObject(closeSettingsButton.gameObject);
                    }
                });
            }
        }

        private void OnCloseSettingsClicked()
        {
            SoundManager.Instance?.PlayButtonClick();
            AudioManager.Instance?.PlayUI("UI_BACK");
            if (settingsTransition != null)
            {
                settingsTransition.Close(() =>
                {
                    RestorePreviousFocus();
                });
            }
        }

        private void OnHowToPlayClicked()
        {
            SoundManager.Instance?.PlayButtonClick();
            lastFocusedElement = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;

            if (howToPlayTransition != null)
            {
                howToPlayTransition.Open(() =>
                {
                    if (closeHowToPlayButton != null && EventSystem.current != null)
                    {
                        EventSystem.current.SetSelectedGameObject(null);
                        EventSystem.current.SetSelectedGameObject(closeHowToPlayButton.gameObject);
                    }
                });
            }
        }

        private void OnCloseHowToPlayClicked()
        {
            SoundManager.Instance?.PlayButtonClick();
            AudioManager.Instance?.PlayUI("UI_BACK");
            if (howToPlayTransition != null)
            {
                howToPlayTransition.Close(() =>
                {
                    RestorePreviousFocus();
                });
            }
        }

        private void RestorePreviousFocus()
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                var target = (lastFocusedElement != null && lastFocusedElement.activeInHierarchy)
                    ? lastFocusedElement
                    : (resumeButton != null ? resumeButton.gameObject : null);
                if (target != null)
                {
                    EventSystem.current.SetSelectedGameObject(target);
                }
            }
        }

        private void OnQuitClicked()
        {
            SoundManager.Instance?.PlayButtonClick();
            TimeScaleController.ResetTimeScale();
            SceneManager.LoadScene("MainMenu");
        }
    }
}
