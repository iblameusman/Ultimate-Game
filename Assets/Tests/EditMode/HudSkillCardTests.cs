using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DodgeGame.UI;
using DodgeGame.Data;

namespace DodgeGame.Tests
{
    [TestFixture]
    public class HudSkillCardTests
    {
        private GameObject testRoot;
        private SkillCardHud phaseCard;
        private SkillCardHud dashCard;
        private SkillCardHud slowMoCard;
        private HudTheme theme;

        [SetUp]
        public void SetUp()
        {
            testRoot = new GameObject("TestRoot");
            theme = ScriptableObject.CreateInstance<HudTheme>();

            phaseCard = CreateCard("PhaseCard", SkillCardType.Phase);
            dashCard = CreateCard("DashCard", SkillCardType.Dash);
            slowMoCard = CreateCard("SlowMoCard", SkillCardType.SlowMotion);
        }

        [TearDown]
        public void TearDown()
        {
            if (testRoot != null) Object.DestroyImmediate(testRoot);
            if (theme != null) Object.DestroyImmediate(theme);
        }

        private SkillCardHud CreateCard(string name, SkillCardType type)
        {
            var cardGo = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            cardGo.transform.SetParent(testRoot.transform, false);

            var rt = cardGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(240f, 88f);

            var cg = cardGo.GetComponent<CanvasGroup>();

            var bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(cardGo.transform, false);
            var bgImg = bgGo.GetComponent<Image>();

            var borderGo = new GameObject("BorderHighlight", typeof(RectTransform), typeof(Image));
            borderGo.transform.SetParent(cardGo.transform, false);
            var borderImg = borderGo.GetComponent<Image>();

            var cdGo = new GameObject("CooldownFill", typeof(RectTransform), typeof(Image));
            cdGo.transform.SetParent(cardGo.transform, false);
            var cdImg = cdGo.GetComponent<Image>();
            cdImg.type = Image.Type.Filled;

            var keyGo = new GameObject("KeyChip", typeof(RectTransform));
            keyGo.transform.SetParent(cardGo.transform, false);
            var keyTextGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            keyTextGo.transform.SetParent(keyGo.transform, false);
            var keyTmp = keyTextGo.GetComponent<TextMeshProUGUI>();

            var nameGo = new GameObject("SkillName", typeof(RectTransform), typeof(TextMeshProUGUI));
            nameGo.transform.SetParent(cardGo.transform, false);
            var nameTmp = nameGo.GetComponent<TextMeshProUGUI>();

            var stateGo = new GameObject("StateLine", typeof(RectTransform), typeof(TextMeshProUGUI));
            stateGo.transform.SetParent(cardGo.transform, false);
            var stateTmp = stateGo.GetComponent<TextMeshProUGUI>();

            GameObject chargeBarGo = null;
            Image chargeFillImg = null;
            if (type == SkillCardType.SlowMotion)
            {
                chargeBarGo = new GameObject("ChargeBar", typeof(RectTransform));
                chargeBarGo.transform.SetParent(cardGo.transform, false);
                var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
                fillGo.transform.SetParent(chargeBarGo.transform, false);
                chargeFillImg = fillGo.GetComponent<Image>();
                chargeFillImg.type = Image.Type.Filled;
            }

            var card = cardGo.AddComponent<SkillCardHud>();
            card.SkillType = type;
            card.Theme = theme;

            // Use reflection or direct setter if fields are serialized
            var so = new UnityEditor.SerializedObject(card);
            so.FindProperty("skillType").enumValueIndex = (int)type;
            so.FindProperty("cardBackground").objectReferenceValue = bgImg;
            so.FindProperty("borderHighlight").objectReferenceValue = borderImg;
            so.FindProperty("cooldownFill").objectReferenceValue = cdImg;
            so.FindProperty("canvasGroup").objectReferenceValue = cg;
            so.FindProperty("keyChipText").objectReferenceValue = keyTmp;
            so.FindProperty("skillNameText").objectReferenceValue = nameTmp;
            so.FindProperty("stateLineText").objectReferenceValue = stateTmp;
            so.FindProperty("theme").objectReferenceValue = theme;
            if (type == SkillCardType.SlowMotion)
            {
                so.FindProperty("chargeBarContainer").objectReferenceValue = chargeBarGo;
                so.FindProperty("chargeBarFill").objectReferenceValue = chargeFillImg;
            }
            so.ApplyModifiedProperties();

            card.SetState(SkillCardState.Ready, forceRefresh: true);
            card.UpdateKeyChip();
            return card;
        }

