using NUnit.Framework;
using UnityEngine;
using DodgeGame.Core;
using DodgeGame.Data;

namespace DodgeGame.Tests.EditMode
{
    [TestFixture]
    public class CameraControllerTests
    {
        private GameObject go;
        private ArenaCameraController controller;
        private CameraConfig config;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("TestCameraRig");
            controller = go.AddComponent<ArenaCameraController>();
            config = ScriptableObject.CreateInstance<CameraConfig>();
            config.SetDefaults(0f, 42f, 17f);
            config.SetLimits(25f, 75f, 10f, 28f);
            controller.SetConfig(config);
            controller.InitializeFromDefaults();
        }

        [TearDown]
        public void TearDown()
        {
            if (go != null) Object.DestroyImmediate(go);
            if (config != null) Object.DestroyImmediate(config);
        }

        [Test]
        public void PitchIsClampedWhenDrivenPastLimits()
        {
            // Below min limit
            controller.SetTargetPitch(10f);
            Assert.AreEqual(25f, controller.CurrentPitch, 0.01f, "Pitch must not drop below min pitch 25.");

            // Above max limit
            controller.SetTargetPitch(90f);
            Assert.AreEqual(75f, controller.CurrentPitch, 0.01f, "Pitch must not exceed max pitch 75.");
        }

        [Test]
        public void DistanceIsClampedWhenDrivenPastLimits()
        {
            // Below min distance
            controller.SetTargetDistance(5f);
            Assert.AreEqual(10f, controller.CurrentDistance, 0.01f, "Distance must not drop below min distance 10.");

            // Above max distance
            controller.SetTargetDistance(45f);
            Assert.AreEqual(28f, controller.CurrentDistance, 0.01f, "Distance must not exceed max distance 28.");
        }

        [Test]
        public void ResetCameraRestoresDefaultPitchAndDistance()
        {
            // Alter state
            controller.SetTargetPitch(60f);
            controller.SetTargetDistance(22f);
            Assert.AreNotEqual(42f, controller.CurrentPitch);
            Assert.AreNotEqual(17f, controller.CurrentDistance);

            // Reset
            controller.ResetCamera();
            // In reset, defaults are immediately targeted
            Assert.AreEqual(42f, config.DefaultPitch, 0.01f);
            Assert.AreEqual(17f, config.DefaultDistance, 0.01f);
        }
    }
}
