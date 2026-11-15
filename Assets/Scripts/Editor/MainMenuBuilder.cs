using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;
using DodgeGame.Core;
using DodgeGame.Data;
using DodgeGame.UI;

namespace DodgeGame.Editor
{
    public static class MainMenuBuilder
    {
        [MenuItem("DodgeGame/Build Main Menu Scene")]
        public static void BuildMainMenuScene()
        {
            string scenePath = "Assets/Scenes/MainMenu.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            var menuTheme = AssetDatabase.LoadAssetAtPath<MenuTheme>("Assets/ScriptableObjects/MenuTheme.asset");
            var fontDisplay = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Art/Fonts/FontDisplay.asset");
            var fontUI = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Art/Fonts/FontUI.asset");
            var fontBody = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Art/Fonts/FontBody.asset");

            var logoSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Logo.png");
            var hexSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HexPattern.png");
            var panelHeaderSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Panel_Header.png");
            var btnDarkSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Button_Dark.png");
            var btnPrimarySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Button_Primary.png");
            var iconPlaySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Icon_Play.png");
            var iconHtpSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Icon_HowToPlay.png");
            var iconSettingsSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Icon_Settings.png");
            var iconAboutSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Icon_About.png");
            var iconQuitSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Icon_Quit.png");
            var iconPhaseSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Icon_Phase.png");
            var decalGooSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Decal_Goo.png");
            var shadowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Shadow_Blob.png");

            EnsurePanelModalLightSprite();
            var panelModalLightSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Panel_ModalLight.png");
            var sliderTrackSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Slider_Track.png");
            var sliderFillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Slider_Fill.png");
            var sliderHandleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Slider_Handle.png");
            var toggleTrackSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Toggle_Track.png");
            var toggleKnobSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Toggle_Knob.png");
            var btnCloseDangerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Btn_Close_Danger.png");

            var camConfig = AssetDatabase.LoadAssetAtPath<CameraConfig>("Assets/ScriptableObjects/SO_MenuCameraConfig.asset")
                         ?? AssetDatabase.LoadAssetAtPath<CameraConfig>("Assets/ScriptableObjects/SO_CameraConfig.asset");

            var mainCamGo = GameObject.Find("Main Camera");
            if (mainCamGo == null) mainCamGo = new GameObject("Main Camera");
            var cam = mainCamGo.GetComponent<Camera>() ?? mainCamGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.957f, 0.965f, 0.973f, 1f);
            cam.fieldOfView = 40f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 100f;
            cam.targetTexture = null;
            mainCamGo.tag = "MainCamera";

            var audioListener = mainCamGo.GetComponent<AudioListener>() ?? mainCamGo.AddComponent<AudioListener>();

            var autoRotate = mainCamGo.GetComponent("AutoRotate");
            if (autoRotate != null) UnityEngine.Object.DestroyImmediate(autoRotate);

            Vector3 arenaCenter = Vector3.zero;

            var camController = mainCamGo.GetComponent<ArenaCameraController>() ?? mainCamGo.AddComponent<ArenaCameraController>();
            var camSo = new SerializedObject(camController);
            camSo.FindProperty("config").objectReferenceValue = camConfig;
            camSo.FindProperty("orbitCenter").vector3Value = arenaCenter;
            Vector2 menuScreenOffset = new Vector2(5.2f, 1.0f);
            camSo.FindProperty("screenOffset").vector2Value = menuScreenOffset;
            camSo.FindProperty("targetCamera").objectReferenceValue = mainCamGo.transform;
            camSo.ApplyModifiedProperties();

            float cameraPitch = 40f;
            float cameraDist = 23f;
            Quaternion initialCamRot = Quaternion.Euler(cameraPitch, 0f, 0f);
            Vector3 camOffset = initialCamRot * new Vector3(-menuScreenOffset.x, menuScreenOffset.y, -cameraDist);
            mainCamGo.transform.position = arenaCenter + camOffset;
            mainCamGo.transform.rotation = initialCamRot;

            var dirLightGo = GameObject.Find("Directional Light");
            if (dirLightGo == null) dirLightGo = new GameObject("Directional Light");
            var dirLight = dirLightGo.GetComponent<Light>() ?? dirLightGo.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = Color.white;
            dirLight.intensity = 1.25f;
            dirLightGo.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            var arenaGo = GameObject.Find("Arena");
            if (arenaGo == null)
            {
                arenaGo = new GameObject("Arena");
            }
            arenaGo.transform.position = arenaCenter;

            while (arenaGo.transform.childCount > 0)
            {
                UnityEngine.Object.DestroyImmediate(arenaGo.transform.GetChild(0).gameObject);
            }

            if (!Directory.Exists("Assets/Art/Materials")) Directory.CreateDirectory("Assets/Art/Materials");

            var floorMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/M_ArenaFloor.mat");
            if (floorMat == null)
            {
                floorMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                floorMat.color = new Color(0.25f, 0.29f, 0.33f, 1f);
                floorMat.SetFloat("_Smoothness", 0.65f);
                floorMat.SetFloat("_Metallic", 0.45f);
                AssetDatabase.CreateAsset(floorMat, "Assets/Art/Materials/M_ArenaFloor.mat");
            }

            var wallMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/M_ArenaWall.mat");
            if (wallMat == null)
            {
                wallMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                wallMat.color = new Color(0.54f, 0.58f, 0.61f, 1f);
                wallMat.SetFloat("_Smoothness", 0.4f);
                wallMat.SetFloat("_Metallic", 0.6f);
                AssetDatabase.CreateAsset(wallMat, "Assets/Art/Materials/M_ArenaWall.mat");
            }

            var floorGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            floorGo.name = "Floor";
            floorGo.transform.SetParent(arenaGo.transform, false);
            floorGo.transform.localPosition = new Vector3(0f, -0.1f, 0f);
            floorGo.transform.localScale = new Vector3(13.5f, 0.2f, 13.5f);
            floorGo.GetComponent<MeshRenderer>().sharedMaterial = floorMat;

            float radius = 6.4f;
            float apothem = radius * Mathf.Cos(22.5f * Mathf.Deg2Rad);
            float segLength = 2f * radius * Mathf.Sin(22.5f * Mathf.Deg2Rad) * 1.05f;

            for (int i = 0; i < 8; i++)
            {
                float angleDeg = i * 45f;
                float angleRad = angleDeg * Mathf.Deg2Rad;

                var wGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wGo.name = $"Wall_{i}";
                wGo.transform.SetParent(arenaGo.transform, false);

                wGo.transform.localPosition = new Vector3(apothem * Mathf.Sin(angleRad), 0.5f, apothem * Mathf.Cos(angleRad));
                wGo.transform.localRotation = Quaternion.Euler(0f, angleDeg, 0f);
                wGo.transform.localScale = new Vector3(segLength, 1.2f, 0.45f);
                wGo.GetComponent<MeshRenderer>().sharedMaterial = wallMat;
            }

            SetupBallWithMaterial(arenaGo.transform, "DecorativeFireBall", new Vector3(-2.8f, 0.75f, -2.2f),
                new Color(1f, 0.28f, 0.05f), new Color(1f, 0.45f, 0f), 2.2f, shadowSprite);

            SetupStickyBall(arenaGo.transform, new Vector3(0f, 0.75f, 2.5f), decalGooSprite, shadowSprite);


            SetupBallWithMaterial(arenaGo.transform, "DecorativeGunBall", new Vector3(3.0f, 0.75f, -2.0f),
                new Color(0.1f, 0.55f, 1.0f), new Color(0.1f, 0.7f, 1.0f), 1.6f, shadowSprite);

            SetupMenuDodger(arenaGo.transform);

            var eventSystemGo = GameObject.Find("EventSystem");
            if (eventSystemGo == null) eventSystemGo = new GameObject("EventSystem");
            var es = eventSystemGo.GetComponent<EventSystem>() ?? eventSystemGo.AddComponent<EventSystem>();
            var inputModule = eventSystemGo.GetComponent<InputSystemUIInputModule>() ?? eventSystemGo.AddComponent<InputSystemUIInputModule>();

            var canvasGo = GameObject.Find("Canvas");
            if (canvasGo == null) canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.GetComponent<Canvas>() ?? canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 5.0f;

            var scaler = canvasGo.GetComponent<CanvasScaler>() ?? canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var raycaster = canvasGo.GetComponent<GraphicRaycaster>() ?? canvasGo.AddComponent<GraphicRaycaster>();
            var menuUI = canvasGo.GetComponent<MainMenuUI>() ?? canvasGo.AddComponent<MainMenuUI>();

            while (canvasGo.transform.childCount > 0)
            {
                UnityEngine.Object.DestroyImmediate(canvasGo.transform.GetChild(0).gameObject);
            }

            var bgGo = CreateUIObject("Background", canvasGo.transform);
            SetStretch(bgGo.GetComponent<RectTransform>());

            var hexGo = CreateUIObject("HexPatternOverlay", bgGo.transform);
            SetStretch(hexGo.GetComponent<RectTransform>());
            var hexImg = hexGo.AddComponent<Image>();
            if (hexSprite != null)
            {
                hexImg.sprite = hexSprite;
                hexImg.type = Image.Type.Tiled;
            }
            hexImg.color = new Color(0.867f, 0.890f, 0.910f, 0.25f);
            hexImg.raycastTarget = false;

            var mainColGo = CreateUIObject("MainColumn", canvasGo.transform);
            var mainColRt = mainColGo.GetComponent<RectTransform>();
            SetAnchors(mainColRt, 0.04f, 0.64f, 0.48f, 0.98f);
            var mainColCg = mainColGo.AddComponent<CanvasGroup>();

            var logoGo = CreateUIObject("Logo", mainColGo.transform);
            var logoRt = logoGo.GetComponent<RectTransform>();
            SetAnchors(logoRt, 0f, 0.22f, 1.0f, 1.0f);
            var logoImg = logoGo.AddComponent<Image>();
            if (logoSprite != null) logoImg.sprite = logoSprite;
            logoImg.preserveAspect = true;
            logoImg.raycastTarget = false;
            var logoCg = logoGo.AddComponent<CanvasGroup>();

            var subGo = CreateUIObject("SubtitleContainer", mainColGo.transform);
            var subRt = subGo.GetComponent<RectTransform>();
            SetAnchors(subRt, 0f, 0.04f, 1.0f, 0.18f);
            var subHlg = subGo.AddComponent<HorizontalLayoutGroup>();
            subHlg.childAlignment = TextAnchor.MiddleCenter;
            subHlg.spacing = 14;
            subHlg.childControlWidth = false;
            subHlg.childControlHeight = false;

            var leftRule = CreateUIObject("LeftRule", subGo.transform);
            var leftRuleImg = leftRule.AddComponent<Image>();
            leftRuleImg.color = new Color(0.118f, 0.165f, 0.200f, 0.7f);
            var leftRuleRt = leftRule.GetComponent<RectTransform>();
            leftRuleRt.sizeDelta = new Vector2(40f, 2f);

            var subTextGo = CreateUIObject("SubtitleText", subGo.transform);
            var subTmp = subTextGo.AddComponent<TextMeshProUGUI>();
            subTmp.text = "An Arena Survival Dodge Game";
            if (fontDisplay != null) subTmp.font = fontDisplay;
            subTmp.fontSize = 28;
            subTmp.fontStyle = FontStyles.Bold | FontStyles.Italic;
            subTmp.color = new Color(0.118f, 0.165f, 0.200f, 1f);
            subTmp.alignment = TextAlignmentOptions.Center;
            subTmp.textWrappingMode = TextWrappingModes.NoWrap;
            var subTextRt = subTextGo.GetComponent<RectTransform>();
            subTextRt.sizeDelta = new Vector2(520f, 36f);

