using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DodgeGame.Data;
using DodgeGame.Player;

namespace DodgeGame.Core
{
    public enum GameState
    {
        Countdown,
        Playing,
        Paused,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private GameConfig config;

        [Header("Scene References")]
        [SerializeField] private PlayerHealth playerHealth;

        [Header("Game Flow Tuning")]
        [SerializeField] private float countdownDuration = 3.0f;
        [SerializeField] private float deathSlowMoTimeScale = 0.25f;
        [SerializeField] private float slowMoRealDuration = 0.5f;

        private GameState currentState = GameState.Countdown;
        private float score;
        private int lastReportedScore = -1;
        private float survivalTimer = 0f;
        private float countdownRemaining = 3.0f;
        private string gameOverReason = "Eliminated by Ball";
        private bool isNewHighScore = false;

        public event Action<int> OnScoreChanged;
        public event Action<int> OnGameOver;
        public event Action<GameState> OnStateChanged;
        public event Action<float> OnSurvivalTimeUpdated;

        public GameState CurrentState => currentState;
        public int CurrentScore => Mathf.FloorToInt(score);
        public float SurvivalTimer => survivalTimer;
        public float CountdownRemaining => countdownRemaining;
        public bool IsPlaying => currentState == GameState.Playing;
        public string GameOverReason => gameOverReason;
        public bool IsNewHighScore => isNewHighScore;
        
        public static string GetHighScoreKey(Difficulty difficulty)
        {
            return $"DodgeGame_HighScore_{difficulty}";
        }

        public static int GetHighScore(Difficulty difficulty)
        {
            return PlayerPrefs.GetInt(GetHighScoreKey(difficulty), 0);
        }

        public int HighScore
        {
            get
            {
                Difficulty diff = SettingsService.Instance != null && SettingsService.Instance.CurrentSettings != null
                    ? SettingsService.Instance.CurrentSettings.CurrentDifficulty
                    : Difficulty.Normal;
                return GetHighScore(diff);
            }
        }

