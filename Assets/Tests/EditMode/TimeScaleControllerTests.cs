using NUnit.Framework;
using UnityEngine;
using DodgeGame.Core;

namespace DodgeGame.Tests.EditMode
{
    [TestFixture]
    public class TimeScaleControllerTests
    {
        private GameObject controllerGo;
        private TimeScaleController controller;

        [SetUp]
        public void SetUp()
        {
            TimeScaleController.ResetTimeScale();
            controllerGo = new GameObject("TestTimeScaleController");
            controller = controllerGo.AddComponent<TimeScaleController>();
        }

        [TearDown]
        public void TearDown()
        {
            if (controller != null)
            {
                controller.RestoreNormalTime();
            }
            if (controllerGo != null)
            {
                Object.DestroyImmediate(controllerGo);
            }
            TimeScaleController.ResetTimeScale();
        }

        [Test]
        public void DefaultTimeScale_IsOne()
        {
            Assert.AreEqual(1.0f, controller.CurrentTimeScale, "Initial CurrentTimeScale must be 1.0.");
            Assert.AreEqual(1.0f, Time.timeScale, "Time.timeScale must be 1.0.");
            Assert.AreEqual(0.02f, Time.fixedDeltaTime, 0.001f, "Time.fixedDeltaTime must be 0.02.");
        }

        [Test]
        public void SingleRequest_AppliesConfiguredScale()
        {
            controller.RequestTimeScale(0.4f, 2.0f, "SlowMoTest");

            Assert.AreEqual(0.4f, controller.CurrentTimeScale, 0.001f);
            Assert.AreEqual(0.4f, Time.timeScale, 0.001f);
            Assert.AreEqual(0.02f * 0.4f, Time.fixedDeltaTime, 0.001f);
        }

        [Test]
        public void StackedRequests_LowestScaleWins()
        {
            controller.RequestTimeScale(0.5f, 3.0f, "SlowMo");
            controller.RequestTimeScale(0.2f, 1.0f, "HitStop");
            controller.RequestTimeScale(0.8f, 2.0f, "DimensionShift");

            Assert.AreEqual(0.2f, controller.CurrentTimeScale, 0.001f, "Lowest scale (0.2) must win when stacked.");
            Assert.AreEqual(0.2f, Time.timeScale, 0.001f);
        }

        [Test]
        public void ReleasingRequestsInAnyOrder_RestoresNormalTimeWhenEmpty()
        {
            controller.RequestTimeScale(0.4f, 0f, "ReqA");
            controller.RequestTimeScale(0.1f, 0f, "ReqB");
            controller.RequestTimeScale(0.6f, 0f, "ReqC");

            // Release lowest (ReqB) first -> Next lowest (ReqA: 0.4) should become active
            controller.ReleaseTimeScale("ReqB");
            Assert.AreEqual(0.4f, controller.CurrentTimeScale, 0.001f, "ReqA (0.4) should become active.");

            // Release ReqA -> ReqC (0.6) should become active
            controller.ReleaseTimeScale("ReqA");
            Assert.AreEqual(0.6f, controller.CurrentTimeScale, 0.001f, "ReqC (0.6) should become active.");

            // Release last request -> should restore 1.0
            controller.ReleaseTimeScale("ReqC");
            Assert.AreEqual(1.0f, controller.CurrentTimeScale, 0.001f, "Releasing all requests must restore 1.0.");
            Assert.AreEqual(1.0f, Time.timeScale, 0.001f);
            Assert.AreEqual(0.02f, Time.fixedDeltaTime, 0.001f);
        }

        [Test]
        public void RestoreNormalTime_ClearsAllRequestsImmediately()
        {
            controller.RequestTimeScale(0.1f, 5.0f, "Freeze");
            controller.RequestTimeScale(0.4f, 5.0f, "SlowMo");

            controller.RestoreNormalTime();

            Assert.AreEqual(0, controller.ActiveRequestCount, "ActiveRequestCount must be 0 after RestoreNormalTime.");
            Assert.AreEqual(1.0f, controller.CurrentTimeScale, 0.001f);
            Assert.AreEqual(1.0f, Time.timeScale, 0.001f);
            Assert.AreEqual(0.02f, Time.fixedDeltaTime, 0.001f);
        }
    }
}