            var rightRule = CreateUIObject("RightRule", subGo.transform);
            var rightRuleImg = rightRule.AddComponent<Image>();
            rightRuleImg.color = new Color(0.118f, 0.165f, 0.200f, 0.7f);
            var rightRuleRt = rightRule.GetComponent<RectTransform>();
            rightRuleRt.sizeDelta = new Vector2(40f, 2f);

            var btnColGo = CreateUIObject("ButtonColumn", canvasGo.transform);
            var btnColRt = btnColGo.GetComponent<RectTransform>();
            SetAnchors(btnColRt, 0.04f, 0f, 0.38f, 1f);

            List<RectTransform> animatedRows = new List<RectTransform>();

            var playBtn = CreateMenuButton("PlayGameButton", btnColGo.transform,
                0f, 0.515f, 1f, 0.620f,
                btnPrimarySprite, Color.white,
                iconPlaySprite, "PLAY GAME", fontDisplay, 50,
                Color.white, true);
            animatedRows.Add(playBtn.GetComponent<RectTransform>());

            var htpBtn = CreateMenuButton("HowToPlayButton", btnColGo.transform,
                0f, 0.402f, 1f, 0.490f,
                btnDarkSprite, Color.white,
                iconHtpSprite, "HOW TO PLAY", fontDisplay, 44,
                Color.white, false);
            animatedRows.Add(htpBtn.GetComponent<RectTransform>());

            var settingsBtn = CreateMenuButton("SettingsButton", btnColGo.transform,
                0f, 0.302f, 1f, 0.390f,
                btnDarkSprite, Color.white,
                iconSettingsSprite, "SETTINGS", fontDisplay, 44,
                Color.white, false);
            animatedRows.Add(settingsBtn.GetComponent<RectTransform>());

            var aboutBtn = CreateMenuButton("AboutButton", btnColGo.transform,
                0f, 0.212f, 1f, 0.300f,
                btnDarkSprite, Color.white,
                iconAboutSprite, "ABOUT", fontDisplay, 44,
                Color.white, false);
            animatedRows.Add(aboutBtn.GetComponent<RectTransform>());

            var quitBtn = CreateMenuButton("QuitButton", btnColGo.transform,
                0f, 0.112f, 1f, 0.200f,
                btnDarkSprite, Color.white,
                iconQuitSprite, "QUIT", fontDisplay, 44,
                Color.white, false);
            animatedRows.Add(quitBtn.GetComponent<RectTransform>());

            var highScoreGo = CreateUIObject("HighScoreText", btnColGo.transform);
            var highScoreRt = highScoreGo.GetComponent<RectTransform>();
            SetAnchors(highScoreRt, 0f, 0.052f, 1f, 0.098f);
            var highScoreTmp = highScoreGo.AddComponent<TextMeshProUGUI>();
            highScoreTmp.text = "BEST (Normal): 0";
            if (fontUI != null) highScoreTmp.font = fontUI;
            highScoreTmp.fontSize = 22;
            highScoreTmp.fontStyle = FontStyles.Bold;
            highScoreTmp.color = new Color(0.118f, 0.165f, 0.200f, 0.9f);
            highScoreTmp.alignment = TextAlignmentOptions.Center;
            animatedRows.Add(highScoreRt);

            var headerGo = CreateUIObject("HeaderPanel", canvasGo.transform);
            var headerRt = headerGo.GetComponent<RectTransform>();
            SetAnchors(headerRt, 0.57f, 0.74f, 0.97f, 0.96f);
            var headerImg = headerGo.AddComponent<Image>();
            if (panelHeaderSprite != null)
            {
                headerImg.sprite = panelHeaderSprite;
                headerImg.type = Image.Type.Sliced;
            }
            headerImg.color = Color.white;

            var headerShadow = headerGo.AddComponent<Shadow>();
            headerShadow.effectColor = new Color(0f, 0f, 0f, 0.15f);
            headerShadow.effectDistance = new Vector2(0f, -4f);
            headerShadow.useGraphicAlpha = true;

            var iconZoneGo = CreateUIObject("IconZone", headerGo.transform);
            var iconZoneRt = iconZoneGo.GetComponent<RectTransform>();
            SetAnchors(iconZoneRt, 0.06f, 0f, 0.26f, 1f);

            float panelHeightPx = (0.96f - 0.74f) * 1080f;
            float iconHeightPx = panelHeightPx * 0.60f;
            var phaseIconGo = CreateUIObject("PhaseIcon", iconZoneGo.transform);
            var phaseIconRt = phaseIconGo.GetComponent<RectTransform>();
            phaseIconRt.anchorMin = new Vector2(0.5f, 0.5f);
            phaseIconRt.anchorMax = new Vector2(0.5f, 0.5f);
            phaseIconRt.sizeDelta = new Vector2(iconHeightPx, iconHeightPx);
            var phaseIconImg = phaseIconGo.AddComponent<Image>();
            if (iconPhaseSprite != null) phaseIconImg.sprite = iconPhaseSprite;
            phaseIconImg.preserveAspect = true;
            phaseIconImg.raycastTarget = false;

            var textZoneGo = CreateUIObject("TextZone", headerGo.transform);
            var textZoneRt = textZoneGo.GetComponent<RectTransform>();
            SetAnchors(textZoneRt, 0.32f, 0f, 0.94f, 1f);

            var textVlg = textZoneGo.AddComponent<VerticalLayoutGroup>();
            textVlg.childAlignment = TextAnchor.MiddleLeft;
            textVlg.spacing = 6f;
            textVlg.childControlWidth = true;
            textVlg.childControlHeight = true;
            textVlg.childForceExpandWidth = false;
            textVlg.childForceExpandHeight = false;

            var titleGo = CreateUIObject("TitleText", textZoneGo.transform);
            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "Phase Ability";
            if (fontDisplay != null) titleTmp.font = fontDisplay;
            titleTmp.fontStyle = FontStyles.Bold | FontStyles.Italic;
            titleTmp.fontSize = 40;
            titleTmp.color = new Color(0.118f, 0.165f, 0.200f, 1f);
            titleTmp.textWrappingMode = TextWrappingModes.NoWrap;
            titleTmp.alignment = TextAlignmentOptions.MidlineLeft;
            titleTmp.raycastTarget = false;

            var descGo = CreateUIObject("DescriptionText", textZoneGo.transform);
            var descTmp = descGo.AddComponent<TextMeshProUGUI>();
            descTmp.text = "Shift dimension to evade balls";
            if (fontBody != null) descTmp.font = fontBody;
            descTmp.fontStyle = FontStyles.Normal;
            descTmp.fontSize = 24;
            descTmp.color = new Color(0.290f, 0.345f, 0.388f, 1f);
            descTmp.textWrappingMode = TextWrappingModes.Normal;
            descTmp.maxVisibleLines = 2;
            descTmp.alignment = TextAlignmentOptions.MidlineLeft;
            descTmp.raycastTarget = false;

            var cdGo = CreateUIObject("CooldownText", textZoneGo.transform);
            var cdTmp = cdGo.AddComponent<TextMeshProUGUI>();
            cdTmp.text = "Cooldown: 5.0 s";
            if (fontBody != null) cdTmp.font = fontBody;
            cdTmp.fontStyle = FontStyles.Normal;
            cdTmp.fontSize = 20;
            cdTmp.color = new Color(0.478f, 0.529f, 0.569f, 1f);
            cdTmp.textWrappingMode = TextWrappingModes.NoWrap;
            cdTmp.alignment = TextAlignmentOptions.MidlineLeft;
            cdTmp.raycastTarget = false;

            var footerGo = CreateUIObject("Footer", canvasGo.transform);
            var footerRt = footerGo.GetComponent<RectTransform>();
            SetAnchors(footerRt, 0f, 0f, 1.0f, 0.08f);

            var taglineGo = CreateUIObject("TaglineContainer", footerGo.transform);
            var taglineRt = taglineGo.GetComponent<RectTransform>();
            taglineRt.anchorMin = new Vector2(0.5f, 0.5f);
            taglineRt.anchorMax = new Vector2(0.5f, 0.5f);
            taglineRt.sizeDelta = new Vector2(850f, 36f);
            var tagHlg = taglineGo.AddComponent<HorizontalLayoutGroup>();
            tagHlg.childAlignment = TextAnchor.MiddleCenter;
            tagHlg.spacing = 16;
            tagHlg.childControlWidth = false;
            tagHlg.childControlHeight = false;

            var tagRuleL = CreateUIObject("RuleLeft", taglineGo.transform);
            var tagRuleLImg = tagRuleL.AddComponent<Image>();
            tagRuleLImg.color = new Color(0.118f, 0.165f, 0.200f, 0.5f);
            tagRuleL.GetComponent<RectTransform>().sizeDelta = new Vector2(50f, 2f);

            var tagTextGo = CreateUIObject("TaglineText", taglineGo.transform);
            var tagTmp = tagTextGo.AddComponent<TextMeshProUGUI>();
            tagTmp.text = "STAY ALIVE FOR THE HIGHEST SCORE";
            if (fontDisplay != null) tagTmp.font = fontDisplay;
            tagTmp.fontSize = 30;
            tagTmp.fontStyle = FontStyles.Bold | FontStyles.Italic;
            tagTmp.characterSpacing = 8f;
            tagTmp.color = new Color(0.118f, 0.165f, 0.200f, 1f);
            tagTmp.textWrappingMode = TextWrappingModes.NoWrap;
            tagTextGo.GetComponent<RectTransform>().sizeDelta = new Vector2(680f, 36f);

            var tagRuleR = CreateUIObject("RuleRight", taglineGo.transform);
            var tagRuleRImg = tagRuleR.AddComponent<Image>();
            tagRuleRImg.color = new Color(0.118f, 0.165f, 0.200f, 0.5f);
            tagRuleR.GetComponent<RectTransform>().sizeDelta = new Vector2(50f, 2f);

            var versionGo = CreateUIObject("VersionText", footerGo.transform);
            var versionRt = versionGo.GetComponent<RectTransform>();
            versionRt.anchorMin = new Vector2(0.90f, 0.1f);
            versionRt.anchorMax = new Vector2(0.98f, 0.8f);
            versionRt.offsetMin = Vector2.zero;
            versionRt.offsetMax = Vector2.zero;
            var versionTmp = versionGo.AddComponent<TextMeshProUGUI>();
            versionTmp.text = "v1.0";
            if (fontBody != null) versionTmp.font = fontBody;
            versionTmp.fontSize = 24;
            versionTmp.fontStyle = FontStyles.Bold | FontStyles.Italic;
            versionTmp.color = new Color(0.118f, 0.165f, 0.200f, 0.85f);
            versionTmp.alignment = TextAlignmentOptions.Right;

            var hintsGo = CreateUIObject("InputHintsBar", footerGo.transform);
            var hintsRt = hintsGo.GetComponent<RectTransform>();
            hintsRt.anchorMin = new Vector2(0.04f, 0.1f);
            hintsRt.anchorMax = new Vector2(0.48f, 0.8f);
            hintsRt.offsetMin = Vector2.zero;
            hintsRt.offsetMax = Vector2.zero;
            var hintsTmp = hintsGo.AddComponent<TextMeshProUGUI>();
            hintsTmp.text = "Hold RMB to orbit camera | Scroll to zoom | Home to reset view";
            if (fontBody != null) hintsTmp.font = fontBody;
            hintsTmp.fontSize = 18;
            hintsTmp.color = new Color(0.45f, 0.52f, 0.58f, 1f);
            hintsTmp.alignment = TextAlignmentOptions.Left;
            var hintsBar = hintsGo.AddComponent<InputHintsBar>();
            var hintsSo = new SerializedObject(hintsBar);
            hintsSo.FindProperty("hintText").objectReferenceValue = hintsTmp;
            hintsSo.ApplyModifiedProperties();

