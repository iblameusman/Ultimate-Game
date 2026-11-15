using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using DodgeGame.Data;
using DodgeGame.UI;

namespace DodgeGame.Editor
{
    public static class HudCenterFeedbackBuilder
    {
        [MenuItem("Tools/DodgeGame/Build Center Floating Feedback HUD")]
        public static void BuildCenterFeedbackHud()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainGame.unity", OpenSceneMode.Single);

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
            var fontDisplay = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Art/Fonts/FontDisplay.asset");
            var fontUI = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Art/Fonts/FontUI.asset");
            var typographyConfig = AssetDatabase.LoadAssetAtPath<TypographyConfig>("Assets/ScriptableObjects/SO_Typography.asset")
                                ?? Resources.Load<TypographyConfig>("SO_Typography");

            Color textPrimary = theme != null ? theme.TextPrimary : new Color(30f / 255f, 42f / 255f, 51f / 255f, 1f);

            var timerGroupTransform = hudTransform.Find("TopCenter_TimerGroup");
            GameObject streakGo = null;
            TextMeshProUGUI streakTmp = null;
            CanvasGroup streakCg = null;

            if (timerGroupTransform != null)
            {
                var streakTransform = timerGroupTransform.Find("StreakCounter");
                if (streakTransform == null)
                {
                    streakGo = new GameObject("StreakCounter", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(CanvasGroup), typeof(TextStyler));
                    streakGo.transform.SetParent(timerGroupTransform, false);
                }
                else
                {
                    streakGo = streakTransform.gameObject;
                }

                var streakRect = streakGo.GetComponent<RectTransform>();
                streakRect.sizeDelta = new Vector2(300f, 48f);

                streakTmp = streakGo.GetComponent<TextMeshProUGUI>();
                streakTmp.text = "";
                if (fontDisplay != null) streakTmp.font = fontDisplay;
                streakTmp.alignment = TextAlignmentOptions.Center;

                var styler = streakGo.GetComponent<TextStyler>();
                if (styler == null) styler = streakGo.AddComponent<TextStyler>();
                styler.Config = typographyConfig;
                styler.Role = TypographyRole.Heading;
                styler.SetCustomFontSize(40f);
                styler.SetCustomFontStyle(FontStyles.Bold);
                styler.SetCustomColor(textPrimary);
                styler.SetOutline(1.5f, textPrimary);
                styler.EnforceNoWrap = true;

                streakCg = streakGo.GetComponent<CanvasGroup>();
                streakCg.alpha = 0f;
                streakCg.blocksRaycasts = false;
                streakCg.interactable = false;
            }

            var floatTransform = hudTransform.Find("FloatingTextContainer");
            GameObject floatGo;
            if (floatTransform == null)
            {
                floatGo = new GameObject("FloatingTextContainer", typeof(RectTransform));
                floatGo.transform.SetParent(hudTransform, false);
            }
            else
            {
                floatGo = floatTransform.gameObject;
            }

            var floatRect = floatGo.GetComponent<RectTransform>();
            floatRect.anchorMin = Vector2.zero;
            floatRect.anchorMax = Vector2.one;
            floatRect.offsetMin = Vector2.zero;
            floatRect.offsetMax = Vector2.zero;

            var uiJuice = Object.FindAnyObjectByType<UIJuice>();
            if (uiJuice != null)
            {
                var so = new SerializedObject(uiJuice);
                if (streakTmp != null) so.FindProperty("streakCounterText").objectReferenceValue = streakTmp;
                if (streakCg != null) so.FindProperty("streakCanvasGroup").objectReferenceValue = streakCg;
                so.FindProperty("floatingTextContainer").objectReferenceValue = floatRect;
                if (theme != null) so.FindProperty("theme").objectReferenceValue = theme;
                so.ApplyModifiedProperties();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[HudCenterFeedbackBuilder] Successfully built and saved Center Floating Feedback HUD!");
        }
    }
}
