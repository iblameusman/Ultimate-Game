using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using DodgeGame.UI;
using DodgeGame.Arena;

namespace DodgeGame.Editor
{
    public static class MainMenuSceneCreator
    {
        [MenuItem("Tools/Build Main Menu Scene")]
        public static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var lightObj = new GameObject("Directional Light");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.3f;
            light.color = new Color(0.92f, 0.96f, 1f);
            lightObj.transform.rotation = Quaternion.Euler(45f, -25f, 0f);

            var camObj = new GameObject("Main Camera");
            var cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(-3.5f, 10.5f, -16f);
            camObj.transform.rotation = Quaternion.Euler(28f, 12f, 0f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.03f, 0.04f, 0.07f);
            var autoRotate = camObj.AddComponent<AutoRotate>();
            autoRotate.rotationSpeed = 5.5f;
            autoRotate.pivotPoint = new Vector3(2.5f, 0.5f, 0f);
            autoRotate.orbitAroundPivot = true;

            var volumeObj = new GameObject("Global Volume");
            var volume = volumeObj.AddComponent<Volume>();
            volume.isGlobal = true;
            var volumeProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>("Assets/Settings/MainGameVolumeProfile.asset");
            if (volumeProfile != null)
            {
                volume.sharedProfile = volumeProfile;
            }

