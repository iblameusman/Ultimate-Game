using UnityEngine;

namespace DodgeGame.Data
{
    public enum CameraMode
    {
        ThirdPerson,
        Overview
    }

    public enum CameraDistancePreset
    {
        Close,
        Normal,
        Far
    }

    [CreateAssetMenu(fileName = "CameraConfig", menuName = "DodgeGame/Camera Config")]
    public class CameraConfig : ScriptableObject
    {
        [Header("Overview / Orbit Angles & Position")]
        [SerializeField] private float defaultYaw = 0f;
        [SerializeField] private float defaultPitch = 42f;
        [SerializeField] private float defaultDistance = 17f;

        [Header("Overview Sensitivities")]
        [SerializeField] private float orbitSpeedMouse = 100f;
        [SerializeField] private float orbitSpeedGamepad = 120f;
        [SerializeField] private float zoomSpeed = 6f;
        [SerializeField] private bool invertY = false;

        [Header("Overview Limits")]
        [SerializeField] private float minPitch = 25f;
        [SerializeField] private float maxPitch = 75f;
        [SerializeField] private float minDistance = 10f;
        [SerializeField] private float maxDistance = 28f;

        [Header("Overview Timing")]
        [SerializeField] private float smoothingTime = 0.12f;
        [SerializeField] private float resetDuration = 0.4f;

        [Header("Third Person Mouse Look")]
        [Tooltip("Degrees of camera rotation per pixel of mouse movement (Minecraft-like ~0.08-0.15).")]
        [SerializeField] private float mouseSensitivity = 0.1f;
        [Tooltip("Smoothing time for mouse look. Keep small so it feels direct but not jittery.")]
        [SerializeField] private float mouseSmoothing = 0.03f;
        [Tooltip("Lock and hide the cursor during gameplay so the mouse always controls the camera.")]
        [SerializeField] private bool lockCursorInGameplay = true;

        [Header("Third Person Camera Follow")]
        [Tooltip("Base distance from pivot to camera.")]
        [SerializeField] private float followDistance = 9f;
        [SerializeField] private float minFollowDistance = 5f;
        [SerializeField] private float maxFollowDistance = 14f;

        [Tooltip("Height of pivot above player position (feet).")]
        [SerializeField] private float followHeight = 2.2f;

        [Tooltip("Height of LookAt target above player position (chest level).")]
        [SerializeField] private float lookAtHeight = 1.2f;

        [Tooltip("Default pitch in degrees.")]
        [SerializeField] private float tpDefaultPitch = 22f;
        [SerializeField] private float tpMinPitch = 8f;
        [SerializeField] private float tpMaxPitch = 55f;

        [Tooltip("Base Field Of View.")]
        [SerializeField] private float fieldOfView = 60f;
        [SerializeField] private float dashFovKick = 6f;
        [SerializeField] private float dashFovReturnDuration = 0.3f;

        [Header("Third Person Damping & Smoothing")]
        [SerializeField] private float positionDamping = 0.12f;
        [SerializeField] private float rotationDamping = 0.08f;
        [SerializeField] private float verticalDamping = 0.35f;
        [SerializeField] private float lookAheadDistance = 0.6f;

        [Header("Auto-Recenter")]
        [SerializeField] private float autoRecenterDelay = 1.5f;
        [SerializeField] private float recenterSpeed = 60f;

        [Header("Collision & Bounds")]
        [SerializeField] private float sphereCastRadius = 0.3f;
        [SerializeField] private float collisionOffset = 0.2f;
        [SerializeField] private float minFloorHeight = 0.5f;
        [SerializeField] private float pullInDamping = 0.08f;
        [SerializeField] private float returnDamping = 0.4f;
        [SerializeField] private LayerMask arenaLayerMask = 1 << 11;
        [SerializeField] private Vector3 arenaBoundsMin = new Vector3(-19f, 0.5f, -19f);
        [SerializeField] private Vector3 arenaBoundsMax = new Vector3(19f, 15f, 19f);

