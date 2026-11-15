using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using DodgeGame.Data;
using DodgeGame.UI;

namespace DodgeGame.Editor
{
    public static class HudTopCenterBuilder
    {
        [MenuItem("Tools/DodgeGame/Build Top-Center Timer HUD")]
        public static void BuildTopCenterTimerHud()
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

            var hudTransform = canvasGo.transform.Find("HUD");
            if (hudTransform == null)
            {
                var hudGo = new GameObject("HUD", typeof(RectTransform));
                hudGo.transform.SetParent(canvasGo.transform, false);
                hudTransform = hudGo.transform;
            }

            var theme = AssetDatabase.LoadAssetAtPath<HudTheme>("Assets/Scripts/Data/HudTheme.asset")
                     ?? Resources.Load<HudTheme>("HudTheme");
            var panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Textures/Panel_Hud.png");
            var pillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Textures/Pill_Light.png");
            var fontDisplay = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Art/Fonts/FontDisplay.asset");
            var fontUI = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Art/Fonts/FontUI.asset");
            var typographyConfig = AssetDatabase.LoadAssetAtPath<TypographyConfig>("Assets/ScriptableObjects/SO_Typography.asset")
                                ?? Resources.Load<TypographyConfig>("SO_Typography");

            Color panelCol = theme != null ? theme.PanelColor : new Color(0.914f, 0.929f, 0.941f, 0.85f);
            Color textPrimary = theme != null ? theme.TextPrimary : new Color(30f / 255f, 42f / 255f, 51f / 255f, 1f);
            Color pillCol = new Color(0.863f, 0.890f, 0.910f, 1f);

            var oldTimer = hudTransform.Find("TimerText");
            if (oldTimer != null) oldTimer.gameObject.SetActive(false);

            var groupTransform = hudTransform.Find("TopCenter_TimerGroup");
            GameObject groupGo;
            if (groupTransform == null)
            {
                groupGo = new GameObject("TopCenter_TimerGroup", typeof(RectTransform));
                groupGo.transform.SetParent(hudTransform, false);
            }
            else
            {
                groupGo = groupTransform.gameObject;
            }

            var groupRect = groupGo.GetComponent<RectTransform>();
            groupRect.anchorMin = new Vector2(0.5f, 0.97f);
            groupRect.anchorMax = new Vector2(0.5f, 0.97f);
            groupRect.pivot = new Vector2(0.5f, 1f);
            groupRect.anchoredPosition = Vector2.zero;

            var groupVlg = groupGo.GetComponent<VerticalLayoutGroup>();
            if (groupVlg == null) groupVlg = groupGo.AddComponent<VerticalLayoutGroup>();
            groupVlg.spacing = 0f;
            groupVlg.childAlignment = TextAnchor.MiddleCenter;
            groupVlg.childControlWidth = false;
            groupVlg.childControlHeight = false;
            groupVlg.childForceExpandWidth = false;
            groupVlg.childForceExpandHeight = false;

            var groupCsf = groupGo.GetComponent<ContentSizeFitter>();
            if (groupCsf == null) groupCsf = groupGo.AddComponent<ContentSizeFitter>();
            groupCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            groupCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var timerUI = groupGo.GetComponent<TimerUI>();
            if (timerUI == null) timerUI = groupGo.AddComponent<TimerUI>();