            var arenaRoot = new GameObject("Arena");
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.parent = arenaRoot.transform;
            floor.transform.position = new Vector3(0f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(20f, 1f, 20f);
            var floorMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_ArenaFloor.mat");
            if (floorMat != null) floor.GetComponent<MeshRenderer>().material = floorMat;

            Vector3[] wallPos = { new Vector3(0, 1.5f, 10f), new Vector3(0, 1.5f, -10f), new Vector3(10f, 1.5f, 0), new Vector3(-10f, 1.5f, 0) };
            Vector3[] wallScale = { new Vector3(20, 3, 0.5f), new Vector3(20, 3, 0.5f), new Vector3(0.5f, 3, 20), new Vector3(0.5f, 3, 20) };
            var wallMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_ArenaWall.mat");
            for (int i = 0; i < 4; i++)
            {
                var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = "Wall_" + i;
                wall.transform.parent = arenaRoot.transform;
                wall.transform.position = wallPos[i];
                wall.transform.localScale = wallScale[i];
                if (wallMat != null) wall.GetComponent<MeshRenderer>().material = wallMat;
            }

            var gunMesh = FindMeshInAsset("Assets/Art/Balls/SpikedMetalBall/SpikedMetalBall_Clean.fbx", "spike1");
            var iceMesh = FindMeshInAsset("Assets/Art/Balls/IceBall/IceBall_Clean.fbx", "iceBall");
            var fireMesh = FindMeshInAsset("Assets/Art/Balls/FireBall/FireBall_Mesh.fbx", "FireBallMesh");

            var gunMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_GunBallMetal.mat");
            var fireMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_FireBall.mat");
            var stickyMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_StickyBall.mat");

            CreateDecorativeBall(arenaRoot.transform, "DecorativeSpikedBall", gunMesh, gunMat,
                new Vector3(-4f, 1.4f, 3.5f), Vector3.one * 1.5f, new Vector3(0.3f, 1f, 0.2f), 30f);

            CreateDecorativeBall(arenaRoot.transform, "DecorativeFireBall", fireMesh, fireMat,
                new Vector3(-1.5f, 1.3f, -1.5f), Vector3.one * 1.4f, new Vector3(0.5f, 1f, -0.2f), 35f);

            CreateDecorativeBall(arenaRoot.transform, "DecorativeIceBall", iceMesh, stickyMat,
                new Vector3(1.5f, 1.2f, 5.5f), Vector3.one * 1.3f, new Vector3(0.4f, 1f, 0.5f), 40f);

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            var canvasObj = new GameObject("Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();
            var menuUI = canvasObj.AddComponent<MainMenuUI>();

            var menuPanelObj = new GameObject("SideMenuPanel");
            menuPanelObj.transform.SetParent(canvasObj.transform, false);
            var panelImg = menuPanelObj.AddComponent<Image>();
            panelImg.color = new Color(0.04f, 0.07f, 0.12f, 0.88f);
            var panelRect = menuPanelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(0.36f, 1f);
            panelRect.offsetMin = new Vector2(40f, 40f);
            panelRect.offsetMax = new Vector2(0f, -40f);

            var accentBar = new GameObject("AccentBar");
            accentBar.transform.SetParent(menuPanelObj.transform, false);
            var barImg = accentBar.AddComponent<Image>();
            barImg.color = new Color(0.2f, 0.9f, 1f, 0.9f);
            var barRect = accentBar.GetComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0f, 0f);
            barRect.anchorMax = new Vector2(0f, 1f);
            barRect.offsetMin = new Vector2(0f, 0f);
            barRect.offsetMax = new Vector2(4f, 0f);

            var titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(menuPanelObj.transform, false);
            var title = titleObj.AddComponent<TextMeshProUGUI>();
            title.text = "ULTIMATE\nDODGE";
            title.fontSize = 52;
            title.fontStyle = FontStyles.Bold;
            title.lineSpacing = -10;
            title.alignment = TextAlignmentOptions.TopLeft;
            title.color = new Color(0.25f, 0.95f, 1.0f);
            var rectTitle = title.rectTransform;
            rectTitle.anchorMin = new Vector2(0f, 1f);
            rectTitle.anchorMax = new Vector2(1f, 1f);
            rectTitle.offsetMin = new Vector2(35f, -160f);
            rectTitle.offsetMax = new Vector2(-25f, -40f);

            var subObj = new GameObject("SubtitleText");
            subObj.transform.SetParent(menuPanelObj.transform, false);
            var sub = subObj.AddComponent<TextMeshProUGUI>();
            sub.text = "HOMING SURVIVAL ARENA";
            sub.fontSize = 20;
            sub.fontStyle = FontStyles.Bold;
            sub.alignment = TextAlignmentOptions.TopLeft;
            sub.color = new Color(1f, 0.82f, 0.2f);
            var rectSub = sub.rectTransform;
            rectSub.anchorMin = new Vector2(0f, 1f);
            rectSub.anchorMax = new Vector2(1f, 1f);
            rectSub.offsetMin = new Vector2(35f, -195f);
            rectSub.offsetMax = new Vector2(-25f, -165f);

            var scoreCardObj = new GameObject("ScoreCard");
            scoreCardObj.transform.SetParent(menuPanelObj.transform, false);
            var scoreCardImg = scoreCardObj.AddComponent<Image>();
            scoreCardImg.color = new Color(0.08f, 0.13f, 0.22f, 0.95f);
            var scoreCardRect = scoreCardObj.GetComponent<RectTransform>();
            scoreCardRect.anchorMin = new Vector2(0f, 1f);
            scoreCardRect.anchorMax = new Vector2(1f, 1f);
            scoreCardRect.offsetMin = new Vector2(30f, -270f);
            scoreCardRect.offsetMax = new Vector2(-30f, -215f);

            var highObj = new GameObject("HighScoreText");
            highObj.transform.SetParent(scoreCardObj.transform, false);
            var high = highObj.AddComponent<TextMeshProUGUI>();
            high.text = "BEST SCORE: 0";
            high.fontSize = 24;
            high.fontStyle = FontStyles.Bold;
            high.alignment = TextAlignmentOptions.Center;
            high.color = new Color(0.85f, 0.92f, 1f);
            var rectHigh = high.rectTransform;
            rectHigh.anchorMin = Vector2.zero;
            rectHigh.anchorMax = Vector2.one;
            rectHigh.offsetMin = Vector2.zero;
            rectHigh.offsetMax = Vector2.zero;

            Button CreateMenuBtn(string name, string labelText, float anchoredY, Color btnColor)
            {
                var btnObj = new GameObject(name);
                btnObj.transform.SetParent(menuPanelObj.transform, false);
                var img = btnObj.AddComponent<Image>();
                img.color = btnColor;
                var btn = btnObj.AddComponent<Button>();
                var r = btnObj.GetComponent<RectTransform>();
                r.anchorMin = new Vector2(0f, 1f);
                r.anchorMax = new Vector2(1f, 1f);
                r.offsetMin = new Vector2(30f, anchoredY - 56f);
                r.offsetMax = new Vector2(-30f, anchoredY);

                var txtObj = new GameObject("Text");
                txtObj.transform.SetParent(btnObj.transform, false);
                var txt = txtObj.AddComponent<TextMeshProUGUI>();
                txt.text = labelText;
                txt.fontSize = 22;
                txt.fontStyle = FontStyles.Bold;
                txt.alignment = TextAlignmentOptions.Center;
                txt.color = Color.white;
                var tr = txt.rectTransform;
                tr.anchorMin = Vector2.zero;
                tr.anchorMax = Vector2.one;
                tr.offsetMin = Vector2.zero;
                tr.offsetMax = Vector2.zero;

                return btn;
            }

            var playBtn = CreateMenuBtn("PlayButton", "PLAY GAME", -310f, new Color(0.12f, 0.68f, 0.32f, 0.95f));
            var howBtn = CreateMenuBtn("HowToPlayButton", "HOW TO PLAY", -385f, new Color(0.16f, 0.48f, 0.85f, 0.95f));
            var muteBtn = CreateMenuBtn("MuteButton", "AUDIO: ON", -460f, new Color(0.38f, 0.25f, 0.65f, 0.95f));
            var quitBtn = CreateMenuBtn("QuitButton", "QUIT GAME", -535f, new Color(0.72f, 0.22f, 0.22f, 0.95f));

            var verObj = new GameObject("VersionText");
            verObj.transform.SetParent(menuPanelObj.transform, false);
            var ver = verObj.AddComponent<TextMeshProUGUI>();
            ver.text = "ULTIMATE EDITION v2.0";
            ver.fontSize = 15;
            ver.alignment = TextAlignmentOptions.Center;
            ver.color = new Color(0.5f, 0.6f, 0.75f, 0.7f);
            var verRect = ver.rectTransform;
            verRect.anchorMin = new Vector2(0f, 0f);
            verRect.anchorMax = new Vector2(1f, 0f);
            verRect.offsetMin = new Vector2(20f, 15f);
            verRect.offsetMax = new Vector2(-20f, 40f);

            var modalDimmer = new GameObject("HowToPlayPanel");
            modalDimmer.transform.SetParent(canvasObj.transform, false);
            var dimmerImg = modalDimmer.AddComponent<Image>();
            dimmerImg.color = new Color(0f, 0f, 0f, 0.75f);
            var dimmerRect = modalDimmer.GetComponent<RectTransform>();
            dimmerRect.anchorMin = Vector2.zero;
            dimmerRect.anchorMax = Vector2.one;
            dimmerRect.offsetMin = Vector2.zero;
            dimmerRect.offsetMax = Vector2.zero;

            var modalCard = new GameObject("ModalCard");
            modalCard.transform.SetParent(modalDimmer.transform, false);
            var cardImg = modalCard.AddComponent<Image>();
            cardImg.color = new Color(0.06f, 0.10f, 0.16f, 0.98f);
            var cardRect = modalCard.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = Vector2.zero;
            cardRect.sizeDelta = new Vector2(820, 560);

            var modalTitle = new GameObject("ModalTitle");
            modalTitle.transform.SetParent(modalCard.transform, false);
            var mTitleTxt = modalTitle.AddComponent<TextMeshProUGUI>();
            mTitleTxt.text = "HOW TO PLAY";
            mTitleTxt.fontSize = 36;
            mTitleTxt.fontStyle = FontStyles.Bold;
            mTitleTxt.alignment = TextAlignmentOptions.Center;
            mTitleTxt.color = new Color(0.25f, 0.95f, 1f);
            var mtr = modalTitle.GetComponent<RectTransform>();
            mtr.anchorMin = new Vector2(0.5f, 1f);
            mtr.anchorMax = new Vector2(0.5f, 1f);
            mtr.anchoredPosition = new Vector2(0, -45);
            mtr.sizeDelta = new Vector2(700, 50);

            var modalBody = new GameObject("ModalBody");
            modalBody.transform.SetParent(modalCard.transform, false);
            var mBodyTxt = modalBody.AddComponent<TextMeshProUGUI>();
            mBodyTxt.text = "<b>PLAYER CONTROLS:</b>\n" +
                            "• <b>WASD / Arrow Keys:</b> Smooth movement across the arena\n" +
                            "• <b>Space:</b> Jump over rolling balls & hazards\n" +
                            "• <b>Shift / Right-Click:</b> Dash Burst with invulnerability\n" +
                            "• <b>E:</b> Dimension Shift (Phase into ethereal plane)\n\n" +
                            "<b>HAZARDS & SURVIVAL:</b>\n" +
                            "• <b>Ball Bouncing:</b> Balls rebound with dynamic squash & stretch impacts.\n" +
                            "• <b>Homing Danger:</b> After 3 bounces, balls lock onto your position!\n" +
                            "• <b>Impact Feedback:</b> Getting hit triggers damage recoil & invulnerability blink.\n" +
                            "• Survive as long as you can to establish the highest record score!";
            mBodyTxt.fontSize = 20;
            mBodyTxt.lineSpacing = 14;
            mBodyTxt.alignment = TextAlignmentOptions.TopLeft;
            mBodyTxt.color = new Color(0.92f, 0.94f, 0.98f);
            var mbr = modalBody.GetComponent<RectTransform>();
            mbr.anchorMin = new Vector2(0.5f, 0.5f);
            mbr.anchorMax = new Vector2(0.5f, 0.5f);
            mbr.anchoredPosition = new Vector2(0, -10);
            mbr.sizeDelta = new Vector2(720, 340);

            var closeBtnObj = new GameObject("CloseButton");
            closeBtnObj.transform.SetParent(modalCard.transform, false);
            var closeImg = closeBtnObj.AddComponent<Image>();
            closeImg.color = new Color(0.18f, 0.55f, 0.90f, 0.95f);
            var closeBtn = closeBtnObj.AddComponent<Button>();
            var cbr = closeBtnObj.GetComponent<RectTransform>();
            cbr.anchorMin = new Vector2(0.5f, 0f);
            cbr.anchorMax = new Vector2(0.5f, 0f);
            cbr.anchoredPosition = new Vector2(0, 45);
            cbr.sizeDelta = new Vector2(220, 50);

            var closeTxtObj = new GameObject("CloseText");
            closeTxtObj.transform.SetParent(closeBtnObj.transform, false);
            var closeTxt = closeTxtObj.AddComponent<TextMeshProUGUI>();
            closeTxt.text = "GOT IT";
            closeTxt.fontSize = 22;
            closeTxt.fontStyle = FontStyles.Bold;
            closeTxt.alignment = TextAlignmentOptions.Center;
            closeTxt.color = Color.white;
            var ctr = closeTxt.rectTransform;
            ctr.anchorMin = Vector2.zero;
            ctr.anchorMax = Vector2.one;
            ctr.offsetMin = Vector2.zero;
            ctr.offsetMax = Vector2.zero;

            modalDimmer.SetActive(false);

            var so = new SerializedObject(menuUI);
            so.FindProperty("playButton").objectReferenceValue = playBtn;
            so.FindProperty("howToPlayButton").objectReferenceValue = howBtn;
            so.FindProperty("quitButton").objectReferenceValue = quitBtn;
            so.FindProperty("muteButton").objectReferenceValue = muteBtn;
            so.FindProperty("highScoreText").objectReferenceValue = high;
            so.FindProperty("muteButtonText").objectReferenceValue = muteBtn.GetComponentInChildren<TextMeshProUGUI>();
            so.FindProperty("howToPlayPanel").objectReferenceValue = modalDimmer;
            so.FindProperty("closeHowToPlayButton").objectReferenceValue = closeBtn;
            so.FindProperty("gameSceneName").stringValue = "MainGame";
            so.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
            Debug.Log("[MainMenuSceneCreator] Successfully created and saved Assets/Scenes/MainMenu.unity!");

            var buildScenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/MainGame.unity", true)
            };
            EditorBuildSettings.scenes = buildScenes;
            Debug.Log("[MainMenuSceneCreator] Configured EditorBuildSettings scenes: 0 = MainMenu, 1 = MainGame");
        }

        private static Mesh FindMeshInAsset(string assetPath, string meshName)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (var a in assets)
            {
                if (a is Mesh m && (string.IsNullOrEmpty(meshName) || m.name == meshName))
                    return m;
            }
            return null;
        }

        private static GameObject CreateDecorativeBall(Transform parent, string name, Mesh mesh, Material mat,
            Vector3 pos, Vector3 scale, Vector3 rotationAxis, float rotationSpeed)
        {
            var go = new GameObject(name);
            go.transform.parent = parent;
            go.transform.position = pos;
            go.transform.localScale = scale;

            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;

            var rot = go.AddComponent<AutoRotate>();
            rot.orbitAroundPivot = false;
            rot.rotationAxis = rotationAxis;
            rot.rotationSpeed = rotationSpeed;

            return go;
        }
    }
}
