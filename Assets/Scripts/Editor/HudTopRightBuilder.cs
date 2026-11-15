using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using DodgeGame.Data;
using DodgeGame.UI;

namespace DodgeGame.Editor
{
    public static class HudTopRightBuilder
    {
        [MenuItem("Tools/DodgeGame/Build Top-Right Lives HUD")]
        public static void BuildTopRightLivesHud()
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
            var panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Panel_Hud.png");
            var heartFullSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Icon_Heart_Full.png");
            var heartEmptySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Icon_Heart_Empty.png");
            var pauseSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Icon_Pause.png");
            var fontUI = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Art/Fonts/FontUI.asset");
            var typographyConfig = AssetDatabase.LoadAssetAtPath<TypographyConfig>("Assets/ScriptableObjects/SO_Typography.asset")
                                ?? Resources.Load<TypographyConfig>("SO_Typography");

            Color panelCol = theme != null ? theme.PanelColor : new Color(0.914f, 0.929f, 0.941f, 0.85f);
            Color textPrimary = theme != null ? theme.TextPrimary : new Color(30f / 255f, 42f / 255f, 51f / 255f, 1f);
            Color textSecondary = theme != null ? theme.TextSecondary : new Color(74f / 255f, 88f / 255f, 99f / 255f, 1f);
            Color dangerCol = theme != null ? theme.Danger : new Color(0.898f, 0.282f, 0.302f, 1f);
            Color cooldownCol = theme != null ? theme.CooldownFill : new Color(0.604f, 0.651f, 0.686f, 1f);

            var oldLives = hudTransform.Find("LivesContainer");
            if (oldLives != null) oldLives.gameObject.SetActive(false);
            var oldLivesText = hudTransform.Find("LivesText");
            if (oldLivesText != null) oldLivesText.gameObject.SetActive(false);

            var groupTransform = hudTransform.Find("TopRight_Group");
            GameObject groupGo;
            if (groupTransform == null)
            {
                groupGo = new GameObject("TopRight_Group", typeof(RectTransform));
                groupGo.transform.SetParent(hudTransform, false);
            }
            else
            {
                groupGo = groupTransform.gameObject;
            }

            var groupRect = groupGo.GetComponent<RectTransform>();
            groupRect.anchorMin = new Vector2(0.97f, 0.97f);
            groupRect.anchorMax = new Vector2(0.97f, 0.97f);
            groupRect.pivot = new Vector2(1f, 1f);
            groupRect.anchoredPosition = Vector2.zero;

            var groupHlg = groupGo.GetComponent<HorizontalLayoutGroup>();
            if (groupHlg == null) groupHlg = groupGo.AddComponent<HorizontalLayoutGroup>();
            groupHlg.spacing = 8f;
            groupHlg.childAlignment = TextAnchor.MiddleRight;
            groupHlg.childControlWidth = false;
            groupHlg.childControlHeight = false;
            groupHlg.childForceExpandWidth = false;
            groupHlg.childForceExpandHeight = false;

            var groupCsf = groupGo.GetComponent<ContentSizeFitter>();
            if (groupCsf == null) groupCsf = groupGo.AddComponent<ContentSizeFitter>();
            groupCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            groupCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var livesDisplay = groupGo.GetComponent<LivesDisplayUI>();
            if (livesDisplay == null) livesDisplay = groupGo.AddComponent<LivesDisplayUI>();

