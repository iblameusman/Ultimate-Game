using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DodgeGame.Core;
using DodgeGame.Data;

namespace DodgeGame.UI
{
    public class CaptionUI : MonoBehaviour
    {
        public static CaptionUI Instance { get; private set; }

        [Header("Typography & Styling")]
        [SerializeField] private TypographyConfig typographyConfig;
        [SerializeField] private TypographyRole role = TypographyRole.Caption;

        [Header("Caption Display")]
        [SerializeField] private TextMeshProUGUI captionText;
        [SerializeField] private CanvasGroup captionCanvasGroup;

        [Header("Visual Cue Display")]
        [SerializeField] private GameObject visualCueContainer;
        [SerializeField] private TextMeshProUGUI visualCueText;
        [SerializeField] private CanvasGroup visualCueCanvasGroup;

        private Coroutine activeCaptionRoutine;
        private Coroutine activeCueRoutine;
        private TweenHandle captionFadeTween;
        private TweenHandle cueFadeTween;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureComponents();
        }

        private void OnEnable()
        {
            AudioManager.OnCaptionRequested -= ShowCaption;
            AudioManager.OnCaptionRequested += ShowCaption;

            GameEvents.OnBallSpawned -= HandleBallSpawnedVisualCue;
            GameEvents.OnBallSpawned += HandleBallSpawnedVisualCue;

            GameEvents.OnPlayerHit -= HandlePlayerHitVisualCue;
            GameEvents.OnPlayerHit += HandlePlayerHitVisualCue;
        }

        private void OnDisable()
        {
            AudioManager.OnCaptionRequested -= ShowCaption;
            GameEvents.OnBallSpawned -= HandleBallSpawnedVisualCue;
            GameEvents.OnPlayerHit -= HandlePlayerHitVisualCue;

            captionFadeTween?.Stop();
            cueFadeTween?.Stop();
        }

