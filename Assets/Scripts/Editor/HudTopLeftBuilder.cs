using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using DodgeGame.Data;
using DodgeGame.UI;

namespace DodgeGame.Editor
{
    public static class HudTopLeftBuilder
    {
        [MenuItem("Tools/DodgeGame/Build Top-Left Score HUD")]
        public static void BuildTopLeftScoreHud()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != "MainGame")
            {
                EditorSceneManager.OpenScene("Assets/Scenes/MainGame.unity");
            }

            var canvasGo = GameObject.Find("Canvas");
            if (canvasGo == null)
            {
                Debug.LogError("Canvas not found in MainGame scene.");
                return;
            }

            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var hudTransform = canvasGo.transform.Find("HUD");
            if (hudTransform == null)
            {
                var hudGo = new GameObject("HUD", typeof(RectTransform));
                hudGo.transform.SetParent(canvasGo.transform, false);
                hudTransform = hudGo.transform;
            }

            var hudRect = hudTransform.GetComponent<RectTransform>();
            hudRect.anchorMin = Vector2.zero;
            hudRect.anchorMax = Vector2.one;
            hudRect.offsetMin = Vector2.zero;
            hudRect.offsetMax = Vector2.zero;

            var safeAreaFitter = hudTransform.GetComponent<SafeAreaFitter>();
            if (safeAreaFitter == null) safeAreaFitter = hudTransform.gameObject.AddComponent<SafeAreaFitter>();
            safeAreaFitter.ApplySafeArea();

            var theme = AssetDatabase.LoadAssetAtPath<HudTheme>("Assets/Scripts/Data/HudTheme.asset")
                     ?? Resources.Load<HudTheme>("HudTheme");
            var panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Textures/Panel_Hud.png");
            var fontDisplay = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Art/Fonts/FontDisplay.asset");
            var fontUI = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Art/Fonts/FontUI.asset");
            var typographyConfig = AssetDatabase.LoadAssetAtPath<TypographyConfig>("Assets/ScriptableObjects/SO_Typography.asset")
                                ?? Resources.Load<TypographyConfig>("SO_Typography");

            Color panelCol = theme != null ? theme.PanelColor : new Color(0.914f, 0.929f, 0.941f, 0.85f);
            Color textPrimary = theme != null ? theme.TextPrimary : new Color(30f / 255f, 42f / 255f, 51f / 255f, 1f);
            Color textSecondary = theme != null ? theme.TextSecondary : new Color(74f / 255f, 88f / 255f, 99f / 255f, 1f);

            var oldScore = hudTransform.Find("ScoreText");
            if (oldScore != null) oldScore.gameObject.SetActive(false);
            var oldHighScore = hudTransform.Find("HighScoreText") ?? canvasGo.transform.Find("HighScoreText");
            if (oldHighScore != null) oldHighScore.gameObject.SetActive(false);

            var groupTransform = hudTransform.Find("TopLeft_ScoreGroup");
            GameObject groupGo;
            if (groupTransform == null)
            {
                groupGo = new GameObject("TopLeft_ScoreGroup", typeof(RectTransform));
                groupGo.transform.SetParent(hudTransform, false);
            }
            else
            {
                groupGo = groupTransform.gameObject;
            }

            var groupRect = groupGo.GetComponent<RectTransform>();
            groupRect.anchorMin = new Vector2(0.03f, 0.97f);
            groupRect.anchorMax = new Vector2(0.03f, 0.97f);
            groupRect.pivot = new Vector2(0f, 1f);
            groupRect.anchoredPosition = Vector2.zero;

            var groupVlg = groupGo.GetComponent<VerticalLayoutGroup>();
            if (groupVlg == null) groupVlg = groupGo.AddComponent<VerticalLayoutGroup>();
            groupVlg.spacing = 8f;
            groupVlg.childAlignment = TextAnchor.UpperLeft;
            groupVlg.childControlWidth = false;
            groupVlg.childControlHeight = false;
            groupVlg.childForceExpandWidth = false;
            groupVlg.childForceExpandHeight = false;