            var pauseBtnTransform = groupGo.transform.Find("PauseButton");
            GameObject pauseBtnGo;
            if (pauseBtnTransform == null)
            {
                pauseBtnGo = new GameObject("PauseButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                pauseBtnGo.transform.SetParent(groupGo.transform, false);
            }
            else
            {
                pauseBtnGo = pauseBtnTransform.gameObject;
            }

            var pauseBtnRect = pauseBtnGo.GetComponent<RectTransform>();
            pauseBtnRect.sizeDelta = new Vector2(48f, 48f);

            var pauseBtnImg = pauseBtnGo.GetComponent<Image>();
            pauseBtnImg.sprite = panelSprite;
            pauseBtnImg.type = Image.Type.Sliced;
            pauseBtnImg.color = panelCol;

            var pauseBtn = pauseBtnGo.GetComponent<Button>();
            pauseBtn.targetGraphic = pauseBtnImg;

            var pauseIconTransform = pauseBtnGo.transform.Find("Icon");
            GameObject pauseIconGo;
            if (pauseIconTransform == null)
            {
                pauseIconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                pauseIconGo.transform.SetParent(pauseBtnGo.transform, false);
            }
            else
            {
                pauseIconGo = pauseIconTransform.gameObject;
            }

            var pauseIconRect = pauseIconGo.GetComponent<RectTransform>();
            pauseIconRect.anchorMin = new Vector2(0.5f, 0.5f);
            pauseIconRect.anchorMax = new Vector2(0.5f, 0.5f);
            pauseIconRect.pivot = new Vector2(0.5f, 0.5f);
            pauseIconRect.anchoredPosition = Vector2.zero;
            pauseIconRect.sizeDelta = new Vector2(24f, 24f);

            var pauseIconImg = pauseIconGo.GetComponent<Image>();
            pauseIconImg.sprite = pauseSprite;
            pauseIconImg.color = textPrimary;

            var livesPanelTransform = groupGo.transform.Find("LivesPanel");
            GameObject livesPanelGo;
            if (livesPanelTransform == null)
            {
                livesPanelGo = new GameObject("LivesPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                livesPanelGo.transform.SetParent(groupGo.transform, false);
            }
            else
            {
                livesPanelGo = livesPanelTransform.gameObject;
            }

            var livesPanelImg = livesPanelGo.GetComponent<Image>();
            livesPanelImg.sprite = panelSprite;
            livesPanelImg.type = Image.Type.Sliced;
            livesPanelImg.color = panelCol;

            var livesPanelVlg = livesPanelGo.GetComponent<VerticalLayoutGroup>();
            if (livesPanelVlg == null) livesPanelVlg = livesPanelGo.AddComponent<VerticalLayoutGroup>();
            livesPanelVlg.padding = new RectOffset(14, 14, 6, 8);
            livesPanelVlg.spacing = 4f;
            livesPanelVlg.childAlignment = TextAnchor.UpperCenter;
            livesPanelVlg.childControlWidth = false;
            livesPanelVlg.childControlHeight = false;
            livesPanelVlg.childForceExpandWidth = false;
            livesPanelVlg.childForceExpandHeight = false;

            var livesPanelCsf = livesPanelGo.GetComponent<ContentSizeFitter>();
            if (livesPanelCsf == null) livesPanelCsf = livesPanelGo.AddComponent<ContentSizeFitter>();
            livesPanelCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            livesPanelCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var livesLabelTransform = livesPanelGo.transform.Find("LivesLabel");
            GameObject livesLabelGo;
            if (livesLabelTransform == null)
            {
                livesLabelGo = new GameObject("LivesLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(TextStyler));
                livesLabelGo.transform.SetParent(livesPanelGo.transform, false);
            }
            else
            {
                livesLabelGo = livesLabelTransform.gameObject;
            }

            var livesLabelTmp = livesLabelGo.GetComponent<TextMeshProUGUI>();
            livesLabelTmp.text = "LIVES";
            if (fontUI != null) livesLabelTmp.font = fontUI;
            livesLabelTmp.alignment = TextAlignmentOptions.Center;

            var livesLabelStyler = livesLabelGo.GetComponent<TextStyler>();
            livesLabelStyler.Config = typographyConfig;
            livesLabelStyler.Role = TypographyRole.Label;
            livesLabelStyler.SetCustomFontSize(18f);
            livesLabelStyler.SetCustomFontStyle(FontStyles.Bold);
            livesLabelStyler.SetCustomColor(textSecondary);
            livesLabelStyler.EnforceNoWrap = true;

            var livesLabelCsf = livesLabelGo.GetComponent<ContentSizeFitter>();
            if (livesLabelCsf == null) livesLabelCsf = livesLabelGo.AddComponent<ContentSizeFitter>();
            livesLabelCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            livesLabelCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var heartsRowTransform = livesPanelGo.transform.Find("HeartsRow");
            GameObject heartsRowGo;
            if (heartsRowTransform == null)
            {
                heartsRowGo = new GameObject("HeartsRow", typeof(RectTransform));
                heartsRowGo.transform.SetParent(livesPanelGo.transform, false);
            }
            else
            {
                heartsRowGo = heartsRowTransform.gameObject;
            }

            var heartsRowHlg = heartsRowGo.GetComponent<HorizontalLayoutGroup>();
            if (heartsRowHlg == null) heartsRowHlg = heartsRowGo.AddComponent<HorizontalLayoutGroup>();
            heartsRowHlg.spacing = 8f;
            heartsRowHlg.childAlignment = TextAnchor.MiddleCenter;
            heartsRowHlg.childControlWidth = false;
            heartsRowHlg.childControlHeight = false;
            heartsRowHlg.childForceExpandWidth = false;
            heartsRowHlg.childForceExpandHeight = false;

            var heartsRowCsf = heartsRowGo.GetComponent<ContentSizeFitter>();
            if (heartsRowCsf == null) heartsRowCsf = heartsRowGo.AddComponent<ContentSizeFitter>();
            heartsRowCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            heartsRowCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var heartSlotsList = new List<LivesDisplayUI.HeartSlot>();
            for (int i = 0; i < 5; i++)
            {
                string slotName = $"Heart_{i}";
                var slotTransform = heartsRowGo.transform.Find(slotName);
                GameObject slotGo;
                if (slotTransform == null)
                {
                    slotGo = new GameObject(slotName, typeof(RectTransform));
                    slotGo.transform.SetParent(heartsRowGo.transform, false);
                }
                else
                {
                    slotGo = slotTransform.gameObject;
                }

                var slotRect = slotGo.GetComponent<RectTransform>();
                slotRect.sizeDelta = new Vector2(48f, 48f);

                var emptyTransform = slotGo.transform.Find("Empty");
                GameObject emptyGo;
                if (emptyTransform == null)
                {
                    emptyGo = new GameObject("Empty", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    emptyGo.transform.SetParent(slotGo.transform, false);
                }
                else
                {
                    emptyGo = emptyTransform.gameObject;
                }

                var emptyRect = emptyGo.GetComponent<RectTransform>();
                emptyRect.anchorMin = Vector2.zero;
                emptyRect.anchorMax = Vector2.one;
                emptyRect.offsetMin = Vector2.zero;
                emptyRect.offsetMax = Vector2.zero;

                var emptyImg = emptyGo.GetComponent<Image>();
                emptyImg.sprite = heartEmptySprite;
                emptyImg.color = cooldownCol;

                var fullTransform = slotGo.transform.Find("Full");
                GameObject fullGo;
                if (fullTransform == null)
                {
                    fullGo = new GameObject("Full", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
                    fullGo.transform.SetParent(slotGo.transform, false);
                }
                else
                {
                    fullGo = fullTransform.gameObject;
                }

                var fullRect = fullGo.GetComponent<RectTransform>();
                fullRect.anchorMin = Vector2.zero;
                fullRect.anchorMax = Vector2.one;
                fullRect.offsetMin = Vector2.zero;
                fullRect.offsetMax = Vector2.zero;

                var fullImg = fullGo.GetComponent<Image>();
                fullImg.sprite = heartFullSprite;
                fullImg.color = dangerCol;

                var fullCg = fullGo.GetComponent<CanvasGroup>();
                if (fullCg == null) fullCg = fullGo.AddComponent<CanvasGroup>();

                var slot = new LivesDisplayUI.HeartSlot
                {
                    Root = slotRect,
                    EmptyIcon = emptyImg,
                    FullIcon = fullImg,
                    FullCg = fullCg
                };
                heartSlotsList.Add(slot);
            }

            var livesSo = new SerializedObject(livesDisplay);
            livesSo.FindProperty("theme").objectReferenceValue = theme;
            livesSo.FindProperty("pauseButton").objectReferenceValue = pauseBtn;
            livesSo.FindProperty("heartsContainer").objectReferenceValue = heartsRowGo.GetComponent<RectTransform>();

            var slotsProp = livesSo.FindProperty("heartSlots");
            slotsProp.arraySize = heartSlotsList.Count;
            for (int i = 0; i < heartSlotsList.Count; i++)
            {
                var elem = slotsProp.GetArrayElementAtIndex(i);
                elem.FindPropertyRelative("Root").objectReferenceValue = heartSlotsList[i].Root;
                elem.FindPropertyRelative("EmptyIcon").objectReferenceValue = heartSlotsList[i].EmptyIcon;
                elem.FindPropertyRelative("FullIcon").objectReferenceValue = heartSlotsList[i].FullIcon;
                elem.FindPropertyRelative("FullCg").objectReferenceValue = heartSlotsList[i].FullCg;
            }
            livesSo.ApplyModifiedProperties();

            livesDisplay.SetLivesImmediate(3);

            LayoutRebuilder.ForceRebuildLayoutImmediate(groupRect);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[HudTopRightBuilder] Successfully built and saved Top-Right Lives & Pause HUD!");
        }
    }
}
