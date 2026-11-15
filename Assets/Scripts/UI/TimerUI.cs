using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DodgeGame.Core;
using DodgeGame.Data;

namespace DodgeGame.UI
{
    public class TimerUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI difficultyChipText;
        [SerializeField] private HudTheme theme;

        private GameManager gameManager;
        private SlowMotionController slowMoController;
        private Color normalTimerColor = new Color(30f / 255f, 42f / 255f, 51f / 255f, 1f);
        private Color slowMoTimerColor = new Color(47f / 255f, 155f / 255f, 255f / 255f, 1f);
        private bool isSlowMoActive = false;

        public HudTheme Theme
        {
            get => theme;
            set
            {
                theme = value;
                UpdateThemeColors();
            }
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (gameManager != null)
            {
                gameManager.OnSurvivalTimeUpdated += HandleSurvivalTimeUpdated;
            }

            if (slowMoController != null)
            {
                slowMoController.OnSlowMoStart += HandleSlowMoStart;
                slowMoController.OnSlowMoEnd += HandleSlowMoEnd;
            }
        }

        private void OnDisable()
        {
            if (gameManager != null)
            {
                gameManager.OnSurvivalTimeUpdated -= HandleSurvivalTimeUpdated;
            }

            if (slowMoController != null)
            {
                slowMoController.OnSlowMoStart -= HandleSlowMoStart;
                slowMoController.OnSlowMoEnd -= HandleSlowMoEnd;
            }
        }

        private void Start()
        {
            UpdateDifficultyDisplay();
            UpdateTimerText(gameManager != null ? gameManager.SurvivalTimer : 0f);
        }

        private void ResolveReferences()
        {
            if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
            if (slowMoController == null) slowMoController = FindAnyObjectByType<SlowMotionController>();

            if (theme == null)
            {
                theme = Resources.Load<HudTheme>("HudTheme");
#if UNITY_EDITOR
                if (theme == null)
                {
                    theme = UnityEditor.AssetDatabase.LoadAssetAtPath<HudTheme>("Assets/Scripts/Data/HudTheme.asset");
                }
#endif
            }

            if (timerText == null)
            {
                timerText = transform.Find("TimerPanel/TimerValue")?.GetComponent<TextMeshProUGUI>()
                         ?? transform.Find("TimerValue")?.GetComponent<TextMeshProUGUI>();
            }

            if (difficultyChipText == null)
            {
                difficultyChipText = transform.Find("TimerPanel/DifficultyChip/DifficultyText")?.GetComponent<TextMeshProUGUI>();
            }

            UpdateThemeColors();
        }

        public void UpdateThemeColors()
        {
            if (theme != null)
            {
                normalTimerColor = theme.TextPrimary;
                slowMoTimerColor = theme.AccentActive;
            }
            else
            {
                normalTimerColor = new Color(30f / 255f, 42f / 255f, 51f / 255f, 1f);
                slowMoTimerColor = new Color(47f / 255f, 155f / 255f, 255f / 255f, 1f);
            }
        }

        public void HandleSurvivalTimeUpdated(float survivalTime)
        {
            UpdateTimerText(survivalTime);
        }

        public void UpdateTimerText(float survivalTime)
        {
            if (timerText == null) return;

            int totalHundredths = Mathf.RoundToInt(survivalTime * 100f);
            int min = totalHundredths / 6000;
            int sec = (totalHundredths / 100) % 60;
            int cs = totalHundredths % 100;

            timerText.text = TextStyler.FormatTabular($"{min:00}:{sec:00}.{cs:00}");
        }

        public void HandleSlowMoStart()
        {
            isSlowMoActive = true;
            if (timerText != null)
            {
                timerText.color = slowMoTimerColor;
            }
        }

        public void HandleSlowMoEnd()
        {
            isSlowMoActive = false;
            if (timerText != null)
            {
                timerText.color = normalTimerColor;
            }
        }

        public void UpdateDifficultyDisplay()
        {
            if (difficultyChipText == null) return;

            string displayName = "Normal";
            if (SettingsService.Instance != null && SettingsService.Instance.CurrentSettings != null)
            {
                displayName = SettingsService.Instance.CurrentSettings.SelectedDifficulty.ToString();
            }
            difficultyChipText.text = displayName;
        }

        public bool IsSlowMoIndicatorActive => isSlowMoActive;
        public TextMeshProUGUI TimerText => timerText;
        public TextMeshProUGUI DifficultyChipText => difficultyChipText;
    }
}