            var modalsGo = CreateUIObject("Modals", canvasGo.transform);
            SetStretch(modalsGo.GetComponent<RectTransform>());

            var dimmerGo = CreateUIObject("DimmerOverlay", modalsGo.transform);
            SetStretch(dimmerGo.GetComponent<RectTransform>());
            var dimmerImg = dimmerGo.AddComponent<Image>();
            dimmerImg.color = new Color(0f, 0f, 0f, 0.4f);
            dimmerGo.SetActive(false);

            var htpModal = CreateModalPanel("HowToPlayPanel", modalsGo.transform, panelHeaderSprite, new Vector2(1050f, 720f));
            var htpTransition = htpModal.AddComponent<MenuPanelTransition>();
            var closeHtpBtn = SetupModalHeader(htpModal.transform, "HOW TO PLAY", fontDisplay);

            var htpContent = CreateUIObject("Content", htpModal.transform);
            var htpContentRt = htpContent.GetComponent<RectTransform>();
            SetAnchors(htpContentRt, 0.05f, 0.12f, 0.95f, 0.86f);
            var htpTmp = htpContent.AddComponent<TextMeshProUGUI>();
            if (fontBody != null) htpTmp.font = fontBody;
            htpTmp.fontSize = 22;
            htpTmp.color = new Color(0.118f, 0.165f, 0.200f, 1f);
            htpTmp.lineSpacing = 12f;
            htpTmp.text = "<b>OBJECTIVE:</b> Survive in the arena avoiding all hazardous balls.\n\n" +
                          "<b>CONTROLS:</b>\n" +
                          "• Move: WASD / Left Stick      • Dash: Left Shift / Gamepad B\n" +
                          "• Dimension Phase: Space / Gamepad A   • Slow Motion: E / Gamepad Y\n\n" +
                          "<b>CAMERA CONTROLS:</b>\n" +
                          "• Drag Right Mouse / Right Stick to orbit view.\n" +
                          "• Scroll Wheel / Bumpers (LB/RB) to zoom.\n" +
                          "• Press Home / Double-click to reset camera.\n\n" +
                          "<b>HAZARD BALLS:</b>\n" +
                          "• <color=#FF5500>Fire Ball</color>: Leaves burning tiles.  • <color=#22DD22>Sticky Ball</color>: Leaves slow goo.\n" +
                          "• <color=#00AAFF>Gun Ball</color>: Aims & shoots.";

            var htpBackBtn = CreateModalBackButton(htpModal.transform, fontUI);

            var settingsCtrl = BuildRedesignedSettingsModal(
                modalsGo.transform,
                fontDisplay, fontBody, fontUI,
                panelModalLightSprite, sliderTrackSprite, sliderFillSprite, sliderHandleSprite,
                toggleTrackSprite, toggleKnobSprite, btnCloseDangerSprite, btnDarkSprite,
                out var closeSettingsBtn, out var settingsPanelUIInstance);

            var aboutModal = CreateModalPanel("AboutPanel", modalsGo.transform, panelHeaderSprite, new Vector2(850f, 620f));
            var aboutTransition = aboutModal.AddComponent<MenuPanelTransition>();
            var closeAboutBtn = SetupModalHeader(aboutModal.transform, "ABOUT", fontDisplay);

            var aboutContent = CreateUIObject("Content", aboutModal.transform);
            var aboutContentRt = aboutContent.GetComponent<RectTransform>();
            SetAnchors(aboutContentRt, 0.08f, 0.20f, 0.92f, 0.86f);
            var aboutTmp = aboutContent.AddComponent<TextMeshProUGUI>();
            if (fontBody != null) aboutTmp.font = fontBody;
            aboutTmp.fontSize = 22;
            aboutTmp.color = new Color(0.118f, 0.165f, 0.200f, 1f);
            aboutTmp.lineSpacing = 10f;
            aboutTmp.text = "<b>ULTIMATE DODGE</b>\n" +
                            "One arena. Endless balls. Zero excuses.\n" +
                            "Dash, phase and slow time to outlast the chaos.\n\n" +
                            "<b>MADE BY</b>\n" +
                            "Usman & Usarim\n\n" +
                            "<b>BUILT WITH</b>\n" +
                            "Unity 6  •  Universal Render Pipeline  •  v1.0";

            var gitHubBtnGo = CreateUIObject("GitHubButton", aboutModal.transform);
            var gitHubBtnRt = gitHubBtnGo.GetComponent<RectTransform>();
            gitHubBtnRt.anchorMin = new Vector2(0.2f, 0.10f);
            gitHubBtnRt.anchorMax = new Vector2(0.55f, 0.18f);
            gitHubBtnRt.offsetMin = Vector2.zero;
            gitHubBtnRt.offsetMax = Vector2.zero;
            var gitHubImg = gitHubBtnGo.AddComponent<Image>();
            if (btnDarkSprite != null) gitHubImg.sprite = btnDarkSprite;
            gitHubImg.color = new Color(0.184f, 0.220f, 0.243f, 1f);
            var gitHubBtn = gitHubBtnGo.AddComponent<Button>();
            var gitHubTxtGo = CreateUIObject("Text", gitHubBtnGo.transform);
            SetStretch(gitHubTxtGo.GetComponent<RectTransform>());
            var gitHubTmp = gitHubTxtGo.AddComponent<TextMeshProUGUI>();
            gitHubTmp.text = "PROJECT REPOSITORY";
            if (fontUI != null) gitHubTmp.font = fontUI;
            gitHubTmp.fontSize = 22;
            gitHubTmp.fontStyle = FontStyles.Bold;
            gitHubTmp.alignment = TextAlignmentOptions.Center;
            gitHubTmp.color = Color.white;

            var aboutBackBtn = CreateModalBackButton(aboutModal.transform, fontUI);

            var diffModalController = BuildDifficultyModal(modalsGo.transform, fontDisplay, fontBody, fontUI);

            var screenFadeGo = CreateUIObject("ScreenFadeOverlay", canvasGo.transform);
            SetStretch(screenFadeGo.GetComponent<RectTransform>());
            var screenFadeImg = screenFadeGo.AddComponent<Image>();
            screenFadeImg.color = Color.black;
            var screenFadeCg = screenFadeGo.AddComponent<CanvasGroup>();
            screenFadeCg.alpha = 0f;
            screenFadeGo.SetActive(false);

            var menuSo = new SerializedObject(menuUI);
            menuSo.FindProperty("playButton").objectReferenceValue = playBtn;
            menuSo.FindProperty("howToPlayButton").objectReferenceValue = htpBtn;
            menuSo.FindProperty("settingsButton").objectReferenceValue = settingsBtn;
            menuSo.FindProperty("aboutButton").objectReferenceValue = aboutBtn;
            menuSo.FindProperty("quitButton").objectReferenceValue = quitBtn;

            menuSo.FindProperty("highScoreText").objectReferenceValue = highScoreTmp;
            menuSo.FindProperty("versionText").objectReferenceValue = versionTmp;

            menuSo.FindProperty("howToPlayTransition").objectReferenceValue = htpTransition;
            menuSo.FindProperty("closeHowToPlayButton").objectReferenceValue = closeHtpBtn;
            menuSo.FindProperty("howToPlayBackButton").objectReferenceValue = htpBackBtn;

            menuSo.FindProperty("settingsModalController").objectReferenceValue = settingsCtrl;
            menuSo.FindProperty("closeSettingsButton").objectReferenceValue = closeSettingsBtn;
            menuSo.FindProperty("settingsPanelUI").objectReferenceValue = settingsPanelUIInstance;

            menuSo.FindProperty("aboutTransition").objectReferenceValue = aboutTransition;
            menuSo.FindProperty("closeAboutButton").objectReferenceValue = closeAboutBtn;
            menuSo.FindProperty("aboutBackButton").objectReferenceValue = aboutBackBtn;
            menuSo.FindProperty("aboutLinkButton").objectReferenceValue = gitHubBtn;

            menuSo.FindProperty("inputHintsBar").objectReferenceValue = hintsBar;
            menuSo.FindProperty("mainColumnCanvasGroup").objectReferenceValue = mainColCg;
            menuSo.FindProperty("logoTransform").objectReferenceValue = logoRt;
            menuSo.FindProperty("logoCanvasGroup").objectReferenceValue = logoCg;

            var animListProp = menuSo.FindProperty("animatedButtonRows");
            animListProp.ClearArray();
            for (int i = 0; i < animatedRows.Count; i++)
            {
                animListProp.InsertArrayElementAtIndex(i);
                animListProp.GetArrayElementAtIndex(i).objectReferenceValue = animatedRows[i];
            }

            menuSo.FindProperty("screenFadeCanvasGroup").objectReferenceValue = screenFadeCg;
            menuSo.FindProperty("difficultyModal").objectReferenceValue = diffModalController;
            menuSo.ApplyModifiedProperties();

