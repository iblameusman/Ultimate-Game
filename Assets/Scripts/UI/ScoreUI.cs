using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DodgeGame.Core;
using DodgeGame.Player;
using DodgeGame.Balls;
using DodgeGame.Data;

namespace DodgeGame.UI
{
    public class ScoreUI : MonoBehaviour
    {
        [Header("UI Text References")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI scoreLabel;
        [SerializeField] private RectTransform scorePanel;
        [SerializeField] private TextMeshProUGUI bestText;
        [SerializeField] private RectTransform bestPanel;
        [SerializeField] private TextMeshProUGUI livesText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private GameObject threatWarningBanner;

        [Header("Event & Target Sources")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerController playerController;

        private float warningBlinkTimer = 0f;

        private void Awake()
        {
            if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
            if (playerHealth == null) playerHealth = FindAnyObjectByType<PlayerHealth>();
            if (playerController == null) playerController = FindAnyObjectByType<PlayerController>();

            if (scoreText == null)
            {
                scoreText = transform.Find("HUD/TopLeft_ScoreGroup/ScorePanel/ScoreValue")?.GetComponent<TextMeshProUGUI>()
                         ?? transform.Find("HUD/ScoreText")?.GetComponent<TextMeshProUGUI>();
            }
            if (scoreLabel == null)
            {
                scoreLabel = transform.Find("HUD/TopLeft_ScoreGroup/ScorePanel/ScoreLabel")?.GetComponent<TextMeshProUGUI>();
            }
            if (scorePanel == null)
            {
                scorePanel = transform.Find("HUD/TopLeft_ScoreGroup/ScorePanel")?.GetComponent<RectTransform>();
            }
            if (bestText == null)
            {
                bestText = transform.Find("HUD/TopLeft_ScoreGroup/BestPanel/BestText")?.GetComponent<TextMeshProUGUI>()
                        ?? transform.Find("HighScoreText")?.GetComponent<TextMeshProUGUI>();
            }
            if (bestPanel == null)
            {
                bestPanel = transform.Find("HUD/TopLeft_ScoreGroup/BestPanel")?.GetComponent<RectTransform>();
            }
            if (highScoreText == null) highScoreText = bestText;
            if (livesText == null) livesText = transform.Find("HUD/LivesText")?.GetComponent<TextMeshProUGUI>();
            if (countdownText == null) countdownText = transform.Find("HUD/CountdownText")?.GetComponent<TextMeshProUGUI>();
            if (timerText == null)
            {
                timerText = transform.Find("HUD/TopCenter_TimerGroup/TimerPanel/TimerValue")?.GetComponent<TextMeshProUGUI>()
                         ?? transform.Find("HUD/TimerText")?.GetComponent<TextMeshProUGUI>()
                         ?? transform.Find("TimerText")?.GetComponent<TextMeshProUGUI>();
            }
            if (threatWarningBanner == null) threatWarningBanner = transform.Find("ThreatWarningBanner")?.gameObject;

            if (threatWarningBanner != null) threatWarningBanner.SetActive(false);
            if (countdownText != null) countdownText.gameObject.SetActive(false);

            if (GetComponent<CaptionUI>() == null)
            {
                gameObject.AddComponent<CaptionUI>();
            }
        }

        private void OnEnable()
        {
            if (gameManager != null)
            {
                gameManager.OnScoreChanged += UpdateScoreDisplay;
                gameManager.OnSurvivalTimeUpdated += UpdateTimerDisplay;
                gameManager.OnStateChanged += HandleStateChanged;
            }

            if (playerHealth != null)
            {
                playerHealth.OnLivesChanged += UpdateLivesDisplay;
            }
        }

        private void OnDisable()
        {
            if (gameManager != null)
            {
                gameManager.OnScoreChanged -= UpdateScoreDisplay;
                gameManager.OnSurvivalTimeUpdated -= UpdateTimerDisplay;
                gameManager.OnStateChanged -= HandleStateChanged;
            }

            if (playerHealth != null)
            {
                playerHealth.OnLivesChanged -= UpdateLivesDisplay;
            }
        }

        private void Start()
        {
            UpdateScoreDisplay(gameManager != null ? gameManager.CurrentScore : 0);
            UpdateTimerDisplay(gameManager != null ? gameManager.SurvivalTimer : 0f);
            UpdateLivesDisplay(playerHealth != null ? playerHealth.CurrentLives : 1);
            UpdateBestDisplay();
        }

        private void Update()
        {
            if (gameManager == null) return;

            if (gameManager.CurrentState == GameState.Countdown)
            {
                if (countdownText != null)
                {
                    if (!countdownText.gameObject.activeSelf) countdownText.gameObject.SetActive(true);
                    float rem = gameManager.CountdownRemaining;
                    if (rem > 0.5f)
                    {
                        countdownText.text = Mathf.CeilToInt(rem).ToString();
                    }
                    else
                    {
                        countdownText.text = "GO!";
                    }
                }
            }
            else
            {
                if (countdownText != null && countdownText.gameObject.activeSelf)
                {
                    countdownText.gameObject.SetActive(false);
                }
            }

            if (gameManager.CurrentState == GameState.Playing)
            {
                bool anyHoming = false;
                var allBalls = FindObjectsByType<BaseBall>(FindObjectsInactive.Exclude);
                for (int i = 0; i < allBalls.Length; i++)
                {
                    if (allBalls[i] != null && allBalls[i].IsHoming)
                    {
                        anyHoming = true;
                        break;
                    }
                }

                if (threatWarningBanner != null)
                {
                    if (anyHoming)
                    {
                        warningBlinkTimer += Time.deltaTime * 6f;
                        threatWarningBanner.SetActive(Mathf.Sin(warningBlinkTimer) > -0.2f);
                    }
                    else
                    {
                        threatWarningBanner.SetActive(false);
                    }
                }
            }
            else
            {
                if (threatWarningBanner != null) threatWarningBanner.SetActive(false);
            }
        }

        private void HandleStateChanged(GameState state)
        {
            if (state == GameState.GameOver)
            {
                if (threatWarningBanner != null) threatWarningBanner.SetActive(false);
                if (countdownText != null) countdownText.gameObject.SetActive(false);
            }
        }

        private void UpdateScoreDisplay(int newScore)
        {
            if (UIJuice.Instance != null)
            {
                UIJuice.Instance.AnimateScore(newScore);
            }
            else if (scoreText != null)
            {
                scoreText.text = newScore.ToString();
            }

            if (gameManager != null && newScore > gameManager.HighScore)
            {
                UpdateBestDisplay();
            }
        }

        public void UpdateBestDisplay()
        {
            if (bestText == null && highScoreText != null) bestText = highScoreText;
            if (bestText == null) return;

            int best = gameManager != null ? gameManager.HighScore : 0;
            string diffName = GetSelectedDifficultyDisplayName();
            bestText.text = $"BEST ({diffName}): {best}";
        }

        public static string GetSelectedDifficultyDisplayName()
        {
            Difficulty diff = Difficulty.Normal;
            if (SettingsService.Instance != null && SettingsService.Instance.CurrentSettings != null)
            {
                diff = SettingsService.Instance.CurrentSettings.SelectedDifficulty;
            }
            return diff.ToString().ToUpperInvariant();
        }

        private void UpdateTimerDisplay(float survivalTime)
        {
            if (timerText != null)
            {
                int totalHundredths = Mathf.RoundToInt(survivalTime * 100f);
                int min = totalHundredths / 6000;
                int sec = (totalHundredths / 100) % 60;
                int ms = totalHundredths % 100;
                timerText.text = TextStyler.FormatTabular($"{min:00}:{sec:00}.{ms:00}");
            }
        }

        private void UpdateLivesDisplay(int currentLives)
        {
            if (UIJuice.Instance != null)
            {
                UIJuice.Instance.UpdateLives(currentLives);
            }

            if (livesText != null)
            {
                livesText.text = currentLives <= 1 ? "1 HIT = DEATH" : $"LIVES: {currentLives}";
            }
        }
    }
}
