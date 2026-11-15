using NUnit.Framework;
using UnityEngine;
using TMPro;
using DodgeGame.UI;
using DodgeGame.Data;

namespace DodgeGame.Tests
{
    [TestFixture]
    public class HudFeedbackTests
    {
        private GameObject testRoot;
        private UIJuice uiJuice;
        private HudTheme theme;
        private TextMeshProUGUI streakText;
        private CanvasGroup streakCg;
        private RectTransform floatContainer;

        [SetUp]
        public void SetUp()
        {
            testRoot = new GameObject("TestRoot", typeof(RectTransform), typeof(Canvas));
            theme = ScriptableObject.CreateInstance<HudTheme>();

            var streakGo = new GameObject("StreakCounter", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(CanvasGroup));
            streakGo.transform.SetParent(testRoot.transform, false);
            streakText = streakGo.GetComponent<TextMeshProUGUI>();
            streakCg = streakGo.GetComponent<CanvasGroup>();
            streakCg.alpha = 0f;

            var floatContainerGo = new GameObject("FloatingTextContainer", typeof(RectTransform));
            floatContainerGo.transform.SetParent(testRoot.transform, false);
            floatContainer = floatContainerGo.GetComponent<RectTransform>();

            var juiceGo = new GameObject("UIJuice", typeof(UIJuice));
            juiceGo.transform.SetParent(testRoot.transform, false);
            uiJuice = juiceGo.GetComponent<UIJuice>();

            var so = new UnityEditor.SerializedObject(uiJuice);
            so.FindProperty("streakCounterText").objectReferenceValue = streakText;
            so.FindProperty("streakCanvasGroup").objectReferenceValue = streakCg;
            so.FindProperty("floatingTextContainer").objectReferenceValue = floatContainer;
            so.FindProperty("theme").objectReferenceValue = theme;
            so.ApplyModifiedProperties();
        }

        [TearDown]
        public void TearDown()
        {
            if (testRoot != null) Object.DestroyImmediate(testRoot);
            if (theme != null) Object.DestroyImmediate(theme);
        }

        [Test]
        public void DodgeStreakCounter_ShowsExpectedFormat_AtThresholds()
        {
            // At 5 dodges
            uiJuice.TriggerStreak(5);
            Assert.AreEqual("STREAK x5", streakText.text);
            Assert.AreEqual(1f, streakCg.alpha);

            // At 10 dodges
            uiJuice.TriggerStreak(10);
            Assert.AreEqual("STREAK x10", streakText.text);
            Assert.AreEqual(1f, streakCg.alpha);

            // At 15 dodges
            uiJuice.TriggerStreak(15);
            Assert.AreEqual("STREAK x15", streakText.text);
            Assert.AreEqual(1f, streakCg.alpha);

            // At 20 dodges
            uiJuice.TriggerStreak(20);
            Assert.AreEqual("STREAK x20", streakText.text);
            Assert.AreEqual(1f, streakCg.alpha);
        }

        [Test]
        public void DodgeStreakCounter_HidesOnStreakLoss()
        {
            uiJuice.TriggerStreak(5);
            Assert.AreEqual("STREAK x5", streakText.text);

            uiJuice.TriggerStreakLoss();
            // Fast fade initiated
            Assert.LessOrEqual(streakCg.alpha, 1f);
        }

        [Test]
        public void FloatingDelta_SpawnsAbovePlayer_WithHeadingRole()
        {
            // Spawn delta +10
            uiJuice.TriggerFloatingDelta(10);

            // Item created under container
            Assert.GreaterOrEqual(floatContainer.childCount, 1);
            var itemTmp = floatContainer.GetChild(0).GetComponent<TextMeshProUGUI>();
            Assert.NotNull(itemTmp);
            Assert.AreEqual("+10", itemTmp.text);
            Assert.AreEqual(40f, itemTmp.fontSize);
            Assert.IsTrue((itemTmp.fontStyle & FontStyles.Bold) != 0);
        }

        [Test]
        public void NearMissPopup_SpawnsCloseLabel_WithLabelRole()
        {
            uiJuice.TriggerNearMiss();

            Assert.GreaterOrEqual(floatContainer.childCount, 1);
            var itemTmp = floatContainer.GetChild(0).GetComponent<TextMeshProUGUI>();
            Assert.NotNull(itemTmp);
            Assert.AreEqual("CLOSE!", itemTmp.text);
            Assert.AreEqual(24f, itemTmp.fontSize);
            Assert.IsTrue((itemTmp.fontStyle & FontStyles.Bold) != 0);
        }
    }
}
