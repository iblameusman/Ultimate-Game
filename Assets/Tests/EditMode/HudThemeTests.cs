using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using DodgeGame.Data;

namespace DodgeGame.Tests.EditMode
{
    [TestFixture]
    public class HudThemeTests
    {
        [Test]
        public void HudTheme_AssetExists_AndHasAllRequiredColors()
        {
            var theme = AssetDatabase.LoadAssetAtPath<HudTheme>("Assets/Scripts/Data/HudTheme.asset")
                     ?? Resources.Load<HudTheme>("HudTheme");

            Assert.IsNotNull(theme, "HudTheme.asset should exist in Assets/Scripts/Data/ or Resources.");

            // Verify none are fully transparent except the glow
            Assert.Greater(theme.PanelColor.a, 0f, "PanelColor must not be fully transparent.");
            Assert.AreEqual(0.85f, theme.PanelColor.a, 0.05f, "PanelColor opacity must be 85%.");

            Assert.Greater(theme.PanelBorderColor.a, 0f, "PanelBorderColor must not be fully transparent.");
            Assert.Greater(theme.PanelInnerBevelColor.a, 0f, "PanelInnerBevelColor must not be fully transparent.");
            Assert.Greater(theme.TextPrimary.a, 0f, "TextPrimary must not be fully transparent.");
            Assert.Greater(theme.TextSecondary.a, 0f, "TextSecondary must not be fully transparent.");
            Assert.Greater(theme.AccentReady.a, 0f, "AccentReady must not be fully transparent.");
            Assert.Greater(theme.AccentActive.a, 0f, "AccentActive must not be fully transparent.");
            Assert.Greater(theme.CooldownFill.a, 0f, "CooldownFill must not be fully transparent.");
            Assert.Greater(theme.Danger.a, 0f, "Danger must not be fully transparent.");

            // Glow is allowed / expected to be transparent
            Assert.LessOrEqual(theme.GlowColor.a, 0.01f, "GlowColor may be fully transparent.");
        }

        [Test]
        public void HudTheme_ColorValues_MatchSpecificationHex()
        {
            var theme = AssetDatabase.LoadAssetAtPath<HudTheme>("Assets/Scripts/Data/HudTheme.asset")
                     ?? Resources.Load<HudTheme>("HudTheme");

            Assert.IsNotNull(theme);

            // Primary Text #1E2A33: 30/255, 42/255, 51/255
            Assert.AreEqual(30f / 255f, theme.TextPrimary.r, 0.02f);
            Assert.AreEqual(42f / 255f, theme.TextPrimary.g, 0.02f);
            Assert.AreEqual(51f / 255f, theme.TextPrimary.b, 0.02f);

            // Accent Ready #3DDC2A: 61/255, 220/255, 42/255
            Assert.AreEqual(61f / 255f, theme.AccentReady.r, 0.02f);
            Assert.AreEqual(220f / 255f, theme.AccentReady.g, 0.02f);
            Assert.AreEqual(42f / 255f, theme.AccentReady.b, 0.02f);

            // Accent Active #2F9BFF: 47/255, 155/255, 255/255
            Assert.AreEqual(47f / 255f, theme.AccentActive.r, 0.02f);
            Assert.AreEqual(155f / 255f, theme.AccentActive.g, 0.02f);
            Assert.AreEqual(255f / 255f, theme.AccentActive.b, 0.02f);

            // Danger #E5484D: 229/255, 72/255, 77/255
            Assert.AreEqual(229f / 255f, theme.Danger.r, 0.02f);
            Assert.AreEqual(72f / 255f, theme.Danger.g, 0.02f);
            Assert.AreEqual(77f / 255f, theme.Danger.b, 0.02f);
        }
    }
}
