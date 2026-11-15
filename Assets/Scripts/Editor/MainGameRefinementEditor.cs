using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using DodgeGame.Core;
using DodgeGame.Data;
using DodgeGame.UI;

namespace DodgeGame.Editor
{
    public static class MainGameRefinementEditor
    {
        [MenuItem("Tools/Execute Phase 8 Refinement")]
        public static void ExecutePhase8Refinement()
        {
            RefineMainGameScene();
            ExecuteNamingPass();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Phase 8] Refinement and naming pass completed successfully!");
        }

        public static void RefineMainGameScene()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path != "Assets/Scenes/MainGame.unity")
            {
                activeScene = EditorSceneManager.OpenScene("Assets/Scenes/MainGame.unity", OpenSceneMode.Single);
            }

            var canvasObj = GameObject.Find("Canvas");
            if (canvasObj == null)
            {
                Debug.LogError("[Phase 8] Canvas not found in MainGame scene!");
                return;
            }

            var typography = AssetDatabase.LoadAssetAtPath<TypographyConfig>("Assets/Resources/DefaultTypography.asset")
                          ?? AssetDatabase.LoadAssetAtPath<TypographyConfig>("Assets/ScriptableObjects/DefaultTypography.asset");

            var fontUI = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Art/Fonts/FontUI.asset");
            var fontDisplay = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Art/Fonts/FontDisplay.asset");
            var fontBody = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Art/Fonts/FontBody.asset");

            Color charcoalDark = new Color(0.043f, 0.071f, 0.059f, 0.95f);
            Color charcoalCard = new Color(0.078f, 0.11f, 0.094f, 0.95f);
            Color accentGreen = new Color(0f, 1f, 0.616f, 1f);
            Color steelGrey = new Color(0.3f, 0.38f, 0.34f, 1f);
            Color textWhite = new Color(0.95f, 0.98f, 0.96f, 1f);
            Color goldRecord = new Color(1f, 0.84f, 0.2f, 1f);

            var canvasTr = canvasObj.transform;
            var hudTr = canvasTr.Find("HUD");
            if (hudTr == null)
            {
                Debug.LogError("[Phase 8] HUD not found under Canvas!");
                return;
            }


            var scoreTextTr = hudTr.Find("ScoreText");
            if (scoreTextTr != null)
            {
                var rect = scoreTextTr.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2(40f, -36f);
                rect.sizeDelta = new Vector2(340f, 50f);

                var tmp = scoreTextTr.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.alignment = TextAlignmentOptions.TopLeft;
                    if (fontDisplay != null) tmp.font = fontDisplay;
                    tmp.fontSize = 40f;
                    tmp.color = accentGreen;
                }
            }

            var livesContainerTr = hudTr.Find("LivesContainer");
            if (livesContainerTr != null)
            {
                var rect = livesContainerTr.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(1f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 1f);
                rect.anchoredPosition = new Vector2(-40f, -36f);
                rect.sizeDelta = new Vector2(160f, 44f);

                var layout = livesContainerTr.GetComponent<HorizontalLayoutGroup>();
                if (layout != null)
                {
                    layout.childAlignment = TextAnchor.MiddleRight;
                    layout.spacing = 10f;
                    layout.childControlWidth = false;
                    layout.childControlHeight = false;
                    layout.childForceExpandWidth = false;
                    layout.childForceExpandHeight = false;
                }

                for (int i = 0; i < livesContainerTr.childCount; i++)
                {
                    var iconTr = livesContainerTr.GetChild(i) as RectTransform;
                    if (iconTr != null)
                    {
                        iconTr.sizeDelta = new Vector2(36f, 36f);
                    }
                }
            }

            var livesTextTr = hudTr.Find("LivesText");
            if (livesTextTr != null)
            {
                livesTextTr.gameObject.SetActive(false);
            }

            var timerTextTr = hudTr.Find("TimerText");
            if (timerTextTr != null)
            {
                var rect = timerTextTr.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 1f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0f, -36f);
                rect.sizeDelta = new Vector2(240f, 44f);

                var tmp = timerTextTr.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.alignment = TextAlignmentOptions.Center;
                    if (fontUI != null) tmp.font = fontUI;
                    tmp.fontSize = 28f;
                    tmp.color = textWhite;
                }
            }

            var highScoreTr = hudTr.Find("HighScoreText");
            if (highScoreTr != null)
            {
                var rect = highScoreTr.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2(40f, -86f);
                rect.sizeDelta = new Vector2(250f, 30f);

                var tmp = highScoreTr.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.alignment = TextAlignmentOptions.Left;
                    if (fontBody != null) tmp.font = fontBody;
                    tmp.fontSize = 20f;
                    tmp.color = steelGrey;
                }
            }

            var skillsContainerTr = hudTr.Find("SkillsContainer");
            if (skillsContainerTr == null)
            {
                var skillsGo = new GameObject("SkillsContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                skillsGo.transform.SetParent(hudTr, false);
                skillsContainerTr = skillsGo.transform;
            }

            var skillsRect = skillsContainerTr.GetComponent<RectTransform>();
            skillsRect.anchorMin = new Vector2(0.5f, 0f);
            skillsRect.anchorMax = new Vector2(0.5f, 0f);
            skillsRect.pivot = new Vector2(0.5f, 0f);
            skillsRect.anchoredPosition = new Vector2(0f, 32f);
            skillsRect.sizeDelta = new Vector2(760f, 52f);

            var skillsLayout = skillsContainerTr.GetComponent<HorizontalLayoutGroup>();
            skillsLayout.childAlignment = TextAnchor.MiddleCenter;
            skillsLayout.spacing = 16f;
            skillsLayout.childControlWidth = false;
            skillsLayout.childControlHeight = false;
            skillsLayout.childForceExpandWidth = false;
            skillsLayout.childForceExpandHeight = false;

            var dimensionMeterTr = hudTr.Find("DimensionMeter");
            var dashIndicatorTr = hudTr.Find("DashIndicator");
            var slowMoMeterTr = hudTr.Find("SlowMotionMeter");

            FormatSkillSlot(dimensionMeterTr, skillsContainerTr, "SHIFT: READY [Q]", charcoalCard, accentGreen, fontUI);
            FormatSkillSlot(dashIndicatorTr, skillsContainerTr, "DASH: READY [SHIFT]", charcoalCard, accentGreen, fontUI);
            FormatSkillSlot(slowMoMeterTr, skillsContainerTr, "SLOW-MO: READY [E]", charcoalCard, accentGreen, fontUI);

            var pausePanelTr = canvasTr.Find("PausePanel");
            if (pausePanelTr != null)
            {
                var pauseRect = pausePanelTr.GetComponent<RectTransform>();
                pauseRect.anchorMin = Vector2.zero;
                pauseRect.anchorMax = Vector2.one;
                pauseRect.sizeDelta = Vector2.zero;
                pauseRect.anchoredPosition = Vector2.zero;

                var pauseBg = pausePanelTr.GetComponent<Image>();
                if (pauseBg != null)
                {
                    pauseBg.color = new Color(0.035f, 0.055f, 0.045f, 0.88f);
                }

                var pauseTransition = pausePanelTr.GetComponent<MenuPanelTransition>();
                if (pauseTransition == null)
                {
                    pauseTransition = pausePanelTr.gameObject.AddComponent<MenuPanelTransition>();
                }

                var pauseCg = pausePanelTr.GetComponent<CanvasGroup>();
                if (pauseCg == null)
                {
                    pauseCg = pausePanelTr.gameObject.AddComponent<CanvasGroup>();
                }

                var pauseUI = canvasObj.GetComponent<PauseUI>();
                if (pauseUI == null)
                {
                    pauseUI = canvasObj.AddComponent<PauseUI>();
                }
                var oldPauseOnPanel = pausePanelTr.GetComponent<PauseUI>();
                if (oldPauseOnPanel != null)
                {
                    UnityEngine.Object.DestroyImmediate(oldPauseOnPanel);
                }

                var mainColTr = pausePanelTr.Find("MainPauseColumn");
                if (mainColTr == null)
                {
                    var mainColGo = new GameObject("MainPauseColumn", typeof(RectTransform), typeof(CanvasGroup), typeof(VerticalLayoutGroup));
                    mainColGo.transform.SetParent(pausePanelTr, false);
                    mainColTr = mainColGo.transform;
                }

                var colRect = mainColTr.GetComponent<RectTransform>();
                colRect.anchorMin = new Vector2(0.5f, 0.5f);
                colRect.anchorMax = new Vector2(0.5f, 0.5f);
                colRect.pivot = new Vector2(0.5f, 0.5f);
                colRect.anchoredPosition = Vector2.zero;
                colRect.sizeDelta = new Vector2(400f, 520f);

                var colLayout = mainColTr.GetComponent<VerticalLayoutGroup>();
                colLayout.childAlignment = TextAnchor.MiddleCenter;
                colLayout.spacing = 14f;
                colLayout.childControlWidth = true;
                colLayout.childControlHeight = false;
                colLayout.childForceExpandWidth = true;
                colLayout.childForceExpandHeight = false;

                var titleTr = pausePanelTr.Find("Title") ?? mainColTr.Find("Title");
                if (titleTr != null)
                {
                    titleTr.SetParent(mainColTr, false);
                    var tmp = titleTr.GetComponent<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        tmp.text = "PAUSED";
                        tmp.alignment = TextAlignmentOptions.Center;
                        if (fontDisplay != null) tmp.font = fontDisplay;
                        tmp.fontSize = 54f;
                        tmp.color = accentGreen;
                    }
                    var trRect = titleTr.GetComponent<RectTransform>();
                    trRect.sizeDelta = new Vector2(400f, 70f);
                }

                var scoreReadoutTr = mainColTr.Find("CurrentScoreText") ?? pausePanelTr.Find("CurrentScoreText");
                if (scoreReadoutTr == null)
                {
                    var scoreGo = new GameObject("CurrentScoreText", typeof(RectTransform), typeof(TextMeshProUGUI));
                    scoreGo.transform.SetParent(mainColTr, false);
                    scoreReadoutTr = scoreGo.transform;
                }
                else
                {
                    scoreReadoutTr.SetParent(mainColTr, false);
                }
                var scoreTmp = scoreReadoutTr.GetComponent<TextMeshProUGUI>();
                scoreTmp.text = "CURRENT SCORE: 0";
                scoreTmp.alignment = TextAlignmentOptions.Center;
                if (fontUI != null) scoreTmp.font = fontUI;
                scoreTmp.fontSize = 26f;
                scoreTmp.color = textWhite;
                (scoreReadoutTr as RectTransform).sizeDelta = new Vector2(400f, 40f);

                var resumeBtnTr = pausePanelTr.Find("ResumeButton") ?? mainColTr.Find("ResumeButton");
                if (resumeBtnTr != null)
                {
                    resumeBtnTr.SetParent(mainColTr, false);
                    FormatMenuButton(resumeBtnTr, "RESUME", fontUI, accentGreen);
                }

                var settingsBtnTr = pausePanelTr.Find("SettingsButton") ?? mainColTr.Find("SettingsButton");
                if (settingsBtnTr == null)
                {
                    var btnGo = new GameObject("SettingsButton", typeof(RectTransform), typeof(Image), typeof(Button));
                    btnGo.transform.SetParent(mainColTr, false);
                    settingsBtnTr = btnGo.transform;
                }
                FormatMenuButton(settingsBtnTr, "SETTINGS", fontUI, textWhite);

                var howToPlayBtnTr = pausePanelTr.Find("HowToPlayButton") ?? mainColTr.Find("HowToPlayButton");
                if (howToPlayBtnTr == null)
                {
                    var btnGo = new GameObject("HowToPlayButton", typeof(RectTransform), typeof(Image), typeof(Button));
                    btnGo.transform.SetParent(mainColTr, false);
                    howToPlayBtnTr = btnGo.transform;
                }
                FormatMenuButton(howToPlayBtnTr, "HOW TO PLAY", fontUI, textWhite);

                var quitBtnTr = pausePanelTr.Find("MainMenuButton") ?? pausePanelTr.Find("QuitButton") ?? mainColTr.Find("MainMenuButton") ?? mainColTr.Find("QuitButton");
                if (quitBtnTr != null)
                {
                    quitBtnTr.gameObject.name = "QuitButton";
                    quitBtnTr.SetParent(mainColTr, false);
                    FormatMenuButton(quitBtnTr, "QUIT TO MENU", fontUI, textWhite);
                }

                var oldRestartInPause = pausePanelTr.Find("RestartButton") ?? mainColTr.Find("RestartButton");
                if (oldRestartInPause != null)
                {
                    oldRestartInPause.gameObject.SetActive(false);
                }

                var settingsSubTr = pausePanelTr.Find("SettingsSubpanel");
                if (settingsSubTr == null)
                {
                    var subGo = new GameObject("SettingsSubpanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(MenuPanelTransition));
                    subGo.transform.SetParent(pausePanelTr, false);
                    settingsSubTr = subGo.transform;

                    var subRect = settingsSubTr.GetComponent<RectTransform>();
                    subRect.anchorMin = new Vector2(0.5f, 0.5f);
                    subRect.anchorMax = new Vector2(0.5f, 0.5f);
                    subRect.pivot = new Vector2(0.5f, 0.5f);
                    subRect.sizeDelta = new Vector2(500f, 460f);

                    var subImg = settingsSubTr.GetComponent<Image>();
                    subImg.color = charcoalDark;

                    var sTitleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
                    sTitleGo.transform.SetParent(settingsSubTr, false);
                    var stRect = sTitleGo.GetComponent<RectTransform>();
                    stRect.anchoredPosition = new Vector2(0f, 170f);
                    stRect.sizeDelta = new Vector2(400f, 50f);
                    var stTmp = sTitleGo.GetComponent<TextMeshProUGUI>();
                    stTmp.text = "SETTINGS";
                    stTmp.alignment = TextAlignmentOptions.Center;
                    if (fontDisplay != null) stTmp.font = fontDisplay;
                    stTmp.fontSize = 44f;
                    stTmp.color = accentGreen;

                    var sBackGo = new GameObject("BackButton", typeof(RectTransform), typeof(Image), typeof(Button));
                    sBackGo.transform.SetParent(settingsSubTr, false);
                    var sbRect = sBackGo.GetComponent<RectTransform>();
                    sbRect.anchoredPosition = new Vector2(0f, -170f);
                    sbRect.sizeDelta = new Vector2(300f, 50f);
                    FormatMenuButton(sBackGo.transform, "BACK", fontUI, textWhite);

                    settingsSubTr.gameObject.SetActive(false);
                }

                var howToPlaySubTr = pausePanelTr.Find("HowToPlaySubpanel");
                if (howToPlaySubTr == null)
                {
                    var subGo = new GameObject("HowToPlaySubpanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(MenuPanelTransition));
                    subGo.transform.SetParent(pausePanelTr, false);
                    howToPlaySubTr = subGo.transform;

                    var subRect = howToPlaySubTr.GetComponent<RectTransform>();
                    subRect.anchorMin = new Vector2(0.5f, 0.5f);
                    subRect.anchorMax = new Vector2(0.5f, 0.5f);
                    subRect.pivot = new Vector2(0.5f, 0.5f);
                    subRect.sizeDelta = new Vector2(560f, 460f);

                    var subImg = howToPlaySubTr.GetComponent<Image>();
                    subImg.color = charcoalDark;

                    var hTitleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
                    hTitleGo.transform.SetParent(howToPlaySubTr, false);
                    var htRect = hTitleGo.GetComponent<RectTransform>();
                    htRect.anchoredPosition = new Vector2(0f, 170f);
                    htRect.sizeDelta = new Vector2(400f, 50f);
                    var htTmp = hTitleGo.GetComponent<TextMeshProUGUI>();
                    htTmp.text = "HOW TO PLAY";
                    htTmp.alignment = TextAlignmentOptions.Center;
                    if (fontDisplay != null) htTmp.font = fontDisplay;
                    htTmp.fontSize = 44f;
                    htTmp.color = accentGreen;

                    var hBodyGo = new GameObject("BodyText", typeof(RectTransform), typeof(TextMeshProUGUI));
                    hBodyGo.transform.SetParent(howToPlaySubTr, false);
                    var hbRect = hBodyGo.GetComponent<RectTransform>();
                    hbRect.anchoredPosition = new Vector2(0f, 20f);
                    hbRect.sizeDelta = new Vector2(500f, 220f);
                    var hbTmp = hBodyGo.GetComponent<TextMeshProUGUI>();
                    hbTmp.text = "<b>SKILL LOADOUT</b>\n\n" +
                                 "<b>[Q] Phase Shift:</b> Enter ethereal dimension, invincible to balls.\n" +
                                 "<b>[Shift] Dash:</b> High-speed directional burst.\n" +
                                 "<b>[E] Slow Motion:</b> Dilates arena time. Charge meter via dodges!\n\n" +
                                 "Survive as long as possible and dodge balls to rack up high scores!";
                    hbTmp.alignment = TextAlignmentOptions.Center;
                    if (fontBody != null) hbTmp.font = fontBody;
                    hbTmp.fontSize = 20f;
                    hbTmp.color = textWhite;

                    var hBackGo = new GameObject("BackButton", typeof(RectTransform), typeof(Image), typeof(Button));
                    hBackGo.transform.SetParent(howToPlaySubTr, false);
                    var hbBtnRect = hBackGo.GetComponent<RectTransform>();
                    hbBtnRect.anchoredPosition = new Vector2(0f, -170f);
                    hbBtnRect.sizeDelta = new Vector2(300f, 50f);
                    FormatMenuButton(hBackGo.transform, "BACK", fontUI, textWhite);

                    howToPlaySubTr.gameObject.SetActive(false);
                }

                var pauseSo = new SerializedObject(pauseUI);
                pauseSo.FindProperty("pausePanel").objectReferenceValue = pausePanelTr.gameObject;
                pauseSo.FindProperty("pauseTransition").objectReferenceValue = pauseTransition;
                pauseSo.FindProperty("titleText").objectReferenceValue = titleTr != null ? titleTr.GetComponent<TextMeshProUGUI>() : null;
                pauseSo.FindProperty("scoreText").objectReferenceValue = scoreReadoutTr != null ? scoreReadoutTr.GetComponent<TextMeshProUGUI>() : null;
                pauseSo.FindProperty("resumeButton").objectReferenceValue = resumeBtnTr != null ? resumeBtnTr.GetComponent<Button>() : null;
                pauseSo.FindProperty("settingsButton").objectReferenceValue = settingsBtnTr != null ? settingsBtnTr.GetComponent<Button>() : null;
                pauseSo.FindProperty("howToPlayButton").objectReferenceValue = howToPlayBtnTr != null ? howToPlayBtnTr.GetComponent<Button>() : null;
                pauseSo.FindProperty("quitButton").objectReferenceValue = quitBtnTr != null ? quitBtnTr.GetComponent<Button>() : null;

                pauseSo.FindProperty("settingsTransition").objectReferenceValue = settingsSubTr.GetComponent<MenuPanelTransition>();
                var sBackBtn = settingsSubTr.Find("BackButton");
                pauseSo.FindProperty("closeSettingsButton").objectReferenceValue = sBackBtn != null ? sBackBtn.GetComponent<Button>() : null;

                pauseSo.FindProperty("howToPlayTransition").objectReferenceValue = howToPlaySubTr.GetComponent<MenuPanelTransition>();
                var hBackBtn = howToPlaySubTr.Find("BackButton");
                pauseSo.FindProperty("closeHowToPlayButton").objectReferenceValue = hBackBtn != null ? hBackBtn.GetComponent<Button>() : null;

                pauseSo.FindProperty("mainPauseColumn").objectReferenceValue = mainColTr.GetComponent<CanvasGroup>();
                pauseSo.ApplyModifiedProperties();

                pausePanelTr.gameObject.SetActive(false);
            }

            var gameOverPanelTr = canvasTr.Find("GameOverPanel");
            if (gameOverPanelTr != null)
            {
                var goRect = gameOverPanelTr.GetComponent<RectTransform>();
                goRect.anchorMin = Vector2.zero;
                goRect.anchorMax = Vector2.one;
                goRect.sizeDelta = Vector2.zero;
                goRect.anchoredPosition = Vector2.zero;

                var goBg = gameOverPanelTr.GetComponent<Image>();
                if (goBg != null)
                {
                    goBg.color = new Color(0.035f, 0.055f, 0.045f, 0.94f);
                }

                var goCg = gameOverPanelTr.GetComponent<CanvasGroup>();
                if (goCg == null)
                {
                    goCg = gameOverPanelTr.gameObject.AddComponent<CanvasGroup>();
                }

                var gameOverUI = canvasObj.GetComponent<GameOverUI>();
                if (gameOverUI == null)
                {
                    gameOverUI = canvasObj.AddComponent<GameOverUI>();
                }
                var oldGoOnPanel = gameOverPanelTr.GetComponent<GameOverUI>();
                if (oldGoOnPanel != null)
                {
                    UnityEngine.Object.DestroyImmediate(oldGoOnPanel);
                }

                var goColTr = gameOverPanelTr.Find("GameOverColumn");
                if (goColTr == null)
                {
                    var colGo = new GameObject("GameOverColumn", typeof(RectTransform), typeof(VerticalLayoutGroup));
                    colGo.transform.SetParent(gameOverPanelTr, false);
                    goColTr = colGo.transform;
                }

                var colRect = goColTr.GetComponent<RectTransform>();
                colRect.anchorMin = new Vector2(0.5f, 0.5f);
                colRect.anchorMax = new Vector2(0.5f, 0.5f);
                colRect.pivot = new Vector2(0.5f, 0.5f);
                colRect.anchoredPosition = Vector2.zero;
                colRect.sizeDelta = new Vector2(500f, 560f);

                var colLayout = goColTr.GetComponent<VerticalLayoutGroup>();
                colLayout.childAlignment = TextAnchor.MiddleCenter;
                colLayout.spacing = 10f;
                colLayout.childControlWidth = true;
                colLayout.childControlHeight = false;
                colLayout.childForceExpandWidth = true;
                colLayout.childForceExpandHeight = false;

                var titleTr = gameOverPanelTr.Find("GameOverTitle") ?? goColTr.Find("GameOverTitle");
                if (titleTr != null)
                {
                    titleTr.SetParent(goColTr, false);
                    var tmp = titleTr.GetComponent<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        tmp.text = "GAME OVER";
                        tmp.alignment = TextAlignmentOptions.Center;
                        if (fontDisplay != null) tmp.font = fontDisplay;
                        tmp.fontSize = 58f;
                        tmp.color = new Color(1f, 0.35f, 0.35f, 1f);
                    }
                    (titleTr as RectTransform).sizeDelta = new Vector2(500f, 70f);
                }

                var reasonTr = gameOverPanelTr.Find("ReasonText") ?? goColTr.Find("ReasonText");
                if (reasonTr == null)
                {
                    var reasonGo = new GameObject("ReasonText", typeof(RectTransform), typeof(TextMeshProUGUI));
                    reasonGo.transform.SetParent(goColTr, false);
                    reasonTr = reasonGo.transform;
                }
                else
                {
                    reasonTr.SetParent(goColTr, false);
                }
                var rTmp = reasonTr.GetComponent<TextMeshProUGUI>();
                rTmp.text = "ELIMINATED BY HAZARD";
                rTmp.alignment = TextAlignmentOptions.Center;
                if (fontBody != null) rTmp.font = fontBody;
                rTmp.fontSize = 22f;
                rTmp.color = steelGrey;
                (reasonTr as RectTransform).sizeDelta = new Vector2(500f, 32f);

                var scoreHeaderTr = goColTr.Find("FinalScoreHeader");
                if (scoreHeaderTr == null)
                {
                    var shGo = new GameObject("FinalScoreHeader", typeof(RectTransform), typeof(TextMeshProUGUI));
                    shGo.transform.SetParent(goColTr, false);
                    scoreHeaderTr = shGo.transform;
                }
                var shTmp = scoreHeaderTr.GetComponent<TextMeshProUGUI>();
                shTmp.text = "FINAL SCORE";
                shTmp.alignment = TextAlignmentOptions.Center;
                if (fontUI != null) shTmp.font = fontUI;
                shTmp.fontSize = 20f;
                shTmp.color = textWhite;
                (scoreHeaderTr as RectTransform).sizeDelta = new Vector2(500f, 26f);

                var scoreTr = gameOverPanelTr.Find("FinalScoreText") ?? goColTr.Find("FinalScoreText");
                if (scoreTr != null)
                {
                    scoreTr.SetParent(goColTr, false);
                    var tmp = scoreTr.GetComponent<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        tmp.text = "0";
                        tmp.alignment = TextAlignmentOptions.Center;
                        if (fontDisplay != null) tmp.font = fontDisplay;
                        tmp.fontSize = 80f;
                        tmp.color = accentGreen;
                    }
                    (scoreTr as RectTransform).sizeDelta = new Vector2(500f, 90f);
                }

                var badgeTr = gameOverPanelTr.Find("NewRecordBadge") ?? goColTr.Find("NewRecordBadge");
                if (badgeTr == null)
                {
                    var badgeGo = new GameObject("NewRecordBadge", typeof(RectTransform), typeof(TextMeshProUGUI));
                    badgeGo.transform.SetParent(goColTr, false);
                    badgeTr = badgeGo.transform;
                }
                else
                {
                    badgeTr.SetParent(goColTr, false);
                }
                var bTmp = badgeTr.GetComponent<TextMeshProUGUI>();
                bTmp.text = "★ NEW BEST! ★";
                bTmp.alignment = TextAlignmentOptions.Center;
                if (fontUI != null) bTmp.font = fontUI;
                bTmp.fontSize = 26f;
                bTmp.color = goldRecord;
                (badgeTr as RectTransform).sizeDelta = new Vector2(500f, 36f);
                badgeTr.gameObject.SetActive(false);

                var bestTr = gameOverPanelTr.Find("HighScoreText") ?? goColTr.Find("HighScoreText");
                if (bestTr == null)
                {
                    var bestGo = new GameObject("HighScoreText", typeof(RectTransform), typeof(TextMeshProUGUI));
                    bestGo.transform.SetParent(goColTr, false);
                    bestTr = bestGo.transform;
                }
                else
                {
                    bestTr.SetParent(goColTr, false);
                }
                var bestTmp = bestTr.GetComponent<TextMeshProUGUI>();
                bestTmp.text = "BEST: 0";
                bestTmp.alignment = TextAlignmentOptions.Center;
                if (fontUI != null) bestTmp.font = fontUI;
                bestTmp.fontSize = 24f;
                bestTmp.color = steelGrey;
                (bestTr as RectTransform).sizeDelta = new Vector2(500f, 34f);

                var restartBtnTr = gameOverPanelTr.Find("RestartButton") ?? goColTr.Find("RestartButton");
                if (restartBtnTr != null)
                {
                    restartBtnTr.SetParent(goColTr, false);
                    FormatMenuButton(restartBtnTr, "PLAY AGAIN", fontUI, accentGreen);
                }

                var menuBtnTr = gameOverPanelTr.Find("MainMenuButton") ?? goColTr.Find("MainMenuButton");
                if (menuBtnTr != null)
                {
                    menuBtnTr.SetParent(goColTr, false);
                    FormatMenuButton(menuBtnTr, "MAIN MENU", fontUI, textWhite);
                }

                var goSo = new SerializedObject(gameOverUI);
                goSo.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanelTr.gameObject;
                goSo.FindProperty("panelCanvasGroup").objectReferenceValue = goCg;
                goSo.FindProperty("titleText").objectReferenceValue = titleTr != null ? titleTr.GetComponent<TextMeshProUGUI>() : null;
                goSo.FindProperty("finalScoreText").objectReferenceValue = scoreTr != null ? scoreTr.GetComponent<TextMeshProUGUI>() : null;
                goSo.FindProperty("highScoreText").objectReferenceValue = bestTr != null ? bestTr.GetComponent<TextMeshProUGUI>() : null;
                goSo.FindProperty("reasonText").objectReferenceValue = reasonTr != null ? reasonTr.GetComponent<TextMeshProUGUI>() : null;
                goSo.FindProperty("newRecordBadge").objectReferenceValue = badgeTr.gameObject;
                goSo.FindProperty("restartButton").objectReferenceValue = restartBtnTr != null ? restartBtnTr.GetComponent<Button>() : null;
                goSo.FindProperty("mainMenuButton").objectReferenceValue = menuBtnTr != null ? menuBtnTr.GetComponent<Button>() : null;
                goSo.FindProperty("gameManager").objectReferenceValue = GameObject.FindAnyObjectByType<GameManager>();
                goSo.ApplyModifiedProperties();

                gameOverPanelTr.gameObject.SetActive(false);
            }

            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
            Debug.Log("[Phase 8] MainGame scene hierarchy and HUD layout successfully updated!");
        }

        private static void FormatSkillSlot(Transform slotTr, Transform parentContainer, string defaultLabel, Color cardBg, Color accent, TMP_FontAsset font)
        {
            if (slotTr == null) return;
            slotTr.SetParent(parentContainer, false);

            var rect = slotTr.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(236f, 48f);

            var img = slotTr.GetComponent<Image>();
            if (img != null)
            {
                img.color = cardBg;
            }

            var textTr = slotTr.Find("Text") ?? slotTr.Find("StatusText");
            if (textTr != null)
            {
                var tmp = textTr.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.alignment = TextAlignmentOptions.Center;
                    if (font != null) tmp.font = font;
                    tmp.fontSize = 18f;
                    tmp.text = defaultLabel;
                    tmp.color = accent;
                }
                var tRect = textTr.GetComponent<RectTransform>();
                tRect.anchorMin = Vector2.zero;
                tRect.anchorMax = Vector2.one;
                tRect.sizeDelta = Vector2.zero;
                tRect.anchoredPosition = Vector2.zero;
            }
        }

        private static void FormatMenuButton(Transform btnTr, string labelText, TMP_FontAsset font, Color textColor)
        {
            if (btnTr == null) return;

            var rect = btnTr.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(360f, 52f);

            var img = btnTr.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color(0.11f, 0.14f, 0.17f, 0.95f);
            }

            var btnState = btnTr.GetComponent<ButtonStateFX>();
            if (btnState == null)
            {
                btnState = btnTr.gameObject.AddComponent<ButtonStateFX>();
            }

            var textTr = btnTr.Find("Text");
            if (textTr == null)
            {
                var textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                textGo.transform.SetParent(btnTr, false);
                textTr = textGo.transform;
            }

            var tRect = textTr.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.sizeDelta = Vector2.zero;
            tRect.anchoredPosition = Vector2.zero;

            var tmp = textTr.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = labelText;
                tmp.alignment = TextAlignmentOptions.Center;
                if (font != null) tmp.font = font;
                tmp.fontSize = 24f;
                tmp.color = textColor;
            }
        }

        public static void ExecuteNamingPass()
        {
            RenameAssetInFolder("Assets/Prefabs", ".prefab", "Prefab_");

            RenameAssetInFolder("Assets/Materials", ".mat", "Mat_", "M_");

            RenameAssetInFolder("Assets/ScriptableObjects", ".asset", "SO_", "Default");
            RenameAssetInFolder("Assets/Resources", ".asset", "SO_", "Default");
        }

        private static void RenameAssetInFolder(string folderPath, string extension, string targetPrefix, string stripPrefix = null)
        {
            if (!Directory.Exists(folderPath)) return;

            string[] files = Directory.GetFiles(folderPath, "*" + extension);
            foreach (var file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                if (fileName.StartsWith(targetPrefix)) continue;

                string cleanName = fileName;
                if (!string.IsNullOrEmpty(stripPrefix) && cleanName.StartsWith(stripPrefix))
                {
                    cleanName = cleanName.Substring(stripPrefix.Length);
                }

                string newName = targetPrefix + cleanName;
                string assetPath = file.Replace('\\', '/');
                string result = AssetDatabase.RenameAsset(assetPath, newName);
                if (string.IsNullOrEmpty(result))
                {
                    Debug.Log($"[NamingPass] Renamed: {fileName} -> {newName}");
                }
                else
                {
                    Debug.LogWarning($"[NamingPass] Failed to rename {fileName}: {result}");
                }
            }
        }
    }
}