            EditorUtility.SetDirty(canvasGo);
            EditorUtility.SetDirty(mainCamGo);
            EditorUtility.SetDirty(arenaGo);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[MainMenuBuilder] MainMenu rebuilt successfully with ScreenSpaceCamera and all features!");
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            if (go.layer < 0) go.layer = 5;
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void SetStretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void SetAnchors(RectTransform rt, float minX, float minY, float maxX, float maxY)
        {
            rt.anchorMin = new Vector2(minX, minY);
            rt.anchorMax = new Vector2(maxX, maxY);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static Button CreateMenuButton(string name, Transform parent,
            float minX, float minY, float maxX, float maxY,
            Sprite bgSprite, Color bgColor, Sprite iconSprite, string label, TMP_FontAsset font, float fontSize, Color textColor, bool isPrimary)
        {
            var btnGo = CreateUIObject(name, parent);
            btnGo.AddComponent<CanvasGroup>();
            var btnRt = btnGo.GetComponent<RectTransform>();
            SetAnchors(btnRt, minX, minY, maxX, maxY);

            var img = btnGo.AddComponent<Image>();
            if (bgSprite != null)
            {
                img.sprite = bgSprite;
                img.type = Image.Type.Sliced;
            }
            img.color = bgColor;

            var btn = btnGo.AddComponent<Button>();
            var btnFX = btnGo.AddComponent<ButtonStateFX>();
            var fxSo = new SerializedObject(btnFX);
            var btnImgProp = fxSo.FindProperty("buttonImage");
            if (btnImgProp != null) btnImgProp.objectReferenceValue = img;
            var normColProp = fxSo.FindProperty("normalColor");
            if (normColProp != null) normColProp.colorValue = bgColor;
            Color hoverColor = isPrimary ? new Color(1.10f, 1.10f, 1.10f, 1f) : new Color(1.18f, 1.18f, 1.18f, 1f);
            var highColProp = fxSo.FindProperty("highlightColor");
            if (highColProp != null) highColProp.colorValue = hoverColor;
            fxSo.ApplyModifiedProperties();

            if (iconSprite != null)
            {
                var iconZoneGo = CreateUIObject("IconZone", btnGo.transform);
                var iconZoneRt = iconZoneGo.GetComponent<RectTransform>();
                SetAnchors(iconZoneRt, 0f, 0f, 0.12f, 1f);

                var iconGo = CreateUIObject("Icon", iconZoneGo.transform);
                var iconRt = iconGo.GetComponent<RectTransform>();
                iconRt.anchorMin = new Vector2(0.5f, 0.5f);
                iconRt.anchorMax = new Vector2(0.5f, 0.5f);
                iconRt.anchoredPosition = Vector2.zero;
                float btnHeightPx = (maxY - minY) * 1080f;
                float iconSize = btnHeightPx * 0.55f;
                iconRt.sizeDelta = new Vector2(iconSize, iconSize);

                var iconImg = iconGo.AddComponent<Image>();
                iconImg.sprite = iconSprite;
                iconImg.color = Color.white;
                iconImg.preserveAspect = true;
                iconImg.raycastTarget = false;

                var divGo = CreateUIObject("Divider", btnGo.transform);
                var divRt = divGo.GetComponent<RectTransform>();
                SetAnchors(divRt, 0.12f, 0.18f, 0.12f, 0.82f);
                divRt.sizeDelta = new Vector2(2f, 0f);
                divRt.anchoredPosition = Vector2.zero;
                var divImg = divGo.AddComponent<Image>();
                divImg.color = isPrimary ? new Color(0.118f, 0.165f, 0.200f, 0.30f) : new Color(1f, 1f, 1f, 0.25f);
                divImg.raycastTarget = false;
            }

            var txtGo = CreateUIObject("Text", btnGo.transform);
            var txtRt = txtGo.GetComponent<RectTransform>();
            SetAnchors(txtRt, 0.16f, 0f, 0.98f, 1f);
            var tmp = txtGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            if (font != null) tmp.font = font;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold | FontStyles.Italic;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.raycastTarget = false;

            return btn;
        }

        private static Button CreateDifficultyButton(string name, Transform parent, string label, TMP_FontAsset font)
        {
            var btnGo = CreateUIObject(name, parent);
            var img = btnGo.AddComponent<Image>();
            img.color = new Color(0.18f, 0.22f, 0.25f, 1f);

            var btn = btnGo.AddComponent<Button>();
            btnGo.AddComponent<ButtonStateFX>();

            var txtGo = CreateUIObject("Text", btnGo.transform);
            SetStretch(txtGo.GetComponent<RectTransform>());
            var tmp = txtGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            if (font != null) tmp.font = font;
            tmp.fontSize = 20;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            return btn;
        }

        private static DifficultyModalController BuildDifficultyModal(
            Transform parent,
            TMP_FontAsset fontDisplay,
            TMP_FontAsset fontBody,
            TMP_FontAsset fontUI)
        {
            var modalRoot = CreateUIObject("DifficultyModal", parent);
            SetStretch(modalRoot.GetComponent<RectTransform>());
            var rootCg = modalRoot.AddComponent<CanvasGroup>();

            var dimmerGo = CreateUIObject("Dimmer", modalRoot.transform);
            SetStretch(dimmerGo.GetComponent<RectTransform>());
            var dimmerImg = dimmerGo.AddComponent<Image>();
            dimmerImg.color = new Color(0f, 0f, 0f, 0.55f);
            var dimmerCg = dimmerGo.AddComponent<CanvasGroup>();

            var panelGo = CreateUIObject("Panel", modalRoot.transform);
            var panelRt = panelGo.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.27f, 0.24f);
            panelRt.anchorMax = new Vector2(0.73f, 0.76f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            var panelImg = panelGo.AddComponent<Image>();
            panelImg.color = new Color(0.914f, 0.929f, 0.941f, 1f);
            var panelCg = panelGo.AddComponent<CanvasGroup>();

            var titleGo = CreateUIObject("Title", panelGo.transform);
            var titleRt = titleGo.GetComponent<RectTransform>();
            SetAnchors(titleRt, 0.05f, 0.88f, 0.95f, 0.98f);
            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "CHOOSE YOUR DIFFICULTY";
            if (fontDisplay != null) titleTmp.font = fontDisplay;
            titleTmp.fontSize = 28;
            titleTmp.fontStyle = FontStyles.Bold | FontStyles.Italic;
            titleTmp.color = new Color(0.118f, 0.165f, 0.200f, 1f);
            titleTmp.alignment = TextAlignmentOptions.Center;

            var subGo = CreateUIObject("Subtitle", panelGo.transform);
            var subRt = subGo.GetComponent<RectTransform>();
            SetAnchors(subRt, 0.05f, 0.80f, 0.95f, 0.87f);
            var subTmp = subGo.AddComponent<TextMeshProUGUI>();
            subTmp.text = "Pick wisely.";
            if (fontBody != null) subTmp.font = fontBody;
            subTmp.fontSize = 18;
            subTmp.color = new Color(0.290f, 0.345f, 0.388f, 1f);
            subTmp.alignment = TextAlignmentOptions.Center;

            Sprite iconEasy = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Icon_Diff_Easy.png");
            Sprite iconNormal = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Icon_Diff_Normal.png");
            Sprite iconHard = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Icon_Diff_Hard.png");
            Sprite iconNightmare = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Icon_Diff_Nightmare.png");

            var easyCard = CreateCard(panelGo.transform, Difficulty.Easy, "EASY", "A gentle warm-up.", iconEasy, 0.04f, 0.2475f, fontDisplay, fontBody);
            var normalCard = CreateCard(panelGo.transform, Difficulty.Normal, "NORMAL", "The standard experience.", iconNormal, 0.2775f, 0.485f, fontDisplay, fontBody);
            var hardCard = CreateCard(panelGo.transform, Difficulty.Hard, "HARD", "For skilled dodgers.", iconHard, 0.515f, 0.7225f, fontDisplay, fontBody);
            var nightmareCard = CreateCard(panelGo.transform, Difficulty.Nightmare, "NIGHTMARE", "Only the brave.", iconNightmare, 0.7525f, 0.96f, fontDisplay, fontBody);

            var backBtnGo = CreateUIObject("BackButton", panelGo.transform);
            var backRt = backBtnGo.GetComponent<RectTransform>();
            SetAnchors(backRt, 0.30f, 0.04f, 0.70f, 0.15f);
            var backImg = backBtnGo.AddComponent<Image>();
            backImg.color = new Color(0.184f, 0.220f, 0.243f, 1f);
            var backBtn = backBtnGo.AddComponent<Button>();
            backBtn.targetGraphic = backImg;
            var backTxtGo = CreateUIObject("Text", backBtnGo.transform);
            SetStretch(backTxtGo.GetComponent<RectTransform>());
            var backTmp = backTxtGo.AddComponent<TextMeshProUGUI>();
            backTmp.text = "BACK";
            if (fontUI != null) backTmp.font = fontUI;
            backTmp.fontSize = 20;
            backTmp.fontStyle = FontStyles.Bold;
            backTmp.color = Color.white;
            backTmp.alignment = TextAlignmentOptions.Center;

            SetupButtonNavigation(easyCard.GetComponent<Button>(), null, normalCard.GetComponent<Button>(), null, backBtn);
            SetupButtonNavigation(normalCard.GetComponent<Button>(), easyCard.GetComponent<Button>(), hardCard.GetComponent<Button>(), null, backBtn);
            SetupButtonNavigation(hardCard.GetComponent<Button>(), normalCard.GetComponent<Button>(), nightmareCard.GetComponent<Button>(), null, backBtn);
            SetupButtonNavigation(nightmareCard.GetComponent<Button>(), hardCard.GetComponent<Button>(), null, null, backBtn);
            SetupButtonNavigation(backBtn, null, null, normalCard.GetComponent<Button>(), null);

            var controller = modalRoot.AddComponent<DifficultyModalController>();
            var ctrlSo = new SerializedObject(controller);
            ctrlSo.FindProperty("dimmerCanvasGroup").objectReferenceValue = dimmerCg;
            ctrlSo.FindProperty("dimmerImage").objectReferenceValue = dimmerImg;
            ctrlSo.FindProperty("modalPanelRect").objectReferenceValue = panelRt;
            ctrlSo.FindProperty("modalPanelCanvasGroup").objectReferenceValue = panelCg;
            ctrlSo.FindProperty("titleText").objectReferenceValue = titleTmp;
            ctrlSo.FindProperty("subtitleText").objectReferenceValue = subTmp;
            ctrlSo.FindProperty("easyCard").objectReferenceValue = easyCard;
            ctrlSo.FindProperty("normalCard").objectReferenceValue = normalCard;
            ctrlSo.FindProperty("hardCard").objectReferenceValue = hardCard;
            ctrlSo.FindProperty("nightmareCard").objectReferenceValue = nightmareCard;
            ctrlSo.FindProperty("backButton").objectReferenceValue = backBtn;
            var diffConfigAsset = AssetDatabase.LoadAssetAtPath<DifficultyConfig>("Assets/Resources/SO_DifficultyConfig.asset")
                                ?? AssetDatabase.LoadAssetAtPath<DifficultyConfig>("Assets/ScriptableObjects/SO_DifficultyConfig.asset");
            ctrlSo.FindProperty("difficultyConfig").objectReferenceValue = diffConfigAsset;
            ctrlSo.ApplyModifiedProperties();

            string prefabPath = "Assets/Prefabs/UI/DifficultyModal.prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(modalRoot, prefabPath, InteractionMode.AutomatedAction);

            modalRoot.SetActive(false);
            return controller;
        }

        private static DifficultyCardUI CreateCard(
            Transform parent,
            Difficulty diff,
            string name,
            string desc,
            Sprite icon,
            float minX,
            float maxX,
            TMP_FontAsset fontDisplay,
            TMP_FontAsset fontBody)
        {
            var cardGo = CreateUIObject("Card_" + diff.ToString(), parent);
            var cardRt = cardGo.GetComponent<RectTransform>();
            SetAnchors(cardRt, minX, 0.20f, maxX, 0.78f);

            var btn = cardGo.AddComponent<Button>();

            var glowGo = CreateUIObject("Glow", cardGo.transform);
            var glowRt = glowGo.GetComponent<RectTransform>();
            glowRt.anchorMin = new Vector2(-0.06f, -0.04f);
            glowRt.anchorMax = new Vector2(1.06f, 1.04f);
            glowRt.offsetMin = Vector2.zero;
            glowRt.offsetMax = Vector2.zero;
            var glowImg = glowGo.AddComponent<Image>();
            glowImg.color = new Color(0.239f, 0.863f, 0.165f, 0.40f);
            glowGo.SetActive(false);

            var bgGo = CreateUIObject("Background", cardGo.transform);
            SetStretch(bgGo.GetComponent<RectTransform>());
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.18f, 0.22f, 0.25f, 1f);
            btn.targetGraphic = bgImg;

            var contentGo = CreateUIObject("Content", cardGo.transform);
            var contentRt = contentGo.GetComponent<RectTransform>();
            SetStretch(contentRt);

            var iconGo = CreateUIObject("Icon", contentGo.transform);
            var iconRt = iconGo.GetComponent<RectTransform>();
            SetAnchors(iconRt, 0.15f, 0.44f, 0.85f, 0.92f);
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = icon;
            iconImg.color = Color.white;
            iconImg.preserveAspect = true;

            var nameGo = CreateUIObject("Name", contentGo.transform);
            var nameRt = nameGo.GetComponent<RectTransform>();
            SetAnchors(nameRt, 0.05f, 0.26f, 0.95f, 0.42f);
            var nameTmp = nameGo.AddComponent<TextMeshProUGUI>();
            nameTmp.text = name;
            if (fontDisplay != null) nameTmp.font = fontDisplay;
            nameTmp.fontSize = 20;
            nameTmp.fontStyle = FontStyles.Bold | FontStyles.Italic;
            nameTmp.color = Color.white;
            nameTmp.alignment = TextAlignmentOptions.Center;

            var descGo = CreateUIObject("Description", contentGo.transform);
            var descRt = descGo.GetComponent<RectTransform>();
            SetAnchors(descRt, 0.05f, 0.06f, 0.95f, 0.24f);
            var descTmp = descGo.AddComponent<TextMeshProUGUI>();
            descTmp.text = desc;
            if (fontBody != null) descTmp.font = fontBody;
            descTmp.fontSize = 13;
            descTmp.color = new Color(0.788f, 0.820f, 0.843f, 1f);
            descTmp.alignment = TextAlignmentOptions.Center;
            descTmp.textWrappingMode = TextWrappingModes.Normal;

            var cardUI = cardGo.AddComponent<DifficultyCardUI>();
            var cardSo = new SerializedObject(cardUI);
            cardSo.FindProperty("difficulty").enumValueIndex = (int)diff;
            cardSo.FindProperty("cardBackground").objectReferenceValue = bgImg;
            cardSo.FindProperty("glowImage").objectReferenceValue = glowImg;
            cardSo.FindProperty("iconImage").objectReferenceValue = iconImg;
            cardSo.FindProperty("nameText").objectReferenceValue = nameTmp;
            cardSo.FindProperty("descriptionText").objectReferenceValue = descTmp;
            cardSo.FindProperty("shakeTarget").objectReferenceValue = contentRt;
            cardSo.ApplyModifiedProperties();

            return cardUI;
        }

