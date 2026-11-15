using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using DodgeGame.Core;
using DodgeGame.UI;

namespace DodgeGame.Tests.PlayMode
{
    [TestFixture]
    public class MenuNavigationTests
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            TimeScaleController.ResetTimeScale();
            yield return SceneManager.LoadSceneAsync("MainMenu");
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            TimeScaleController.ResetTimeScale();
            yield return null;
        }

        [UnityTest]
        public IEnumerator MenuNavigation_HowToPlayButton_OpensModalAndFocusesElement()
        {
            var menuUI = Object.FindAnyObjectByType<MainMenuUI>();
            Assert.IsNotNull(menuUI, "MainMenuUI instance must be present in MainMenu scene.");
            Assert.IsNotNull(menuUI.HowToPlayButton, "HowToPlayButton must be wired.");

            menuUI.HowToPlayButton.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.4f);

            Assert.IsNotNull(menuUI.HowToPlayTransition, "HowToPlay transition must exist.");
            Assert.IsTrue(menuUI.HowToPlayTransition.IsOpen, "HowToPlay panel must be open after button click.");

            var es = EventSystem.current;
            Assert.IsNotNull(es, "EventSystem must exist.");
            Assert.IsNotNull(es.currentSelectedGameObject, "An element inside the modal must receive focus.");
        }

        [UnityTest]
        public IEnumerator MenuNavigation_SettingsButton_OpensModalAndFocusesMasterVolumeSlider()
        {
            var menuUI = Object.FindAnyObjectByType<MainMenuUI>();
            Assert.IsNotNull(menuUI, "MainMenuUI instance must be present.");
            Assert.IsNotNull(menuUI.SettingsButton, "SettingsButton must be wired.");

            menuUI.SettingsButton.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.4f);

            bool isModalOpen = (menuUI.SettingsModalController != null && menuUI.SettingsModalController.IsOpen) ||
                               (menuUI.SettingsTransition != null && menuUI.SettingsTransition.IsOpen);
            Assert.IsTrue(isModalOpen, "Settings panel must be open after button click.");

            var es = EventSystem.current;
            Assert.IsNotNull(es, "EventSystem must exist.");
            if (menuUI.SettingsPanelUI != null && menuUI.SettingsPanelUI.MasterVolumeSlider != null)
            {
                Assert.AreEqual(menuUI.SettingsPanelUI.MasterVolumeSlider.gameObject, es.currentSelectedGameObject,
                    "Master Volume slider must receive focus when opening Settings modal.");
            }
        }

        [UnityTest]
        public IEnumerator MenuNavigation_AboutButton_OpensAboutModal()
        {
            var menuUI = Object.FindAnyObjectByType<MainMenuUI>();
            Assert.IsNotNull(menuUI, "MainMenuUI instance must be present.");
            Assert.IsNotNull(menuUI.AboutButton, "AboutButton must be wired.");

            menuUI.AboutButton.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.4f);

            Assert.IsNotNull(menuUI.AboutTransition, "About transition must exist.");
            Assert.IsTrue(menuUI.AboutTransition.IsOpen, "About panel must be open after button click.");
        }

        [UnityTest]
        public IEnumerator MenuNavigation_EscapeClosesModalAndRestoresFocus()
        {
            var menuUI = Object.FindAnyObjectByType<MainMenuUI>();
            Assert.IsNotNull(menuUI, "MainMenuUI must be present.");

            // 1. Open Settings modal
            menuUI.SettingsButton.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.4f);
            bool isModalOpen = (menuUI.SettingsModalController != null && menuUI.SettingsModalController.IsOpen) ||
                               (menuUI.SettingsTransition != null && menuUI.SettingsTransition.IsOpen);
            Assert.IsTrue(isModalOpen, "Settings modal must be open.");

            // 2. Invoke escape / close active modal
            menuUI.CloseActiveModal();
            yield return new WaitForSecondsRealtime(0.4f);

            bool isStillOpen = (menuUI.SettingsModalController != null && menuUI.SettingsModalController.IsOpen) ||
                               (menuUI.SettingsTransition != null && menuUI.SettingsTransition.IsOpen);
            Assert.IsFalse(isStillOpen, "Settings modal must be closed after escape/close.");

            // 3. Focus restoration
            var es = EventSystem.current;
            if (es != null && es.currentSelectedGameObject != null)
            {
                Assert.AreEqual(menuUI.SettingsButton.gameObject, es.currentSelectedGameObject,
                    "Focus must be restored to Settings button when returning from settings panel.");
            }
        }

        [UnityTest]
        public IEnumerator MenuNavigation_PlayGameButton_OpensDifficultyModal()
        {
            var menuUI = Object.FindAnyObjectByType<MainMenuUI>();
            Assert.IsNotNull(menuUI, "MainMenuUI must be present.");
            Assert.IsNotNull(menuUI.PlayButton, "PlayButton must be wired.");

            menuUI.PlayButton.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.15f);

            Assert.IsNotNull(menuUI.DifficultyModal, "DifficultyModal must be wired.");
            Assert.IsTrue(menuUI.DifficultyModal.IsOpen, "DifficultyModal must be open after clicking Play.");
        }
    }
}
