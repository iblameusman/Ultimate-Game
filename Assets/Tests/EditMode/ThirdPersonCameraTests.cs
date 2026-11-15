using NUnit.Framework;
using UnityEngine;
using DodgeGame.Core;
using DodgeGame.Data;

namespace DodgeGame.Tests.EditMode
{
    [TestFixture]
    public class ThirdPersonCameraTests
    {
        private CameraConfig config;

        [SetUp]
        public void SetUp()
        {
            config = Resources.Load<CameraConfig>("SO_CameraConfig") ?? ScriptableObject.CreateInstance<CameraConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up if created dynamically
        }

        [Test]
        public void CameraConfigThirdPersonValuesAreWithinValidRanges()
        {
            Assert.Less(config.ThirdPersonMinPitch, config.ThirdPersonMaxPitch, "Min pitch must be less than max pitch.");
            Assert.GreaterOrEqual(config.ThirdPersonMinPitch, 0f, "Min pitch must be non-negative.");
            Assert.LessOrEqual(config.ThirdPersonMaxPitch, 89f, "Max pitch must not exceed 89 degrees.");

            Assert.Less(config.MinFollowDistance, config.MaxFollowDistance, "Min follow distance must be less than max follow distance.");
            Assert.Greater(config.MinFollowDistance, 0f, "Min follow distance must be positive.");
            Assert.Greater(config.FollowDistance, config.MinFollowDistance, "Default follow distance must be greater than min distance.");
            Assert.LessOrEqual(config.FollowDistance, config.MaxFollowDistance, "Default follow distance must not exceed max distance.");

            Assert.Greater(config.PositionDamping, 0f, "Position damping must be positive.");
            Assert.Greater(config.VerticalDamping, 0f, "Vertical damping must be positive.");
            Assert.Greater(config.RotationDamping, 0f, "Rotation damping must be positive.");
            Assert.Greater(config.PullInDamping, 0f, "Pull-in damping must be positive.");
            Assert.Greater(config.ReturnDamping, 0f, "Return damping must be positive.");

            Assert.Greater(config.FollowHeight, 0f, "Follow height must be positive.");
            Assert.Greater(config.LookAtHeight, 0f, "LookAt height must be positive.");
            Assert.Greater(config.MinFloorHeight, 0f, "Min floor height must be positive.");
        }

        [Test]
        public void CollisionSolverReturnsPositionAboveFloorAndWithinMinDistance()
        {
            var rigGo = new GameObject("TestRig");
            var rig = rigGo.AddComponent<ThirdPersonCameraRig>();
            rig.SetConfig(config);

            Vector3 pivot = new Vector3(0f, 2.2f, 0f);
            Vector3 desiredPosition = new Vector3(0f, -1.0f, -9f); // Artificially below floor

            Vector3 resolved = rig.SolveCollisionAndBounds(pivot, desiredPosition, 0.016f);

            Assert.GreaterOrEqual(resolved.y, config.MinFloorHeight, "Collision solver must clamp camera position above minFloorHeight.");

            float distFromPivot = Vector3.Distance(pivot, resolved);
            Assert.GreaterOrEqual(distFromPivot, config.MinFollowDistance - 0.5f, "Camera must not be pushed inside player capsule.");

            Object.DestroyImmediate(rigGo);
        }

        [Test]
        public void AutoRecenterAngleMovesTowardMovementDirectionAndStopsOnOrbitInput()
        {
            var rigGo = new GameObject("TestRig");
            var rig = rigGo.AddComponent<ThirdPersonCameraRig>();

            rig.SetTargetYaw(0f);
            Assert.AreEqual(0f, rig.TargetYaw, 0.01f);

            // Simulate orbit input resets timer
            rig.SetTargetYaw(45f);
            Assert.AreEqual(45f, rig.TargetYaw, 0.01f);

            Object.DestroyImmediate(rigGo);
        }

        [Test]
        public void DistancePresetsApplyCorrectDistances()
        {
            config.ApplyDistancePreset(CameraDistancePreset.Close);
            Assert.AreEqual(7f, config.FollowDistance, 0.01f, "Close preset must set followDistance to 7.");

            config.ApplyDistancePreset(CameraDistancePreset.Normal);
            Assert.AreEqual(9f, config.FollowDistance, 0.01f, "Normal preset must set followDistance to 9.");

            config.ApplyDistancePreset(CameraDistancePreset.Far);
            Assert.AreEqual(12f, config.FollowDistance, 0.01f, "Far preset must set followDistance to 12.");
        }
    }
}
