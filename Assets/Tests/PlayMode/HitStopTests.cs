using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using DodgeGame.Core;
using DodgeGame.Data;

namespace DodgeGame.Tests.PlayMode
{
    [TestFixture]
    public class HitStopTests
    {
        private GameObject coreObject;
        private TimeScaleController timeScaleController;
        private HitStop hitStop;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            TimeScaleController.ResetTimeScale();
            GameEvents.ClearAllListeners();

            coreObject = new GameObject("HitStopCore");
            timeScaleController = coreObject.AddComponent<TimeScaleController>();
            hitStop = coreObject.AddComponent<HitStop>();

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            TimeScaleController.ResetTimeScale();
            GameEvents.ClearAllListeners();

            if (coreObject != null)
            {
                Object.Destroy(coreObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator HitStop_FreezesTimeScaleToZero()
        {
            hitStop.TriggerHitStop();
            yield return null;

            Assert.IsTrue(hitStop.IsHitStopping, "HitStop should be marked active.");
            Assert.AreEqual(0.0f, Time.timeScale, 0.001f, "Hit stop must freeze Time.timeScale to 0.0.");
        }

        [UnityTest]
        public IEnumerator HitStop_DoesNotStackOnRapidHits()
        {
            hitStop.TriggerHitStop();
            yield return null;
            Assert.IsTrue(hitStop.IsHitStopping, "First hit stop active.");

            // Attempt secondary triggers during active freeze
            hitStop.TriggerHitStop();
            GameEvents.TriggerPlayerHit(2);
            GameEvents.TriggerPlayerHit(1);

            yield return null;
            Assert.IsTrue(hitStop.IsHitStopping, "HitStop remains active without duplicating requests.");

            // Wait for single hit stop duration (0.05s default)
            yield return new WaitForSecondsRealtime(0.12f);

            Assert.IsFalse(hitStop.IsHitStopping, "HitStop must end after its single duration.");
            Assert.AreEqual(1.0f, Time.timeScale, 0.01f, "Time scale must be 1.0 after single unstacked duration.");
        }

        [UnityTest]
        public IEnumerator HitStop_AlwaysReturnsToOne()
        {
            hitStop.TriggerHitStop();
            yield return null;

            Assert.AreEqual(0f, Time.timeScale, 0.001f);

            // Wait beyond hit stop duration
            yield return new WaitForSecondsRealtime(0.15f);

            Assert.IsFalse(hitStop.IsHitStopping, "HitStop routine should be completed.");
            Assert.AreEqual(1.0f, Time.timeScale, 0.01f, "Time.timeScale must strictly return to 1.0.");
        }
    }
}