            var groupCsf = groupGo.GetComponent<ContentSizeFitter>();
            if (groupCsf == null) groupCsf = groupGo.AddComponent<ContentSizeFitter>();
            groupCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            groupCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scorePanelTransform = groupGo.transform.Find("ScorePanel");
            GameObject scorePanelGo;
            if (scorePanelTransform == null)
            {
                scorePanelGo = new GameObject("ScorePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                scorePanelGo.transform.SetParent(groupGo.transform, false);
            }
            else
            {
                scorePanelGo = scorePanelTransform.gameObject;
            }

            var scorePanelImage = scorePanelGo.GetComponent<Image>();
            scorePanelImage.sprite = panelSprite;
            scorePanelImage.type = Image.Type.Sliced;
            scorePanelImage.color = panelCol;

            var scorePanelVlg = scorePanelGo.GetComponent<VerticalLayoutGroup>();
            if (scorePanelVlg == null) scorePanelVlg = scorePanelGo.AddComponent<VerticalLayoutGroup>();
            scorePanelVlg.padding = new RectOffset(20, 20, 10, 10);
            scorePanelVlg.spacing = 2f;
            scorePanelVlg.childAlignment = TextAnchor.UpperLeft;
            scorePanelVlg.childControlWidth = true;
            scorePanelVlg.childControlHeight = true;
            scorePanelVlg.childForceExpandWidth = false;
            scorePanelVlg.childForceExpandHeight = false;

            var scorePanelCsf = scorePanelGo.GetComponent<ContentSizeFitter>();
            if (scorePanelCsf == null) scorePanelCsf = scorePanelGo.AddComponent<ContentSizeFitter>();
            scorePanelCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            scorePanelCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scoreLabelTransform = scorePanelGo.transform.Find("ScoreLabel");
            GameObject scoreLabelGo;
            if (scoreLabelTransform == null)
            {
                scoreLabelGo = new GameObject("ScoreLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(TextStyler));
                scoreLabelGo.transform.SetParent(scorePanelGo.transform, false);
            }
            else
            {
                scoreLabelGo = scoreLabelTransform.gameObject;
            }

            var scoreLabelTmp = scoreLabelGo.GetComponent<TextMeshProUGUI>();
            scoreLabelTmp.text = "SCORE";
            if (fontUI != null) scoreLabelTmp.font = fontUI;
            scoreLabelTmp.alignment = TextAlignmentOptions.TopLeft;

            var scoreLabelStyler = scoreLabelGo.GetComponent<TextStyler>();
            scoreLabelStyler.Config = typographyConfig;
            scoreLabelStyler.Role = TypographyRole.Label;
            scoreLabelStyler.SetCustomFontSize(22f);
            scoreLabelStyler.SetCustomFontStyle(FontStyles.Bold);
            scoreLabelStyler.SetCustomColor(textSecondary);
            scoreLabelStyler.EnforceNoWrap = true;

            var scoreLabelCsf = scoreLabelGo.GetComponent<ContentSizeFitter>();
            if (scoreLabelCsf == null) scoreLabelCsf = scoreLabelGo.AddComponent<ContentSizeFitter>();
            scoreLabelCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            scoreLabelCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scoreValueTransform = scorePanelGo.transform.Find("ScoreValue");
            GameObject scoreValueGo;
            if (scoreValueTransform == null)
            {
                scoreValueGo = new GameObject("ScoreValue", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(TextStyler));
                scoreValueGo.transform.SetParent(scorePanelGo.transform, false);
            }
            else
            {
                scoreValueGo = scoreValueTransform.gameObject;
            }

            var scoreValueTmp = scoreValueGo.GetComponent<TextMeshProUGUI>();
            scoreValueTmp.text = "0";
            if (fontDisplay != null) scoreValueTmp.font = fontDisplay;
            scoreValueTmp.alignment = TextAlignmentOptions.TopLeft;

            var scoreValueStyler = scoreValueGo.GetComponent<TextStyler>();
            scoreValueStyler.Config = typographyConfig;
            scoreValueStyler.Role = TypographyRole.Display;
            scoreValueStyler.SetCustomFontSize(64f);
            scoreValueStyler.SetCustomFontStyle(FontStyles.Bold | FontStyles.Italic);
            scoreValueStyler.SetCustomColor(textPrimary);
            scoreValueStyler.EnforceNoWrap = true;

            var scoreValueCsf = scoreValueGo.GetComponent<ContentSizeFitter>();
            if (scoreValueCsf == null) scoreValueCsf = scoreValueGo.AddComponent<ContentSizeFitter>();
            scoreValueCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            scoreValueCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var bestPanelTransform = groupGo.transform.Find("BestPanel");
            GameObject bestPanelGo;
            if (bestPanelTransform == null)
            {
                bestPanelGo = new GameObject("BestPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                bestPanelGo.transform.SetParent(groupGo.transform, false);
            }
            else
            {
                bestPanelGo = bestPanelTransform.gameObject;
            }

            var bestPanelImage = bestPanelGo.GetComponent<Image>();
            bestPanelImage.sprite = panelSprite;
            bestPanelImage.type = Image.Type.Sliced;
            bestPanelImage.color = panelCol;

            var bestPanelHlg = bestPanelGo.GetComponent<HorizontalLayoutGroup>();
            if (bestPanelHlg == null) bestPanelHlg = bestPanelGo.AddComponent<HorizontalLayoutGroup>();
            bestPanelHlg.padding = new RectOffset(14, 14, 6, 6);
            bestPanelHlg.spacing = 0f;
            bestPanelHlg.childAlignment = TextAnchor.MiddleCenter;
            bestPanelHlg.childControlWidth = true;
            bestPanelHlg.childControlHeight = true;
            bestPanelHlg.childForceExpandWidth = false;
            bestPanelHlg.childForceExpandHeight = false;

            var bestPanelCsf = bestPanelGo.GetComponent<ContentSizeFitter>();
            if (bestPanelCsf == null) bestPanelCsf = bestPanelGo.AddComponent<ContentSizeFitter>();
            bestPanelCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            bestPanelCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var bestTextTransform = bestPanelGo.transform.Find("BestText");
            GameObject bestTextGo;
            if (bestTextTransform == null)
            {
                bestTextGo = new GameObject("BestText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(TextStyler));
                bestTextGo.transform.SetParent(bestPanelGo.transform, false);
            }
            else
            {
                bestTextGo = bestTextTransform.gameObject;
            }

            var bestTextTmp = bestTextGo.GetComponent<TextMeshProUGUI>();
            bestTextTmp.text = "BEST (NORMAL): 0";
            if (fontUI != null) bestTextTmp.font = fontUI;
            bestTextTmp.alignment = TextAlignmentOptions.Center;

            var bestTextStyler = bestTextGo.GetComponent<TextStyler>();
            bestTextStyler.Config = typographyConfig;
            bestTextStyler.Role = TypographyRole.Label;
            bestTextStyler.SetCustomFontSize(24f);
            bestTextStyler.SetCustomColor(textSecondary);
            bestTextStyler.EnforceNoWrap = true;

            var bestTextCsf = bestTextGo.GetComponent<ContentSizeFitter>();
            if (bestTextCsf == null) bestTextCsf = bestTextGo.AddComponent<ContentSizeFitter>();
            bestTextCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            bestTextCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var uiJuice = hudTransform.GetComponent<UIJuice>();
            if (uiJuice != null)
            {
                var juiceSo = new SerializedObject(uiJuice);
                juiceSo.FindProperty("scoreText").objectReferenceValue = scoreValueTmp;
                juiceSo.FindProperty("scorePanel").objectReferenceValue = scorePanelGo.GetComponent<RectTransform>();
                juiceSo.ApplyModifiedProperties();
            }

            var scoreUI = canvasGo.GetComponent<ScoreUI>();
            if (scoreUI != null)
            {
                var scoreSo = new SerializedObject(scoreUI);
                scoreSo.FindProperty("scoreText").objectReferenceValue = scoreValueTmp;
                scoreSo.FindProperty("scoreLabel").objectReferenceValue = scoreLabelTmp;
                scoreSo.FindProperty("scorePanel").objectReferenceValue = scorePanelGo.GetComponent<RectTransform>();
                scoreSo.FindProperty("bestText").objectReferenceValue = bestTextTmp;
                scoreSo.FindProperty("bestPanel").objectReferenceValue = bestPanelGo.GetComponent<RectTransform>();
                scoreSo.FindProperty("highScoreText").objectReferenceValue = bestTextTmp;
                scoreSo.ApplyModifiedProperties();
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(groupRect);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[HudTopLeftBuilder] Successfully built and saved Top-Left Score and Best HUD!");
        }
    }
}
