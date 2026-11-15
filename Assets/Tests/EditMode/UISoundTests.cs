using NUnit.Framework;
using UnityEngine;
using DodgeGame.Core;
using DodgeGame.Data;

namespace DodgeGame.Tests.EditMode
{
    [TestFixture]
    public class UISoundTests
    {
        private GameObject audioManagerGo;
        private AudioManager audioManager;

        [SetUp]
        public void SetUp()
        {
            if (AudioManager.Instance != null)
            {
                Object.DestroyImmediate(AudioManager.Instance.gameObject);
            }

            audioManagerGo = new GameObject("AudioManager_Test");
            audioManager = audioManagerGo.AddComponent<AudioManager>();
        }

        [TearDown]
        public void TearDown()
        {
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
        public void UISounds_PlaySuccessfully_ThroughUIRouting()
        {
            bool hoverPlayed = audioManager.PlayUI("UI_HOVER");
            Assert.IsTrue(hoverPlayed, "UI_HOVER should play successfully.");

            bool selectPlayed = audioManager.PlayUI("UI_SELECT");
            Assert.IsTrue(selectPlayed, "UI_SELECT should play successfully.");

            bool backPlayed = audioManager.PlayUI("UI_BACK");
            Assert.IsTrue(backPlayed, "UI_BACK should play successfully.");

            bool errorPlayed = audioManager.PlayUI("UI_ERROR");
            Assert.IsTrue(errorPlayed, "UI_ERROR should play successfully.");
        }

        [Test]
        public void UISounds_IgnoreComedyLevelSetting()
        {
            // Set comedy level to Off
            audioManager.OverrideComedyLevel = ComedyLevel.Off;

            bool hoverPlayed = audioManager.PlayUI("UI_HOVER");
            Assert.IsTrue(hoverPlayed, "UI_HOVER should play even when ComedyLevel is Off.");

            bool selectPlayed = audioManager.PlayUI("UI_SELECT");
            Assert.IsTrue(selectPlayed, "UI_SELECT should play even when ComedyLevel is Off.");

            bool errorPlayed = audioManager.PlayUI("UI_ERROR");
            Assert.IsTrue(errorPlayed, "UI_ERROR should play even when ComedyLevel is Off.");
        }

        [Test]
        public void DifficultyChange_PlaysSelectWithPitchRise()
        {
            bool risePlayed = audioManager.PlayUI("UI_SELECT_RISE");
            Assert.IsTrue(risePlayed, "UI_SELECT_RISE should play successfully.");

            float pitch = audioManager.GetActiveClipPitch("UI_SELECT");
            Assert.Greater(pitch, 1.0f, "UI_SELECT_RISE pitch should be greater than 1.0.");
        }
    }
}
