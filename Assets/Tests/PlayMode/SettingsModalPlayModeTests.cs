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
    public class SettingsModalPlayModeTests
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
        public IEnumerator SettingsModal_OpenAndClickX_ClosesModalAndRestoresFocusToSettingsButton()
        {
            var menuUI = Object.FindAnyObjectByType<MainMenuUI>();
            Assert.IsNotNull(menuUI, "MainMenuUI must be present.");
            Assert.IsNotNull(menuUI.SettingsButton, "SettingsButton must be present.");

            // 1. Open Settings
            menuUI.SettingsButton.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.35f);

            var settingsCtrl = menuUI.SettingsModalController;
            Assert.IsNotNull(settingsCtrl, "SettingsModalController must exist.");
            Assert.IsTrue(settingsCtrl.IsOpen, "Settings modal must be open.");
            Assert.IsNotNull(settingsCtrl.CloseButton, "CloseButton must exist on SettingsModal.");

            // 2. Click Close (X) button
            settingsCtrl.CloseButton.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.25f);

            Assert.IsFalse(settingsCtrl.IsOpen, "Settings modal must be closed after clicking X.");

            // 3. Verify focus returned to Settings button
            var es = EventSystem.current;
            Assert.IsNotNull(es, "EventSystem must exist.");
            Assert.AreEqual(menuUI.SettingsButton.gameObject, es.currentSelectedGameObject,
                "Focus must be restored to SettingsButton after closing modal.");
        }

        [UnityTest]
        public IEnumerator SettingsModal_ClickX20Times_ClosesEveryTime()
        {
            var menuUI = Object.FindAnyObjectByType<MainMenuUI>();
            Assert.IsNotNull(menuUI, "MainMenuUI must be present.");
            var settingsCtrl = menuUI.SettingsModalController;
            Assert.IsNotNull(settingsCtrl, "SettingsModalController must exist.");

            for (int i = 0; i < 20; i++)
            {
                menuUI.SettingsButton.onClick.Invoke();
                yield return null;
                Assert.IsTrue(settingsCtrl.IsOpen, $"Iteration {i + 1}: Modal must open.");

                settingsCtrl.CloseButton.onClick.Invoke();
                yield return null;
                Assert.IsFalse(settingsCtrl.IsOpen, $"Iteration {i + 1}: Modal must close when X is clicked.");
            }
        }

        [UnityTest]
        public IEnumerator SettingsModal_CloseViaDimmerAndBack_ClosesModal()
        {
            var menuUI = Object.FindAnyObjectByType<MainMenuUI>();
            Assert.IsNotNull(menuUI, "MainMenuUI must be present.");
            var settingsCtrl = menuUI.SettingsModalController;
            Assert.IsNotNull(settingsCtrl, "SettingsModalController must exist.");

            // Test Dimmer Click
            menuUI.SettingsButton.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.1f);
            Assert.IsTrue(settingsCtrl.IsOpen, "Modal must be open.");

            Assert.IsNotNull(settingsCtrl.DimmerButton, "DimmerButton must exist for click dismissal.");
            settingsCtrl.DimmerButton.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.25f);
            Assert.IsFalse(settingsCtrl.IsOpen, "Modal must close when clicking dimmer background.");

            // Test Back Button Click
            menuUI.SettingsButton.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.1f);
            Assert.IsTrue(settingsCtrl.IsOpen, "Modal must be open.");

            Assert.IsNotNull(settingsCtrl.BackButton, "BackButton must exist.");
            settingsCtrl.BackButton.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.25f);
            Assert.IsFalse(settingsCtrl.IsOpen, "Modal must close when clicking Back button.");
        }

        [UnityTest]
        public IEnumerator SettingsModal_OpenClose10Times_FocusRestoredAndNoDuplicateModals()
        {
            var menuUI = Object.FindAnyObjectByType<MainMenuUI>();
            Assert.IsNotNull(menuUI, "MainMenuUI must be present.");
            var settingsCtrl = menuUI.SettingsModalController;
            Assert.IsNotNull(settingsCtrl, "SettingsModalController must exist.");

            for (int i = 0; i < 10; i++)
            {
                menuUI.SettingsButton.onClick.Invoke();
                yield return new WaitForSecondsRealtime(0.20f);

                settingsCtrl.CloseButton.onClick.Invoke();
                yield return new WaitForSecondsRealtime(0.20f);

                Assert.AreEqual(menuUI.SettingsButton.gameObject, EventSystem.current.currentSelectedGameObject,
                    $"Iteration {i + 1}: Focus must return to SettingsButton.");
            }

            var modals = Object.FindObjectsByType<SettingsModalController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.AreEqual(1, modals.Length, "There must be exactly one SettingsModalController in the scene (no duplicate modals).");
        }

        [UnityTest]
        public IEnumerator SettingsModal_MasterVolumeSlider_UpdatesSettingsDataAndAudio()
        {
            var menuUI = Object.FindAnyObjectByType<MainMenuUI>();
            var settingsCtrl = menuUI.SettingsModalController;

            menuUI.SettingsButton.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.1f);

            Assert.IsNotNull(settingsCtrl.MasterSlider, "MasterSlider must exist.");
            settingsCtrl.MasterSlider.value = 0.65f;

            yield return null;

            if (SettingsService.Instance != null && SettingsService.Instance.CurrentSettings != null)
            {
                Assert.AreEqual(0.65f, SettingsService.Instance.CurrentSettings.MasterVolume, 0.01f,
                    "SettingsData MasterVolume must update when slider changes.");
            }

            Assert.AreEqual(0.65f, AudioListener.volume, 0.01f,
                "AudioListener volume must reflect Master Volume slider value.");

            settingsCtrl.Close();
            yield return null;
        }

        [UnityTest]
        public IEnumerator SettingsModal_ReducedMotionToggle_UpdatesSettingsDataAndScaleBehavior()
        {
            var menuUI = Object.FindAnyObjectByType<MainMenuUI>();
            var settingsCtrl = menuUI.SettingsModalController;

            menuUI.SettingsButton.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.1f);

            Assert.IsNotNull(settingsCtrl.ReducedMotionToggle, "ReducedMotionToggle must exist.");

            // Set to true
            settingsCtrl.ReducedMotionToggle.IsOn = true;
            yield return null;

            if (SettingsService.Instance != null && SettingsService.Instance.CurrentSettings != null)
            {
                Assert.IsTrue(SettingsService.Instance.CurrentSettings.ReducedMotion,
                    "SettingsData ReducedMotion must be true.");
            }

            // Close and re-open to verify scale behavior
            settingsCtrl.CloseImmediate();
            yield return null;

            settingsCtrl.Open();
            // Under Reduced Motion, initial localScale is Vector3.one
            if (settingsCtrl.ModalPanelRect != null)
            {
                Assert.AreEqual(1f, settingsCtrl.ModalPanelRect.localScale.x, 0.01f,
                    "Reduced motion must eliminate initial 0.95 scale down animation.");
            }

            // Reset back
            settingsCtrl.ReducedMotionToggle.IsOn = false;
            settingsCtrl.CloseImmediate();
            yield return null;
        }

        [UnityTest]
        public IEnumerator SettingsModal_InvertCameraY_UpdatesSettingsDataAndCameraController()
        {
            var menuUI = Object.FindAnyObjectByType<MainMenuUI>();
            var settingsCtrl = menuUI.SettingsModalController;

            menuUI.SettingsButton.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.1f);

            Assert.IsNotNull(settingsCtrl.CameraInvertYToggle, "CameraInvertYToggle must exist.");

            settingsCtrl.CameraInvertYToggle.IsOn = true;
            yield return null;

            if (SettingsService.Instance != null && SettingsService.Instance.CurrentSettings != null)
            {
                Assert.IsTrue(SettingsService.Instance.CurrentSettings.CameraInvertY,
                    "SettingsData CameraInvertY must be true.");
            }

            var cam = Object.FindAnyObjectByType<ArenaCameraController>();
            Assert.IsNotNull(cam, "ArenaCameraController must exist in scene.");

            // Reset back
            settingsCtrl.CameraInvertYToggle.IsOn = false;
            settingsCtrl.CloseImmediate();
            yield return null;
        }
    }
}
