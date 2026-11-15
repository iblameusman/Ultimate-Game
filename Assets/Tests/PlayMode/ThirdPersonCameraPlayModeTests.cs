using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using DodgeGame.Core;
using DodgeGame.Data;
using DodgeGame.Player;
using Unity.Cinemachine;

namespace DodgeGame.Tests.PlayMode
{
    [TestFixture]
    public class ThirdPersonCameraPlayModeTests
    {
        private GameObject playerObject;
        private Rigidbody rb;
        private CapsuleCollider capsuleCollider;
        private PlayerController playerController;
        private DashController dashController;

        private GameObject mainCameraObject;
        private Camera mainCamera;
        private CinemachineBrain brain;

        private GameObject cmCameraObject;
        private CinemachineCamera cmCamera;

        private GameObject rigObject;
        private ThirdPersonCameraRig rig;

        private GameObject timeObject;
        private TimeScaleController timeScaleController;

        private GameObject shakeObject;
        private CameraShake cameraShake;

        private CameraConfig config;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            TimeScaleController.ResetTimeScale();
            GameEvents.ClearAllListeners();

            config = Resources.Load<CameraConfig>("SO_CameraConfig") ?? ScriptableObject.CreateInstance<CameraConfig>();

            timeObject = new GameObject("TestTimeScaleController");
            timeScaleController = timeObject.AddComponent<TimeScaleController>();

            shakeObject = new GameObject("TestCameraShake");
            cameraShake = shakeObject.AddComponent<CameraShake>();

            // Main camera with brain
            mainCameraObject = new GameObject("Main Camera");
            mainCameraObject.tag = "MainCamera";
            mainCamera = mainCameraObject.AddComponent<Camera>();
            brain = mainCameraObject.AddComponent<Unity.Cinemachine.CinemachineBrain>();

            // Cinemachine Camera
            cmCameraObject = new GameObject("CinemachineCamera");
            cmCamera = cmCameraObject.AddComponent<Unity.Cinemachine.CinemachineCamera>();
            cmCamera.Priority.Value = 20;

            // Player
            playerObject = new GameObject("TestPlayer");
            playerObject.tag = "Player";
            rb = playerObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = false;
            capsuleCollider = playerObject.AddComponent<CapsuleCollider>();
            playerController = playerObject.AddComponent<PlayerController>();
            dashController = playerObject.AddComponent<DashController>();

            playerObject.transform.position = Vector3.zero;
            playerObject.transform.rotation = Quaternion.identity;

            // Third person rig
            rigObject = new GameObject("TestThirdPersonRig");
            rig = rigObject.AddComponent<ThirdPersonCameraRig>();
            rig.SetConfig(config);

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            TimeScaleController.ResetTimeScale();
            GameEvents.ClearAllListeners();

            if (playerObject != null) Object.DestroyImmediate(playerObject);
            if (rigObject != null) Object.DestroyImmediate(rigObject);
            if (cmCameraObject != null) Object.DestroyImmediate(cmCameraObject);
            if (mainCameraObject != null) Object.DestroyImmediate(mainCameraObject);
            if (timeObject != null) Object.DestroyImmediate(timeObject);
            if (shakeObject != null) Object.DestroyImmediate(shakeObject);

            yield return null;
        }

        [UnityTest]
        public IEnumerator CameraStaysAboveFloorAndInsideArenaBoundsAtCorners()
        {
            Vector3[] testCorners = new Vector3[]
            {
                new Vector3(8f, 0.5f, 8f),
                new Vector3(-8f, 0.5f, 8f),
                new Vector3(-8f, 0.5f, -8f),
                new Vector3(8f, 0.5f, -8f)
            };

            foreach (var corner in testCorners)
            {
                playerObject.transform.position = corner;
                yield return new WaitForSecondsRealtime(0.15f);

                Vector3 camPos = cmCameraObject.transform.position;
                Assert.GreaterOrEqual(camPos.y, config.MinFloorHeight - 0.1f,
                    $"Camera Y {camPos.y} must not drop below minFloorHeight {config.MinFloorHeight} at corner {corner}.");

                Assert.GreaterOrEqual(camPos.x, config.ArenaBoundsMin.x - 0.5f, "Camera must stay within arena X min.");
                Assert.LessOrEqual(camPos.x, config.ArenaBoundsMax.x + 0.5f, "Camera must stay within arena X max.");
                Assert.GreaterOrEqual(camPos.z, config.ArenaBoundsMin.z - 0.5f, "Camera must stay within arena Z min.");
                Assert.LessOrEqual(camPos.z, config.ArenaBoundsMax.z + 0.5f, "Camera must stay within arena Z max.");
            }
        }

