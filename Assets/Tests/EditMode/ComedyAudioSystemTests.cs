using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using DodgeGame.Core;
using DodgeGame.Data;

namespace DodgeGame.Tests.EditMode
{
    [TestFixture]
    public class ComedyAudioSystemTests
    {
        private GameObject audioManagerGo;
        private AudioManager audioManager;
        private GameObject settingsServiceGo;
        private SettingsService settingsService;
        private ComedyAudioConfig config;

        [SetUp]
        public void SetUp()
        {
            GameEvents.ClearAllListeners();

            if (AudioManager.Instance != null)
            {
                Object.DestroyImmediate(AudioManager.Instance.gameObject);
            }
            if (SettingsService.Instance != null)
            {
                Object.DestroyImmediate(SettingsService.Instance.gameObject);
            }

            settingsServiceGo = new GameObject("TestSettingsService");
            settingsService = settingsServiceGo.AddComponent<SettingsService>();
            settingsService.InitializeSettings();
            settingsService.CurrentSettings.ComedyLevel = ComedyLevel.Full;

            // Load configuration
            config = Resources.Load<ComedyAudioConfig>("ComedyAudioConfig");
#if UNITY_EDITOR
            if (config == null)
            {
                config = UnityEditor.AssetDatabase.LoadAssetAtPath<ComedyAudioConfig>("Assets/Scripts/Data/ComedyAudioConfig.asset");
            }
#endif
        }

        [TearDown]
        public void TearDown()
        {
            GameEvents.ClearAllListeners();

            if (audioManagerGo != null)
            {
                Object.DestroyImmediate(audioManagerGo);
            }
            if (AudioManager.Instance != null)
            {
                Object.DestroyImmediate(AudioManager.Instance.gameObject);
            }
            if (settingsServiceGo != null)
            {
                Object.DestroyImmediate(settingsServiceGo);
            }
            if (SettingsService.Instance != null)
            {
                Object.DestroyImmediate(SettingsService.Instance.gameObject);
            }
        }

        [Test]
        public void ComedyAudioConfig_AllClipIdsAreUnique_AndValid()
        {
            Assert.IsNotNull(config, "ComedyAudioConfig asset could not be loaded.");

            bool isValid = config.ValidateConfig(out string errorMessage);
            Assert.IsTrue(isValid, $"Config validation failed: {errorMessage}");

            string[] requiredIds =
            {
                // Zone 1: Close Dodges
                "CD_01", "CD_02", "CD_03", "CD_04",
                // Zone 2: Streaks
                "ST_05", "ST_10", "ST_15", "ST_20", "ST_BREAK",
                // Zone 3: Direct Hits
                "DH_01", "DH_02", "DH_03", "DH_04", "DH_05", "DH_06",
                // Zone 4: Special Spawns
                "SB_GUN", "SB_NUKE", "SB_FIRE", "SB_STICKY", "SB_FAST", "SB_RARE", "SB_DUN"
            };

            foreach (string id in requiredIds)
            {
                var entry = config.GetEntry(id);
                Assert.IsNotNull(entry, $"Missing required entry: {id}");
                Assert.IsNotNull(entry.Clips, $"Clips array is null for {id}");
                Assert.GreaterOrEqual(entry.Clips.Length, 1, $"Entry {id} must have at least 1 clip variant");
                Assert.GreaterOrEqual(entry.Volume, 0f, $"Entry {id} volume must be >= 0");
                Assert.LessOrEqual(entry.Volume, 1f, $"Entry {id} volume must be <= 1");
                Assert.GreaterOrEqual(entry.Cooldown, 0f, $"Entry {id} cooldown must be >= 0");
                Assert.GreaterOrEqual(entry.Priority, 1, $"Entry {id} priority must be >= 1");
                Assert.LessOrEqual(entry.Priority, 5, $"Entry {id} priority must be <= 5");
            }
        }

        [Test]
        public void Priority_PoolFull_LowestPriorityClipStopsFirst()
        {
            audioManagerGo = new GameObject("TestAudioManager");
            audioManager = audioManagerGo.AddComponent<AudioManager>();
            audioManager.ComedyConfig = config;

            Debug.Log($"Config entries count: {config.Entries.Count}");
            var entry = config.GetEntry("CD_01");
            Debug.Log($"Entry CD_01: {entry != null}, clips: {entry?.Clips?.Length ?? 0}, randomClip: {entry?.GetRandomClip()}");
            Debug.Log($"ActiveChannelCount: {audioManager.ActiveChannelCount}, PoolSize: {audioManager.PoolSize}");

            bool p1 = audioManager.Play("CD_01");
            Debug.Log($"Play CD_01 result: {p1}, dspTime: {AudioSettings.dspTime}");

            audioManager.Play("CD_02");
            audioManager.Play("CD_03");
            audioManager.Play("CD_04");

            // Pool now has 4 comedy clips playing (max comedy concurrency = 4)
            // Now play a Priority 5 sound (DH_01)
            bool playedHighPri = audioManager.Play("DH_01");
            Debug.Log($"Play DH_01 result: {playedHighPri}");
            Assert.IsTrue(playedHighPri, "Priority 5 sound (DH_01) should successfully play and evict a lower priority sound.");
        }

