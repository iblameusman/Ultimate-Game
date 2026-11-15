using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using DodgeGame.Core;
using DodgeGame.Data;

namespace DodgeGame.Tests.PlayMode
{
    [TestFixture]
    public class CameraOrbitTests
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            TimeScaleController.ResetTimeScale();
            yield return SceneManager.LoadSceneAsync("MainMenu");
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            TimeScaleController.ResetTimeScale();
            yield return null;
        }

        [UnityTest]
        public IEnumerator SimulatingOrbitChangesCameraYaw()
        {
            var cameraController = Object.FindAnyObjectByType<ArenaCameraController>();
            Assert.IsNotNull(cameraController, "ArenaCameraController must be present in the scene.");

            float initialYaw = cameraController.CurrentYaw;

            // Change yaw by +45 degrees
            cameraController.SetTargetYaw(initialYaw + 45f);
            yield return new WaitForSecondsRealtime(0.25f);

            Assert.AreNotEqual(initialYaw, cameraController.CurrentYaw,
                "Camera yaw must change when orbit target changes.");
            Assert.AreEqual(initialYaw + 45f, cameraController.CurrentYaw, 5.0f,
                "Camera yaw should smoothly approach target yaw.");
        }

        [UnityTest]
        public IEnumerator SimulatingScrollChangesCameraDistanceWithinLimits()
        {
            var cameraController = Object.FindAnyObjectByType<ArenaCameraController>();
            Assert.IsNotNull(cameraController, "ArenaCameraController must be present.");

            var config = cameraController.Config;
            Assert.IsNotNull(config, "CameraConfig must be assigned to ArenaCameraController.");

            // Zoom in past min limit
            cameraController.SetTargetDistance(5f);
            yield return new WaitForSecondsRealtime(0.2f);
            Assert.GreaterOrEqual(cameraController.CurrentDistance, config.MinDistance - 0.1f,
                "Distance must not drop below min limit.");

            // Zoom out past max limit
            cameraController.SetTargetDistance(50f);
            yield return new WaitForSecondsRealtime(0.2f);
            Assert.LessOrEqual(cameraController.CurrentDistance, config.MaxDistance + 0.1f,
                "Distance must not exceed max limit.");
        }
    }
}