        private void Awake()
        {
            if ((UnityEngine.Object)Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            TimeScaleController.ResetTimeScale();

            if (playerHealth == null)
            {
                playerHealth = FindAnyObjectByType<PlayerHealth>();
            }

            if (SoundManager.Instance == null)
            {
                var sm = new GameObject("SoundManager");
                sm.AddComponent<SoundManager>();
            }
        }

        private void OnEnable()
        {
            if (playerHealth != null)
            {
                playerHealth.OnDeath += HandlePlayerDeath;
            }
            PauseController.OnGlobalPauseChanged += HandlePauseChanged;
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.OnDeath -= HandlePlayerDeath;
            }
            PauseController.OnGlobalPauseChanged -= HandlePauseChanged;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void HandlePauseChanged(bool isPaused)
        {
            if (currentState == GameState.GameOver) return;
            currentState = isPaused ? GameState.Paused : GameState.Playing;
            OnStateChanged?.Invoke(currentState);
        }

        private void Start()
        {
            score = 0f;
            lastReportedScore = 0;
            survivalTimer = 0f;
            OnScoreChanged?.Invoke(0);

            if (currentState != GameState.Playing)
            {
                StartCoroutine(CountdownRoutine());
            }
        }

        private IEnumerator CountdownRoutine()
        {
            currentState = GameState.Countdown;
            OnStateChanged?.Invoke(currentState);

            countdownRemaining = countdownDuration;
            int lastSec = Mathf.CeilToInt(countdownRemaining);
            if (SoundManager.Instance != null) SoundManager.Instance.PlayCountdownBeep(false);

            while (countdownRemaining > 0f)
            {
                countdownRemaining -= Time.deltaTime;
                int currentSec = Mathf.CeilToInt(countdownRemaining);
                if (currentSec > 0 && currentSec != lastSec)
                {
                    lastSec = currentSec;
                    if (SoundManager.Instance != null) SoundManager.Instance.PlayCountdownBeep(false);
                }
                yield return null;
            }

            countdownRemaining = 0f;
            if (SoundManager.Instance != null) SoundManager.Instance.PlayCountdownBeep(true);

            currentState = GameState.Playing;
            OnStateChanged?.Invoke(currentState);
            DodgeGame.Audio.SoundEventHook.Play(gameObject, DodgeGame.Audio.SoundEventId.GameplayStart);
        }

        public void StartGameImmediate()
        {
            StopAllCoroutines();
            countdownRemaining = 0f;
            currentState = GameState.Playing;
            OnStateChanged?.Invoke(currentState);
            DodgeGame.Audio.SoundEventHook.Play(gameObject, DodgeGame.Audio.SoundEventId.GameplayStart);
        }

        private void Update()
        {
            if (currentState == GameState.Playing)
            {
                survivalTimer += Time.deltaTime;
                OnSurvivalTimeUpdated?.Invoke(survivalTimer);

                float rate = config != null ? config.ScorePerSecond : 10f;
                float scoreMult = SettingsService.Instance != null ? SettingsService.Instance.GetCurrentDifficultyPreset().ScoreMultiplier : 1f;
                score += rate * scoreMult * Time.unscaledDeltaTime;
                int intScore = Mathf.FloorToInt(score);
                if (intScore != lastReportedScore)
                {
                    lastReportedScore = intScore;
                    OnScoreChanged?.Invoke(intScore);
                }
            }
        }

        public void TriggerHitStop(float duration = 0.05f)
        {
            HitStop.Instance?.TriggerHitStop();
        }

        public void TriggerGameOver(string reason = "Hit by Ball")
        {
            if (currentState == GameState.GameOver) return;
            gameOverReason = reason;
            HandlePlayerDeath();
        }

        private void HandlePlayerDeath()
        {
            if (currentState == GameState.GameOver) return;

            currentState = GameState.GameOver;
            try
            {
                if ((UnityEngine.Object)AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayGameOver();
                }
            }
            catch (Exception) { }

            int finalScore = CurrentScore;
            Difficulty diff = SettingsService.Instance != null && SettingsService.Instance.CurrentSettings != null
                ? SettingsService.Instance.CurrentSettings.CurrentDifficulty
                : Difficulty.Normal;
            string diffKey = GetHighScoreKey(diff);
            int best = PlayerPrefs.GetInt(diffKey, 0);

            if (finalScore > best)
            {
                isNewHighScore = true;
                PlayerPrefs.SetInt(diffKey, finalScore);
                PlayerPrefs.SetInt("DodgeGame_HighScore", finalScore);
                PlayerPrefs.Save();
            }
            else
            {
                isNewHighScore = false;
            }

            StartCoroutine(GameOverRoutine(finalScore));
        }

        private IEnumerator GameOverRoutine(int finalScore)
        {
            if (TimeScaleController.Instance != null)
            {
                TimeScaleController.Instance.RequestTimeScale(deathSlowMoTimeScale, slowMoRealDuration, "GameOverSlowMo");
            }

            yield return new WaitForSecondsRealtime(slowMoRealDuration);

            if (TimeScaleController.Instance != null)
            {
                TimeScaleController.Instance.ReleaseTimeScale("GameOverSlowMo");
                TimeScaleController.Instance.RequestTimeScale(0f, 0f, "GameOverFreeze");
            }

            OnStateChanged?.Invoke(currentState);
            OnGameOver?.Invoke(finalScore);
            GameEvents.TriggerGameOver(finalScore);
        }

        public void TogglePause()
        {
            if (PauseController.Instance != null)
            {
                PauseController.Instance.TogglePause();
            }
        }

        public void Restart()
        {
#if UNITY_EDITOR || DEBUG
            Debug.Log("[GameManager] Restarting game...");
#endif
            TimeScaleController.ResetTimeScale();
            if ((UnityEngine.Object)AudioManager.Instance != null)
            {
                AudioManager.Instance.ClearQueuedClips();
                AudioManager.Instance.StopAllComedyClips();
            }
            int activeSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(activeSceneIndex);
        }

        public void LoadMainMenu()
        {
#if UNITY_EDITOR || DEBUG
            Debug.Log("[GameManager] Returning to Main Menu...");
#endif
            TimeScaleController.ResetTimeScale();
            SceneManager.LoadScene("MainMenu");
        }
    }
}