        private static void SetupButtonNavigation(Button btn, Button left, Button right, Button up, Button down)
        {
            if (btn == null) return;
            var nav = btn.navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnLeft = left;
            nav.selectOnRight = right;
            nav.selectOnUp = up;
            nav.selectOnDown = down;
            btn.navigation = nav;
        }

        private static GameObject CreateModalPanel(string name, Transform parent, Sprite bgSprite, Vector2 size)
        {
            var modalGo = CreateUIObject(name, parent);
            var rt = modalGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;

            var img = modalGo.AddComponent<Image>();
            if (bgSprite != null)
            {
                img.sprite = bgSprite;
                img.type = Image.Type.Sliced;
            }
            img.color = new Color(0.96f, 0.97f, 0.98f, 0.98f);

            modalGo.SetActive(false);
            return modalGo;
        }

        private static Button SetupModalHeader(Transform modal, string title, TMP_FontAsset font)
        {
            var titleGo = CreateUIObject("ModalTitle", modal);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.06f, 0.88f);
            titleRt.anchorMax = new Vector2(0.85f, 0.96f);
            titleRt.offsetMin = Vector2.zero;
            titleRt.offsetMax = Vector2.zero;
            var tmp = titleGo.AddComponent<TextMeshProUGUI>();
            tmp.text = title;
            if (font != null) tmp.font = font;
            tmp.fontSize = 36;
            tmp.fontStyle = FontStyles.Bold | FontStyles.Italic;
            tmp.color = new Color(0.118f, 0.165f, 0.200f, 1f);

            var closeGo = CreateUIObject("CloseButton", modal);
            var closeRt = closeGo.GetComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(0.90f, 0.88f);
            closeRt.anchorMax = new Vector2(0.96f, 0.95f);
            closeRt.offsetMin = Vector2.zero;
            closeRt.offsetMax = Vector2.zero;
            var closeImg = closeGo.AddComponent<Image>();
            closeImg.color = new Color(0.8f, 0.25f, 0.25f, 0.8f);
            var closeBtn = closeGo.AddComponent<Button>();

            var closeTxtGo = CreateUIObject("Text", closeGo.transform);
            SetStretch(closeTxtGo.GetComponent<RectTransform>());
            var closeTmp = closeTxtGo.AddComponent<TextMeshProUGUI>();
            closeTmp.text = "X";
            if (font != null) closeTmp.font = font;
            closeTmp.fontSize = 24;
            closeTmp.fontStyle = FontStyles.Bold;
            closeTmp.color = Color.white;
            closeTmp.alignment = TextAlignmentOptions.Center;

            return closeBtn;
        }

        private static Button CreateModalBackButton(Transform modal, TMP_FontAsset font)
        {
            var backGo = CreateUIObject("BackButton", modal);
            var backRt = backGo.GetComponent<RectTransform>();
            backRt.anchorMin = new Vector2(0.70f, 0.05f);
            backRt.anchorMax = new Vector2(0.95f, 0.14f);
            backRt.offsetMin = Vector2.zero;
            backRt.offsetMax = Vector2.zero;
            var backImg = backGo.AddComponent<Image>();
            backImg.color = new Color(0.184f, 0.220f, 0.243f, 1f);
            var backBtn = backGo.AddComponent<Button>();
            backGo.AddComponent<ButtonStateFX>();

            var backTxtGo = CreateUIObject("Text", backGo.transform);
            SetStretch(backTxtGo.GetComponent<RectTransform>());
            var backTmp = backTxtGo.AddComponent<TextMeshProUGUI>();
            backTmp.text = "BACK";
            if (font != null) backTmp.font = font;
            backTmp.fontSize = 24;
            backTmp.fontStyle = FontStyles.Bold;
            backTmp.color = Color.white;
            backTmp.alignment = TextAlignmentOptions.Center;

            return backBtn;
        }

