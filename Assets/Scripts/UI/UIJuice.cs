using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DodgeGame.Core;
using DodgeGame.Data;

namespace DodgeGame.UI
{
    public class UIJuice : MonoBehaviour
    {
        public static UIJuice Instance { get; private set; }

        [Header("Score Animation")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private RectTransform scorePanel;
        [SerializeField] private Color normalScoreColor = Color.white;
        [SerializeField] private Color streakPulseColor = new Color(0f, 1f, 0.67f, 1f);

        [Header("Streak Counter")]
        [SerializeField] private TextMeshProUGUI streakCounterText;
        [SerializeField] private CanvasGroup streakCanvasGroup;

        [Header("Floating Feedback Popups")]
        [SerializeField] private RectTransform floatingTextContainer;
        [SerializeField] private GameObject floatingTextPrefab;
        [SerializeField] private int floatingPoolSize = 10;

        [Header("Theme")]
        [SerializeField] private HudTheme theme;

        [Header("Lives Icons")]
        [SerializeField] private RectTransform livesContainer;
        [SerializeField] private GameObject[] lifeIcons;

        [Header("Screen Edge Red Flash")]
        [SerializeField] private CanvasGroup redFlashCanvasGroup;

        private class PooledText
        {
            public GameObject Root;
            public RectTransform Rect;
            public TextMeshProUGUI Tmp;
            public CanvasGroup CanvasGroup;
            public TweenHandle PosTween;
            public TweenHandle AlphaTween;
            public bool InUse;
        }

        private readonly List<PooledText> textPool = new List<PooledText>();

        private int displayedScore = 0;
        private int targetScore = 0;
        private int currentLives = 3;
        private int dodgeStreak = 0;

        private Vector3 originalScoreScale = Vector3.one;
        private Transform cachedPlayerTransform;
        private TweenHandle scoreTickTween;
        private TweenHandle scorePulseTween;
        private TweenHandle streakPulseTween;
        private TweenHandle streakFadeTween;
        private TweenHandle redFlashTweenIn;
        private TweenHandle redFlashTweenOut;

        public TextMeshProUGUI StreakCounterText => streakCounterText;
        public CanvasGroup StreakCanvasGroup => streakCanvasGroup;
        public RectTransform FloatingTextContainer => floatingTextContainer;
        public HudTheme Theme { get => theme; set => theme = value; }
        public void TriggerFloatingDelta(int delta) => SpawnFloatingDelta(delta);
        public void TriggerNearMiss() => SpawnNearMissPopup();
        public void TriggerStreak(int threshold) => HandleStreakChanged(threshold);
        public void TriggerStreakLoss() => HandleStreakBroken(0);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            ResolveReferences();
            InitializeFloatingPool();

            if (scoreText != null)
            {
                originalScoreScale = scoreText.rectTransform.localScale;
                normalScoreColor = scoreText.color;
            }

            if (redFlashCanvasGroup != null)
            {
                redFlashCanvasGroup.alpha = 0f;
                redFlashCanvasGroup.blocksRaycasts = false;
                redFlashCanvasGroup.interactable = false;
            }
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerHit += HandlePlayerHit;
            GameEvents.OnPlayerDodged += HandlePlayerDodged;
            GameEvents.OnStreakChanged += HandleStreakChanged;
            GameEvents.OnStreakBroken += HandleStreakBroken;
            GameEvents.OnNearMiss += HandleNearMiss;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerHit -= HandlePlayerHit;
            GameEvents.OnPlayerDodged -= HandlePlayerDodged;
            GameEvents.OnStreakChanged -= HandleStreakChanged;
            GameEvents.OnStreakBroken -= HandleStreakBroken;
            GameEvents.OnNearMiss -= HandleNearMiss;

            scoreTickTween?.Stop();
            scorePulseTween?.Stop();
            streakPulseTween?.Stop();
            streakFadeTween?.Stop();
            redFlashTweenIn?.Stop();
            redFlashTweenOut?.Stop();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void ResolveReferences()
        {
            if (scoreText == null)
            {
                scoreText = transform.Find("TopLeft_ScoreGroup/ScorePanel/ScoreValue")?.GetComponent<TextMeshProUGUI>()
                         ?? transform.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
            }

            if (scorePanel == null)
            {
                scorePanel = transform.Find("TopLeft_ScoreGroup/ScorePanel")?.GetComponent<RectTransform>()
                          ?? (scoreText != null ? scoreText.transform.parent as RectTransform : null);
            }

            if (livesContainer == null)
            {
                livesContainer = transform.Find("LivesContainer")?.GetComponent<RectTransform>();
            }

            if (lifeIcons == null || lifeIcons.Length == 0)
            {
                if (livesContainer != null)
                {
                    int childCount = livesContainer.childCount;
                    lifeIcons = new GameObject[childCount];
                    for (int i = 0; i < childCount; i++)
                    {
                        lifeIcons[i] = livesContainer.GetChild(i).gameObject;
                    }
                }
            }

            if (redFlashCanvasGroup == null)
            {
                var flashObj = transform.root.Find("RedFlashOverlay") ?? transform.Find("RedFlashOverlay");
                if (flashObj != null)
                {
                    redFlashCanvasGroup = flashObj.GetComponent<CanvasGroup>();
                }
            }

            if (streakCounterText == null)
            {
                streakCounterText = transform.Find("TopCenter_TimerGroup/StreakCounter")?.GetComponent<TextMeshProUGUI>()
                                 ?? transform.Find("StreakCounter")?.GetComponent<TextMeshProUGUI>()
                                 ?? transform.Find("StreakCounterText")?.GetComponent<TextMeshProUGUI>();
            }

            if (streakCounterText != null && streakCanvasGroup == null)
            {
                streakCanvasGroup = streakCounterText.GetComponent<CanvasGroup>();
            }

            if (floatingTextContainer == null)
            {
                floatingTextContainer = transform.Find("FloatingTextContainer")?.GetComponent<RectTransform>();
            }

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
        }

        private void InitializeFloatingPool()
        {
            if (floatingTextContainer == null)
            {
                var containerGo = new GameObject("FloatingTextContainer", typeof(RectTransform));
                containerGo.transform.SetParent(transform, false);
                floatingTextContainer = containerGo.GetComponent<RectTransform>();
            }

            for (int i = 0; i < floatingPoolSize; i++)
            {
                GameObject go;
                if (floatingTextPrefab != null)
                {
                    go = Instantiate(floatingTextPrefab, floatingTextContainer);
                }
                else
                {
                    go = new GameObject($"FloatText_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(CanvasGroup));
                    go.transform.SetParent(floatingTextContainer, false);
                    var tmp = go.GetComponent<TextMeshProUGUI>();
                    tmp.fontSize = 28;
                    tmp.fontStyle = FontStyles.Bold;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.color = streakPulseColor;
                }

                go.SetActive(false);

                var pooled = new PooledText
                {
                    Root = go,
                    Rect = go.GetComponent<RectTransform>(),
                    Tmp = go.GetComponent<TextMeshProUGUI>(),
                    CanvasGroup = go.GetComponent<CanvasGroup>(),
                    InUse = false
                };

                if (pooled.CanvasGroup == null)
                {
                    pooled.CanvasGroup = go.AddComponent<CanvasGroup>();
                }

                textPool.Add(pooled);
            }
        }


        public void AnimateScore(int newScore)
        {
            int oldScore = targetScore;
            targetScore = newScore;
            int delta = targetScore - oldScore;

            if (delta > 0)
            {
                SpawnFloatingDelta(delta);
                PulseScorePanel();
            }

            if (scoreText == null) return;

            scoreTickTween?.Stop();
            int startVal = displayedScore;
            int endVal = targetScore;

            scoreTickTween = Tween.Value(
                startVal,
                endVal,
                0.35f,
                (val) =>
                {
                    displayedScore = Mathf.RoundToInt(val);
                    scoreText.text = displayedScore.ToString();
                },
                EaseType.EaseOutCubic,
                useUnscaledTime: true,
                onComplete: () =>
                {
                    displayedScore = targetScore;
                    scoreText.text = targetScore.ToString();
                }
            );
        }

        public void PulseScorePanel()
        {
            if (scorePanel == null && scoreText != null)
            {
                scorePanel = scoreText.transform.parent as RectTransform;
            }
            if (scorePanel == null) return;

            scorePulseTween?.Stop();
            scorePanel.localScale = Vector3.one;

            scorePulseTween = Tween.Scale(
                scorePanel,
                Vector3.one * 1.06f,
                0.12f,
                EaseType.EaseOutCubic,
                useUnscaledTime: true,
                onComplete: () =>
                {
                    scorePulseTween = Tween.Scale(
                        scorePanel,
                        Vector3.one,
                        0.14f,
                        EaseType.EaseOutCubic,
                        useUnscaledTime: true
                    );
                }
            );
        }

        public static TweenHandle RollScoreCounter(TextMeshProUGUI targetText, int targetScore, float duration = 0.5f, string prefix = "")
        {
            if (targetText == null) return null;
            return Tween.Value(
                0f,
                (float)targetScore,
                duration,
                val =>
                {
                    if (targetText != null)
                    {
                        targetText.text = $"{prefix}{Mathf.RoundToInt(val)}";
                    }
                },
                EaseType.EaseOutCubic,
                true,
                () =>
                {
                    if (targetText != null)
                    {
                        targetText.text = $"{prefix}{targetScore}";
                    }
                }
            );
        }

        private PooledText GetAvailablePooledText()
        {
            if (textPool.Count == 0)
            {
                InitializeFloatingPool();
            }

            for (int i = 0; i < textPool.Count; i++)
            {
                if (!textPool[i].InUse)
                {
                    return textPool[i];
                }
            }

            if (textPool.Count > 0)
            {
                var item = textPool[0];
                item.PosTween?.Stop();
                item.AlphaTween?.Stop();
                return item;
            }
            return null;
        }

        public Vector2 GetPlayerScreenPosition(Vector3 worldOffset)
        {
            if (cachedPlayerTransform == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) cachedPlayerTransform = player.transform;
            }

            Camera cam = Camera.main;
            if (cachedPlayerTransform != null && cam != null)
            {
                Vector3 worldPos = cachedPlayerTransform.position + worldOffset;
                Vector3 screenPoint = cam.WorldToScreenPoint(worldPos);

                Canvas canvas = GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    RectTransform canvasRect = canvas.transform as RectTransform;
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam, out Vector2 localPos))
                    {
                        return localPos;
                    }
                }
                return new Vector2(screenPoint.x - Screen.width * 0.5f, screenPoint.y - Screen.height * 0.5f);
            }

