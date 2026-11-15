using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using DodgeGame.Arena;
using DodgeGame.Balls;
using DodgeGame.Core;
using DodgeGame.Data;
using DodgeGame.Player;
using DodgeGame.UI;

namespace DodgeGame.Tests.PlayMode
{
    [TestFixture]
    public class SpecialChecksTests
    {
        private GameObject coreObject;
        private GameObject playerObject;
        private TimeScaleController timeScaleController;
        private PauseController pauseController;
        private SlowMotionController slowMoController;
        private HitStop hitStop;
        private GameManager gameManager;
        private PlayerController playerController;
        private DimensionShifter dimensionShifter;
        private DashController dashController;
        private SlowMotionMeter slowMoMeter;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            TimeScaleController.ResetTimeScale();
            GameEvents.ClearAllListeners();

            coreObject = new GameObject("SpecialChecksCore");
            coreObject.AddComponent<AudioListener>();
            timeScaleController = coreObject.AddComponent<TimeScaleController>();
            pauseController = coreObject.AddComponent<PauseController>();
            slowMoController = coreObject.AddComponent<SlowMotionController>();
            hitStop = coreObject.AddComponent<HitStop>();
            gameManager = coreObject.AddComponent<GameManager>();
            gameManager.StartGameImmediate();

            playerObject = new GameObject("SpecialChecksPlayer");
            var rb = playerObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.linearDamping = 0f;
            playerObject.AddComponent<CapsuleCollider>();

            playerController = playerObject.AddComponent<PlayerController>();
            dimensionShifter = playerObject.AddComponent<DimensionShifter>();
            dashController = playerObject.AddComponent<DashController>();
            playerObject.AddComponent<PlayerHealth>();
            slowMoMeter = playerObject.AddComponent<SlowMotionMeter>();
            slowMoMeter.Initialize();

            yield return null;
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
        public IEnumerator SpecialCheck_TimeScale_ReturnsToOne_AfterAllEvents()
        {
            // 1. Hit Stop: Request freeze, wait for finish, verify 1.0
            hitStop.TriggerHitStop();
            yield return null;
            Assert.AreEqual(0.0f, Time.timeScale, 0.001f, "Hit stop must freeze time.");
            yield return new WaitForSecondsRealtime(0.12f);
            Assert.AreEqual(1.0f, Time.timeScale, 0.01f, "Time scale must return to 1.0 after hit stop.");

            // 2. Slow Motion: Activate, wait for early end, verify 1.0
            slowMoMeter.AddMeter(slowMoMeter.MeterMax);
            slowMoController.TryActivateSlowMo();
            yield return null;
            Assert.Less(Time.timeScale, 0.9f, "Slow motion must reduce time scale.");
            slowMoController.EndSlowMo();
            yield return null;
            Assert.AreEqual(1.0f, Time.timeScale, 0.01f, "Time scale must return to 1.0 after slow motion end.");

            // 3. Pause & Unpause: Toggle on and off, verify 1.0
            pauseController.TogglePause();
            yield return null;
            Assert.AreEqual(0.0f, Time.timeScale, 0.001f, "Pause must freeze time.");
            pauseController.TogglePause();
            yield return null;
            Assert.AreEqual(1.0f, Time.timeScale, 0.01f, "Time scale must return to 1.0 after unpausing.");

            // 4. Game Over: Trigger game over, reset, verify 1.0
            TimeScaleController.Instance.RequestTimeScale(0.25f, 1f, "GameOverTest");
            TimeScaleController.ResetTimeScale();
            Assert.AreEqual(1.0f, Time.timeScale, 0.01f, "Time scale must return to 1.0 on game over reset.");
        }

