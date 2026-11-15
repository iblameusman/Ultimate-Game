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
    public class PhaseTests
    {
        private GameObject playerObject;
        private DimensionShifter dimensionShifter;
        private PlayerController playerController;
        private PlayerHealth playerHealth;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            TimeScaleController.ResetTimeScale();
            GameEvents.ClearAllListeners();

            playerObject = new GameObject("TestPlayer");
            playerObject.AddComponent<CapsuleCollider>();
            playerObject.AddComponent<Rigidbody>();

            dimensionShifter = playerObject.AddComponent<DimensionShifter>();
            playerHealth = playerObject.AddComponent<PlayerHealth>();
            playerController = playerObject.AddComponent<PlayerController>();
            playerObject.AddComponent<AudioListener>();

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
        public IEnumerator Phase_TriggerShift_TransitionsToShiftedState()
        {
            Assert.IsFalse(dimensionShifter.IsShifted, "Player should start unshifted.");

            bool shifted = dimensionShifter.TryTriggerShift();
            Assert.IsTrue(shifted, "Triggering dimension shift must succeed.");
            Assert.IsTrue(dimensionShifter.IsShifted, "IsShifted flag must be true.");

            yield return null;
        }

        [UnityTest]
        public IEnumerator Phase_DisablesCollisionWithBalls()
        {
            bool shifted = dimensionShifter.TryTriggerShift();
            Assert.IsTrue(shifted, "Shift must succeed.");

            yield return null;

            int shiftedLayer = LayerMask.NameToLayer("PlayerShifted");
            int ballLayer = LayerMask.NameToLayer("Ball");

            if (shiftedLayer >= 0)
            {
                Assert.AreEqual(shiftedLayer, playerObject.layer,
                    "Player layer must transition to PlayerShifted upon shift start.");

                if (ballLayer >= 0)
                {
                    bool ignoresCollision = Physics.GetIgnoreLayerCollision(shiftedLayer, ballLayer);
                    Assert.IsTrue(ignoresCollision,
                        "Physics matrix must ignore collisions between PlayerShifted and Ball layers.");
                }
            }
        }

        [UnityTest]
        public IEnumerator Phase_DamageIsIgnoredWhilePhased()
        {
            dimensionShifter.TryTriggerShift();
            Assert.IsTrue(dimensionShifter.IsShifted, "Player must be shifted.");

            int livesBefore = playerHealth.CurrentLives;
            bool damageApplied = playerHealth.TakeDamage(1);

            Assert.IsFalse(damageApplied, "TakeDamage must return false while phased.");
            Assert.AreEqual(livesBefore, playerHealth.CurrentLives,
                "Current lives must not decrease when taking damage during phase.");

            yield return null;
        }

        [UnityTest]
        public IEnumerator Phase_InstantKillIgnoredWhilePhased()
        {
            dimensionShifter.TryTriggerShift();
            Assert.IsTrue(dimensionShifter.IsShifted, "Player must be shifted.");

            playerHealth.KillInstant("Ball Collision");

            Assert.IsFalse(playerHealth.IsDead, "Player must not die from instant kill while phased.");
            Assert.Greater(playerHealth.CurrentLives, 0, "Lives must remain above zero.");

            yield return null;
        }
    }
}
