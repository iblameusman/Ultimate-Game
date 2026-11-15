using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DodgeGame.Data;
using DodgeGame.UI;

namespace DodgeGame.Tests.EditMode
{
    [TestFixture]
    public class HudTypographyTests
    {
        private GameObject go;
        private CanvasScaler scaler;
        private TextMeshProUGUI text;
        private TextStyler styler;

        [SetUp]
        public void Setup()
        {
            go = new GameObject("TestText");
            var canvasGo = new GameObject("Canvas");
            canvasGo.AddComponent<Canvas>();
            scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            go.transform.SetParent(canvasGo.transform, false);
            text = go.AddComponent<TextMeshProUGUI>();
#if UNITY_EDITOR
            text.font = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Art/Fonts/FontDisplay.asset");
#endif
            styler = go.AddComponent<TextStyler>();
        }

        [TearDown]
        public void Teardown()
        {
            if (go != null)
            {
                if (go.transform.parent != null)
                {
                    Object.DestroyImmediate(go.transform.parent.gameObject);
                }
                else
                {
                    Object.DestroyImmediate(go);
                }
            }
        }

        [Test]
        public void TextStyler_EnforcesNoWrap_AndOverflowMode()
        {
            styler.EnforceNoWrap = true;
            styler.ApplyStyle();

            Assert.AreEqual(TextWrappingModes.NoWrap, text.textWrappingMode, "HUD text must have word wrapping disabled.");
            Assert.AreEqual(TextOverflowModes.Overflow, text.overflowMode, "HUD text overflow mode must be Overflow.");
        }

        [Test]
        public void TextStyler_ScoreValue_AppliesDisplayRole_64px_BoldItalic()
        {
            styler.Role = TypographyRole.Display;
            styler.SetCustomFontSize(64f);
            styler.SetCustomFontStyle(FontStyles.Bold | FontStyles.Italic);

            Assert.AreEqual(64f, text.fontSize, 0.5f, "Score value font size must be 64px at 1080p.");
            Assert.IsTrue((text.fontStyle & FontStyles.Bold) != 0, "Score value must be bold.");
            Assert.IsTrue((text.fontStyle & FontStyles.Italic) != 0, "Score value must be italic.");
        }

        [Test]
        public void TextStyler_ScoreLabel_AppliesLabelRole_22px_Bold()
        {
            styler.Role = TypographyRole.Label;
            styler.SetCustomFontSize(22f);
            styler.SetCustomFontStyle(FontStyles.Bold);

            Assert.AreEqual(22f, text.fontSize, 0.5f, "Score label font size must be 22px.");
            Assert.IsTrue((text.fontStyle & FontStyles.Bold) != 0, "Score label must be bold.");
        }

        [Test]
        public void TextStyler_Best_AppliesLabelRole_24px()
        {
            styler.Role = TypographyRole.Label;
            styler.SetCustomFontSize(24f);

            Assert.AreEqual(24f, text.fontSize, 0.5f, "Best label font size must be 24px.");
        }

        [Test]
        public void TextStyler_Timer_AppliesHeadingRole_36px_AndTabularFormat()
        {
            styler.Role = TypographyRole.Heading;
            styler.SetCustomFontSize(36f);

            Assert.AreEqual(36f, text.fontSize, 0.5f, "Timer font size must be 36px.");

            string formatted = TextStyler.FormatTabular("01:23.45");
            Assert.IsTrue(formatted.StartsWith("<mspace=") && formatted.EndsWith("</mspace>"), "Timer must format using tabular figures.");
        }

        [Test]
        public void TextStyler_SkillLabels_AppliesButtonRole_26px_BoldItalicUppercase()
        {
            styler.Role = TypographyRole.Button;
            styler.SetCustomFontSize(26f);
            styler.SetCustomFontStyle(FontStyles.Bold | FontStyles.Italic | FontStyles.UpperCase);

            Assert.AreEqual(26f, text.fontSize, 0.5f, "Skill label font size must be 26px.");
            Assert.IsTrue((text.fontStyle & FontStyles.Bold) != 0, "Skill label must be bold.");
            Assert.IsTrue((text.fontStyle & FontStyles.Italic) != 0, "Skill label must be italic.");
            Assert.IsTrue((text.fontStyle & FontStyles.UpperCase) != 0, "Skill label must be uppercase.");
        }

        [Test]
        public void TextStyler_Popups_AppliesHeadingRole_40px_AndOutline()
        {
            styler.Role = TypographyRole.Heading;
            styler.SetCustomFontSize(40f);
            Color darkNavy = new Color(30f / 255f, 42f / 255f, 51f / 255f, 1f);
            styler.SetOutline(1.5f, darkNavy);

            Assert.AreEqual(40f, text.fontSize, 0.5f, "Popup font size must be 40px.");
            Assert.AreEqual(1.5f, styler.CustomOutlineWidth, 0.05f, "Popup outline width must be configured as 1.5px.");
            Assert.Greater(text.outlineWidth, 0f, "Popup TMP outline width must be active.");
            Color outlineCol = text.outlineColor;
            Assert.AreEqual(darkNavy.r, outlineCol.r, 0.05f);
            Assert.AreEqual(darkNavy.g, outlineCol.g, 0.05f);
            Assert.AreEqual(darkNavy.b, outlineCol.b, 0.05f);
        }
    }
}