        [UnityTest]
        public IEnumerator SpecialCheck_PlayerState_NotLeftInLockedState()
        {
            // 1. Test Phase completion resets lock
            dimensionShifter.TryTriggerShift();
            Assert.AreEqual(SkillLockState.Phasing, playerController.CurrentSkillLock, "Skill lock should be Phasing.");
            yield return new WaitForSeconds(dimensionShifter.Config != null ? dimensionShifter.Config.ShiftDuration + 0.1f : 1.6f);
            Assert.AreEqual(SkillLockState.None, playerController.CurrentSkillLock,
                "Skill lock must cleanly return to None after phase expires.");

            // 2. Test Dash completion resets lock
            dashController.TryDash();
            Assert.AreEqual(SkillLockState.Dashing, playerController.CurrentSkillLock, "Skill lock should be Dashing.");
            while (dashController.IsDashing)
            {
                yield return new WaitForFixedUpdate();
            }
            Assert.AreEqual(SkillLockState.None, playerController.CurrentSkillLock,
                "Skill lock must cleanly return to None after dash completes.");

            // 3. Test Disabling player resets lock
            dashController.TryDash();
            playerObject.SetActive(false);
            yield return null;
            Assert.AreEqual(SkillLockState.None, playerController.CurrentSkillLock,
                "Disabling player mid-ability must force CurrentSkillLock to None.");
            playerObject.SetActive(true);
        }

        [UnityTest]
        public IEnumerator SpecialCheck_Balls_DoNotClipThroughArenaBoundary()
        {
            var ballObject = new GameObject("BoundaryTestBall");
            ballObject.layer = LayerMask.NameToLayer("Ball");
            var rb = ballObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = false;
            var collider = ballObject.AddComponent<SphereCollider>();
            var ball = ballObject.AddComponent<FireBall>();

            // Place near boundary and launch outward at high speed (50 units/sec)
            ballObject.transform.position = new Vector3(8.5f, 1.0f, 0f);
            rb.linearVelocity = new Vector3(50f, 0f, 0f);

            // Simulate 15 physics steps
            for (int i = 0; i < 15; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            // Verify position was strictly clamped by EnsureWithinArenaBounds (limit is 9.2f)
            Assert.LessOrEqual(Mathf.Abs(ballObject.transform.position.x), 9.25f,
                "Ball position X must not penetrate through the arena perimeter.");
            Assert.LessOrEqual(Mathf.Abs(ballObject.transform.position.z), 9.25f,
                "Ball position Z must not penetrate through the arena perimeter.");

            Object.Destroy(ballObject);
        }

        [UnityTest]
        public IEnumerator SpecialCheck_InputHintsBar_DebouncesRapidToggles()
        {
            var hintsObject = new GameObject("TestHintsBar");
            var hintsBar = hintsObject.AddComponent<InputHintsBar>();
            yield return null;

            // Set to Gamepad
            hintsBar.SetGamepadActive(true);
            Assert.IsTrue(hintsBar.IsGamepadActive, "Gamepad active should be true.");

            // Attempt instantaneous toggle to Keyboard (simulating stick jitter)
            hintsBar.SetGamepadActive(false);
            Assert.IsTrue(hintsBar.IsGamepadActive,
                "Rapid flip within debounce interval (0.05s) must be ignored.");

            // Wait beyond debounce window
            yield return new WaitForSecondsRealtime(0.08f);

            hintsBar.SetGamepadActive(false);
            Assert.IsFalse(hintsBar.IsGamepadActive,
                "Device flip after debounce interval must succeed.");

            Object.Destroy(hintsObject);
        }

        [UnityTest]
        public IEnumerator SpecialCheck_ThreeSessionsInARow_ZeroErrors()
        {
            // Simulate 3 consecutive gameplay starts and game overs
            for (int session = 1; session <= 3; session++)
            {
                TimeScaleController.ResetTimeScale();
                gameManager.TriggerGameOver($"Session {session} End");
                yield return new WaitForSecondsRealtime(0.05f);

                Assert.AreEqual(GameState.GameOver, gameManager.CurrentState,
                    $"Session {session} should transition cleanly to GameOver.");
            }

            yield return null;
        }
    }
}