        [Header("Occlusion & Effects")]
        [SerializeField] private bool wallOcclusionPitchRaise = true;
        [SerializeField] private bool fadeWallSegments = false;
        [SerializeField] private float wallFadeDuration = 0.15f;
        [SerializeField] private float landingNudgeAmount = 0.05f;
        [SerializeField] private float landingNudgeDuration = 0.2f;

        public float DefaultYaw => defaultYaw;
        public float DefaultPitch => defaultPitch;
        public float DefaultDistance => defaultDistance;
        public float OrbitSpeedMouse => orbitSpeedMouse;
        public float OrbitSpeedGamepad => orbitSpeedGamepad;
        public float ZoomSpeed => zoomSpeed;
        public float OrbitSensitivity => orbitSpeedGamepad;
        public float ZoomSensitivity => zoomSpeed;
        public bool InvertY { get => invertY; set => invertY = value; }
        public float MinPitch => minPitch;
        public float MaxPitch => maxPitch;
        public float MinDistance => minDistance;
        public float MaxDistance => maxDistance;
        public float SmoothingTime => smoothingTime;
        public float ResetDuration => resetDuration;

        public float MouseSensitivity => mouseSensitivity;
        public float MouseSmoothing => mouseSmoothing;
        public bool LockCursorInGameplay => lockCursorInGameplay;

        public float FollowDistance { get => followDistance; set => followDistance = Mathf.Clamp(value, minFollowDistance, maxFollowDistance); }
        public float MinFollowDistance => minFollowDistance;
        public float MaxFollowDistance => maxFollowDistance;
        public float FollowHeight => followHeight;
        public float LookAtHeight => lookAtHeight;
        public float ThirdPersonDefaultPitch => tpDefaultPitch;
        public float ThirdPersonMinPitch => tpMinPitch;
        public float ThirdPersonMaxPitch => tpMaxPitch;
        public float FieldOfView { get => fieldOfView; set => fieldOfView = Mathf.Clamp(value, 40f, 90f); }
        public float DashFovKick => dashFovKick;
        public float DashFovReturnDuration => dashFovReturnDuration;
        public float PositionDamping => positionDamping;
        public float RotationDamping => rotationDamping;
        public float VerticalDamping => verticalDamping;
        public float LookAheadDistance => lookAheadDistance;
        public float AutoRecenterDelay => autoRecenterDelay;
        public float RecenterSpeed => recenterSpeed;

        public float SphereCastRadius => sphereCastRadius;
        public float CollisionOffset => collisionOffset;
        public float MinFloorHeight => minFloorHeight;
        public float PullInDamping => pullInDamping;
        public float ReturnDamping => returnDamping;
        public LayerMask ArenaLayerMask { get => arenaLayerMask; set => arenaLayerMask = value; }
        public Vector3 ArenaBoundsMin => arenaBoundsMin;
        public Vector3 ArenaBoundsMax => arenaBoundsMax;
        public bool WallOcclusionPitchRaise => wallOcclusionPitchRaise;
        public bool FadeWallSegments { get => fadeWallSegments; set => fadeWallSegments = value; }
        public float WallFadeDuration => wallFadeDuration;
        public float LandingNudgeAmount => landingNudgeAmount;
        public float LandingNudgeDuration => landingNudgeDuration;

        public void SetInvertY(bool inverted)
        {
            invertY = inverted;
        }

        public void SetDefaults(float yaw, float pitch, float distance)
        {
            defaultYaw = yaw;
            defaultPitch = pitch;
            defaultDistance = distance;
        }

        public void SetLimits(float minP, float maxP, float minD, float maxD)
        {
            minPitch = minP;
            maxPitch = maxP;
            minDistance = minD;
            maxDistance = maxD;
        }

        public void ApplyDistancePreset(CameraDistancePreset preset)
        {
            switch (preset)
            {
                case CameraDistancePreset.Close:
                    followDistance = 7f;
                    break;
                case CameraDistancePreset.Normal:
                    followDistance = 9f;
                    break;
                case CameraDistancePreset.Far:
                    followDistance = 12f;
                    break;
            }
        }
    }
}
