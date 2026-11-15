using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using DodgeGame.Data;
using DodgeGame.UI;

namespace DodgeGame.Editor
{
    public static class HudSkillBarBuilder
    {
        [MenuItem("Tools/DodgeGame/Build Bottom-Center Skill Bar HUD")]
        public static void BuildSkillBarHud()
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
            var panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Panel_Hud.png");
            var pillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Pill_Light.png");
            var fontUI = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Art/Fonts/FontUI.asset");
            var fontDisplay = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Art/Fonts/FontDisplay.asset");
            var typographyConfig = AssetDatabase.LoadAssetAtPath<TypographyConfig>("Assets/ScriptableObjects/SO_Typography.asset")
                                ?? Resources.Load<TypographyConfig>("SO_Typography");

            Color panelCol = theme != null ? theme.PanelColor : new Color(0.914f, 0.929f, 0.941f, 0.85f);
            Color textPrimary = theme != null ? theme.TextPrimary : new Color(30f / 255f, 42f / 255f, 51f / 255f, 1f);
            Color textSecondary = theme != null ? theme.TextSecondary : new Color(74f / 255f, 88f / 255f, 99f / 255f, 1f);
            Color readyCol = theme != null ? theme.AccentReady : new Color(0.239f, 0.863f, 0.165f, 1f);
            Color cooldownCol = theme != null ? theme.CooldownFill : new Color(0.604f, 0.651f, 0.686f, 1f);
            Color pillCol = new Color(0.863f, 0.890f, 0.910f, 1f);

            var oldSkills = hudTransform.Find("SkillsContainer");
            if (oldSkills != null) oldSkills.gameObject.SetActive(false);

            var groupTransform = hudTransform.Find("BottomCenter_SkillBar");
            GameObject groupGo;
            if (groupTransform == null)
            {
                groupGo = new GameObject("BottomCenter_SkillBar", typeof(RectTransform));
                groupGo.transform.SetParent(hudTransform, false);
            }
            else
            {
                groupGo = groupTransform.gameObject;
            }

            var groupRect = groupGo.GetComponent<RectTransform>();
            groupRect.anchorMin = new Vector2(0.5f, 0.04f);
            groupRect.anchorMax = new Vector2(0.5f, 0.04f);
            groupRect.pivot = new Vector2(0.5f, 0f);
            groupRect.anchoredPosition = Vector2.zero;

            var groupHlg = groupGo.GetComponent<HorizontalLayoutGroup>();
            if (groupHlg == null) groupHlg = groupGo.AddComponent<HorizontalLayoutGroup>();
            groupHlg.spacing = 8f;
            groupHlg.childAlignment = TextAnchor.MiddleCenter;
            groupHlg.childControlWidth = false;
            groupHlg.childControlHeight = false;
            groupHlg.childForceExpandWidth = false;
            groupHlg.childForceExpandHeight = false;

            var groupCsf = groupGo.GetComponent<ContentSizeFitter>();
            if (groupCsf == null) groupCsf = groupGo.AddComponent<ContentSizeFitter>();
            groupCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            groupCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var cardTypes = new SkillCardType[] { SkillCardType.Phase, SkillCardType.Dash, SkillCardType.SlowMotion };
            var cardNames = new string[] { "PHASE", "DASH", "SLOW MOTION" };
            var defaultKeys = new string[] { "[Q]", "[Shift]", "[E]" };