        private void EnsureComponents()
        {
            if (typographyConfig == null)
            {
                typographyConfig = Resources.Load<TypographyConfig>("TypographyConfig");
            }

            if (captionText == null)
            {
                var capGo = transform.Find("CaptionText")?.gameObject;
                if (capGo == null)
                {
                    capGo = new GameObject("CaptionText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(CanvasGroup));
                    capGo.transform.SetParent(transform, false);
                    var rt = capGo.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.5f, 0.15f);
                    rt.anchorMax = new Vector2(0.5f, 0.15f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(600f, 40f);
                    rt.anchoredPosition = Vector2.zero;
                }
                captionText = capGo.GetComponent<TextMeshProUGUI>();
                captionCanvasGroup = capGo.GetComponent<CanvasGroup>();
            }

            if (captionCanvasGroup == null && captionText != null)
            {
                captionCanvasGroup = captionText.GetComponent<CanvasGroup>() ?? captionText.gameObject.AddComponent<CanvasGroup>();
            }

            if (captionCanvasGroup != null)
            {
                captionCanvasGroup.alpha = 0f;
                captionCanvasGroup.blocksRaycasts = false;
                captionCanvasGroup.interactable = false;
            }

            if (captionText != null)
            {
                captionText.alignment = TextAlignmentOptions.Center;
                ApplyTypography();
            }

            if (visualCueContainer == null)
            {
                var cueGo = transform.Find("VisualCueContainer")?.gameObject;
                if (cueGo == null)
                {
                    cueGo = new GameObject("VisualCueContainer", typeof(RectTransform), typeof(CanvasGroup));
                    cueGo.transform.SetParent(transform, false);
                    var rt = cueGo.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.5f, 0.85f);
                    rt.anchorMax = new Vector2(0.5f, 0.85f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(500f, 48f);
                    rt.anchoredPosition = Vector2.zero;

                    var cueTxtGo = new GameObject("CueText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                    cueTxtGo.transform.SetParent(cueGo.transform, false);
                    var txtRt = cueTxtGo.GetComponent<RectTransform>();
                    txtRt.anchorMin = Vector2.zero;
                    txtRt.anchorMax = Vector2.one;
                    txtRt.sizeDelta = Vector2.zero;

                    visualCueText = cueTxtGo.GetComponent<TextMeshProUGUI>();
                    visualCueText.alignment = TextAlignmentOptions.Center;
                    visualCueText.fontSize = 24;
                    visualCueText.fontStyle = FontStyles.Bold;
                    visualCueText.color = new Color(1f, 0.88f, 0.2f, 1f);
                }
                visualCueContainer = cueGo;
                visualCueCanvasGroup = cueGo.GetComponent<CanvasGroup>() ?? cueGo.AddComponent<CanvasGroup>();
            }

            if (visualCueCanvasGroup != null)
            {
                visualCueCanvasGroup.alpha = 0f;
                visualCueCanvasGroup.blocksRaycasts = false;
                visualCueCanvasGroup.interactable = false;
            }
        }

        private void ApplyTypography()
        {
            if (captionText == null) return;

            if (typographyConfig != null)
            {
                var style = typographyConfig.GetStyle(role);
                if (style.Font != null) captionText.font = style.Font;
                captionText.fontSize = style.Size;
                captionText.color = style.Color;
                captionText.characterSpacing = style.CharacterSpacing;
                captionText.alignment = TextAlignmentOptions.Center;
            }
            else
            {
                captionText.fontSize = 26;
                captionText.fontStyle = FontStyles.Bold;
                captionText.color = new Color(0.9f, 0.95f, 1f, 1f);
            }
        }

        public void ShowCaption(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            if (SettingsService.Instance != null && SettingsService.Instance.CurrentSettings != null)
            {
                if (!SettingsService.Instance.CurrentSettings.CaptionsEnabled) return;
            }

            if (activeCaptionRoutine != null)
            {
                StopCoroutine(activeCaptionRoutine);
            }
            activeCaptionRoutine = StartCoroutine(CaptionRoutine(text));
        }

        private IEnumerator CaptionRoutine(string text)
        {
            if (captionText != null) captionText.text = text;
            if (captionCanvasGroup == null) yield break;

            captionFadeTween?.Stop();
            captionFadeTween = Tween.Alpha(captionCanvasGroup, 1f, 0.2f, EaseType.Linear, useUnscaledTime: true);
            yield return new WaitForSecondsRealtime(0.2f);

            yield return new WaitForSecondsRealtime(1.2f);

            captionFadeTween?.Stop();
            captionFadeTween = Tween.Alpha(captionCanvasGroup, 0f, 0.2f, EaseType.Linear, useUnscaledTime: true);
            yield return new WaitForSecondsRealtime(0.2f);

            if (captionText != null) captionText.text = "";
            activeCaptionRoutine = null;
        }


        private void HandleBallSpawnedVisualCue(BallSpawnInfo info)
        {
            string cueMessage = null;
            if (info.IsRare) cueMessage = "! RARE BALL INCOMING !";
            else if (info.Category == BallCategory.Nuclear) cueMessage = "! NUCLEAR HAZARD !";
            else if (info.Category == BallCategory.Gun) cueMessage = "! GUN TURRET ACTIVE !";
            else if (info.IsDun) cueMessage = "! RAPID THREAT SPAWN !";
            else if (info.Category == BallCategory.Fire) cueMessage = "! FIRE BALL SPAWNED !";
            else if (info.Category == BallCategory.Sticky) cueMessage = "! STICKY BALL SPAWNED !";
            else if (info.IsFast) cueMessage = "! HIGH SPEED BALL !";

            if (!string.IsNullOrEmpty(cueMessage))
            {
                ShowVisualCue(cueMessage, new Color(1f, 0.85f, 0.2f, 1f));
            }
        }

        private void HandlePlayerHitVisualCue(int livesRemaining)
        {
            string hitMessage = livesRemaining <= 0 ? "ELIMINATED!" : "DIRECT HIT!";
            Color cueColor = livesRemaining <= 0 ? new Color(1f, 0.2f, 0.2f, 1f) : new Color(1f, 0.4f, 0.3f, 1f);
            ShowVisualCue(hitMessage, cueColor, 0.8f);
        }

        public void ShowVisualCue(string text, Color textColor, float duration = 1.0f)
        {
            if (activeCueRoutine != null)
            {
                StopCoroutine(activeCueRoutine);
            }
            activeCueRoutine = StartCoroutine(VisualCueRoutine(text, textColor, duration));
        }

        private IEnumerator VisualCueRoutine(string text, Color textColor, float duration)
        {
            if (visualCueText != null)
            {
                visualCueText.text = text;
                visualCueText.color = textColor;
            }
            if (visualCueCanvasGroup == null) yield break;

            cueFadeTween?.Stop();
            cueFadeTween = Tween.Alpha(visualCueCanvasGroup, 1f, 0.15f, EaseType.EaseOutCubic, useUnscaledTime: true);
            yield return new WaitForSecondsRealtime(0.15f);

            yield return new WaitForSecondsRealtime(duration);

            cueFadeTween?.Stop();
            cueFadeTween = Tween.Alpha(visualCueCanvasGroup, 0f, 0.25f, EaseType.Linear, useUnscaledTime: true);
            yield return new WaitForSecondsRealtime(0.25f);

            if (visualCueText != null) visualCueText.text = "";
            activeCueRoutine = null;
        }
    }
}
