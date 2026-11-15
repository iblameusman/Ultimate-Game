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
    public class DashTests
    {
        private GameObject playerObject;
        private Rigidbody rb;
        private PlayerController playerController;
        private DimensionShifter dimensionShifter;
        private PlayerHealth playerHealth;
        private DashController dashController;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            TimeScaleController.ResetTimeScale();
            GameEvents.ClearAllListeners();

            playerObject = new GameObject("TestPlayer");
            rb = playerObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.linearDamping = 0f;
            rb.isKinematic = false;

            playerController = playerObject.AddComponent<PlayerController>();
            dimensionShifter = playerObject.AddComponent<DimensionShifter>();
            playerHealth = playerObject.AddComponent<PlayerHealth>();
            dashController = playerObject.AddComponent<DashController>();
            playerObject.AddComponent<AudioListener>();

            playerObject.transform.position = Vector3.zero;
            playerObject.transform.rotation = Quaternion.identity;

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            TimeScaleController.ResetTimeScale();
            GameEvents.ClearAllListeners();

            if (playerObject != null)
            {
                Object.Destroy(playerObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator Dash_MovesPlayerConfiguredDistance_WithinFivePercent()
        {
            float targetDistance = dashController.Config != null ? dashController.Config.Distance : 3.5f;
            float duration = dashController.Config != null ? dashController.Config.Duration : 0.15f;
            Vector3 startPos = playerObject.transform.position;

            bool dashed = dashController.TryDash();
            Assert.IsTrue(dashed, "TryDash must succeed when Ready.");
            Assert.IsTrue(dashController.IsDashing, "Player must be in Dashing state.");

            // Wait for dash duration plus a few fixed updates for physics integration
            float elapsed = 0f;
            while (dashController.IsDashing && elapsed < duration + 0.2f)
            {
                elapsed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            Assert.IsFalse(dashController.IsDashing, "Dash must terminate once duration completes.");
            float actualDistance = Vector3.Distance(startPos, playerObject.transform.position);

            // Within 5% tolerance
            float allowedError = targetDistance * 0.05f;
            Assert.AreEqual(targetDistance, actualDistance, allowedError,
                $"Dash distance {actualDistance} should be within 5% of target {targetDistance}.");
        }

        [UnityTest]
        public IEnumerator Dash_CooldownBlocksSecondDash()
        {
            bool firstDash = dashController.TryDash();
            Assert.IsTrue(firstDash, "First dash must succeed.");

            // Wait until dash state transitions into Cooldown
            while (dashController.IsDashing)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.AreEqual(DashState.Cooldown, dashController.CurrentState, "Dash must transition to Cooldown.");

            // Attempting another dash while on cooldown must fail
            bool secondDash = dashController.TryDash();
            Assert.IsFalse(secondDash, "Dash attempt during cooldown must be blocked.");
            Assert.AreEqual(DashState.Cooldown, dashController.CurrentState, "State should remain in Cooldown.");
        }

        [UnityTest]
        public IEnumerator Dash_IsBlockedDuringPhase()
        {
            bool shifted = dimensionShifter.TryTriggerShift();
            Assert.IsTrue(shifted, "Dimension shift must succeed.");
            Assert.IsTrue(dimensionShifter.IsShifted, "Player must be currently shifted.");

            bool dashAttempt = dashController.TryDash();
            Assert.IsFalse(dashAttempt, "Dash must be blocked while Phase is active.");
            Assert.AreNotEqual(DashState.Dashing, dashController.CurrentState, "Player must not enter Dashing state.");

            yield return null;
        }
    }
}