        [Test]
        public void SkillCard_StateTransitions_ReadyToActiveToCooldownToReady()
        {
            // Ready
            phaseCard.SetState(SkillCardState.Ready);
            Assert.AreEqual(SkillCardState.Ready, phaseCard.CurrentState);
            Assert.AreEqual("READY", phaseCard.StateLineText.text);
            Assert.AreEqual(1f, phaseCard.CardCanvasGroup.alpha);

            // Active
            phaseCard.SetState(SkillCardState.Active);
            Assert.AreEqual(SkillCardState.Active, phaseCard.CurrentState);
            Assert.AreEqual(1f, phaseCard.CardCanvasGroup.alpha);

            // Cooldown
            phaseCard.SetState(SkillCardState.Cooldown);
            phaseCard.UpdateCooldown(2.5f, 0.5f);
            Assert.AreEqual(SkillCardState.Cooldown, phaseCard.CurrentState);
            Assert.AreEqual("2.5s", phaseCard.StateLineText.text);
            Assert.AreEqual(0.5f, phaseCard.CooldownFill.fillAmount);

            // Back to Ready
            phaseCard.SetState(SkillCardState.Ready);
            Assert.AreEqual(SkillCardState.Ready, phaseCard.CurrentState);
            Assert.AreEqual("READY", phaseCard.StateLineText.text);
            Assert.AreEqual(0f, phaseCard.CooldownFill.fillAmount);
        }

        [Test]
        public void SkillCard_LockedState_OpacityIsPointFour_AndTextIsLocked()
        {
            dashCard.SetState(SkillCardState.Locked);
            Assert.AreEqual(SkillCardState.Locked, dashCard.CurrentState);
            Assert.AreEqual(0.4f, dashCard.CardCanvasGroup.alpha, 0.01f);
            Assert.AreEqual("LOCKED", dashCard.StateLineText.text);
        }

        [Test]
        public void SkillCard_KeyChips_SwitchBetweenKeyboardAndGamepad()
        {
            // Keyboard mode
            phaseCard.SetInputDevice(false);
            dashCard.SetInputDevice(false);
            slowMoCard.SetInputDevice(false);

            Assert.AreEqual("[Q]", phaseCard.KeyChipText.text);
            Assert.AreEqual("[Shift]", dashCard.KeyChipText.text);
            Assert.AreEqual("[E]", slowMoCard.KeyChipText.text);

            // Gamepad mode
            phaseCard.SetInputDevice(true);
            dashCard.SetInputDevice(true);
            slowMoCard.SetInputDevice(true);

            Assert.AreEqual("LB", phaseCard.KeyChipText.text);
            Assert.AreEqual("B", dashCard.KeyChipText.text);
            Assert.AreEqual("Y", slowMoCard.KeyChipText.text);
        }

        [Test]
        public void SlowMotionCard_ChargeBar_UpdatesPercentageAndReady()
        {
            // 50% charge
            slowMoCard.UpdateSlowMoCharge(50f, 100f);
            Assert.AreEqual("50%", slowMoCard.StateLineText.text);

            // 100% charge
            slowMoCard.UpdateSlowMoCharge(100f, 100f);
            Assert.AreEqual(SkillCardState.Ready, slowMoCard.CurrentState);
            Assert.AreEqual("READY", slowMoCard.StateLineText.text);
        }
    }
}
