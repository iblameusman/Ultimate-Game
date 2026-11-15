using System;
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
    public static class MainMenuRefinementEditor
    {
        [MenuItem("Tools/Refine MainMenu Scene Phase 3")]
        public static void RefineMainMenuScene()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path != "Assets/Scenes/MainMenu.unity")
            {
                activeScene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
            }

            var menuConfig = AssetDatabase.LoadAssetAtPath<MenuConfig>("Assets/ScriptableObjects/DefaultMenuConfig.asset");
            var typographyConfig = AssetDatabase.LoadAssetAtPath<TypographyConfig>("Assets/ScriptableObjects/DefaultTypography.asset");
            var defaultSettings = AssetDatabase.LoadAssetAtPath<SettingsData>("Assets/ScriptableObjects/DefaultSettings.asset");

            Color charcoal = menuConfig != null ? menuConfig.Charcoal : new Color(0.055f, 0.078f, 0.094f, 1f);
            Color accentGreen = menuConfig != null ? menuConfig.AccentGreen : new Color(0.13f, 0.77f, 0.45f, 1f);
            Color steelGrey = menuConfig != null ? menuConfig.SteelGrey : new Color(0.28f, 0.33f, 0.41f, 1f);
            Color textNavy = menuConfig != null ? menuConfig.TextNavy : new Color(0.06f, 0.09f, 0.16f, 1f);
            Color lightText = menuConfig != null ? menuConfig.LightText : new Color(0.94f, 0.96f, 0.98f, 1f);

            var systemsObj = GameObject.Find("[Systems]");
            if (systemsObj == null)
            {
                systemsObj = new GameObject("[Systems]");
            }
            var settingsService = systemsObj.GetComponent<DodgeGame.Core.SettingsService>();
            if (settingsService == null)
            {
                settingsService = systemsObj.AddComponent<DodgeGame.Core.SettingsService>();
            }
            var settingsSo = new SerializedObject(settingsService);
            settingsSo.FindProperty("defaultSettings").objectReferenceValue = defaultSettings;
            settingsSo.ApplyModifiedProperties();

            var canvasObj = GameObject.Find("Canvas");
            if (canvasObj == null)
            {
                Debug.LogError("[MainMenuRefinement] Canvas GameObject not found!");
                return;
            }
            var mainMenuUI = canvasObj.GetComponent<MainMenuUI>();
            if (mainMenuUI == null)
            {
                mainMenuUI = canvasObj.AddComponent<MainMenuUI>();
            }

            var sideMenuTransform = canvasObj.transform.Find("SideMenuPanel");
            if (sideMenuTransform == null)
            {
                Debug.LogError("[MainMenuRefinement] SideMenuPanel not found!");
                return;
            }
            var sideMenuCanvasGroup = sideMenuTransform.GetComponent<CanvasGroup>();
            if (sideMenuCanvasGroup == null)
            {
                sideMenuCanvasGroup = sideMenuTransform.gameObject.AddComponent<CanvasGroup>();
            }

            var muteBtnTrans = sideMenuTransform.Find("MuteButton");
            if (muteBtnTrans != null)
            {
                muteBtnTrans.name = "SettingsButton";
                var txt = muteBtnTrans.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null)
                {
                    txt.text = "SETTINGS";
                }
            }

            var playBtnTrans = sideMenuTransform.Find("PlayButton");
            var howBtnTrans = sideMenuTransform.Find("HowToPlayButton");
            var settingsBtnTrans = sideMenuTransform.Find("SettingsButton");
            var quitBtnTrans = sideMenuTransform.Find("QuitButton");

            Button playBtn = playBtnTrans != null ? playBtnTrans.GetComponent<Button>() : null;
            Button howBtn = howBtnTrans != null ? howBtnTrans.GetComponent<Button>() : null;
            Button settingsBtn = settingsBtnTrans != null ? settingsBtnTrans.GetComponent<Button>() : null;
            Button quitBtn = quitBtnTrans != null ? quitBtnTrans.GetComponent<Button>() : null;

            ConfigureMenuButton(playBtnTrans, charcoal, accentGreen, steelGrey, typographyConfig);
            ConfigureMenuButton(howBtnTrans, charcoal, accentGreen, steelGrey, typographyConfig);
            ConfigureMenuButton(settingsBtnTrans, charcoal, accentGreen, steelGrey, typographyConfig);
            ConfigureMenuButton(quitBtnTrans, charcoal, accentGreen, steelGrey, typographyConfig);

            if (playBtn != null && howBtn != null && settingsBtn != null && quitBtn != null)
            {
                var navPlay = playBtn.navigation;
                navPlay.mode = Navigation.Mode.Explicit;
                navPlay.selectOnDown = howBtn;
                playBtn.navigation = navPlay;

                var navHow = howBtn.navigation;
                navHow.mode = Navigation.Mode.Explicit;
                navHow.selectOnUp = playBtn;
                navHow.selectOnDown = settingsBtn;
                howBtn.navigation = navHow;

                var navSet = settingsBtn.navigation;
                navSet.mode = Navigation.Mode.Explicit;
                navSet.selectOnUp = howBtn;
                navSet.selectOnDown = quitBtn;
                settingsBtn.navigation = navSet;

                var navQuit = quitBtn.navigation;
                navQuit.mode = Navigation.Mode.Explicit;
                navQuit.selectOnUp = settingsBtn;
                quitBtn.navigation = navQuit;
            }

            var eventSystemObj = GameObject.Find("EventSystem");
            if (eventSystemObj != null && playBtn != null)
            {
                var esComp = eventSystemObj.GetComponent<UnityEngine.EventSystems.EventSystem>();
                if (esComp != null)
                {
                    var esSo = new SerializedObject(esComp);
                    esSo.FindProperty("m_FirstSelected").objectReferenceValue = playBtn.gameObject;
                    esSo.ApplyModifiedProperties();
                }
            }

            var hintsTransform = sideMenuTransform.Find("InputHintsBar");
            GameObject hintsObj;
            if (hintsTransform == null)
            {
                hintsObj = new GameObject("InputHintsBar");
                hintsObj.transform.SetParent(sideMenuTransform, false);
            }
            else
            {
                hintsObj = hintsTransform.gameObject;
            }

            var hintsRect = hintsObj.GetComponent<RectTransform>();
            if (hintsRect == null) hintsRect = hintsObj.AddComponent<RectTransform>();
            hintsRect.anchorMin = new Vector2(0f, 0f);
            hintsRect.anchorMax = new Vector2(1f, 0f);
            hintsRect.offsetMin = new Vector2(20f, 50f);
            hintsRect.offsetMax = new Vector2(-20f, 80f);

            var hintsTxt = hintsObj.GetComponent<TextMeshProUGUI>();
            if (hintsTxt == null) hintsTxt = hintsObj.AddComponent<TextMeshProUGUI>();
            hintsTxt.text = "[Enter] Select    [Esc] Back";
            hintsTxt.alignment = TextAlignmentOptions.Center;

            var hintsStyler = hintsObj.GetComponent<TextStyler>();
            if (hintsStyler == null) hintsStyler = hintsObj.AddComponent<TextStyler>();
            hintsStyler.Config = typographyConfig;
            hintsStyler.Role = TypographyRole.Caption;
            hintsStyler.ApplyStyle();

            var hintsBar = hintsObj.GetComponent<InputHintsBar>();
            if (hintsBar == null) hintsBar = hintsObj.AddComponent<InputHintsBar>();
            var hintsSo = new SerializedObject(hintsBar);
            hintsSo.FindProperty("hintText").objectReferenceValue = hintsTxt;
            hintsSo.ApplyModifiedProperties();

            var howPanelTrans = canvasObj.transform.Find("HowToPlayPanel");
            GameObject howPanelObj;
            if (howPanelTrans == null)
            {
                howPanelObj = new GameObject("HowToPlayPanel");
                howPanelObj.transform.SetParent(canvasObj.transform, false);
            }
            else
            {
                howPanelObj = howPanelTrans.gameObject;
            }
            howPanelObj.SetActive(true);

            var howRect = howPanelObj.GetComponent<RectTransform>();
            if (howRect == null) howRect = howPanelObj.AddComponent<RectTransform>();
            howRect.anchorMin = Vector2.zero;
            howRect.anchorMax = Vector2.one;
            howRect.offsetMin = Vector2.zero;
            howRect.offsetMax = Vector2.zero;

            var howBg = howPanelObj.GetComponent<Image>();
            if (howBg == null) howBg = howPanelObj.AddComponent<Image>();
            howBg.color = new Color(0.02f, 0.04f, 0.06f, 0.85f);

            var howCg = howPanelObj.GetComponent<CanvasGroup>();
            if (howCg == null) howCg = howPanelObj.AddComponent<CanvasGroup>();

            var howCardTrans = howPanelObj.transform.Find("ModalCard");
            GameObject howCardObj;
            if (howCardTrans == null)
            {
                howCardObj = new GameObject("ModalCard");
                howCardObj.transform.SetParent(howPanelObj.transform, false);
            }
            else
            {
                howCardObj = howCardTrans.gameObject;
            }
            var howCardRect = howCardObj.GetComponent<RectTransform>();
            if (howCardRect == null) howCardRect = howCardObj.AddComponent<RectTransform>();
            howCardRect.anchorMin = new Vector2(0.5f, 0.5f);
            howCardRect.anchorMax = new Vector2(0.5f, 0.5f);
            howCardRect.anchoredPosition = Vector2.zero;
            howCardRect.sizeDelta = new Vector2(880f, 580f);

            var howCardImg = howCardObj.GetComponent<Image>();
            if (howCardImg == null) howCardImg = howCardObj.AddComponent<Image>();
            howCardImg.color = charcoal;

            var howTransition = howPanelObj.GetComponent<MenuPanelTransition>();
            if (howTransition == null) howTransition = howPanelObj.AddComponent<MenuPanelTransition>();

            var howTransitionSo = new SerializedObject(howTransition);
            howTransitionSo.FindProperty("panelCanvasGroup").objectReferenceValue = howCg;
            howTransitionSo.FindProperty("panelRectTransform").objectReferenceValue = howCardRect;
            howTransitionSo.FindProperty("mainColumnCanvasGroup").objectReferenceValue = sideMenuCanvasGroup;
            howTransitionSo.FindProperty("slideDistance").floatValue = 24f;
            howTransitionSo.FindProperty("openFadeDuration").floatValue = 0.2f;
            howTransitionSo.FindProperty("closeFadeDuration").floatValue = 0.15f;
            howTransitionSo.FindProperty("mainColumnDimOpacity").floatValue = 0.6f;
            howTransitionSo.ApplyModifiedProperties();

            for (int i = howCardObj.transform.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(howCardObj.transform.GetChild(i).gameObject);
            }

            var howTitleObj = CreateTextElement(howCardObj.transform, "ModalTitle", "HOW TO PLAY",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(700f, 50f),
                TypographyRole.Heading, TextAlignmentOptions.Center, typographyConfig);

            var hazardsColumn = new GameObject("HazardsColumn");
            hazardsColumn.transform.SetParent(howCardObj.transform, false);
            var hazardsRect = hazardsColumn.AddComponent<RectTransform>();
            hazardsRect.anchorMin = new Vector2(0f, 1f);
            hazardsRect.anchorMax = new Vector2(0f, 1f);
            hazardsRect.anchoredPosition = new Vector2(230f, -270f);
            hazardsRect.sizeDelta = new Vector2(400f, 400f);

            var hazardsHeader = CreateTextElement(hazardsColumn.transform, "HazardsHeader", "ARENA HAZARDS",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 0f), new Vector2(400f, 30f),
                TypographyRole.Button, TextAlignmentOptions.Left, typographyConfig);
            hazardsHeader.color = accentGreen;

            Color fireColor = menuConfig != null ? menuConfig.FireBallColor : new Color(1f, 0.34f, 0.13f, 1f);
            Color stickyColor = menuConfig != null ? menuConfig.StickyBallColor : new Color(0f, 0.9f, 1f, 1f);
            Color gunColor = menuConfig != null ? menuConfig.GunBallColor : new Color(0.62f, 0.65f, 0.71f, 1f);

            CreateBallCard(hazardsColumn.transform, "FireBallCard", "FIRE BALL", "FIRE", fireColor,
                "Ignites arena tiles on impact with lingering hazard flames.", -40f, textNavy, steelGrey, typographyConfig);

            CreateBallCard(hazardsColumn.transform, "StickyBallCard", "STICKY BALL", "SLOW", stickyColor,
                "Leaves goo pools that reduce player movement speed by 50%.", -125f, textNavy, steelGrey, typographyConfig);

            CreateBallCard(hazardsColumn.transform, "GunBallCard", "GUN BALL", "SNIPER", gunColor,
                "Fires high-speed precision sniper projectiles at the player.", -210f, textNavy, steelGrey, typographyConfig);

            var skillsColumn = new GameObject("SkillsColumn");
            skillsColumn.transform.SetParent(howCardObj.transform, false);
            var skillsRect = skillsColumn.AddComponent<RectTransform>();
            skillsRect.anchorMin = new Vector2(1f, 1f);
            skillsRect.anchorMax = new Vector2(1f, 1f);
            skillsRect.anchoredPosition = new Vector2(-230f, -270f);
            skillsRect.sizeDelta = new Vector2(400f, 400f);

            var skillsHeader = CreateTextElement(skillsColumn.transform, "SkillsHeader", "SURVIVAL SKILLS",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 0f), new Vector2(400f, 30f),
                TypographyRole.Button, TextAlignmentOptions.Left, typographyConfig);
            skillsHeader.color = accentGreen;

            CreateSkillCard(skillsColumn.transform, "PhaseCard", "PHASE ABILITY", "[Q] / [LB]",
                "Shifts into ethereal dimension, gaining total invulnerability to balls and hazards.", -45f, textNavy, steelGrey, accentGreen, typographyConfig);

            CreateSkillCard(skillsColumn.transform, "DashCard", "DASH", "[SHIFT] / [B]",
                "A short, fast burst of speed in your movement direction. Cannot dash during Phase.", -150f, textNavy, steelGrey, accentGreen, typographyConfig);

            CreateSkillCard(skillsColumn.transform, "SlowMotionCard", "SLOW MOTION", "[E] / [Y]",
                "Slows hazards and projectiles while your movement stays at normal responsive speed.", -255f, textNavy, steelGrey, accentGreen, typographyConfig);

            var howCloseBtn = CreateActionBtn(howCardObj.transform, "CloseButton", "BACK",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(180f, 48f),
                charcoal, accentGreen, steelGrey, typographyConfig);

            howPanelObj.SetActive(false);

            var settingsPanelTrans = canvasObj.transform.Find("SettingsPanel");
            GameObject settingsPanelObj;
            if (settingsPanelTrans == null)
            {
                settingsPanelObj = new GameObject("SettingsPanel");
                settingsPanelObj.transform.SetParent(canvasObj.transform, false);
            }
            else
            {
                settingsPanelObj = settingsPanelTrans.gameObject;
            }
            settingsPanelObj.SetActive(true);

            var settingsRect = settingsPanelObj.GetComponent<RectTransform>();
            if (settingsRect == null) settingsRect = settingsPanelObj.AddComponent<RectTransform>();
            settingsRect.anchorMin = Vector2.zero;
            settingsRect.anchorMax = Vector2.one;
            settingsRect.offsetMin = Vector2.zero;
            settingsRect.offsetMax = Vector2.zero;

            var settingsBg = settingsPanelObj.GetComponent<Image>();
            if (settingsBg == null) settingsBg = settingsPanelObj.AddComponent<Image>();
            settingsBg.color = new Color(0.02f, 0.04f, 0.06f, 0.85f);

            var settingsCg = settingsPanelObj.GetComponent<CanvasGroup>();
            if (settingsCg == null) settingsCg = settingsPanelObj.AddComponent<CanvasGroup>();

            var settingsCardTrans = settingsPanelObj.transform.Find("ModalCard");
            GameObject settingsCardObj;
            if (settingsCardTrans == null)
            {
                settingsCardObj = new GameObject("ModalCard");
                settingsCardObj.transform.SetParent(settingsPanelObj.transform, false);
            }
            else
            {
                settingsCardObj = settingsCardTrans.gameObject;
            }
            var settingsCardRect = settingsCardObj.GetComponent<RectTransform>();
            if (settingsCardRect == null) settingsCardRect = settingsCardObj.AddComponent<RectTransform>();
            settingsCardRect.anchorMin = new Vector2(0.5f, 0.5f);
            settingsCardRect.anchorMax = new Vector2(0.5f, 0.5f);
            settingsCardRect.anchoredPosition = Vector2.zero;
            settingsCardRect.sizeDelta = new Vector2(740f, 580f);

            var settingsCardImg = settingsCardObj.GetComponent<Image>();
            if (settingsCardImg == null) settingsCardImg = settingsCardObj.AddComponent<Image>();
            settingsCardImg.color = charcoal;

            var settingsTransition = settingsPanelObj.GetComponent<MenuPanelTransition>();
            if (settingsTransition == null) settingsTransition = settingsPanelObj.AddComponent<MenuPanelTransition>();

            var settingsTransitionSo = new SerializedObject(settingsTransition);
            settingsTransitionSo.FindProperty("panelCanvasGroup").objectReferenceValue = settingsCg;
            settingsTransitionSo.FindProperty("panelRectTransform").objectReferenceValue = settingsCardRect;
            settingsTransitionSo.FindProperty("mainColumnCanvasGroup").objectReferenceValue = sideMenuCanvasGroup;
            settingsTransitionSo.FindProperty("slideDistance").floatValue = 24f;
            settingsTransitionSo.FindProperty("openFadeDuration").floatValue = 0.2f;
            settingsTransitionSo.FindProperty("closeFadeDuration").floatValue = 0.15f;
            settingsTransitionSo.FindProperty("mainColumnDimOpacity").floatValue = 0.6f;
            settingsTransitionSo.ApplyModifiedProperties();

            for (int i = settingsCardObj.transform.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(settingsCardObj.transform.GetChild(i).gameObject);
            }

            CreateTextElement(settingsCardObj.transform, "SettingsTitle", "SETTINGS",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(600f, 50f),
                TypographyRole.Heading, TextAlignmentOptions.Center, typographyConfig);

            var rowsContainer = new GameObject("RowsContainer");
            rowsContainer.transform.SetParent(settingsCardObj.transform, false);
            var rowsRect = rowsContainer.AddComponent<RectTransform>();
            rowsRect.anchorMin = new Vector2(0.5f, 1f);
            rowsRect.anchorMax = new Vector2(0.5f, 1f);
            rowsRect.anchoredPosition = new Vector2(0f, -250f);
            rowsRect.sizeDelta = new Vector2(640f, 360f);

            var masterSlider = CreateSliderRow(rowsContainer.transform, "MasterVolumeRow", "MASTER VOLUME", 0f, 1f, 1f, 0f, textNavy, steelGrey, accentGreen, typographyConfig);
            var musicSlider = CreateSliderRow(rowsContainer.transform, "MusicVolumeRow", "MUSIC VOLUME", 0f, 1f, 1f, -55f, textNavy, steelGrey, accentGreen, typographyConfig);
            var sfxSlider = CreateSliderRow(rowsContainer.transform, "SfxVolumeRow", "SFX VOLUME", 0f, 1f, 1f, -110f, textNavy, steelGrey, accentGreen, typographyConfig);
            var brightnessSlider = CreateSliderRow(rowsContainer.transform, "BrightnessRow", "BRIGHTNESS", 0.8f, 1.2f, 1f, -165f, textNavy, steelGrey, accentGreen, typographyConfig);

            var shakeToggle = CreateToggleRow(rowsContainer.transform, "ScreenShakeToggle", "SCREEN SHAKE", true, new Vector2(-160f, -230f), textNavy, steelGrey, accentGreen, typographyConfig);
            var motionToggle = CreateToggleRow(rowsContainer.transform, "ReducedMotionToggle", "REDUCED MOTION", false, new Vector2(160f, -230f), textNavy, steelGrey, accentGreen, typographyConfig);

            var resetBtn = CreateActionBtn(settingsCardObj.transform, "ResetButton", "RESET TO DEFAULTS",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-120f, 42f), new Vector2(240f, 46f),
                charcoal, accentGreen, steelGrey, typographyConfig);

            var settingsBackBtn = CreateActionBtn(settingsCardObj.transform, "BackButton", "BACK",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(160f, 42f), new Vector2(160f, 46f),
                charcoal, accentGreen, steelGrey, typographyConfig);

            var menuSettingsUI = settingsPanelObj.GetComponent<MenuSettingsUI>();
            if (menuSettingsUI == null) menuSettingsUI = settingsPanelObj.AddComponent<MenuSettingsUI>();

            var menuSettingsSo = new SerializedObject(menuSettingsUI);
            menuSettingsSo.FindProperty("masterSlider").objectReferenceValue = masterSlider;
            menuSettingsSo.FindProperty("musicSlider").objectReferenceValue = musicSlider;
            menuSettingsSo.FindProperty("sfxSlider").objectReferenceValue = sfxSlider;
            menuSettingsSo.FindProperty("brightnessSlider").objectReferenceValue = brightnessSlider;
            menuSettingsSo.FindProperty("screenShakeToggle").objectReferenceValue = shakeToggle;
            menuSettingsSo.FindProperty("reducedMotionToggle").objectReferenceValue = motionToggle;
            menuSettingsSo.FindProperty("resetButton").objectReferenceValue = resetBtn;
            menuSettingsSo.FindProperty("backButton").objectReferenceValue = settingsBackBtn;
            menuSettingsSo.ApplyModifiedProperties();

            settingsPanelObj.SetActive(false);

            var fadeTrans = canvasObj.transform.Find("ScreenFadeOverlay");
            GameObject fadeObj;
            if (fadeTrans == null)
            {
                fadeObj = new GameObject("ScreenFadeOverlay");
                fadeObj.transform.SetParent(canvasObj.transform, false);
            }
            else
            {
                fadeObj = fadeTrans.gameObject;
            }

            var fadeRect = fadeObj.GetComponent<RectTransform>();
            if (fadeRect == null) fadeRect = fadeObj.AddComponent<RectTransform>();
            fadeRect.anchorMin = Vector2.zero;
            fadeRect.anchorMax = Vector2.one;
            fadeRect.offsetMin = Vector2.zero;
            fadeRect.offsetMax = Vector2.zero;

            var fadeImg = fadeObj.GetComponent<Image>();
            if (fadeImg == null) fadeImg = fadeObj.AddComponent<Image>();
            fadeImg.color = Color.black;

            var fadeCg = fadeObj.GetComponent<CanvasGroup>();
            if (fadeCg == null) fadeCg = fadeObj.AddComponent<CanvasGroup>();
            fadeCg.alpha = 0f;

            var loadingTrans = fadeObj.transform.Find("LoadingIndicator");
            GameObject loadingObj;
            if (loadingTrans == null)
            {
                loadingObj = new GameObject("LoadingIndicator");
                loadingObj.transform.SetParent(fadeObj.transform, false);
            }
            else
            {
                loadingObj = loadingTrans.gameObject;
            }

            var loadingRect = loadingObj.GetComponent<RectTransform>();
            if (loadingRect == null) loadingRect = loadingObj.AddComponent<RectTransform>();
            loadingRect.anchorMin = new Vector2(0.5f, 0.5f);
            loadingRect.anchorMax = new Vector2(0.5f, 0.5f);
            loadingRect.anchoredPosition = Vector2.zero;
            loadingRect.sizeDelta = new Vector2(300f, 60f);

            var loadingTxt = loadingObj.GetComponent<TextMeshProUGUI>();
            if (loadingTxt == null) loadingTxt = loadingObj.AddComponent<TextMeshProUGUI>();
            loadingTxt.text = "LOADING...";
            loadingTxt.alignment = TextAlignmentOptions.Center;

            var loadingStyler = loadingObj.GetComponent<TextStyler>();
            if (loadingStyler == null) loadingStyler = loadingObj.AddComponent<TextStyler>();
            loadingStyler.Config = typographyConfig;
            loadingStyler.Role = TypographyRole.Heading;
            loadingStyler.ApplyStyle();

            loadingObj.SetActive(false);
            fadeObj.SetActive(false);

            var mainUIso = new SerializedObject(mainMenuUI);
            mainUIso.FindProperty("playButton").objectReferenceValue = playBtn;
            mainUIso.FindProperty("howToPlayButton").objectReferenceValue = howBtn;
            mainUIso.FindProperty("settingsButton").objectReferenceValue = settingsBtn;
            mainUIso.FindProperty("quitButton").objectReferenceValue = quitBtn;
            var highScoreTrans = sideMenuTransform.Find("ScoreCard/HighScoreText");
            if (highScoreTrans != null)
            {
                mainUIso.FindProperty("highScoreText").objectReferenceValue = highScoreTrans.GetComponent<TextMeshProUGUI>();
            }
            mainUIso.FindProperty("howToPlayTransition").objectReferenceValue = howTransition;
            mainUIso.FindProperty("closeHowToPlayButton").objectReferenceValue = howCloseBtn;
            mainUIso.FindProperty("settingsTransition").objectReferenceValue = settingsTransition;
            mainUIso.FindProperty("settingsPanelUI").objectReferenceValue = menuSettingsUI;
            mainUIso.FindProperty("inputHintsBar").objectReferenceValue = hintsBar;
            mainUIso.FindProperty("mainColumnCanvasGroup").objectReferenceValue = sideMenuCanvasGroup;
            mainUIso.FindProperty("screenFadeCanvasGroup").objectReferenceValue = fadeCg;
            mainUIso.FindProperty("loadingIndicator").objectReferenceValue = loadingObj;
            mainUIso.FindProperty("gameSceneName").stringValue = "MainGame";
            mainUIso.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene, "Assets/Scenes/MainMenu.unity");
            Debug.Log("[MainMenuRefinement] MainMenu scene successfully refined and saved for Phase 3!");
        }

        private static void ConfigureMenuButton(Transform btnTrans, Color charcoal, Color green, Color steelGrey, TypographyConfig config)
        {
            if (btnTrans == null) return;
            var btn = btnTrans.GetComponent<Button>();
            if (btn == null) btn = btnTrans.gameObject.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;

            var img = btnTrans.GetComponent<Image>();
            if (img == null) img = btnTrans.gameObject.AddComponent<Image>();
            img.color = charcoal;

            var fx = btnTrans.GetComponent<ButtonStateFX>();
            if (fx == null) fx = btnTrans.gameObject.AddComponent<ButtonStateFX>();

            var txt = btnTrans.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                var styler = txt.GetComponent<TextStyler>();
                if (styler == null) styler = txt.gameObject.AddComponent<TextStyler>();
                styler.Config = config;
                styler.Role = TypographyRole.Button;
                styler.ApplyStyle();
            }
        }

        private static Button CreateActionBtn(Transform parent, string name, string label,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size,
            Color charcoal, Color green, Color steelGrey, TypographyConfig config)
        {
            var btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            var rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            var img = btnObj.AddComponent<Image>();
            img.color = charcoal;

            var btn = btnObj.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;

            var fx = btnObj.AddComponent<ButtonStateFX>();

            var txtObj = new GameObject("Text");
            txtObj.transform.SetParent(btnObj.transform, false);
            var txtRect = txtObj.AddComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;

            var txt = txtObj.AddComponent<TextMeshProUGUI>();
            txt.text = label;
            txt.alignment = TextAlignmentOptions.Center;

            var styler = txtObj.AddComponent<TextStyler>();
            styler.Config = config;
            styler.Role = TypographyRole.Button;
            styler.ApplyStyle();

            return btn;
        }

        private static TextMeshProUGUI CreateTextElement(Transform parent, string name, string text,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size,
            TypographyRole role, TextAlignmentOptions align, TypographyConfig config)
        {
            var txtObj = new GameObject(name);
            txtObj.transform.SetParent(parent, false);

            var rect = txtObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            var tmp = txtObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.alignment = align;

            var styler = txtObj.AddComponent<TextStyler>();
            styler.Config = config;
            styler.Role = role;
            styler.ApplyStyle();

            return tmp;
        }

        private static void CreateBallCard(Transform parent, string name, string ballTitle, string badgeText, Color badgeColor,
            string description, float anchoredY, Color cardBg, Color borderColor, TypographyConfig config)
        {
            var cardObj = new GameObject(name);
            cardObj.transform.SetParent(parent, false);

            var rect = cardObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(0f, anchoredY);
            rect.sizeDelta = new Vector2(0f, 75f);

            var img = cardObj.AddComponent<Image>();
            img.color = cardBg;

            var badgeObj = new GameObject("Badge");
            badgeObj.transform.SetParent(cardObj.transform, false);
            var badgeRect = badgeObj.AddComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(0f, 1f);
            badgeRect.anchorMax = new Vector2(0f, 1f);
            badgeRect.anchoredPosition = new Vector2(40f, -18f);
            badgeRect.sizeDelta = new Vector2(65f, 22f);

            var badgeImg = badgeObj.AddComponent<Image>();
            badgeImg.color = badgeColor;

            var badgeTxtObj = new GameObject("Text");
            badgeTxtObj.transform.SetParent(badgeObj.transform, false);
            var btr = badgeTxtObj.AddComponent<RectTransform>();
            btr.anchorMin = Vector2.zero;
            btr.anchorMax = Vector2.one;
            btr.offsetMin = Vector2.zero;
            btr.offsetMax = Vector2.zero;

            var badgeTxt = badgeTxtObj.AddComponent<TextMeshProUGUI>();
            badgeTxt.text = badgeText;
            badgeTxt.alignment = TextAlignmentOptions.Center;
            badgeTxt.color = Color.black;
            var badgeStyler = badgeTxtObj.AddComponent<TextStyler>();
            badgeStyler.Config = config;
            badgeStyler.Role = TypographyRole.Caption;
            badgeStyler.ApplyStyle();
            badgeTxt.color = Color.black;

            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(cardObj.transform, false);
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.offsetMin = new Vector2(85f, -28f);
            titleRect.offsetMax = new Vector2(-10f, -8f);

            var titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
            titleTxt.text = ballTitle;
            titleTxt.alignment = TextAlignmentOptions.Left;
            var titleStyler = titleObj.AddComponent<TextStyler>();
            titleStyler.Config = config;
            titleStyler.Role = TypographyRole.Label;
            titleStyler.ApplyStyle();

            var descObj = new GameObject("Desc");
            descObj.transform.SetParent(cardObj.transform, false);
            var descRect = descObj.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0f, 0f);
            descRect.anchorMax = new Vector2(1f, 1f);
            descRect.offsetMin = new Vector2(12f, 8f);
            descRect.offsetMax = new Vector2(-12f, -32f);

            var descTxt = descObj.AddComponent<TextMeshProUGUI>();
            descTxt.text = description;
            descTxt.alignment = TextAlignmentOptions.Left;
            descTxt.textWrappingMode = TextWrappingModes.Normal;
            var descStyler = descObj.AddComponent<TextStyler>();
            descStyler.Config = config;
            descStyler.Role = TypographyRole.Caption;
            descStyler.ApplyStyle();
        }

        private static void CreateSkillCard(Transform parent, string name, string skillTitle, string keyPrompt,
            string description, float anchoredY, Color cardBg, Color borderColor, Color accentGreen, TypographyConfig config)
        {
            var cardObj = new GameObject(name);
            cardObj.transform.SetParent(parent, false);

            var rect = cardObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(0f, anchoredY);
            rect.sizeDelta = new Vector2(0f, 95f);

            var img = cardObj.AddComponent<Image>();
            img.color = cardBg;

            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(cardObj.transform, false);
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.offsetMin = new Vector2(12f, -28f);
            titleRect.offsetMax = new Vector2(-12f, -6f);

            var titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
            titleTxt.text = $"{skillTitle}  <color=#{ColorUtility.ToHtmlStringRGB(accentGreen)}>{keyPrompt}</color>";
            titleTxt.alignment = TextAlignmentOptions.Left;
            var titleStyler = titleObj.AddComponent<TextStyler>();
            titleStyler.Config = config;
            titleStyler.Role = TypographyRole.Label;
            titleStyler.ApplyStyle();

            var descObj = new GameObject("Desc");
            descObj.transform.SetParent(cardObj.transform, false);
            var descRect = descObj.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0f, 0f);
            descRect.anchorMax = new Vector2(1f, 1f);
            descRect.offsetMin = new Vector2(12f, 8f);
            descRect.offsetMax = new Vector2(-12f, -32f);

            var descTxt = descObj.AddComponent<TextMeshProUGUI>();
            descTxt.text = description;
            descTxt.alignment = TextAlignmentOptions.Left;
            descTxt.textWrappingMode = TextWrappingModes.Normal;
            var descStyler = descObj.AddComponent<TextStyler>();
            descStyler.Config = config;
            descStyler.Role = TypographyRole.Caption;
            descStyler.ApplyStyle();
        }

        private static Slider CreateSliderRow(Transform parent, string name, string label, float min, float max, float defaultValue,
            float anchoredY, Color cardBg, Color steelGrey, Color accentGreen, TypographyConfig config)
        {
            var rowObj = new GameObject(name);
            rowObj.transform.SetParent(parent, false);

            var rowRect = rowObj.AddComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0.5f, 1f);
            rowRect.anchorMax = new Vector2(0.5f, 1f);
            rowRect.anchoredPosition = new Vector2(0f, anchoredY);
            rowRect.sizeDelta = new Vector2(620f, 44f);

            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(rowObj.transform, false);
            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0.5f);
            labelRect.anchorMax = new Vector2(0f, 0.5f);
            labelRect.anchoredPosition = new Vector2(120f, 0f);
            labelRect.sizeDelta = new Vector2(240f, 35f);

            var labelTxt = labelObj.AddComponent<TextMeshProUGUI>();
            labelTxt.text = label;
            labelTxt.alignment = TextAlignmentOptions.Left;
            var labelStyler = labelObj.AddComponent<TextStyler>();
            labelStyler.Config = config;
            labelStyler.Role = TypographyRole.Body;
            labelStyler.ApplyStyle();

            var sliderObj = new GameObject("Slider");
            sliderObj.transform.SetParent(rowObj.transform, false);
            var sRect = sliderObj.AddComponent<RectTransform>();
            sRect.anchorMin = new Vector2(1f, 0.5f);
            sRect.anchorMax = new Vector2(1f, 0.5f);
            sRect.anchoredPosition = new Vector2(-160f, 0f);
            sRect.sizeDelta = new Vector2(300f, 22f);

            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(sliderObj.transform, false);
            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImg = bgObj.AddComponent<Image>();
            bgImg.color = cardBg;

            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = new Vector2(4f, 0f);
            fillAreaRect.offsetMax = new Vector2(-4f, 0f);

            var fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillArea.transform, false);
            var fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImg = fillObj.AddComponent<Image>();
            fillImg.color = accentGreen;

            var handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(sliderObj.transform, false);
            var handleAreaRect = handleArea.AddComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(10f, 0f);
            handleAreaRect.offsetMax = new Vector2(-10f, 0f);

            var handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(handleArea.transform, false);
            var handleRect = handleObj.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20f, 26f);
            var handleImg = handleObj.AddComponent<Image>();
            handleImg.color = Color.white;

            var slider = sliderObj.AddComponent<Slider>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = defaultValue;
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImg;

            return slider;
        }

        private static Toggle CreateToggleRow(Transform parent, string name, string label, bool defaultValue,
            Vector2 pos, Color cardBg, Color steelGrey, Color accentGreen, TypographyConfig config)
        {
            var toggleObj = new GameObject(name);
            toggleObj.transform.SetParent(parent, false);

            var toggleRect = toggleObj.AddComponent<RectTransform>();
            toggleRect.anchorMin = new Vector2(0.5f, 1f);
            toggleRect.anchorMax = new Vector2(0.5f, 1f);
            toggleRect.anchoredPosition = pos;
            toggleRect.sizeDelta = new Vector2(280f, 40f);

            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(toggleObj.transform, false);
            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0f, 0.5f);
            bgRect.anchorMax = new Vector2(0f, 0.5f);
            bgRect.anchoredPosition = new Vector2(18f, 0f);
            bgRect.sizeDelta = new Vector2(28f, 28f);

            var bgImg = bgObj.AddComponent<Image>();
            bgImg.color = cardBg;

            var checkObj = new GameObject("Checkmark");
            checkObj.transform.SetParent(bgObj.transform, false);
            var checkRect = checkObj.AddComponent<RectTransform>();
            checkRect.anchorMin = Vector2.zero;
            checkRect.anchorMax = Vector2.one;
            checkRect.offsetMin = new Vector2(5f, 5f);
            checkRect.offsetMax = new Vector2(-5f, -5f);

            var checkImg = checkObj.AddComponent<Image>();
            checkImg.color = accentGreen;

            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(toggleObj.transform, false);
            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(45f, 0f);
            labelRect.offsetMax = Vector2.zero;

            var labelTxt = labelObj.AddComponent<TextMeshProUGUI>();
            labelTxt.text = label;
            labelTxt.alignment = TextAlignmentOptions.Left;
            var labelStyler = labelObj.AddComponent<TextStyler>();
            labelStyler.Config = config;
            labelStyler.Role = TypographyRole.Body;
            labelStyler.ApplyStyle();

            var toggle = toggleObj.AddComponent<Toggle>();
            toggle.graphic = checkImg;
            toggle.targetGraphic = bgImg;
            toggle.isOn = defaultValue;

            return toggle;
        }
    }
}
