using NUnit.Framework;
using UnityEngine;
using DodgeGame.Core;
using DodgeGame.Data;

namespace DodgeGame.Tests.EditMode
{
    [TestFixture]
    public class SettingsServiceTests
    {
        private GameObject serviceGo;
        private SettingsService service;

        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteKey(SettingsService.SETTINGS_PREFS_KEY);
            serviceGo = new GameObject("TestSettingsService");
            service = serviceGo.AddComponent<SettingsService>();
            service.InitializeSettings();
        }

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey(SettingsService.SETTINGS_PREFS_KEY);
            if (serviceGo != null)
            {
                Object.DestroyImmediate(serviceGo);
            }
        }

        [Test]
        public void SaveAndLoadRoundTrip_PreservesAllSettingsValues()
        {
            // Modify settings
            service.SetMasterVolume(0.72f);
            service.SetMusicVolume(0.48f);
            service.SetSfxVolume(0.91f);
            service.SetScreenShake(false);
            service.SetReducedMotion(true);
            service.SetBrightness(1.15f);
            service.SetQualityTier(QualityTier.Low);

            // Save
            service.SaveSettings();

            // Create a fresh new instance to load from PlayerPrefs
            var secondGo = new GameObject("SecondSettingsService");
            var secondService = secondGo.AddComponent<SettingsService>();
            secondService.LoadSettings();

            var loaded = secondService.CurrentSettings;
            Assert.IsNotNull(loaded, "Loaded settings must not be null.");

            Assert.AreEqual(0.72f, loaded.MasterVolume, 0.01f, "MasterVolume must match.");
            Assert.AreEqual(0.48f, loaded.MusicVolume, 0.01f, "MusicVolume must match.");
            Assert.AreEqual(0.91f, loaded.SfxVolume, 0.01f, "SfxVolume must match.");
            Assert.AreEqual(false, loaded.ScreenShakeEnabled, "ScreenShakeEnabled must match.");
            Assert.AreEqual(true, loaded.ReducedMotion, "ReducedMotion must match.");
            Assert.AreEqual(1.15f, loaded.Brightness, 0.01f, "Brightness must match.");
            Assert.AreEqual(QualityTier.Low, loaded.QualityTier, "QualityTier must match.");

            Object.DestroyImmediate(secondGo);
        }

        [Test]
        public void ResetToDefaults_RestoresStandardConfiguration()
        {
            service.SetMasterVolume(0.1f);
            service.SetReducedMotion(true);

            service.ResetToDefaults();

            Assert.AreEqual(1.0f, service.CurrentSettings.MasterVolume, 0.05f, "Default MasterVolume is typically 1.0.");
            Assert.IsFalse(service.CurrentSettings.ReducedMotion, "Default ReducedMotion is false.");
        }
    }
}