            return Vector2.zero;
        }

        private void SpawnFloatingDelta(int delta)
        {
            PooledText item = GetAvailablePooledText();
            if (item == null) return;

            item.InUse = true;
            item.Root.SetActive(true);
            item.Tmp.text = $"+{delta}";
            item.Tmp.fontSize = 40f;
            item.Tmp.fontStyle = FontStyles.Bold;
            Color textCol = theme != null ? theme.TextPrimary : new Color(30f / 255f, 42f / 255f, 51f / 255f, 1f);
            item.Tmp.color = textCol;
            item.Tmp.outlineWidth = 0.2f;
            item.Tmp.outlineColor = textCol;

            Vector2 origin = GetPlayerScreenPosition(new Vector3(0f, 1.8f, 0f));
            item.Rect.anchoredPosition = origin;
            item.CanvasGroup.alpha = 1f;

            Vector2 targetPos = origin + new Vector2(0f, 40f);

            item.PosTween?.Stop();
            item.AlphaTween?.Stop();

            item.PosTween = Tween.Position(item.Rect, targetPos, 0.6f, EaseType.EaseOutCubic, useUnscaledTime: true);
            item.AlphaTween = Tween.Alpha(item.CanvasGroup, 0f, 0.6f, EaseType.Linear, useUnscaledTime: true, onComplete: () =>
            {
                item.Root.SetActive(false);
                item.InUse = false;
            });
        }

        private void HandleNearMiss(PlayerMovementState state)
        {
            SpawnNearMissPopup();
        }

        private void SpawnNearMissPopup()
        {
            PooledText item = GetAvailablePooledText();
            if (item == null) return;

            item.InUse = true;
            item.Root.SetActive(true);
            item.Tmp.text = "CLOSE!";
            item.Tmp.fontSize = 24f;
            item.Tmp.fontStyle = FontStyles.Bold;
            Color readyCol = theme != null ? theme.AccentReady : new Color(0.239f, 0.863f, 0.165f, 1f);
            item.Tmp.color = readyCol;
            item.Tmp.outlineWidth = 0f;

            Vector2 origin = GetPlayerScreenPosition(new Vector3(0.6f, 1.3f, 0f));
            item.Rect.anchoredPosition = origin;
            item.CanvasGroup.alpha = 1f;

            Vector2 targetPos = origin + new Vector2(0f, 15f);

            item.PosTween?.Stop();
            item.AlphaTween?.Stop();

            item.PosTween = Tween.Position(item.Rect, targetPos, 0.4f, EaseType.EaseOutCubic, useUnscaledTime: true);
            item.AlphaTween = Tween.Alpha(item.CanvasGroup, 0f, 0.4f, EaseType.Linear, useUnscaledTime: true, onComplete: () =>
            {
                item.Root.SetActive(false);
                item.InUse = false;
            });
        }


        public void UpdateLives(int newLives)
        {
            if (lifeIcons == null || lifeIcons.Length == 0) return;

            if (newLives < currentLives)
            {
                for (int i = currentLives - 1; i >= Mathf.Max(0, newLives); i--)
                {
                    if (i < lifeIcons.Length && lifeIcons[i] != null)
                    {
                        AnimateLostLife(lifeIcons[i]);
                    }
                }
            }
            else if (newLives > currentLives)
            {
                for (int i = currentLives; i < Mathf.Min(newLives, lifeIcons.Length); i++)
                {
                    if (lifeIcons[i] != null)
                    {
                        AnimateGainedLife(lifeIcons[i]);
                    }
                }
            }

            currentLives = newLives;
        }

        private void AnimateLostLife(GameObject icon)
        {
            var cg = icon.GetComponent<CanvasGroup>();
            if (cg == null) cg = icon.AddComponent<CanvasGroup>();

            Tween.Scale(icon.transform, Vector3.zero, 0.25f, EaseType.EaseOutCubic, useUnscaledTime: true);
            Tween.Alpha(cg, 0f, 0.25f, EaseType.Linear, useUnscaledTime: true);
        }

        private void AnimateGainedLife(GameObject icon)
        {
            var cg = icon.GetComponent<CanvasGroup>();
            if (cg == null) cg = icon.AddComponent<CanvasGroup>();

            icon.SetActive(true);
            icon.transform.localScale = Vector3.zero;
            cg.alpha = 1f;

            Tween.Scale(icon.transform, Vector3.one, 0.25f, EaseType.EaseOutBack, useUnscaledTime: true);
        }


        private void HandlePlayerHit(int livesRemaining)
        {
            dodgeStreak = 0;
            UpdateStreakUI(0);
            UpdateLives(livesRemaining);
            PlayScreenEdgeRedFlash();
        }

        public void PlayScreenEdgeRedFlash()
        {
            if (redFlashCanvasGroup == null) return;

            redFlashTweenIn?.Stop();
            redFlashTweenOut?.Stop();

            redFlashTweenIn = Tween.Alpha(
                redFlashCanvasGroup,
                0.35f,
                0.08f,
                EaseType.EaseOutCubic,
                useUnscaledTime: true,
                onComplete: () =>
                {
                    redFlashTweenOut = Tween.Alpha(
                        redFlashCanvasGroup,
                        0.0f,
                        0.17f,
                        EaseType.Linear,
                        useUnscaledTime: true
                    );
                }
            );
        }


        private void HandlePlayerDodged(int totalDodges)
        {
            dodgeStreak = totalDodges;

            if (dodgeStreak > 0 && (dodgeStreak % 5) == 0)
            {
                PlayScoreGreenPulse();
            }
        }

        private void HandleStreakChanged(int streakThreshold)
        {
            if (streakThreshold == 5 || streakThreshold == 10 || streakThreshold == 15 || streakThreshold == 20)
            {
                UpdateStreakUI(streakThreshold);
                PulseStreakCounter();
            }
        }

        private void HandleStreakBroken(int lostStreak)
        {
            dodgeStreak = 0;
            HideStreakUI();
        }

        private void UpdateStreakUI(int streak)
        {
            if (streakCounterText == null) return;

            streakFadeTween?.Stop();
            streakCounterText.text = $"STREAK x{streak}";
            if (streakCanvasGroup != null)
            {
                streakCanvasGroup.alpha = 1f;
            }
        }

        private void HideStreakUI()
        {
            if (streakCounterText == null) return;

            streakFadeTween?.Stop();
            if (streakCanvasGroup != null)
            {
                streakFadeTween = Tween.Alpha(streakCanvasGroup, 0f, 0.2f, EaseType.Linear, useUnscaledTime: true, onComplete: () =>
                {
                    if (streakCounterText != null) streakCounterText.text = "";
                });
            }
            else
            {
                streakCounterText.text = "";
            }
        }

        public void PulseStreakCounter()
        {
            if (streakCounterText == null) return;

            streakPulseTween?.Stop();
            Vector3 baseScale = Vector3.one;
            Vector3 punchScale = baseScale * 1.35f;

            streakPulseTween = Tween.Scale(
                streakCounterText.rectTransform,
                punchScale,
                0.12f,
                EaseType.EaseOutCubic,
                useUnscaledTime: true,
                onComplete: () =>
                {
                    streakPulseTween = Tween.Scale(
                        streakCounterText.rectTransform,
                        baseScale,
                        0.13f,
                        EaseType.EaseInOutQuad,
                        useUnscaledTime: true,
                        onComplete: () =>
                        {
                            if (streakCounterText != null)
                            {
                                streakCounterText.rectTransform.localScale = baseScale;
                            }
                        }
                    );
                }
            );
        }

        private void PlayScoreGreenPulse()
        {
            if (scoreText == null) return;

            scorePulseTween?.Stop();
            scoreText.color = streakPulseColor;

            Vector3 punchScale = originalScoreScale * 1.18f;

            scorePulseTween = Tween.Scale(
                scoreText.rectTransform,
                punchScale,
                0.12f,
                EaseType.EaseOutCubic,
                useUnscaledTime: true,
                onComplete: () =>
                {
                    scorePulseTween = Tween.Scale(
                        scoreText.rectTransform,
                        originalScoreScale,
                        0.13f,
                        EaseType.EaseInOutQuad,
                        useUnscaledTime: true,
                        onComplete: () =>
                        {
                            scoreText.rectTransform.localScale = originalScoreScale;
                            scoreText.color = normalScoreColor;
                        }
                    );
                }
            );
        }
    }
}