            for (int i = 0; i < 3; i++)
            {
                var cardType = cardTypes[i];
                string cardObjName = $"SkillCard_{cardType}";
                var cardTransform = groupGo.transform.Find(cardObjName);
                GameObject cardGo;
                if (cardTransform == null)
                {
                    cardGo = new GameObject(cardObjName, typeof(RectTransform), typeof(CanvasGroup));
                    cardGo.transform.SetParent(groupGo.transform, false);
                }
                else
                {
                    cardGo = cardTransform.gameObject;
                }

                var cardRect = cardGo.GetComponent<RectTransform>();
                cardRect.sizeDelta = new Vector2(240f, 88f);

                var cardCg = cardGo.GetComponent<CanvasGroup>();

                var bgTransform = cardGo.transform.Find("Background");
                GameObject bgGo;
                if (bgTransform == null)
                {
                    bgGo = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    bgGo.transform.SetParent(cardGo.transform, false);
                }
                else
                {
                    bgGo = bgTransform.gameObject;
                }

                var bgRect = bgGo.GetComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.offsetMin = Vector2.zero;
                bgRect.offsetMax = Vector2.zero;

                var bgImg = bgGo.GetComponent<Image>();
                bgImg.sprite = panelSprite;
                bgImg.type = Image.Type.Sliced;
                bgImg.color = panelCol;

                var borderTransform = cardGo.transform.Find("BorderHighlight");
                GameObject borderGo;
                if (borderTransform == null)
                {
                    borderGo = new GameObject("BorderHighlight", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    borderGo.transform.SetParent(cardGo.transform, false);
                }
                else
                {
                    borderGo = borderTransform.gameObject;
                }

                var borderRect = borderGo.GetComponent<RectTransform>();
                borderRect.anchorMin = Vector2.zero;
                borderRect.anchorMax = Vector2.one;
                borderRect.offsetMin = Vector2.zero;
                borderRect.offsetMax = Vector2.zero;

                var borderImg = borderGo.GetComponent<Image>();
                borderImg.sprite = panelSprite;
                borderImg.type = Image.Type.Sliced;
                borderImg.color = readyCol;

                var cdTransform = cardGo.transform.Find("CooldownFill");
                GameObject cdGo;
                if (cdTransform == null)
                {
                    cdGo = new GameObject("CooldownFill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    cdGo.transform.SetParent(cardGo.transform, false);
                }
                else
                {
                    cdGo = cdTransform.gameObject;
                }

                var cdRect = cdGo.GetComponent<RectTransform>();
                cdRect.anchorMin = Vector2.zero;
                cdRect.anchorMax = Vector2.one;
                cdRect.offsetMin = Vector2.zero;
                cdRect.offsetMax = Vector2.zero;

                var cdImg = cdGo.GetComponent<Image>();
                cdImg.sprite = panelSprite;
                cdImg.type = Image.Type.Filled;
                cdImg.fillMethod = Image.FillMethod.Horizontal;
                cdImg.fillOrigin = 0;
                cdImg.fillAmount = 0f;
                cdImg.color = cooldownCol;

                var chipTransform = cardGo.transform.Find("KeyChip");
                GameObject chipGo;
                if (chipTransform == null)
                {
                    chipGo = new GameObject("KeyChip", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    chipGo.transform.SetParent(cardGo.transform, false);
                }
                else
                {
                    chipGo = chipTransform.gameObject;
                }

                var chipRect = chipGo.GetComponent<RectTransform>();
                chipRect.anchorMin = new Vector2(0.5f, 1f);
                chipRect.anchorMax = new Vector2(0.5f, 1f);
                chipRect.pivot = new Vector2(0.5f, 1f);
                chipRect.anchoredPosition = new Vector2(0f, -6f);
                chipRect.sizeDelta = new Vector2(50f, 20f);

                var chipImg = chipGo.GetComponent<Image>();
                chipImg.sprite = pillSprite;
                chipImg.type = Image.Type.Sliced;
                chipImg.color = pillCol;

                var chipTextTransform = chipGo.transform.Find("Text");
                GameObject chipTextGo;
                if (chipTextTransform == null)
                {
                    chipTextGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(TextStyler));
                    chipTextGo.transform.SetParent(chipGo.transform, false);
                }
                else
                {
                    chipTextGo = chipTextTransform.gameObject;
                }

                var chipTextRect = chipTextGo.GetComponent<RectTransform>();
                chipTextRect.anchorMin = Vector2.zero;
                chipTextRect.anchorMax = Vector2.one;
                chipTextRect.offsetMin = Vector2.zero;
                chipTextRect.offsetMax = Vector2.zero;

                var chipTmp = chipTextGo.GetComponent<TextMeshProUGUI>();
                chipTmp.text = defaultKeys[i];
                if (fontUI != null) chipTmp.font = fontUI;
                chipTmp.alignment = TextAlignmentOptions.Center;

                var chipStyler = chipTextGo.GetComponent<TextStyler>();
                chipStyler.Config = typographyConfig;
                chipStyler.Role = TypographyRole.Caption;
                chipStyler.SetCustomFontSize(14f);
                chipStyler.SetCustomColor(textPrimary);
                chipStyler.EnforceNoWrap = true;

                var nameTransform = cardGo.transform.Find("SkillName");
                GameObject nameGo;
                if (nameTransform == null)
                {
                    nameGo = new GameObject("SkillName", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(TextStyler));
                    nameGo.transform.SetParent(cardGo.transform, false);
                }
                else
                {
                    nameGo = nameTransform.gameObject;
                }

                var nameRect = nameGo.GetComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(0f, 0.5f);
                nameRect.anchorMax = new Vector2(1f, 0.5f);
                nameRect.pivot = new Vector2(0.5f, 0.5f);
                nameRect.anchoredPosition = new Vector2(0f, cardType == SkillCardType.SlowMotion ? 6f : 2f);
                nameRect.sizeDelta = new Vector2(0f, 32f);

                var nameTmp = nameGo.GetComponent<TextMeshProUGUI>();
                nameTmp.text = cardNames[i];
                if (fontDisplay != null) nameTmp.font = fontDisplay;
                nameTmp.alignment = TextAlignmentOptions.Center;

                var nameStyler = nameGo.GetComponent<TextStyler>();
                nameStyler.Config = typographyConfig;
                nameStyler.Role = TypographyRole.Button;
                nameStyler.SetCustomFontSize(26f);
                nameStyler.SetCustomFontStyle(FontStyles.Bold | FontStyles.Italic | FontStyles.UpperCase);
                nameStyler.SetCustomColor(textPrimary);
                nameStyler.EnforceNoWrap = true;

                var stateTransform = cardGo.transform.Find("StateLine");
                GameObject stateGo;
                if (stateTransform == null)
                {
                    stateGo = new GameObject("StateLine", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(TextStyler));
                    stateGo.transform.SetParent(cardGo.transform, false);
                }
                else
                {
                    stateGo = stateTransform.gameObject;
                }

                var stateRect = stateGo.GetComponent<RectTransform>();
                stateRect.anchorMin = new Vector2(0f, 0f);
                stateRect.anchorMax = new Vector2(1f, 0f);
                stateRect.pivot = new Vector2(0.5f, 0f);
                stateRect.anchoredPosition = new Vector2(0f, 6f);
                stateRect.sizeDelta = new Vector2(0f, 20f);

                var stateTmp = stateGo.GetComponent<TextMeshProUGUI>();
                stateTmp.text = "READY";
                if (fontUI != null) stateTmp.font = fontUI;
                stateTmp.alignment = TextAlignmentOptions.Center;

                var stateStyler = stateGo.GetComponent<TextStyler>();
                stateStyler.Config = typographyConfig;
                stateStyler.Role = TypographyRole.Caption;
                stateStyler.SetCustomFontSize(16f);
                stateStyler.SetCustomColor(readyCol);
                stateStyler.EnforceNoWrap = true;

                GameObject chargeBarGo = null;
                Image chargeFillImg = null;
                if (cardType == SkillCardType.SlowMotion)
                {
                    var chargeTransform = cardGo.transform.Find("ChargeBar");
                    if (chargeTransform == null)
                    {
                        chargeBarGo = new GameObject("ChargeBar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                        chargeBarGo.transform.SetParent(cardGo.transform, false);
                    }
                    else
                    {
                        chargeBarGo = chargeTransform.gameObject;
                    }

                    var chargeRect = chargeBarGo.GetComponent<RectTransform>();
                    chargeRect.anchorMin = new Vector2(0.1f, 0.36f);
                    chargeRect.anchorMax = new Vector2(0.9f, 0.36f);
                    chargeRect.pivot = new Vector2(0.5f, 0.5f);
                    chargeRect.sizeDelta = new Vector2(0f, 6f);

                    var chargeBgImg = chargeBarGo.GetComponent<Image>();
                    chargeBgImg.sprite = pillSprite;
                    chargeBgImg.type = Image.Type.Sliced;
                    chargeBgImg.color = new Color(0.7f, 0.75f, 0.8f, 0.5f);

                    var fillTransform = chargeBarGo.transform.Find("Fill");
                    GameObject fillGo;
                    if (fillTransform == null)
                    {
                        fillGo = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                        fillGo.transform.SetParent(chargeBarGo.transform, false);
                    }
                    else
                    {
                        fillGo = fillTransform.gameObject;
                    }

                    var fillRect = fillGo.GetComponent<RectTransform>();
                    fillRect.anchorMin = Vector2.zero;
                    fillRect.anchorMax = Vector2.one;
                    fillRect.offsetMin = Vector2.zero;
                    fillRect.offsetMax = Vector2.zero;

                    chargeFillImg = fillGo.GetComponent<Image>();
                    chargeFillImg.sprite = pillSprite;
                    chargeFillImg.type = Image.Type.Filled;
                    chargeFillImg.fillMethod = Image.FillMethod.Horizontal;
                    chargeFillImg.fillOrigin = 0;
                    chargeFillImg.fillAmount = 1f;
                    chargeFillImg.color = readyCol;
                }

                var cardHud = cardGo.GetComponent<SkillCardHud>();
                if (cardHud == null) cardHud = cardGo.AddComponent<SkillCardHud>();

                var cardSo = new SerializedObject(cardHud);
                cardSo.FindProperty("skillType").enumValueIndex = (int)cardType;
                cardSo.FindProperty("cardBackground").objectReferenceValue = bgImg;
                cardSo.FindProperty("borderHighlight").objectReferenceValue = borderImg;
                cardSo.FindProperty("cooldownFill").objectReferenceValue = cdImg;
                cardSo.FindProperty("canvasGroup").objectReferenceValue = cardCg;
                cardSo.FindProperty("keyChipText").objectReferenceValue = chipTmp;
                cardSo.FindProperty("skillNameText").objectReferenceValue = nameTmp;
                cardSo.FindProperty("stateLineText").objectReferenceValue = stateTmp;
                cardSo.FindProperty("theme").objectReferenceValue = theme;

                if (cardType == SkillCardType.SlowMotion)
                {
                    cardSo.FindProperty("chargeBarContainer").objectReferenceValue = chargeBarGo;
                    cardSo.FindProperty("chargeBarFill").objectReferenceValue = chargeFillImg;
                }

                cardSo.ApplyModifiedProperties();
                cardHud.SetState(SkillCardState.Ready);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(groupRect);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[HudSkillBarBuilder] Successfully built and saved Bottom-Center Skill Bar HUD!");
        }
    }
}
