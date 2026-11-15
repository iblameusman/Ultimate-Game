using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using DodgeGame.Core;
using DodgeGame.Data;
using DodgeGame.Player;

namespace DodgeGame.Tests.PlayMode
{
    [TestFixture]
    public class SlowMotionTests
    {
        private GameObject coreObject;
        private GameObject playerObject;
        private TimeScaleController timeScaleController;
        private PauseController pauseController;
        private SlowMotionController slowMoController;
        private SlowMotionMeter slowMoMeter;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            TimeScaleController.ResetTimeScale();
            GameEvents.ClearAllListeners();

            coreObject = new GameObject("CoreSystems");
            timeScaleController = coreObject.AddComponent<TimeScaleController>();
            pauseController = coreObject.AddComponent<PauseController>();
            slowMoController = coreObject.AddComponent<SlowMotionController>();

            playerObject = new GameObject("PlayerWithMeter");
            playerObject.AddComponent<AudioListener>();
            slowMoMeter = playerObject.AddComponent<SlowMotionMeter>();
            slowMoMeter.Initialize();

            yield return null;

            // Charge meter to full after Start() has run so Slow Motion is ready to activate
            slowMoMeter.AddMeter(slowMoMeter.MeterMax);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            TimeScaleController.ResetTimeScale();
            GameEvents.ClearAllListeners();

            if (playerObject != null) Object.Destroy(playerObject);
            if (coreObject != null) Object.Destroy(coreObject);

            yield return null;
        }

        [UnityTest]
        public IEnumerator SlowMotion_ActivationSetsTimeScale()
        {
            Assert.IsTrue(slowMoMeter.IsReady, "Meter must be ready before activation.");

            bool activated = slowMoController.TryActivateSlowMo();
            Assert.IsTrue(activated, "TryActivateSlowMo must succeed when meter is full.");
            Assert.IsTrue(slowMoController.IsSlowMoActive, "Slow motion must be actively marked.");

            yield return null;

            float expectedScale = slowMoController.Config != null ? slowMoController.Config.TimeScale : 0.4f;
            Assert.AreEqual(expectedScale, Time.timeScale, 0.01f,
                $"Time.timeScale should equal configured slow motion scale ({expectedScale}).");
        }

        [UnityTest]
        public IEnumerator SlowMotion_HitEndsEarlyAndRestoresTimeScale()
        {
            slowMoController.TryActivateSlowMo();
            yield return null;

            Assert.IsTrue(slowMoController.IsSlowMoActive, "Slow motion should be active.");

            // Trigger player hit event
            GameEvents.TriggerPlayerHit(2);
            yield return null;

            Assert.IsFalse(slowMoController.IsSlowMoActive, "Taking damage must end Slow Motion immediately.");
            Assert.AreEqual(1.0f, Time.timeScale, 0.01f, "Time scale must return to 1.0 upon hit cancellation.");
        }

        [UnityTest]
        public IEnumerator SlowMotion_PauseOverridesAndUnpauseRestoresSlowScale()
        {
            slowMoController.TryActivateSlowMo();
            yield return null;

            float slowScale = slowMoController.Config != null ? slowMoController.Config.TimeScale : 0.4f;
            Assert.AreEqual(slowScale, Time.timeScale, 0.01f, "Slow motion scale should be active.");

            // Pause
            pauseController.TogglePause();
            yield return null;

            Assert.IsTrue(pauseController.IsPaused, "Game must be paused.");
            Assert.AreEqual(0.0f, Time.timeScale, 0.001f, "Pausing must force timeScale to 0.0.");

            // Unpause
            pauseController.TogglePause();
            yield return null;

            Assert.IsFalse(pauseController.IsPaused, "Game must be unpaused.");
            Assert.AreEqual(slowScale, Time.timeScale, 0.01f,
                "Resuming while Slow Motion timer is still running must restore the slow motion scale.");
        }
    }
}