        public static void EnsurePanelModalLightSprite()
        {
            string path = "Assets/Art/UI/Panel_ModalLight.png";
            int size = 96;
            float radius = 12f;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color borderCol = new Color(0.722f, 0.761f, 0.792f, 1f);
            Color bevelCol = new Color(1f, 1f, 1f, 1f);
            Color gradTop = new Color(0.914f, 0.929f, 0.941f, 1f);
            Color gradBottom = new Color(0.827f, 0.851f, 0.871f, 1f);

            for (int y = 0; y < size; y++)
            {
                float v = (float)y / (size - 1);
                Color fillCol = Color.Lerp(gradBottom, gradTop, v);
                for (int x = 0; x < size; x++)
                {
                    float dx = 0f;
                    if (x < radius) dx = radius - x;
                    else if (x > size - 1 - radius) dx = x - (size - 1 - radius);

                    float dy = 0f;
                    if (y < radius) dy = radius - y;
                    else if (y > size - 1 - radius) dy = y - (size - 1 - radius);

                    float dist = 0f;
                    if (dx > 0f && dy > 0f)
                    {
                        dist = Mathf.Sqrt(dx * dx + dy * dy) - radius;
                    }
                    else
                    {
                        float edgeDist = Mathf.Min(Mathf.Min(x, size - 1 - x), Mathf.Min(y, size - 1 - y));
                        dist = -edgeDist;
                    }

                    if (dist > 0.5f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else if (dist > -0.5f)
                    {
                        float alpha = Mathf.Clamp01(0.5f - dist);
                        tex.SetPixel(x, y, new Color(borderCol.r, borderCol.g, borderCol.b, alpha));
                    }
                    else if (dist > -1.5f)
                    {
                        tex.SetPixel(x, y, borderCol);
                    }
                    else if (dist > -3.5f)
                    {
                        tex.SetPixel(x, y, bevelCol);
                    }
                    else
                    {
                        tex.SetPixel(x, y, fillCol);
                    }
                }
            }
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteBorder = new Vector4(24, 24, 24, 24);
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
            AssetDatabase.Refresh();
        }

        private static SettingsModalController BuildRedesignedSettingsModal(
            Transform parent,
            TMP_FontAsset fontDisplay,
            TMP_FontAsset fontBody,
            TMP_FontAsset fontUI,
            Sprite panelSprite,
            Sprite trackSprite,
            Sprite fillSprite,
            Sprite handleSprite,
            Sprite toggleTrackSprite,
            Sprite toggleKnobSprite,
            Sprite closeDangerSprite,
            Sprite btnDarkSprite,
            out Button closeBtnOut,
            out MenuSettingsUI menuSettingsUIOut)
        {
            var modalRootGo = CreateUIObject("SettingsModal", parent);
            SetStretch(modalRootGo.GetComponent<RectTransform>());
            var settingsCtrl = modalRootGo.AddComponent<SettingsModalController>();

            var dimmerGo = CreateUIObject("DimmerOverlay", modalRootGo.transform);
            SetStretch(dimmerGo.GetComponent<RectTransform>());
            var dimmerCg = dimmerGo.AddComponent<CanvasGroup>();
            dimmerCg.blocksRaycasts = true;
            dimmerCg.interactable = true;
            var dimmerImg = dimmerGo.AddComponent<Image>();
            dimmerImg.color = new Color(0f, 0f, 0f, 0.55f);
            dimmerImg.raycastTarget = true;
            var dimmerBtn = dimmerGo.AddComponent<Button>();

            var panelGo = CreateUIObject("SettingsPanel", modalRootGo.transform);
            var panelRt = panelGo.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.pivot = new Vector2(0.5f, 0.5f);
            panelRt.anchoredPosition = Vector2.zero;
            panelRt.sizeDelta = new Vector2(1152f, 800f);

            var panelCg = panelGo.AddComponent<CanvasGroup>();
            var panelImg = panelGo.AddComponent<Image>();
            if (panelSprite != null)
            {
                panelImg.sprite = panelSprite;
                panelImg.type = Image.Type.Sliced;
            }
            panelImg.color = Color.white;

            var panelShadow = panelGo.AddComponent<Shadow>();
            panelShadow.effectColor = new Color(0f, 0f, 0f, 0.15f);
            panelShadow.effectDistance = new Vector2(0f, -4f);

            var menuSettingsUI = panelGo.AddComponent<MenuSettingsUI>();
            menuSettingsUIOut = menuSettingsUI;

            var titleGo = CreateUIObject("Title", panelGo.transform);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 1f);
            titleRt.anchorMax = new Vector2(0f, 1f);
            titleRt.pivot = new Vector2(0f, 1f);
            titleRt.anchoredPosition = new Vector2(32f, -32f);
            titleRt.sizeDelta = new Vector2(700f, 72f);

            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "SETTINGS";
            if (fontDisplay != null) titleTmp.font = fontDisplay;
            titleTmp.fontSize = 48;
            titleTmp.fontStyle = FontStyles.Bold | FontStyles.Italic;
            titleTmp.color = new Color(0.118f, 0.165f, 0.200f, 1f);
            titleTmp.alignment = TextAlignmentOptions.MidlineLeft;
            titleTmp.raycastTarget = false;

            var closeGo = CreateUIObject("CloseButton", panelGo.transform);
            var closeRt = closeGo.GetComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(1f, 1f);
            closeRt.anchorMax = new Vector2(1f, 1f);
            closeRt.pivot = new Vector2(1f, 1f);
            closeRt.anchoredPosition = new Vector2(-32f, -32f);
            closeRt.sizeDelta = new Vector2(56f, 56f);

            var closeImg = closeGo.AddComponent<Image>();
            if (closeDangerSprite != null)
            {
                closeImg.sprite = closeDangerSprite;
                closeImg.type = Image.Type.Sliced;
            }
            closeImg.color = new Color(0.898f, 0.282f, 0.302f, 0.90f);
            closeImg.raycastTarget = true;

            var closeBtn = closeGo.AddComponent<Button>();
            closeGo.AddComponent<CloseButtonFX>();

            var closeTxtGo = CreateUIObject("Glyph", closeGo.transform);
            SetStretch(closeTxtGo.GetComponent<RectTransform>());
            var closeTmp = closeTxtGo.AddComponent<TextMeshProUGUI>();
            closeTmp.text = "X";
            if (fontUI != null) closeTmp.font = fontUI;
            closeTmp.fontSize = 24;
            closeTmp.fontStyle = FontStyles.Bold;
            closeTmp.color = Color.white;
            closeTmp.alignment = TextAlignmentOptions.Center;
            closeTmp.raycastTarget = false;
            closeBtnOut = closeBtn;

            var headerDivGo = CreateUIObject("HeaderDivider", panelGo.transform);
            var headerDivRt = headerDivGo.GetComponent<RectTransform>();
            headerDivRt.anchorMin = new Vector2(0f, 1f);
            headerDivRt.anchorMax = new Vector2(1f, 1f);
            headerDivRt.pivot = new Vector2(0.5f, 1f);
            headerDivRt.anchoredPosition = new Vector2(0f, -104f);
            headerDivRt.sizeDelta = new Vector2(-64f, 1f);
            var headerDivImg = headerDivGo.AddComponent<Image>();
            headerDivImg.color = new Color(0.722f, 0.761f, 0.792f, 1f);
            headerDivImg.raycastTarget = false;

            var resetGo = CreateUIObject("ResetDefaultsButton", panelGo.transform);
            var resetRt = resetGo.GetComponent<RectTransform>();
            resetRt.anchorMin = new Vector2(0f, 0f);
            resetRt.anchorMax = new Vector2(0f, 0f);
            resetRt.pivot = new Vector2(0f, 0f);
            resetRt.anchoredPosition = new Vector2(32f, 32f);
            resetRt.sizeDelta = new Vector2(460f, 56f);

            var resetImg = resetGo.AddComponent<Image>();
            if (btnDarkSprite != null)
            {
                resetImg.sprite = btnDarkSprite;
                resetImg.type = Image.Type.Sliced;
            }
            resetImg.color = Color.white;
            var resetBtn = resetGo.AddComponent<Button>();
            resetGo.AddComponent<ButtonStateFX>();

            var resetNormalGo = CreateUIObject("NormalState", resetGo.transform);
            SetStretch(resetNormalGo.GetComponent<RectTransform>());
            var resetTmp = resetNormalGo.AddComponent<TextMeshProUGUI>();
            resetTmp.text = "RESET TO DEFAULTS";
            if (fontDisplay != null) resetTmp.font = fontDisplay;
            resetTmp.fontSize = 24;
            resetTmp.fontStyle = FontStyles.Bold;
            resetTmp.color = Color.white;
            resetTmp.alignment = TextAlignmentOptions.Center;
            resetTmp.raycastTarget = false;

            var resetConfirmGo = CreateUIObject("ConfirmState", resetGo.transform);
            SetStretch(resetConfirmGo.GetComponent<RectTransform>());
            var confirmHlg = resetConfirmGo.AddComponent<HorizontalLayoutGroup>();
            confirmHlg.childAlignment = TextAnchor.MiddleCenter;
            confirmHlg.spacing = 10f;
            confirmHlg.childControlWidth = false;
            confirmHlg.childControlHeight = false;

            var confirmPromptGo = CreateUIObject("Prompt", resetConfirmGo.transform);
            var confirmPromptRt = confirmPromptGo.GetComponent<RectTransform>();
            confirmPromptRt.sizeDelta = new Vector2(190f, 40f);
            var confirmPromptTmp = confirmPromptGo.AddComponent<TextMeshProUGUI>();
            confirmPromptTmp.text = "Reset all settings?";
            if (fontBody != null) confirmPromptTmp.font = fontBody;
            confirmPromptTmp.fontSize = 20;
            confirmPromptTmp.fontStyle = FontStyles.Bold;
            confirmPromptTmp.color = Color.white;
            confirmPromptTmp.alignment = TextAlignmentOptions.MidlineRight;
            confirmPromptTmp.raycastTarget = false;

            var yesGo = CreateUIObject("YesButton", resetConfirmGo.transform);
            var yesRt = yesGo.GetComponent<RectTransform>();
            yesRt.sizeDelta = new Vector2(64f, 36f);
            var yesImg = yesGo.AddComponent<Image>();
            if (btnDarkSprite != null)
            {
                yesImg.sprite = btnDarkSprite;
                yesImg.type = Image.Type.Sliced;
            }
            yesImg.color = new Color(0.2f, 0.35f, 0.2f, 1f);
            var yesBtn = yesGo.AddComponent<Button>();
            yesGo.AddComponent<ButtonStateFX>();
            var yesTxtGo = CreateUIObject("Label", yesGo.transform);
            SetStretch(yesTxtGo.GetComponent<RectTransform>());
            var yesTmp = yesTxtGo.AddComponent<TextMeshProUGUI>();
            yesTmp.text = "YES";
            if (fontDisplay != null) yesTmp.font = fontDisplay;
            yesTmp.fontSize = 20;
            yesTmp.fontStyle = FontStyles.Bold;
            yesTmp.color = new Color(0.239f, 0.863f, 0.165f, 1f);
            yesTmp.alignment = TextAlignmentOptions.Center;
            yesTmp.raycastTarget = false;

            var slashGo = CreateUIObject("Slash", resetConfirmGo.transform);
            var slashRt = slashGo.GetComponent<RectTransform>();
            slashRt.sizeDelta = new Vector2(16f, 40f);
            var slashTmp = slashGo.AddComponent<TextMeshProUGUI>();
            slashTmp.text = "/";
            if (fontBody != null) slashTmp.font = fontBody;
            slashTmp.fontSize = 20;
            slashTmp.fontStyle = FontStyles.Bold;
            slashTmp.color = new Color(0.722f, 0.761f, 0.792f, 1f);
            slashTmp.alignment = TextAlignmentOptions.Center;
            slashTmp.raycastTarget = false;

            var noGo = CreateUIObject("NoButton", resetConfirmGo.transform);
            var noRt = noGo.GetComponent<RectTransform>();
            noRt.sizeDelta = new Vector2(64f, 36f);
            var noImg = noGo.AddComponent<Image>();
            if (btnDarkSprite != null)
            {
                noImg.sprite = btnDarkSprite;
                noImg.type = Image.Type.Sliced;
            }
            noImg.color = new Color(0.35f, 0.2f, 0.2f, 1f);
            var noBtn = noGo.AddComponent<Button>();
            noGo.AddComponent<ButtonStateFX>();
            var noTxtGo = CreateUIObject("Label", noGo.transform);
            SetStretch(noTxtGo.GetComponent<RectTransform>());
            var noTmp = noTxtGo.AddComponent<TextMeshProUGUI>();
            noTmp.text = "NO";
            if (fontDisplay != null) noTmp.font = fontDisplay;
            noTmp.fontSize = 20;
            noTmp.fontStyle = FontStyles.Bold;
            noTmp.color = new Color(0.898f, 0.282f, 0.302f, 1f);
            noTmp.alignment = TextAlignmentOptions.Center;
            noTmp.raycastTarget = false;

            resetConfirmGo.SetActive(false);

            var backGo = CreateUIObject("BackButton", panelGo.transform);
            var backRt = backGo.GetComponent<RectTransform>();
            backRt.anchorMin = new Vector2(1f, 0f);
            backRt.anchorMax = new Vector2(1f, 0f);
            backRt.pivot = new Vector2(1f, 0f);
            backRt.anchoredPosition = new Vector2(-32f, 32f);
            backRt.sizeDelta = new Vector2(200f, 56f);

            var backImg = backGo.AddComponent<Image>();
            if (btnDarkSprite != null)
            {
                backImg.sprite = btnDarkSprite;
                backImg.type = Image.Type.Sliced;
            }
            backImg.color = Color.white;
            var backBtn = backGo.AddComponent<Button>();
            backGo.AddComponent<ButtonStateFX>();

            var backTxtGo = CreateUIObject("Label", backGo.transform);
            SetStretch(backTxtGo.GetComponent<RectTransform>());
            var backTmp = backTxtGo.AddComponent<TextMeshProUGUI>();
            backTmp.text = "BACK";
            if (fontDisplay != null) backTmp.font = fontDisplay;
            backTmp.fontSize = 24;
            backTmp.fontStyle = FontStyles.Bold;
            backTmp.color = Color.white;
            backTmp.alignment = TextAlignmentOptions.Center;
            backTmp.raycastTarget = false;

            var footerDivGo = CreateUIObject("FooterDivider", panelGo.transform);
            var footerDivRt = footerDivGo.GetComponent<RectTransform>();
            footerDivRt.anchorMin = new Vector2(0f, 0f);
            footerDivRt.anchorMax = new Vector2(1f, 0f);
            footerDivRt.pivot = new Vector2(0.5f, 0f);
            footerDivRt.anchoredPosition = new Vector2(0f, 104f);
            footerDivRt.sizeDelta = new Vector2(-64f, 1f);
            var footerDivImg = footerDivGo.AddComponent<Image>();
            footerDivImg.color = new Color(0.722f, 0.761f, 0.792f, 1f);
            footerDivImg.raycastTarget = false;

            var scrollGo = CreateUIObject("ScrollArea", panelGo.transform);
            var scrollRt = scrollGo.GetComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0f, 0f);
            scrollRt.anchorMax = new Vector2(1f, 1f);
            scrollRt.offsetMin = new Vector2(32f, 116f);
            scrollRt.offsetMax = new Vector2(-32f, -106f);

            var scrollRect = scrollGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.inertia = true;
            scrollRect.scrollSensitivity = 30f;

            var viewGo = CreateUIObject("Viewport", scrollGo.transform);
            var viewRt = viewGo.GetComponent<RectTransform>();
            SetStretch(viewRt);
            viewRt.offsetMin = Vector2.zero;
            viewRt.offsetMax = new Vector2(-16f, 0f);
            viewGo.AddComponent<RectMask2D>();
            scrollRect.viewport = viewRt;

            var scrollbarGo = CreateUIObject("VerticalScrollbar", scrollGo.transform);
            var scrollbarRt = scrollbarGo.GetComponent<RectTransform>();
            scrollbarRt.anchorMin = new Vector2(1f, 0f);
            scrollbarRt.anchorMax = new Vector2(1f, 1f);
            scrollbarRt.pivot = new Vector2(1f, 0.5f);
            scrollbarRt.anchoredPosition = Vector2.zero;
            scrollbarRt.sizeDelta = new Vector2(8f, 0f);

            var scrollbarTrackImg = scrollbarGo.AddComponent<Image>();
            scrollbarTrackImg.color = new Color(0.788f, 0.820f, 0.843f, 0.35f);

            var slidingAreaGo = CreateUIObject("Sliding Area", scrollbarGo.transform);
            var slidingAreaRt = slidingAreaGo.GetComponent<RectTransform>();
            SetStretch(slidingAreaRt);

            var scrollHandleGo = CreateUIObject("Handle", slidingAreaGo.transform);
            var scrollHandleRt = scrollHandleGo.GetComponent<RectTransform>();
            SetStretch(scrollHandleRt);
            var scrollHandleImg = scrollHandleGo.AddComponent<Image>();
            scrollHandleImg.color = new Color(0.722f, 0.761f, 0.792f, 1f);

            var scrollbar = scrollbarGo.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollbar.targetGraphic = scrollHandleImg;
            scrollbar.handleRect = scrollHandleRt;

