using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DodgeGame.Core;
using DodgeGame.Data;

namespace DodgeGame.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private CanvasGroup panelCanvasGroup;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private TextMeshProUGUI reasonText;
        [SerializeField] private TextMeshProUGUI twistRevealText;
        [SerializeField] private TextMeshProUGUI difficultyBadgeText;
        [SerializeField] private GameObject newRecordBadge;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Event Source")]
        [SerializeField] private GameManager gameManager;

        [Header("Animation Tuning")]
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float scoreRollDuration = 0.5f;

        private Coroutine activeAnimationRoutine;
        private TweenHandle scoreRollTween;

        private void Awake()
        {
            if (gameOverPanel == null)
            {
                var tr = transform.Find("GameOverPanel");
                gameOverPanel = tr != null ? tr.gameObject : gameObject;
            }

            if (panelCanvasGroup == null && gameOverPanel != null)
            {
                panelCanvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
                if (panelCanvasGroup == null) panelCanvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
            }

            if (newRecordBadge != null) newRecordBadge.SetActive(false);
            if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();

            if (restartButton != null) restartButton.onClick.AddListener(OnRestartButtonClicked);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);

            EnsureTwistElements();
            ApplyLayout();

            gameOverPanel.SetActive(false);
        }

        private static readonly Color BackdropColor = new Color(0.02f, 0.03f, 0.025f, 0.82f);
        private static readonly Color CardColor = new Color(0.07f, 0.095f, 0.085f, 0.97f);
        private static readonly Color AccentGreen = new Color(0f, 1f, 0.616f, 1f);
        private static readonly Color DangerRed = new Color(1f, 0.33f, 0.33f, 1f);
        private static readonly Color TextWhite = new Color(0.95f, 0.98f, 0.96f, 1f);
        private static readonly Color TextMuted = new Color(0.62f, 0.7f, 0.66f, 1f);
        private static readonly Color TwistAmber = new Color(1f, 0.78f, 0.3f, 1f);
        private static readonly Color GoldRecord = new Color(1f, 0.84f, 0.2f, 1f);
        private static readonly Color DarkText = new Color(0.03f, 0.06f, 0.045f, 1f);
        private static readonly Color SecondaryButton = new Color(0.16f, 0.21f, 0.19f, 1f);

        private const float CardWidth = 560f;
        private const float ContentWidth = 480f;

        private void ApplyLayout()
        {
            if (gameOverPanel == null) return;
            var panelTr = gameOverPanel.transform;

            var backdrop = gameOverPanel.GetComponent<Image>();
            if (backdrop == null) backdrop = gameOverPanel.AddComponent<Image>();
            backdrop.color = BackdropColor;
            backdrop.raycastTarget = true;

            var column = panelTr.Find("GameOverColumn") as RectTransform;
            if (column == null) return;

            column.anchorMin = column.anchorMax = column.pivot = new Vector2(0.5f, 0.5f);
            column.anchoredPosition = Vector2.zero;
            column.sizeDelta = new Vector2(CardWidth, column.sizeDelta.y);

            var card = column.GetComponent<Image>();
            if (card == null) card = column.gameObject.AddComponent<Image>();
            card.color = CardColor;
            card.raycastTarget = false;

            var layout = column.GetComponent<VerticalLayoutGroup>();
            if (layout == null) layout = column.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(40, 40, 36, 36);
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var fitter = column.GetComponent<ContentSizeFitter>();
            if (fitter == null) fitter = column.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            if (twistRevealText != null)
            {
                twistRevealText.transform.SetParent(column, false);
            }

            int order = 0;
            StyleText(titleText, column, ref order, 64f, DangerRed, 76f);
            StyleText(reasonText, column, ref order, 22f, TextMuted, 30f);
            PlaceSpacer(column, "SpacerTop", ref order, 14f);
            StyleText(FindText(column, "FinalScoreHeader"), column, ref order, 18f, TextMuted, 24f, 6f);
            StyleText(finalScoreText, column, ref order, 96f, AccentGreen, 104f);
            if (newRecordBadge != null)
            {
                StyleText(newRecordBadge.GetComponent<TextMeshProUGUI>(), column, ref order, 24f, GoldRecord, 32f);
            }
            StyleText(highScoreText, column, ref order, 24f, TextWhite, 32f, 2f);
            if (twistRevealText != null)
            {
                twistRevealText.textWrappingMode = TextWrappingModes.Normal;
                twistRevealText.fontStyle = FontStyles.Bold;
                StyleText(twistRevealText, column, ref order, 20f, TwistAmber, 54f);
            }
            PlaceSpacer(column, "SpacerButtons", ref order, 18f);
            StyleButton(restartButton, column, ref order, AccentGreen, DarkText);
            PlaceSpacer(column, "SpacerBetweenButtons", ref order, 4f);
            StyleButton(mainMenuButton, column, ref order, SecondaryButton, TextWhite);
        }

        private static TextMeshProUGUI FindText(Transform parent, string name)
        {
            var tr = parent.Find(name);
            return tr != null ? tr.GetComponent<TextMeshProUGUI>() : null;
        }

        private static void StyleText(TextMeshProUGUI tmp, Transform column, ref int order, float size, Color color, float height, float spacing = 0f)
        {
            if (tmp == null) return;
            var rt = tmp.rectTransform;
            rt.SetParent(column, false);
            rt.SetSiblingIndex(order++);
            rt.sizeDelta = new Vector2(ContentWidth, height);
            tmp.enableAutoSizing = false;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.characterSpacing = spacing;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.raycastTarget = false;
            SetHeight(rt.gameObject, height);
        }

        private static void StyleButton(Button button, Transform column, ref int order, Color bg, Color label)
        {
            if (button == null) return;
            var rt = button.GetComponent<RectTransform>();
            rt.SetParent(column, false);
            rt.SetSiblingIndex(order++);
            rt.sizeDelta = new Vector2(ContentWidth - 40f, 58f);
            SetHeight(button.gameObject, 58f);

            button.transition = Selectable.Transition.None;

            var img = button.GetComponent<Image>();
            if (img == null) img = button.gameObject.AddComponent<Image>();
            img.color = bg;
            button.targetGraphic = img;

            var fx = button.GetComponent<ButtonStateFX>();
            if (fx != null) fx.SetBaseColors(bg, Color.Lerp(bg, Color.white, 0.15f));

            var tmp = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null)
            {
                tmp.enableAutoSizing = false;
                tmp.fontSize = 26f;
                tmp.color = label;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.raycastTarget = false;
            }
        }

        private static void PlaceSpacer(Transform column, string name, ref int order, float height)
        {
            var tr = column.Find(name);
            if (tr == null)
            {
                var go = new GameObject(name, typeof(RectTransform));
                go.transform.SetParent(column, false);
                tr = go.transform;
            }
            tr.SetSiblingIndex(order++);
            SetHeight(tr.gameObject, height);
        }

        private static void SetHeight(GameObject go, float height)
        {
            var le = go.GetComponent<LayoutElement>();
            if (le == null) le = go.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
            le.flexibleHeight = 0f;
        }

        private void OnEnable()
        {
            if (gameManager != null)
            {
                gameManager.OnGameOver -= HandleGameOver;
                gameManager.OnGameOver += HandleGameOver;
            }
            GameEvents.OnGameOver -= HandleGameOver;
            GameEvents.OnGameOver += HandleGameOver;
        }

        private void OnDisable()
        {
            if (gameManager != null)
            {
                gameManager.OnGameOver -= HandleGameOver;
            }
            GameEvents.OnGameOver -= HandleGameOver;

            if (activeAnimationRoutine != null)
            {
                StopCoroutine(activeAnimationRoutine);
                activeAnimationRoutine = null;
            }
            scoreRollTween?.Stop();
        }

        private void OnDestroy()
        {
            if (restartButton != null) restartButton.onClick.RemoveListener(OnRestartButtonClicked);
            if (mainMenuButton != null) mainMenuButton.onClick.RemoveListener(OnMainMenuButtonClicked);
        }

        private void HandleGameOver(int finalScore)
        {
            if (gameOverPanel == null) return;

            gameOverPanel.SetActive(true);
            DodgeGame.Audio.SoundEventHook.Play(gameObject, DodgeGame.Audio.SoundEventId.GameOver);
            if (activeAnimationRoutine != null) StopCoroutine(activeAnimationRoutine);
            activeAnimationRoutine = StartCoroutine(GameOverSequence(finalScore));
        }

        private void EnsureTwistElements()
        {
            if (gameOverPanel == null) return;

            if (twistRevealText == null)
            {
                var existing = gameOverPanel.transform.Find("TwistRevealText");
                if (existing != null)
                {
                    twistRevealText = existing.GetComponent<TextMeshProUGUI>();
                }
                else
                {
                    var twistGo = new GameObject("TwistRevealText", typeof(RectTransform), typeof(TextMeshProUGUI));
                    twistGo.transform.SetParent(gameOverPanel.transform, false);
                    var rt = twistGo.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.1f, 0.28f);
                    rt.anchorMax = new Vector2(0.9f, 0.36f);
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                    twistRevealText = twistGo.GetComponent<TextMeshProUGUI>();
                    twistRevealText.alignment = TextAlignmentOptions.Center;
                    twistRevealText.color = new Color(0.118f, 0.165f, 0.200f, 1f);
                    twistRevealText.fontSize = 26f;
                    twistRevealText.fontStyle = FontStyles.Bold;
                    if (titleText != null && titleText.font != null)
                    {
                        twistRevealText.font = titleText.font;
                    }
                }
            }

            if (difficultyBadgeText == null)
            {
                var existing = gameOverPanel.transform.Find("DifficultyBadgeText");
                if (existing != null)
                {
                    difficultyBadgeText = existing.GetComponent<TextMeshProUGUI>();
                }
            }

            if (twistRevealText != null) twistRevealText.raycastTarget = false;
            if (difficultyBadgeText != null) difficultyBadgeText.raycastTarget = false;
        }

        private IEnumerator GameOverSequence(int finalScore)
        {
            EnsureTwistElements();

            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;

            float survivalTime = gameManager != null ? gameManager.SurvivalTimer : 0f;
            int minutes = (int)(survivalTime / 60f);
            int seconds = (int)(survivalTime % 60f);
            int millis = (int)((survivalTime * 100f) % 100f);
            string timeStr = $"{minutes:00}:{seconds:00}.{millis:00}";

            if (reasonText != null && gameManager != null)
            {
                reasonText.text = gameManager.GameOverReason;
            }

            Difficulty displayDiff = Difficulty.Normal;
            Difficulty effectiveDiff = Difficulty.Normal;
            if (SettingsService.Instance != null && SettingsService.Instance.CurrentSettings != null)
            {
                displayDiff = SettingsService.Instance.CurrentSettings.SelectedDifficulty;
                effectiveDiff = SettingsService.Instance.CurrentSettings.EffectiveDifficulty;
            }

            if (difficultyBadgeText != null)
            {
                difficultyBadgeText.text = displayDiff.ToString().ToUpperInvariant();
            }

            bool revealTwist = SettingsService.Instance != null &&
                               SettingsService.Instance.DifficultyConfig != null &&
                               SettingsService.Instance.DifficultyConfig.RevealTwist;

            if (twistRevealText != null)
            {
                if (revealTwist && displayDiff != effectiveDiff)
                {
                    twistRevealText.gameObject.SetActive(true);
                    string targetName = effectiveDiff == Difficulty.Normal ? "Medium" : effectiveDiff.ToString();
                    twistRevealText.text = $"Plot twist! You were playing on {targetName}.";
                }
                else
                {
                    twistRevealText.gameObject.SetActive(false);
                }
            }

            int bestScore = gameManager != null ? gameManager.HighScore : GameManager.GetHighScore(displayDiff);
            bool isNewBest = (gameManager != null && gameManager.IsNewHighScore) || (finalScore >= bestScore && finalScore > 0);

            if (highScoreText != null)
            {
                highScoreText.text = $"BEST: {Mathf.Max(bestScore, finalScore)}";
            }

            if (newRecordBadge != null)
            {
                newRecordBadge.SetActive(isNewBest);
                if (isNewBest)
                {
                    newRecordBadge.transform.localScale = Vector3.zero;
                }
            }

            if (finalScoreText != null)
            {
                finalScoreText.text = "0";
            }

            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeInDuration);
                panelCanvasGroup.alpha = Mathf.SmoothStep(0f, 1f, t);
                yield return null;
            }

            panelCanvasGroup.alpha = 1f;
            panelCanvasGroup.interactable = true;
            panelCanvasGroup.blocksRaycasts = true;

            scoreRollTween?.Stop();
            scoreRollTween = Tween.Value(
                0f,
                (float)finalScore,
                scoreRollDuration,
                val =>
                {
                    if (finalScoreText != null)
                    {
                        finalScoreText.text = $"{Mathf.RoundToInt(val)}";
                    }
                },
                EaseType.EaseOutCubic,
                true,
                () =>
                {
                    if (finalScoreText != null)
                    {
                        finalScoreText.text = $"{finalScore}";
                    }

                    if (isNewBest && newRecordBadge != null)
                    {
                        newRecordBadge.transform.localScale = Vector3.zero;
                        Tween.Scale(newRecordBadge.transform, Vector3.one, 0.25f, EaseType.EaseOutBack, true);
                    }
                }
            );

            if (restartButton != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(restartButton.gameObject);
            }

            activeAnimationRoutine = null;
        }

        private void OnRestartButtonClicked()
        {
            SoundManager.Instance?.PlayButtonClick();
            if (gameManager != null) gameManager.Restart();
        }

        private void OnMainMenuButtonClicked()
        {
            SoundManager.Instance?.PlayButtonClick();
            if (gameManager != null) gameManager.LoadMainMenu();
        }
    }
}
