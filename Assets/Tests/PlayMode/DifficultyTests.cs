using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using DodgeGame.Core;
using DodgeGame.Data;
using DodgeGame.UI;
using DodgeGame.Arena;

namespace DodgeGame.Tests.PlayMode
{
    [TestFixture]
    public class DifficultyTests
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
        public IEnumerator ChangingDifficultySelectionUpdatesSettingsData()
        {
            var menuUI = Object.FindAnyObjectByType<MainMenuUI>();
            Assert.IsNotNull(menuUI, "MainMenuUI instance must be present in MainMenu.");

            // Click Hard
            menuUI.OnDifficultyClicked(Difficulty.Hard);
            yield return null;

            Assert.IsNotNull(SettingsService.Instance, "SettingsService instance must exist.");
            Assert.IsNotNull(SettingsService.Instance.CurrentSettings, "CurrentSettings must exist.");
            Assert.AreEqual(Difficulty.Hard, SettingsService.Instance.CurrentSettings.CurrentDifficulty,
                "SettingsData Difficulty must be updated to Hard.");

            // Click Nightmare
            menuUI.OnDifficultyClicked(Difficulty.Nightmare);
            yield return null;
            Assert.AreEqual(Difficulty.Nightmare, SettingsService.Instance.CurrentSettings.CurrentDifficulty,
                "SettingsData Difficulty must be updated to Nightmare.");

            // Click Normal
            menuUI.OnDifficultyClicked(Difficulty.Normal);
            yield return null;
            Assert.AreEqual(Difficulty.Normal, SettingsService.Instance.CurrentSettings.CurrentDifficulty,
                "SettingsData Difficulty must be restored to Normal.");
        }

        [UnityTest]
        public IEnumerator StartingGameOnHardAppliesRealTuningOfEasyToBallSpawner()
        {
            // Set difficulty to Hard in settings (Section 5: Hard maps to real tuning of Easy)
            SettingsService.Instance.SetDifficulty(Difficulty.Hard);
            yield return null;

            Assert.AreEqual(Difficulty.Hard, SettingsService.Instance.CurrentSettings.SelectedDifficulty,
                "Selected difficulty must be Hard.");
            Assert.AreEqual(Difficulty.Easy, SettingsService.Instance.CurrentSettings.EffectiveDifficulty,
                "Effective difficulty must be Easy due to plot twist mapping.");

            // Load MainGame
            yield return SceneManager.LoadSceneAsync("MainGame");
            yield return null;

            var spawner = Object.FindAnyObjectByType<BallSpawner>();
            Assert.IsNotNull(spawner, "BallSpawner must be present in MainGame.");

            var effectivePreset = SettingsService.Instance.GetCurrentDifficultyPreset();
            var easyPreset = SettingsService.Instance.DifficultyConfig != null
                ? SettingsService.Instance.DifficultyConfig.Easy
                : effectivePreset;

            Assert.AreEqual(easyPreset.MaxActiveBalls, effectivePreset.MaxActiveBalls,
                "Effective preset must match Easy preset.");
            Assert.AreEqual(effectivePreset.MaxActiveBalls, spawner.MaxActiveBalls,
                "BallSpawner.MaxActiveBalls should reflect EffectiveDifficulty (Easy preset).");

            // Reset back to Normal for subsequent tests
            SettingsService.Instance.SetDifficulty(Difficulty.Normal);
        }

        [UnityTest]
        public IEnumerator StartingGameOnNightmareAppliesRealTuningOfMediumToBallSpawner()
        {
            // Set difficulty to Nightmare in settings (Nightmare maps to real tuning of Normal/Medium)
            SettingsService.Instance.SetDifficulty(Difficulty.Nightmare);
            yield return null;

            Assert.AreEqual(Difficulty.Nightmare, SettingsService.Instance.CurrentSettings.SelectedDifficulty,
                "Selected difficulty must be Nightmare.");
            Assert.AreEqual(Difficulty.Normal, SettingsService.Instance.CurrentSettings.EffectiveDifficulty,
                "Effective difficulty must be Normal (Medium) due to plot twist mapping.");

            // Load MainGame
            yield return SceneManager.LoadSceneAsync("MainGame");
            yield return null;

            var spawner = Object.FindAnyObjectByType<BallSpawner>();
            Assert.IsNotNull(spawner, "BallSpawner must be present in MainGame.");

            var effectivePreset = SettingsService.Instance.GetCurrentDifficultyPreset();
            var normalPreset = SettingsService.Instance.DifficultyConfig != null
                ? SettingsService.Instance.DifficultyConfig.Normal
                : effectivePreset;

            Assert.AreEqual(normalPreset.MaxActiveBalls, effectivePreset.MaxActiveBalls,
                "Effective preset must match Normal (Medium) preset.");
            Assert.AreEqual(effectivePreset.MaxActiveBalls, spawner.MaxActiveBalls,
                "BallSpawner.MaxActiveBalls should reflect EffectiveDifficulty (Normal/Medium preset).");

            // Reset back to Normal for subsequent tests
            SettingsService.Instance.SetDifficulty(Difficulty.Normal);
        }
    }
}