        [Test]
        public void StreakThresholds_FireExactlyOncePerStreak_AndResetOnDamage()
        {
            var trackerGo = new GameObject("TestStreakTracker");
            var tracker = trackerGo.AddComponent<DodgeStreakTracker>();

            var firedThresholds = new List<int>();
            GameEvents.OnStreakChanged += threshold => firedThresholds.Add(threshold);

            int brokenStreak = -1;
            GameEvents.OnStreakBroken += streak => brokenStreak = streak;

            // Dodges 1 to 4: no threshold
            for (int i = 1; i <= 4; i++)
            {
                tracker.ConfirmDodge();
            }
            Assert.AreEqual(0, firedThresholds.Count, "No threshold should fire before 5 dodges.");

            // 5th dodge: ST_05 fires
            tracker.ConfirmDodge();
            Assert.AreEqual(1, firedThresholds.Count);
            Assert.AreEqual(5, firedThresholds[0]);

            // Dodges 6 to 9: no duplicate threshold
            for (int i = 6; i <= 9; i++)
            {
                tracker.ConfirmDodge();
            }
            Assert.AreEqual(1, firedThresholds.Count, "Threshold 5 should not fire repeatedly.");

            // 10th dodge: ST_10 fires
            tracker.ConfirmDodge();
            Assert.AreEqual(2, firedThresholds.Count);
            Assert.AreEqual(10, firedThresholds[1]);

            // Damage resets streak
            tracker.HandlePlayerHit(2);
            Assert.AreEqual(0, tracker.CurrentStreak, "Damage must reset streak to 0.");
            Assert.AreEqual(10, brokenStreak, "Streak loss at 10 must trigger OnStreakBroken with count 10.");

            // New streak: dodges 1 to 5 again
            firedThresholds.Clear();
            for (int i = 1; i <= 5; i++)
            {
                tracker.ConfirmDodge();
            }
            Assert.AreEqual(1, firedThresholds.Count, "New streak reaching 5 should fire threshold 5 again.");
            Assert.AreEqual(5, firedThresholds[0]);

            Object.DestroyImmediate(trackerGo);
        }

        [Test]
        public void DirectHit_CancelsActiveCloseDodges()
        {
            audioManagerGo = new GameObject("TestAudioManager");
            audioManager = audioManagerGo.AddComponent<AudioManager>();
            audioManager.ComedyConfig = config;

            // Play a close dodge
            bool playedCD = audioManager.Play("CD_01");
            Assert.IsTrue(playedCD);
            Assert.IsTrue(audioManager.IsClipPlaying("CD_01"));

            // Play a direct hit
            bool playedDH = audioManager.Play("DH_01");
            Assert.IsTrue(playedDH);

            // Close dodge should have been cancelled by the Direct Hit
            Assert.IsFalse(audioManager.IsClipPlaying("CD_01"), "Direct Hit must cancel active Close Dodge stings.");
        }

        [Test]
        public void GameOver_DH06_StopsAllOtherComedyClips()
        {
            audioManagerGo = new GameObject("TestAudioManager");
            audioManager = audioManagerGo.AddComponent<AudioManager>();
            audioManager.ComedyConfig = config;

            // Play a streak and a special spawn
            audioManager.Play("ST_05");
            audioManager.Play("SB_FIRE");

            Assert.IsTrue(audioManager.IsClipPlaying("ST_05"));
            Assert.IsTrue(audioManager.IsClipPlaying("SB_FIRE"));

            // Game over plays DH_06 alone
            audioManager.Play("DH_06");

            Assert.IsFalse(audioManager.IsClipPlaying("ST_05"), "DH_06 must stop ST_05.");
            Assert.IsFalse(audioManager.IsClipPlaying("SB_FIRE"), "DH_06 must stop SB_FIRE.");
            Assert.IsTrue(audioManager.IsClipPlaying("DH_06"), "DH_06 should be playing.");
        }

        [Test]
        public void ComedyLevel_Off_PlaysNoComedyClips_Subtle_SuppressesCloseDodges()
        {
            audioManagerGo = new GameObject("TestAudioManager");
            audioManager = audioManagerGo.AddComponent<AudioManager>();
            audioManager.ComedyConfig = config;

            // Test Off
            audioManager.OverrideComedyLevel = ComedyLevel.Off;
            Assert.IsFalse(audioManager.Play("CD_01"), "Off must suppress Close Dodges.");
            Assert.IsFalse(audioManager.Play("DH_01"), "Off must suppress Direct Hits.");
            Assert.IsFalse(audioManager.Play("ST_05"), "Off must suppress Streaks.");
            Assert.IsFalse(audioManager.Play("SB_NUKE"), "Off must suppress Special Spawns.");

            // Test Subtle
            audioManager.OverrideComedyLevel = ComedyLevel.Subtle;
            Assert.IsFalse(audioManager.Play("CD_01"), "Subtle must suppress Close Dodges.");
            Assert.IsTrue(audioManager.Play("DH_03"), "Subtle must allow Direct Hits.");
        }
    }
}
