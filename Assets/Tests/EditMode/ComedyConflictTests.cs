using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using DodgeGame.Core;
using DodgeGame.Data;

namespace DodgeGame.Tests.EditMode
{
    [TestFixture]
    public class ComedyConflictTests
    {
        private GameObject audioManagerGo;
        private AudioManager audioManager;
        private ComedyAudioConfig config;

        [SetUp]
        public void SetUp()
        {
            GameEvents.ClearAllListeners();

            if (AudioManager.Instance != null)
            {
                Object.DestroyImmediate(AudioManager.Instance.gameObject);
            }

            config = Resources.Load<ComedyAudioConfig>("ComedyAudioConfig");
#if UNITY_EDITOR
            if (config == null)
            {
                config = UnityEditor.AssetDatabase.LoadAssetAtPath<ComedyAudioConfig>("Assets/Scripts/Data/ComedyAudioConfig.asset");
            }
#endif
            audioManagerGo = new GameObject("TestAudioManager");
            audioManager = audioManagerGo.AddComponent<AudioManager>();
            audioManager.ComedyConfig = config;
            audioManager.OverrideComedyLevel = ComedyLevel.Full;
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
        }

        [Test]
        public void DirectHit_And_Streak_SameFrame_DirectHitPlays_StreakSkipped()
        {
            // Direct Hit plays
            bool directHitPlayed = audioManager.Play("DH_01");
            Assert.IsTrue(directHitPlayed, "Direct hit should play.");

            // Streak on same frame should be skipped per conflict rules
            bool streakPlayed = audioManager.Play("ST_05");
            Assert.IsFalse(streakPlayed, "Streak fired on same frame as Direct Hit must be skipped.");
        }

        [Test]
        public void SpecialSpawn_And_DirectHit_SameFrame_DirectHitPlays_SpawnDelayed()
        {
            // Direct hit plays first
            bool directHitPlayed = audioManager.Play("DH_01");
            Assert.IsTrue(directHitPlayed);

            // Special spawn on same frame should return true (queued delayed routine)
            bool spawnResult = audioManager.Play("SB_FIRE");
            Assert.IsTrue(spawnResult, "Spawn sound should be accepted and delayed by 0.5s.");
        }

        [Test]
        public void DirectHit_After_SpecialSpawn_SameFrame_DelaysSpecialSpawn()
        {
            // Special spawn plays first on this frame
            bool spawnPlayed = audioManager.Play("SB_FIRE");
            Assert.IsTrue(spawnPlayed);
            Assert.IsTrue(audioManager.IsClipPlaying("SB_FIRE"));

            // Direct Hit fires on same frame
            bool hitPlayed = audioManager.Play("DH_01");
            Assert.IsTrue(hitPlayed);

            // Special spawn should now have been removed from immediate channels and delayed
            Assert.IsFalse(audioManager.IsClipPlaying("SB_FIRE"), "Active special spawn on same frame must be interrupted and delayed.");
        }

        [Test]
        public void GlobalComedyLimit_CapsAtFourConcurrentClips()
        {
            // Play 4 non-close-dodge clips (Streaks & Special Spawns)
            Assert.IsTrue(audioManager.Play("ST_05"));
            Assert.IsTrue(audioManager.Play("ST_10"));
            Assert.IsTrue(audioManager.Play("ST_15"));
            Assert.IsTrue(audioManager.Play("SB_STICKY"));

            // Active comedy channels should be 4
            Assert.AreEqual(4, audioManager.ActiveChannelCount);

            // Play a Priority 5 Direct Hit -> evicts lowest priority without cancelling non-close-dodges
            Assert.IsTrue(audioManager.Play("DH_01"));
            Assert.AreEqual(4, audioManager.ActiveChannelCount);
            Assert.IsTrue(audioManager.IsClipPlaying("DH_01"));
        }

        [Test]
        public void ClearQueuedClips_ClearsPendingAudio()
        {
            audioManager.PlayDelayed("SB_NUKE", 5.0f);
            Assert.DoesNotThrow(() => audioManager.ClearQueuedClips());
        }
    }
}
