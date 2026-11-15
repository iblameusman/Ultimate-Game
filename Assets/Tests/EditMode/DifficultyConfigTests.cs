using System;
using NUnit.Framework;
using UnityEngine;
using DodgeGame.Data;

namespace DodgeGame.Tests.EditMode
{
    [TestFixture]
    public class DifficultyConfigTests
    {
        private DifficultyConfig config;

        [SetUp]
        public void SetUp()
        {
            config = Resources.Load<DifficultyConfig>("SO_DifficultyConfig") ??
                     ScriptableObject.CreateInstance<DifficultyConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            if (config != null && !config.name.Contains("SO_"))
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void MultipliersArePositiveAcrossAllDifficulties()
        {
            foreach (Difficulty diff in Enum.GetValues(typeof(Difficulty)))
            {
                var preset = config.GetPreset(diff);
                Assert.Greater(preset.BallSpeedMultiplier, 0f, $"{diff} BallSpeedMultiplier must be > 0");
                Assert.Greater(preset.SpawnIntervalMultiplier, 0f, $"{diff} SpawnIntervalMultiplier must be > 0");
                Assert.Greater(preset.SkillMeterGainMultiplier, 0f, $"{diff} SkillMeterGainMultiplier must be > 0");
                Assert.Greater(preset.ScoreMultiplier, 0f, $"{diff} ScoreMultiplier must be > 0");
            }
        }

        [Test]
        public void BallSpeedMultiplierStaysWithinRange()
        {
            foreach (Difficulty diff in Enum.GetValues(typeof(Difficulty)))
            {
                var preset = config.GetPreset(diff);
                Assert.GreaterOrEqual(preset.BallSpeedMultiplier, 0.5f, $"{diff} BallSpeedMultiplier must be >= 0.5");
                Assert.LessOrEqual(preset.BallSpeedMultiplier, 3.0f, $"{diff} BallSpeedMultiplier must be <= 3.0");
            }
        }

        [Test]
        public void SpawnIntervalMultiplierStaysWithinRange()
        {
            foreach (Difficulty diff in Enum.GetValues(typeof(Difficulty)))
            {
                var preset = config.GetPreset(diff);
                Assert.GreaterOrEqual(preset.SpawnIntervalMultiplier, 0.2f, $"{diff} SpawnIntervalMultiplier must be >= 0.2");
                Assert.LessOrEqual(preset.SpawnIntervalMultiplier, 5.0f, $"{diff} SpawnIntervalMultiplier must be <= 5.0");
            }
        }

        [Test]
        public void StartingLivesIsAtLeastOneForAllDifficulties()
        {
            foreach (Difficulty diff in Enum.GetValues(typeof(Difficulty)))
            {
                var preset = config.GetPreset(diff);
                Assert.GreaterOrEqual(preset.StartingLives, 1, $"{diff} StartingLives must be >= 1");
            }
        }

        [Test]
        public void GetScoreMultiplierReturnsExpectedValues()
        {
            Assert.AreEqual(0.5f, config.GetScoreMultiplier(Difficulty.Easy), 0.001f);
            Assert.AreEqual(1.0f, config.GetScoreMultiplier(Difficulty.Normal), 0.001f);
            Assert.AreEqual(1.5f, config.GetScoreMultiplier(Difficulty.Hard), 0.001f);
            Assert.AreEqual(2.0f, config.GetScoreMultiplier(Difficulty.Nightmare), 0.001f);
        }

        [Test]
        public void MappingTable_ContainsAllFourEntries_WithExpectedRules()
        {
            Assert.IsNotNull(config.MappingTable);
            Assert.AreEqual(4, config.MappingTable.Length);

            var easyEntry = config.GetMapping(Difficulty.Easy);
            Assert.IsTrue(easyEntry.BlocksStart, "Easy must block start.");
            Assert.AreEqual("UI_REJECT_EASY", easyEntry.RejectVoiceClipId);
            Assert.AreEqual(Difficulty.Easy, easyEntry.EffectiveTier);

            var normalEntry = config.GetMapping(Difficulty.Normal);
            Assert.IsTrue(normalEntry.BlocksStart, "Normal must block start.");
            Assert.AreEqual("UI_REJECT_NORMAL", normalEntry.RejectVoiceClipId);
            Assert.AreEqual(Difficulty.Normal, normalEntry.EffectiveTier);

            var hardEntry = config.GetMapping(Difficulty.Hard);
            Assert.IsFalse(hardEntry.BlocksStart, "Hard must NOT block start.");
            Assert.AreEqual(Difficulty.Easy, hardEntry.EffectiveTier, "Hard effective tier must map to Easy.");

            var nightmareEntry = config.GetMapping(Difficulty.Nightmare);
            Assert.IsFalse(nightmareEntry.BlocksStart, "Nightmare must NOT block start.");
            Assert.AreEqual(Difficulty.Normal, nightmareEntry.EffectiveTier, "Nightmare effective tier must map to Normal.");
        }

        [Test]
        public void RevealTwist_DefaultsToTrue()
        {
            Assert.IsTrue(config.RevealTwist, "RevealTwist flag must default to true.");
        }
    }
}
