using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DodgeGame.Core;
using DodgeGame.Data;
using DodgeGame.UI;

namespace DodgeGame.Tests.EditMode
{
    [TestFixture]
    public class DifficultyModalTests
    {
        private GameObject modalGo;
        private DifficultyModalController controller;
        private DifficultyConfig config;
        private SettingsData settingsData;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<DifficultyConfig>();
            settingsData = ScriptableObject.CreateInstance<SettingsData>();
            settingsData.ResetToDefaults();

            modalGo = new GameObject("DifficultyModal_Test", typeof(RectTransform), typeof(CanvasGroup));
            var dimmerGo = new GameObject("Dimmer", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            dimmerGo.transform.SetParent(modalGo.transform);

            var panelGo = new GameObject("Panel", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            panelGo.transform.SetParent(modalGo.transform);

            var titleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(panelGo.transform);

            var subGo = new GameObject("Subtitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            subGo.transform.SetParent(panelGo.transform);

            var easyCard = CreateTestCard(panelGo.transform, Difficulty.Easy);
            var normalCard = CreateTestCard(panelGo.transform, Difficulty.Normal);
            var hardCard = CreateTestCard(panelGo.transform, Difficulty.Hard);
            var nightmareCard = CreateTestCard(panelGo.transform, Difficulty.Nightmare);

            var backBtnGo = new GameObject("BackButton", typeof(RectTransform), typeof(Image), typeof(Button));
            backBtnGo.transform.SetParent(panelGo.transform);

            controller = modalGo.AddComponent<DifficultyModalController>();

            var so = new UnityEditor.SerializedObject(controller);
            so.FindProperty("dimmerCanvasGroup").objectReferenceValue = dimmerGo.GetComponent<CanvasGroup>();
            so.FindProperty("dimmerImage").objectReferenceValue = dimmerGo.GetComponent<Image>();
            so.FindProperty("modalPanelRect").objectReferenceValue = panelGo.GetComponent<RectTransform>();
            so.FindProperty("modalPanelCanvasGroup").objectReferenceValue = panelGo.GetComponent<CanvasGroup>();
            so.FindProperty("titleText").objectReferenceValue = titleGo.GetComponent<TextMeshProUGUI>();
            so.FindProperty("subtitleText").objectReferenceValue = subGo.GetComponent<TextMeshProUGUI>();
            so.FindProperty("easyCard").objectReferenceValue = easyCard;
            so.FindProperty("normalCard").objectReferenceValue = normalCard;
            so.FindProperty("hardCard").objectReferenceValue = hardCard;
            so.FindProperty("nightmareCard").objectReferenceValue = nightmareCard;
            so.FindProperty("backButton").objectReferenceValue = backBtnGo.GetComponent<Button>();
            so.FindProperty("difficultyConfig").objectReferenceValue = config;
            so.ApplyModifiedProperties();
        }

        private DifficultyCardUI CreateTestCard(Transform parent, Difficulty diff)
        {
            var go = new GameObject("Card_" + diff.ToString(), typeof(RectTransform), typeof(Button));
            go.transform.SetParent(parent);

            var bg = new GameObject("Bg", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(go.transform);

            var glow = new GameObject("Glow", typeof(RectTransform), typeof(Image));
            glow.transform.SetParent(go.transform);

            var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(go.transform);

            var name = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
            name.transform.SetParent(go.transform);

            var desc = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
            desc.transform.SetParent(go.transform);

            var card = go.AddComponent<DifficultyCardUI>();
            var so = new UnityEditor.SerializedObject(card);
            so.FindProperty("difficulty").enumValueIndex = (int)diff;
            so.FindProperty("cardBackground").objectReferenceValue = bg.GetComponent<Image>();
            so.FindProperty("glowImage").objectReferenceValue = glow.GetComponent<Image>();
            so.FindProperty("iconImage").objectReferenceValue = icon.GetComponent<Image>();
            so.FindProperty("nameText").objectReferenceValue = name.GetComponent<TextMeshProUGUI>();
            so.FindProperty("descriptionText").objectReferenceValue = desc.GetComponent<TextMeshProUGUI>();
            so.FindProperty("shakeTarget").objectReferenceValue = go.GetComponent<RectTransform>();
            so.ApplyModifiedProperties();

            return card;
        }

        [TearDown]
        public void TearDown()
        {
            if (modalGo != null) Object.DestroyImmediate(modalGo);
            if (config != null) Object.DestroyImmediate(config);
            if (settingsData != null) Object.DestroyImmediate(settingsData);
        }

        [Test]
        public void ModalOpens_SetsActiveAndIsOpenTrue()
        {
            controller.Open();
            Assert.IsTrue(controller.IsOpen);
            Assert.IsTrue(modalGo.activeSelf);
        }

        [Test]
        public void ModalCloses_SetsIsOpenFalse()
        {
            controller.Open();
            controller.Close();
            Assert.IsFalse(controller.IsOpen);
        }

        [Test]
        public void BlockedDifficulties_AreMarkedInConfig()
        {
            Assert.IsTrue(config.GetMapping(Difficulty.Easy).BlocksStart);
            Assert.IsTrue(config.GetMapping(Difficulty.Normal).BlocksStart);
            Assert.IsFalse(config.GetMapping(Difficulty.Hard).BlocksStart);
            Assert.IsFalse(config.GetMapping(Difficulty.Nightmare).BlocksStart);
        }

        [Test]
        public void EffectiveTiers_MapCorrectly()
        {
            Assert.AreEqual(Difficulty.Easy, config.GetMapping(Difficulty.Hard).EffectiveTier);
            Assert.AreEqual(Difficulty.Normal, config.GetMapping(Difficulty.Nightmare).EffectiveTier);
        }
    }
}
