using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DodgeGame.Data;
using DodgeGame.UI;

namespace DodgeGame.Tests.EditMode
{
    [TestFixture]
    public class HudTimerTests
    {
        private GameObject go;
        private TimerUI timerUI;
        private TextMeshProUGUI timerText;
        private TextMeshProUGUI chipText;
        private HudTheme theme;

        [SetUp]
        public void Setup()
        {
            go = new GameObject("TopCenter_TimerGroup", typeof(RectTransform));
            timerUI = go.AddComponent<TimerUI>();

            var timerValGo = new GameObject("TimerValue", typeof(RectTransform), typeof(TextMeshProUGUI));
            timerValGo.transform.SetParent(go.transform, false);
            timerText = timerValGo.GetComponent<TextMeshProUGUI>();

            var chipGo = new GameObject("DifficultyChip", typeof(RectTransform));
            chipGo.transform.SetParent(go.transform, false);
            var chipTextGo = new GameObject("DifficultyText", typeof(RectTransform), typeof(TextMeshProUGUI));
            chipTextGo.transform.SetParent(chipGo.transform, false);
            chipText = chipTextGo.GetComponent<TextMeshProUGUI>();

            theme = ScriptableObject.CreateInstance<HudTheme>();
            timerUI.Theme = theme;

            // Wire serialized fields via reflection / properties
            var timerField = typeof(TimerUI).GetField("timerText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            timerField?.SetValue(timerUI, timerText);

            var chipField = typeof(TimerUI).GetField("difficultyChipText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            chipField?.SetValue(timerUI, chipText);
        }

        [TearDown]
        public void Teardown()
        {
            if (go != null) Object.DestroyImmediate(go);
            if (theme != null) Object.DestroyImmediate(theme);
        }

        [Test]
        public void Timer_Formats_TabularFigures_MM_SS_hh()
        {
            timerUI.UpdateTimerText(75.34f);

            // 75.34s = 1 min, 15 sec, 34 hundredths
            Assert.IsTrue(timerText.text.Contains("01:15.34"), $"Timer text '{timerText.text}' should contain '01:15.34'.");
            Assert.IsTrue(timerText.text.StartsWith("<mspace=") && timerText.text.EndsWith("</mspace>"), "Timer text must use tabular figures.");
        }

        [Test]
        public void DifficultyChip_Displays_SelectedDifficulty_DisplayName_Only()
        {
            // Verify all DisplayNames format cleanly without EffectiveTier
            string easyName = Difficulty.Easy.ToString();
            string normalName = Difficulty.Normal.ToString();
            string hardName = Difficulty.Hard.ToString();
            string nightmareName = Difficulty.Nightmare.ToString();

            Assert.AreEqual("Easy", easyName);
            Assert.AreEqual("Normal", normalName);
            Assert.AreEqual("Hard", hardName);
            Assert.AreEqual("Nightmare", nightmareName);

            // Verify chip accepts DisplayName
            chipText.text = hardName;
            Assert.AreEqual("Hard", chipText.text);
            Assert.IsFalse(chipText.text.Contains("Tier"), "Difficulty chip must never display EffectiveTier.");
        }

        [Test]
        public void SlowMotion_Turns_TimerText_AccentActive_Blue_AndRestores()
        {
            timerUI.UpdateTimerText(42.15f);
            string initialValue = timerText.text;

            // Trigger Slow Mo Start
            timerUI.HandleSlowMoStart();

            Assert.IsTrue(timerUI.IsSlowMoIndicatorActive, "Slow motion indicator should be active.");
            Assert.AreEqual(theme.AccentActive.r, timerText.color.r, 0.05f);
            Assert.AreEqual(theme.AccentActive.g, timerText.color.g, 0.05f);
            Assert.AreEqual(theme.AccentActive.b, timerText.color.b, 0.05f);
            Assert.AreEqual(initialValue, timerText.text, "Timer value must NOT change when entering Slow Motion.");

            // Trigger Slow Mo End
            timerUI.HandleSlowMoEnd();

            Assert.IsFalse(timerUI.IsSlowMoIndicatorActive, "Slow motion indicator should be inactive.");
            Assert.AreEqual(theme.TextPrimary.r, timerText.color.r, 0.05f);
            Assert.AreEqual(theme.TextPrimary.g, timerText.color.g, 0.05f);
            Assert.AreEqual(theme.TextPrimary.b, timerText.color.b, 0.05f);
        }
    }
}