        [UnityTest]
        public IEnumerator CameraPullsInWhenWallIsBetweenPivotAndCamera()
        {
            // Create a temporary arena wall collider on layer 11
            var wallGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallGo.transform.position = new Vector3(0f, 2.2f, -4f);
            wallGo.transform.localScale = new Vector3(10f, 5f, 1f);
            wallGo.layer = 11; // Arena layer

            playerObject.transform.position = Vector3.zero;
            rig.SetTargetYaw(0f); // Looks from -Z toward +Z, wall is at -4 Z
            yield return new WaitForSecondsRealtime(0.25f);

            float wallDist = rig.CurrentDistance;
            Assert.Less(wallDist, config.FollowDistance, "Camera distance should pull in when near a wall.");

            // Remove wall
            Object.DestroyImmediate(wallGo);
            yield return new WaitForSecondsRealtime(0.45f);

            Assert.Greater(rig.CurrentDistance, wallDist, "Camera distance should return toward default when clear of wall.");
        }

        [UnityTest]
        public IEnumerator MovementIsRelativeToCameraYawAfterOrbit()
        {
            // Default yaw 0: forward is +Z
            mainCameraObject.transform.rotation = Quaternion.Euler(22f, 0f, 0f);
            Vector3 dir0 = playerController.GetCameraRelativeDirection(Vector2.up);
            Assert.Greater(dir0.z, 0.8f, "With 0 yaw, forward input should point roughly +Z.");

            // Orbit 90 degrees: forward should point +X
            mainCameraObject.transform.rotation = Quaternion.Euler(22f, 90f, 0f);
            Vector3 dir90 = playerController.GetCameraRelativeDirection(Vector2.up);
            Assert.Greater(dir90.x, 0.8f, "With 90 yaw, forward input should point roughly +X.");

            // Orbit 180 degrees: forward should point -Z
            mainCameraObject.transform.rotation = Quaternion.Euler(22f, 180f, 0f);
            Vector3 dir180 = playerController.GetCameraRelativeDirection(Vector2.up);
            Assert.Less(dir180.z, -0.8f, "With 180 yaw, forward input should point roughly -Z.");

            yield return null;
        }

        [UnityTest]
        public IEnumerator DashAppliesFovKickAndReturnsToBaseFov()
        {
            float baseFov = config.FieldOfView;
            dashController.TryDash();

            yield return new WaitForSecondsRealtime(0.06f);

            Assert.Greater(cmCamera.Lens.FieldOfView, baseFov + 1f, "Cinemachine camera FOV should kick during dash.");

            // Wait for return duration
            yield return new WaitForSecondsRealtime(0.35f);

            Assert.AreEqual(baseFov, cmCamera.Lens.FieldOfView, 1.0f, "Cinemachine camera FOV should return to base FOV after dash.");
        }

        [UnityTest]
        public IEnumerator PauseAndGameOverFreezeOrbitInput()
        {
            float initYaw = rig.TargetYaw;

            // Simulate Game Over
            GameEvents.TriggerGameOver(100);
            yield return null;

            Assert.AreEqual(CameraMode.Overview, rig.CurrentMode, "Rig must switch to Overview mode on Game Over.");

            // Reset returns to ThirdPerson
            rig.SetMode(CameraMode.ThirdPerson);
            yield return null;

            Assert.AreEqual(CameraMode.ThirdPerson, rig.CurrentMode, "Rig must return to ThirdPerson mode on Reset.");
        }

        [UnityTest]
        public IEnumerator SlowMotionDoesNotChangeCameraFollowSpeed()
        {
            // Normal time move
            TimeScaleController.ResetTimeScale();
            playerObject.transform.position = Vector3.zero;
            yield return new WaitForSecondsRealtime(0.05f);
            playerObject.transform.position = new Vector3(3f, 0.5f, 0f);
            yield return new WaitForSecondsRealtime(0.15f);
            float normalMovedDist = Vector3.Distance(Vector3.zero, cmCameraObject.transform.position);

            // Reset and enter slow motion
            playerObject.transform.position = Vector3.zero;
            yield return new WaitForSecondsRealtime(0.05f);
            TimeScaleController.Instance?.RequestTimeScale(0.4f, 0f, "Test");
            playerObject.transform.position = new Vector3(3f, 0.5f, 0f);
            yield return new WaitForSecondsRealtime(0.15f);
            float slowMovedDist = Vector3.Distance(Vector3.zero, cmCameraObject.transform.position);

            TimeScaleController.Instance?.ReleaseTimeScale("Test");
            TimeScaleController.ResetTimeScale();

            Assert.AreEqual(normalMovedDist, slowMovedDist, 1.5f,
                "Camera follow uses unscaled time so tracking should be virtually identical.");
        }

        [UnityTest]
        public IEnumerator ResetBehindPlayerReturnsToYawWithinDuration()
        {
            playerObject.transform.rotation = Quaternion.Euler(0f, 45f, 0f);

            rig.SetTargetYaw(180f);
            yield return new WaitForSecondsRealtime(0.05f);

            rig.ResetBehindPlayer();
            yield return new WaitForSecondsRealtime(0.45f);

            Assert.AreEqual(45f, rig.CurrentYaw, 5.0f, "Reset must bring camera yaw behind player within 0.5s.");
        }
    }
}