            scrollRect.verticalScrollbar = scrollbar;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;

            var contentGo = CreateUIObject("Content", viewGo.transform);
            var contentRt = contentGo.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.sizeDelta = new Vector2(0f, 0f);
            scrollRect.content = contentRt;

            var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 24f;
            vlg.padding = new RectOffset(0, 0, 24, 24);
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = contentGo.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var audioRows = CreateSectionGroup("AUDIO", "AUDIO", contentGo.transform, fontDisplay ?? fontUI, 290f);
            var masterSlider = CreateRedesignedSliderRow("MasterVolumeRow", audioRows, "Master Volume", fontBody, fontUI, trackSprite, fillSprite, handleSprite, 0f, 1f, 1f, false);
            var musicSlider = CreateRedesignedSliderRow("MusicVolumeRow", audioRows, "Music Volume", fontBody, fontUI, trackSprite, fillSprite, handleSprite, 0f, 1f, 1f, false);
            var sfxSlider = CreateRedesignedSliderRow("SfxVolumeRow", audioRows, "SFX Volume", fontBody, fontUI, trackSprite, fillSprite, handleSprite, 0f, 1f, 1f, false);

            var displayRows = CreateSectionGroup("DISPLAY", "DISPLAY", contentGo.transform, fontDisplay ?? fontUI, 290f);
            var brightSlider = CreateRedesignedSliderRow("BrightnessRow", displayRows, "Brightness", fontBody, fontUI, trackSprite, fillSprite, handleSprite, 0.8f, 1.2f, 1f, true);
            var motionToggle = CreateRedesignedToggleRow("ReducedMotionRow", displayRows, "Reduced Motion", fontBody, fontUI, toggleTrackSprite, toggleKnobSprite, false);
            var shakeToggle = CreateRedesignedToggleRow("ScreenShakeRow", displayRows, "Screen Shake", fontBody, fontUI, toggleTrackSprite, toggleKnobSprite, true);

            var cameraRows = CreateSectionGroup("CAMERA", "CAMERA", contentGo.transform, fontDisplay ?? fontUI, 110f);
            var invertYToggle = CreateRedesignedToggleRow("CameraInvertYRow", cameraRows, "Invert Camera Y", fontBody, fontUI, toggleTrackSprite, toggleKnobSprite, false);

            settingsCtrl.SetReferences(
                dimmerCg, dimmerBtn,
                panelRt, panelCg,
                closeBtn,
                masterSlider, musicSlider, sfxSlider,
                brightSlider,
                motionToggle, shakeToggle, invertYToggle,
                resetBtn, backBtn,
                scrollRect,
                scrollbar,
                resetNormalGo,
                resetConfirmGo,
                yesBtn,
                noBtn);

            var ctrlSo = new SerializedObject(settingsCtrl);
            var scrollProp = ctrlSo.FindProperty("scrollRect");
            if (scrollProp != null) scrollProp.objectReferenceValue = scrollRect;
            var vScrollProp = ctrlSo.FindProperty("verticalScrollbar");
            if (vScrollProp != null) vScrollProp.objectReferenceValue = scrollbar;
            var normProp = ctrlSo.FindProperty("resetNormalState");
            if (normProp != null) normProp.objectReferenceValue = resetNormalGo;
            var confProp = ctrlSo.FindProperty("resetConfirmState");
            if (confProp != null) confProp.objectReferenceValue = resetConfirmGo;
            var yesProp = ctrlSo.FindProperty("resetConfirmYesBtn");
            if (yesProp != null) yesProp.objectReferenceValue = yesBtn;
            var noProp = ctrlSo.FindProperty("resetConfirmNoBtn");
            if (noProp != null) noProp.objectReferenceValue = noBtn;
            ctrlSo.ApplyModifiedProperties();

            var setSo = new SerializedObject(menuSettingsUI);
            setSo.FindProperty("masterSlider").objectReferenceValue = masterSlider;
            setSo.FindProperty("musicSlider").objectReferenceValue = musicSlider;
            setSo.FindProperty("sfxSlider").objectReferenceValue = sfxSlider;
            setSo.FindProperty("brightnessSlider").objectReferenceValue = brightSlider;
            setSo.FindProperty("resetButton").objectReferenceValue = resetBtn;
            setSo.FindProperty("backButton").objectReferenceValue = backBtn;
            setSo.ApplyModifiedProperties();

            if (!Directory.Exists("Assets/Prefabs")) Directory.CreateDirectory("Assets/Prefabs");
            PrefabUtility.SaveAsPrefabAsset(modalRootGo, "Assets/Prefabs/SettingsModal.prefab");
            Debug.Log("[MainMenuBuilder] Saved Assets/Prefabs/SettingsModal.prefab successfully!");

