using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using DodgeGame.Core;
using DodgeGame.Data;
using DodgeGame.UI;

namespace DodgeGame.Tests.EditMode
{
    [TestFixture]
    public class AccessibilityAudioTests
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
        public void Captions_DispatchedWhenEnabled_ForNonCloseDodgeClips()
        {
            settingsService.CurrentSettings.CaptionsEnabled = true;

            string dispatchedCaption = null;
            AudioManager.OnCaptionRequested += cap => dispatchedCaption = cap;

            // Direct hit DH_01 -> should dispatch caption
            audioManager.Play("DH_01");
            Assert.IsNotNull(dispatchedCaption, "Caption should be dispatched for DH_01 when enabled.");
            Assert.AreEqual("[Bonk!]", dispatchedCaption);
        }

        [Test]
        public void Captions_NotDispatched_WhenCaptionsDisabled()
        {
            settingsService.CurrentSettings.CaptionsEnabled = false;

            string dispatchedCaption = null;
            AudioManager.OnCaptionRequested += cap => dispatchedCaption = cap;

            audioManager.Play("DH_01");
            Assert.IsNull(dispatchedCaption, "No caption should be dispatched when Captions setting is disabled.");
        }

        [Test]
        public void Captions_NeverDispatched_ForCloseDodges()
        {
            settingsService.CurrentSettings.CaptionsEnabled = true;

            string dispatchedCaption = null;
            AudioManager.OnCaptionRequested += cap => dispatchedCaption = cap;

            audioManager.Play("CD_01");
            audioManager.Play("CD_02");
            audioManager.Play("CD_03");
            audioManager.Play("CD_04");

            Assert.IsNull(dispatchedCaption, "Close Dodges must never dispatch captions.");
        }
    }
}