            var timerPanelTransform = groupGo.transform.Find("TimerPanel");
            GameObject timerPanelGo;
            if (timerPanelTransform == null)
            {
                timerPanelGo = new GameObject("TimerPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                timerPanelGo.transform.SetParent(groupGo.transform, false);
            }
            else
            {
                timerPanelGo = timerPanelTransform.gameObject;
            }

            var timerPanelImage = timerPanelGo.GetComponent<Image>();
            timerPanelImage.sprite = panelSprite;
            timerPanelImage.type = Image.Type.Sliced;
            timerPanelImage.color = panelCol;

            var timerPanelVlg = timerPanelGo.GetComponent<VerticalLayoutGroup>();
            if (timerPanelVlg == null) timerPanelVlg = timerPanelGo.AddComponent<VerticalLayoutGroup>();
            timerPanelVlg.padding = new RectOffset(22, 22, 8, 8);
            timerPanelVlg.spacing = 4f;
            timerPanelVlg.childAlignment = TextAnchor.MiddleCenter;
            timerPanelVlg.childControlWidth = false;
            timerPanelVlg.childControlHeight = false;
            timerPanelVlg.childForceExpandWidth = false;
            timerPanelVlg.childForceExpandHeight = false;

            var timerPanelCsf = timerPanelGo.GetComponent<ContentSizeFitter>();
            if (timerPanelCsf == null) timerPanelCsf = timerPanelGo.AddComponent<ContentSizeFitter>();
            timerPanelCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            timerPanelCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var timerValueTransform = timerPanelGo.transform.Find("TimerValue");
            GameObject timerValueGo;
            if (timerValueTransform == null)
            {
                timerValueGo = new GameObject("TimerValue", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(TextStyler));
                timerValueGo.transform.SetParent(timerPanelGo.transform, false);
            }
            else
            {
                timerValueGo = timerValueTransform.gameObject;
            }

            var timerValueTmp = timerValueGo.GetComponent<TextMeshProUGUI>();
            timerValueTmp.text = TextStyler.FormatTabular("00:00.00");
            if (fontDisplay != null) timerValueTmp.font = fontDisplay;
            timerValueTmp.alignment = TextAlignmentOptions.Center;

            var timerValueStyler = timerValueGo.GetComponent<TextStyler>();
            timerValueStyler.Config = typographyConfig;
            timerValueStyler.Role = TypographyRole.Heading;
            timerValueStyler.SetCustomFontSize(36f);
            timerValueStyler.SetCustomColor(textPrimary);
            timerValueStyler.EnforceNoWrap = true;

            var timerValueCsf = timerValueGo.GetComponent<ContentSizeFitter>();
            if (timerValueCsf == null) timerValueCsf = timerValueGo.AddComponent<ContentSizeFitter>();
            timerValueCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            timerValueCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var chipTransform = timerPanelGo.transform.Find("DifficultyChip");
            GameObject chipGo;
            if (chipTransform == null)
            {
                chipGo = new GameObject("DifficultyChip", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                chipGo.transform.SetParent(timerPanelGo.transform, false);
            }
            else
            {
                chipGo = chipTransform.gameObject;
            }

            var chipImage = chipGo.GetComponent<Image>();
            chipImage.sprite = pillSprite;
            chipImage.type = Image.Type.Sliced;
            chipImage.color = pillCol;

            var chipHlg = chipGo.GetComponent<HorizontalLayoutGroup>();
            if (chipHlg == null) chipHlg = chipGo.AddComponent<HorizontalLayoutGroup>();
            chipHlg.padding = new RectOffset(12, 12, 3, 3);
            chipHlg.spacing = 0f;
            chipHlg.childAlignment = TextAnchor.MiddleCenter;
            chipHlg.childControlWidth = true;
            chipHlg.childControlHeight = true;
            chipHlg.childForceExpandWidth = false;
            chipHlg.childForceExpandHeight = false;

            var chipCsf = chipGo.GetComponent<ContentSizeFitter>();
            if (chipCsf == null) chipCsf = chipGo.AddComponent<ContentSizeFitter>();
            chipCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            chipCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var diffTextTransform = chipGo.transform.Find("DifficultyText");
            GameObject diffTextGo;
            if (diffTextTransform == null)
            {
                diffTextGo = new GameObject("DifficultyText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(TextStyler));
                diffTextGo.transform.SetParent(chipGo.transform, false);
            }
            else
            {
                diffTextGo = diffTextTransform.gameObject;
            }

            var diffTextTmp = diffTextGo.GetComponent<TextMeshProUGUI>();
            diffTextTmp.text = "Normal";
            if (fontUI != null) diffTextTmp.font = fontUI;
            diffTextTmp.alignment = TextAlignmentOptions.Center;

            var diffTextStyler = diffTextGo.GetComponent<TextStyler>();
            diffTextStyler.Config = typographyConfig;
            diffTextStyler.Role = TypographyRole.Caption;
            diffTextStyler.SetCustomFontSize(18f);
            diffTextStyler.SetCustomColor(textPrimary);
            diffTextStyler.EnforceNoWrap = true;

            var diffTextCsf = diffTextGo.GetComponent<ContentSizeFitter>();
            if (diffTextCsf == null) diffTextCsf = diffTextGo.AddComponent<ContentSizeFitter>();
            diffTextCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            diffTextCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var timerSo = new SerializedObject(timerUI);
            timerSo.FindProperty("timerText").objectReferenceValue = timerValueTmp;
            timerSo.FindProperty("difficultyChipText").objectReferenceValue = diffTextTmp;
            timerSo.FindProperty("theme").objectReferenceValue = theme;
            timerSo.ApplyModifiedProperties();

            var scoreUI = canvasGo.GetComponent<ScoreUI>();
            if (scoreUI != null)
            {
                var scoreSo = new SerializedObject(scoreUI);
                scoreSo.FindProperty("timerText").objectReferenceValue = timerValueTmp;
                scoreSo.ApplyModifiedProperties();
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(groupRect);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[HudTopCenterBuilder] Successfully built and saved Top-Center Timer & Difficulty Chip HUD!");
        }
    }
}