            modalRootGo.SetActive(false);
            return settingsCtrl;
        }

        private static Transform CreateSectionGroup(string name, string headerText, Transform parent, TMP_FontAsset font, float minHeight)
        {
            var sectionGo = CreateUIObject($"Section_{name}", parent);
            var secLe = sectionGo.AddComponent<LayoutElement>();
            secLe.minHeight = minHeight;
            secLe.preferredHeight = minHeight;

            var secVlg = sectionGo.AddComponent<VerticalLayoutGroup>();
            secVlg.spacing = 10f;
            secVlg.padding = new RectOffset(0, 0, 0, 0);
            secVlg.childControlWidth = true;
            secVlg.childControlHeight = true;
            secVlg.childForceExpandWidth = true;
            secVlg.childForceExpandHeight = false;

            var secCsf = sectionGo.AddComponent<ContentSizeFitter>();
            secCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var headingGo = CreateUIObject($"Heading_{name}", sectionGo.transform);
            var headLe = headingGo.AddComponent<LayoutElement>();
            headLe.preferredHeight = 32f;
            headLe.minHeight = 32f;

            var headTmp = headingGo.AddComponent<TextMeshProUGUI>();
            headTmp.text = headerText;
            if (font != null) headTmp.font = font;
            headTmp.fontSize = 26;
            headTmp.fontStyle = FontStyles.Bold | FontStyles.Italic;
            headTmp.color = new Color(0.118f, 0.165f, 0.200f, 1f);
            headTmp.alignment = TextAlignmentOptions.MidlineLeft;
            headTmp.raycastTarget = false;

            var rowsGo = CreateUIObject("Rows", sectionGo.transform);
            var rowsVlg = rowsGo.AddComponent<VerticalLayoutGroup>();
            rowsVlg.spacing = 12f;
            rowsVlg.padding = new RectOffset(0, 0, 0, 0);
            rowsVlg.childControlWidth = true;
            rowsVlg.childControlHeight = true;
            rowsVlg.childForceExpandWidth = true;
            rowsVlg.childForceExpandHeight = false;

            var rowsCsf = rowsGo.AddComponent<ContentSizeFitter>();
            rowsCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return rowsGo.transform;
        }

        private static Slider CreateRedesignedSliderRow(
            string name, Transform parent, string labelText,
            TMP_FontAsset fontBody, TMP_FontAsset fontUI,
            Sprite trackSprite, Sprite fillSprite, Sprite handleSprite,
            float minVal, float maxVal, float defaultVal, bool isBrightness = false)
        {
            var rowGo = CreateUIObject(name, parent);
            var le = rowGo.AddComponent<LayoutElement>();
            le.preferredHeight = 68f;
            le.minHeight = 68f;

            var lblGo = CreateUIObject("Label", rowGo.transform);
            var lblRt = lblGo.GetComponent<RectTransform>();
            SetAnchors(lblRt, 0f, 0f, 0.45f, 1f);
            lblRt.offsetMin = Vector2.zero;
            lblRt.offsetMax = Vector2.zero;
            var lblTmp = lblGo.AddComponent<TextMeshProUGUI>();
            lblTmp.text = labelText;
            if (fontBody != null) lblTmp.font = fontBody;
            lblTmp.fontSize = 28;
            lblTmp.fontStyle = FontStyles.Bold;
            lblTmp.color = new Color(0.118f, 0.165f, 0.200f, 1f);
            lblTmp.alignment = TextAlignmentOptions.MidlineLeft;
            lblTmp.raycastTarget = false;

            var ctrlGo = CreateUIObject("ControlZone", rowGo.transform);
            var ctrlRt = ctrlGo.GetComponent<RectTransform>();
            SetAnchors(ctrlRt, 0.45f, 0f, 1f, 1f);
            ctrlRt.offsetMin = Vector2.zero;
            ctrlRt.offsetMax = Vector2.zero;

            var valGo = CreateUIObject("ValueText", ctrlGo.transform);
            var valRt = valGo.GetComponent<RectTransform>();
            valRt.anchorMin = new Vector2(1f, 0f);
            valRt.anchorMax = new Vector2(1f, 1f);
            valRt.pivot = new Vector2(1f, 0.5f);
            valRt.anchoredPosition = Vector2.zero;
            valRt.sizeDelta = new Vector2(80f, 0f);
            var valTmp = valGo.AddComponent<TextMeshProUGUI>();
            if (fontUI != null) valTmp.font = fontUI;
            valTmp.fontSize = 26;
            valTmp.fontStyle = FontStyles.Normal;
            valTmp.color = new Color(0.118f, 0.165f, 0.200f, 1f);
            valTmp.alignment = TextAlignmentOptions.MidlineRight;
            valTmp.raycastTarget = false;

            var sliderGo = CreateUIObject("Slider", ctrlGo.transform);
            var sliderRt = sliderGo.GetComponent<RectTransform>();
            sliderRt.anchorMin = new Vector2(0f, 0f);
            sliderRt.anchorMax = new Vector2(1f, 1f);
            sliderRt.offsetMin = new Vector2(24f, 0f);
            sliderRt.offsetMax = new Vector2(-104f, 0f);

            var trackGo = CreateUIObject("Background", sliderGo.transform);
            var trackRt = trackGo.GetComponent<RectTransform>();
            trackRt.anchorMin = new Vector2(0f, 0.5f);
            trackRt.anchorMax = new Vector2(1f, 0.5f);
            trackRt.pivot = new Vector2(0.5f, 0.5f);
            trackRt.sizeDelta = new Vector2(0f, 8f);
            var trackImg = trackGo.AddComponent<Image>();
            if (trackSprite != null)
            {
                trackImg.sprite = trackSprite;
                trackImg.type = Image.Type.Sliced;
            }
            trackImg.color = new Color(0.788f, 0.820f, 0.843f, 1f);
            trackImg.raycastTarget = true;

            var fillAreaGo = CreateUIObject("Fill Area", sliderGo.transform);
            var fillAreaRt = fillAreaGo.GetComponent<RectTransform>();
            fillAreaRt.anchorMin = new Vector2(0f, 0.5f);
            fillAreaRt.anchorMax = new Vector2(1f, 0.5f);
            fillAreaRt.pivot = new Vector2(0.5f, 0.5f);
            fillAreaRt.sizeDelta = new Vector2(0f, 8f);
            fillAreaRt.offsetMin = Vector2.zero;
            fillAreaRt.offsetMax = new Vector2(-14f, 0f);

            var fillGo = CreateUIObject("Fill", fillAreaGo.transform);
            var fillRt = fillGo.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.sizeDelta = Vector2.zero;
            var fillImg = fillGo.AddComponent<Image>();
            if (fillSprite != null)
            {
                fillImg.sprite = fillSprite;
                fillImg.type = Image.Type.Sliced;
            }
            fillImg.color = new Color(0.239f, 0.863f, 0.165f, 1f);
            fillImg.raycastTarget = false;

            var handleAreaGo = CreateUIObject("Handle Slide Area", sliderGo.transform);
            var handleAreaRt = handleAreaGo.GetComponent<RectTransform>();
            handleAreaRt.anchorMin = Vector2.zero;
            handleAreaRt.anchorMax = Vector2.one;
            handleAreaRt.offsetMin = new Vector2(14f, 0f);
            handleAreaRt.offsetMax = new Vector2(-14f, 0f);

            var handleGo = CreateUIObject("Handle", handleAreaGo.transform);
            var handleRt = handleGo.GetComponent<RectTransform>();
            handleRt.sizeDelta = new Vector2(28f, 28f);
            var handleImg = handleGo.AddComponent<Image>();
            if (handleSprite != null) handleImg.sprite = handleSprite;
            handleImg.color = Color.white;
            handleImg.raycastTarget = true;

            var stepSlider = sliderGo.AddComponent<SettingsStepSlider>();
            stepSlider.fillRect = fillRt;
            stepSlider.handleRect = handleRt;
            stepSlider.targetGraphic = handleImg;
            stepSlider.direction = Slider.Direction.LeftToRight;
            stepSlider.minValue = minVal;
            stepSlider.maxValue = maxVal;
            stepSlider.value = defaultVal;
            stepSlider.SetReferences(valTmp, isBrightness, 0.05f);

            return stepSlider;
        }

        private static SettingsSwitchToggle CreateRedesignedToggleRow(
            string name, Transform parent, string labelText,
            TMP_FontAsset fontBody, TMP_FontAsset fontUI,
            Sprite trackSprite, Sprite knobSprite, bool initialVal)
        {
            var rowGo = CreateUIObject(name, parent);
            var le = rowGo.AddComponent<LayoutElement>();
            le.preferredHeight = 68f;
            le.minHeight = 68f;

            var lblGo = CreateUIObject("Label", rowGo.transform);
            var lblRt = lblGo.GetComponent<RectTransform>();
            SetAnchors(lblRt, 0f, 0f, 0.45f, 1f);
            lblRt.offsetMin = Vector2.zero;
            lblRt.offsetMax = Vector2.zero;
            var lblTmp = lblGo.AddComponent<TextMeshProUGUI>();
            lblTmp.text = labelText;
            if (fontBody != null) lblTmp.font = fontBody;
            lblTmp.fontSize = 28;
            lblTmp.fontStyle = FontStyles.Bold;
            lblTmp.color = new Color(0.118f, 0.165f, 0.200f, 1f);
            lblTmp.alignment = TextAlignmentOptions.MidlineLeft;
            lblTmp.raycastTarget = false;

            var ctrlGo = CreateUIObject("ControlZone", rowGo.transform);
            var ctrlRt = ctrlGo.GetComponent<RectTransform>();
            SetAnchors(ctrlRt, 0.45f, 0f, 1f, 1f);
            ctrlRt.offsetMin = Vector2.zero;
            ctrlRt.offsetMax = Vector2.zero;

            var valGo = CreateUIObject("ValueText", ctrlGo.transform);
            var valRt = valGo.GetComponent<RectTransform>();
            valRt.anchorMin = new Vector2(1f, 0f);
            valRt.anchorMax = new Vector2(1f, 1f);
            valRt.pivot = new Vector2(1f, 0.5f);
            valRt.anchoredPosition = Vector2.zero;
            valRt.sizeDelta = new Vector2(80f, 0f);
            var valTmp = valGo.AddComponent<TextMeshProUGUI>();
            if (fontUI != null) valTmp.font = fontUI;
            valTmp.fontSize = 26;
            valTmp.fontStyle = FontStyles.Normal;
            valTmp.color = new Color(0.118f, 0.165f, 0.200f, 1f);
            valTmp.alignment = TextAlignmentOptions.MidlineRight;
            valTmp.raycastTarget = false;
            valTmp.text = initialVal ? "On" : "Off";

            var toggleGo = CreateUIObject("SwitchToggle", ctrlGo.transform);
            var toggleRt = toggleGo.GetComponent<RectTransform>();
            toggleRt.anchorMin = new Vector2(0f, 0.5f);
            toggleRt.anchorMax = new Vector2(0f, 0.5f);
            toggleRt.pivot = new Vector2(0f, 0.5f);
            toggleRt.anchoredPosition = new Vector2(24f, 0f);
            toggleRt.sizeDelta = new Vector2(56f, 30f);

            var trackImg = toggleGo.AddComponent<Image>();
            if (trackSprite != null)
            {
                trackImg.sprite = trackSprite;
                trackImg.type = Image.Type.Sliced;
            }
            trackImg.raycastTarget = true;

            var knobGo = CreateUIObject("Knob", toggleGo.transform);
            var knobRt = knobGo.GetComponent<RectTransform>();
            knobRt.anchorMin = new Vector2(0.5f, 0.5f);
            knobRt.anchorMax = new Vector2(0.5f, 0.5f);
            knobRt.pivot = new Vector2(0.5f, 0.5f);
            knobRt.sizeDelta = new Vector2(24f, 24f);
            var knobImg = knobGo.AddComponent<Image>();
            if (knobSprite != null) knobImg.sprite = knobSprite;
            knobImg.color = Color.white;
            knobImg.raycastTarget = false;

            var switchToggle = toggleGo.AddComponent<SettingsSwitchToggle>();
            switchToggle.SetReferences(trackImg, knobRt, null, valTmp);
            switchToggle.SetIsOn(initialVal, false);

            return switchToggle;
        }

        private static void CreateContactShadow(Transform arena, Vector3 localPos, Sprite shadowSprite)
        {
            var shadowGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            shadowGo.name = "ContactShadow";
            shadowGo.transform.SetParent(arena, false);
            shadowGo.transform.localPosition = new Vector3(localPos.x, 0.005f, localPos.z);
            shadowGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            shadowGo.transform.localScale = new Vector3(2.4f, 2.4f, 1f);

            var col = shadowGo.GetComponent<Collider>();
            if (col != null) UnityEngine.Object.DestroyImmediate(col);

            var rend = shadowGo.GetComponent<MeshRenderer>();
            var shadowMat = new Material(Shader.Find("Sprites/Default"));
            if (shadowSprite != null) shadowMat.mainTexture = shadowSprite.texture;
            shadowMat.color = new Color(0f, 0f, 0f, 0.60f);
            rend.sharedMaterial = shadowMat;
            rend.sortingOrder = 0;
        }

        private static void SetupBallWithMaterial(Transform arena, string name, Vector3 localPos, Color baseColor, Color emissionColor, float emissionIntensity, Sprite shadowSprite)
        {
            CreateContactShadow(arena, localPos, shadowSprite);

            var ballGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ballGo.name = name;
            ballGo.transform.SetParent(arena, false);
            ballGo.transform.localPosition = localPos;
            ballGo.transform.localScale = Vector3.one * 1.5f;

            var col = ballGo.GetComponent<Collider>();
            if (col != null) UnityEngine.Object.DestroyImmediate(col);

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.SetColor("_BaseColor", baseColor);
            mat.color = baseColor;
            mat.SetFloat("_Smoothness", 0.4f);
            mat.SetFloat("_Metallic", 0.1f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emissionColor * emissionIntensity);

            ballGo.GetComponent<MeshRenderer>().sharedMaterial = mat;

            var ptLight = ballGo.AddComponent<Light>();
            ptLight.type = LightType.Point;
            ptLight.color = emissionColor;
            ptLight.range = 3.0f;
            ptLight.intensity = 1.0f;
        }

        private static void SetupStickyBall(Transform arena, Vector3 localPos, Sprite decalSprite, Sprite shadowSprite)
        {
            CreateContactShadow(arena, localPos, shadowSprite);

            var ballGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ballGo.name = "DecorativeStickyBall";
            ballGo.transform.SetParent(arena, false);
            ballGo.transform.localPosition = localPos;
            ballGo.transform.localScale = Vector3.one * 1.5f;

            var col = ballGo.GetComponent<Collider>();
            if (col != null) UnityEngine.Object.DestroyImmediate(col);

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            Color stickyColor = new Color(0.2f, 0.9f, 0.2f);
            mat.SetColor("_BaseColor", stickyColor);
            mat.color = stickyColor;
            mat.SetFloat("_Smoothness", 0.4f);
            mat.SetFloat("_Metallic", 0.1f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.1f, 0.8f, 0.1f) * 1.2f);
            ballGo.GetComponent<MeshRenderer>().sharedMaterial = mat;

            var ptLight = ballGo.AddComponent<Light>();
            ptLight.type = LightType.Point;
            ptLight.color = new Color(0.2f, 0.9f, 0.2f);
            ptLight.range = 3.0f;
            ptLight.intensity = 1.0f;

            if (decalSprite != null)
            {
                var puddleGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
                puddleGo.name = "GooPuddle";
                puddleGo.transform.SetParent(arena, false);
                puddleGo.transform.localPosition = new Vector3(localPos.x, 0.01f, localPos.z);
                puddleGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                puddleGo.transform.localScale = new Vector3(3.2f, 3.2f, 1f);

                var puddleCol = puddleGo.GetComponent<Collider>();
                if (puddleCol != null) UnityEngine.Object.DestroyImmediate(puddleCol);

                var rend = puddleGo.GetComponent<MeshRenderer>();
                var puddleMat = new Material(Shader.Find("Sprites/Default"));
                puddleMat.mainTexture = decalSprite.texture;
                puddleMat.color = Color.white;
                rend.sharedMaterial = puddleMat;
                rend.sortingOrder = 1;
            }
        }

        private static void SetupMenuDodger(Transform arena)
        {
            var dodgerFbx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Characters/Dodger/Dodger.fbx");
            var dodgerAvatar = AssetDatabase.LoadAssetAtPath<Avatar>("Assets/Art/Characters/Dodger/Dodger.fbx");
            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Art/Characters/Dodger/Animation/Dodger.controller");

            if (dodgerFbx == null || controller == null) return;

            var existing = GameObject.Find("Dodger_Menu") ?? GameObject.Find("Dodger");
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing);
            }

            var dodgerGo = (GameObject)PrefabUtility.InstantiatePrefab(dodgerFbx);
            dodgerGo.name = "Dodger";
            dodgerGo.transform.SetParent(arena, false);
            dodgerGo.transform.localPosition = new Vector3(0.5f, 0f, 0f);
            dodgerGo.transform.localRotation = Quaternion.Euler(0f, 160f, 0f);

            foreach (var comp in dodgerGo.GetComponentsInChildren<Rigidbody>(true)) UnityEngine.Object.DestroyImmediate(comp);
            foreach (var comp in dodgerGo.GetComponentsInChildren<Collider>(true)) UnityEngine.Object.DestroyImmediate(comp);
            foreach (var comp in dodgerGo.GetComponentsInChildren<DodgeGame.Player.PlayerController>(true)) UnityEngine.Object.DestroyImmediate(comp);

            var anim = dodgerGo.GetComponent<Animator>();
            if (anim == null) anim = dodgerGo.AddComponent<Animator>();
            anim.avatar = dodgerAvatar;
            anim.runtimeAnimatorController = controller;
            anim.applyRootMotion = false;
            anim.updateMode = AnimatorUpdateMode.Normal;
            anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            var menuAnim = dodgerGo.GetComponent<DodgeGame.Player.MenuDodgerAnimator>() ?? dodgerGo.AddComponent<DodgeGame.Player.MenuDodgerAnimator>();
        }
    }
}
