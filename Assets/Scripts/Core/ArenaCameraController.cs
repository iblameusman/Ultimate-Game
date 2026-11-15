using UnityEngine;
using UnityEngine.InputSystem;
using DodgeGame.Data;

namespace DodgeGame.Core
{
    [DefaultExecutionOrder(100)]
    public class ArenaCameraController : MonoBehaviour
    {
        public static ArenaCameraController Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private CameraConfig config;

        [Header("Target & Center")]
        [Tooltip("Center of orbit, default Vector3.zero for arena center.")]
        [SerializeField] private Vector3 orbitCenter = Vector3.zero;

        [Tooltip("Screen space offset of target from viewport center (e.g. shift arena right in MainMenu).")]
        [SerializeField] private Vector2 screenOffset = Vector2.zero;

        [Tooltip("Camera transform driven by this controller. If null, resolves Camera.main.")]
        [SerializeField] private Transform targetCamera;

        public Vector2 ScreenOffset
        {
            get => screenOffset;
            set => screenOffset = value;
        }

        private float currentYaw;
        private float currentPitch;
        private float currentDistance;

        private float targetYaw;
        private float targetPitch;
        private float targetDistance;

        private float yawVelocity;
        private float pitchVelocity;
        private float distanceVelocity;

        private bool isResetting = false;
        private float resetTimer = 0f;
        private float resetStartYaw;
        private float resetStartPitch;
        private float resetStartDistance;

        private float lastClickTime = -1f;
        private const float DoubleClickThreshold = 0.35f;

        private float lastPinchDistance = -1f;

        public float CurrentYaw => currentYaw;
        public float CurrentPitch => currentPitch;
        public float CurrentDistance => currentDistance;
        public CameraConfig Config => config;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            ResolveReferences();
            InitializeFromDefaults();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void ResolveReferences()
        {
            if (config == null)
            {
                config = Resources.Load<CameraConfig>("SO_CameraConfig") ?? Resources.Load<CameraConfig>("CameraConfig");
#if UNITY_EDITOR
                if (config == null)
                {
                    config = UnityEditor.AssetDatabase.LoadAssetAtPath<CameraConfig>("Assets/ScriptableObjects/SO_CameraConfig.asset")
                          ?? UnityEditor.AssetDatabase.LoadAssetAtPath<CameraConfig>("Assets/ScriptableObjects/CameraConfig.asset");
                }
#endif
            }

            if (targetCamera == null)
            {
                var vcam = FindAnyObjectByType<Unity.Cinemachine.CinemachineCamera>();
                if (vcam != null)
                {
                    targetCamera = vcam.transform;
                }
                else if (Camera.main != null)
                {
                    targetCamera = Camera.main.transform;
                }
            }
        }

        public void InitializeFromDefaults()
        {
            if (config == null) return;

            targetYaw = config.DefaultYaw;
            targetPitch = config.DefaultPitch;
            targetDistance = config.DefaultDistance;

            currentYaw = targetYaw;
            currentPitch = targetPitch;
            currentDistance = targetDistance;

            ApplyTransformImmediate();
        }

        private void Update()
        {
            ResolveReferences();

            if (isResetting)
            {
                UpdateResetAnimation();
            }
            else
            {
                if (!IsInputBlocked())
                {
                    HandleInput();
                }
            }

            ApplySmoothDamping();
            UpdateCameraTransform();
        }

        private bool IsInputBlocked()
        {
            if (PauseController.Instance != null && PauseController.Instance.IsPaused) return true;
            if (UI.SettingsModalController.IsAnyModalOpen) return true;

            var settingsModals = FindObjectsByType<UI.SettingsModalController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var modal in settingsModals)
            {
                if (modal != null && modal.IsOpen) return true;
            }

            var diffModals = FindObjectsByType<UI.DifficultyModalController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var modal in diffModals)
            {
                if (modal != null && modal.gameObject.activeInHierarchy) return true;
            }

            var openPanels = FindObjectsByType<UI.MenuPanelTransition>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var panel in openPanels)
            {
                if (panel != null && panel.IsOpen) return true;
            }

            return false;
        }

        private void HandleInput()
        {
            if (config == null) return;

            bool invertY = config.InvertY;
            if (SettingsService.Instance?.CurrentSettings != null)
            {
                invertY = SettingsService.Instance.CurrentSettings.CameraInvertY;
            }

            float dt = Time.unscaledDeltaTime;
            float yawDelta = 0f;
            float pitchDelta = 0f;
            float zoomDelta = 0f;

            if (Mouse.current != null)
            {
                bool overUI = UnityEngine.EventSystems.EventSystem.current != null && 
                              UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

                if (!overUI && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame))
                {
                    float now = Time.unscaledTime;
                    if (now - lastClickTime < DoubleClickThreshold)
                    {
                        ResetToDefault();
                        return;
                    }
                    lastClickTime = now;
                }

                if (!overUI)
                {
                    if (Mouse.current.rightButton.isPressed)
                    {
                        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
                        yawDelta += mouseDelta.x * (config.OrbitSpeedMouse * 0.08f);
                        float ySign = invertY ? 1f : -1f;
                        pitchDelta += mouseDelta.y * ySign * (config.OrbitSpeedMouse * 0.08f);
                    }

                    float scroll = Mouse.current.scroll.ReadValue().y;
                    if (Mathf.Abs(scroll) > 0.01f)
                    {
                        zoomDelta -= Mathf.Sign(scroll) * config.ZoomSpeed;
                    }
                }
            }

            if (Gamepad.current != null)
            {
                if (Gamepad.current.rightStickButton.wasPressedThisFrame)
                {
                    ResetToDefault();
                    return;
                }

                Vector2 stick = Gamepad.current.rightStick.ReadValue();
                if (stick.sqrMagnitude > 0.01f)
                {
                    yawDelta += stick.x * config.OrbitSpeedGamepad * dt;
                    float ySign = invertY ? 1f : -1f;
                    pitchDelta += stick.y * ySign * config.OrbitSpeedGamepad * dt;
                }

                if (Gamepad.current.rightShoulder.isPressed)
                {
                    zoomDelta -= config.ZoomSpeed * dt * 2f;
                }
                if (Gamepad.current.leftShoulder.isPressed)
                {
                    zoomDelta += config.ZoomSpeed * dt * 2f;
                }
            }

            if (Keyboard.current != null)
            {
                if (Keyboard.current.homeKey.wasPressedThisFrame)
                {
                    ResetToDefault();
                    return;
                }

                if (Keyboard.current.pageUpKey.isPressed) zoomDelta -= config.ZoomSpeed * dt * 2f;
                if (Keyboard.current.pageDownKey.isPressed) zoomDelta += config.ZoomSpeed * dt * 2f;
            }

            if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
            {
                int touchCount = 0;
                for (int i = 0; i < Touchscreen.current.touches.Count; i++)
                {
                    if (Touchscreen.current.touches[i].isInProgress) touchCount++;
                }

                if (touchCount == 1)
                {
                    var touch0 = Touchscreen.current.touches[0];
                    Vector2 delta = touch0.delta.ReadValue();
                    yawDelta += delta.x * (config.OrbitSensitivity * 0.05f);
                    float ySign = invertY ? 1f : -1f;
                    pitchDelta += delta.y * ySign * (config.OrbitSensitivity * 0.05f);
                    lastPinchDistance = -1f;
                }
                else if (touchCount >= 2)
                {
                    Vector2 pos0 = Touchscreen.current.touches[0].position.ReadValue();
                    Vector2 pos1 = Touchscreen.current.touches[1].position.ReadValue();
                    float currentPinchDist = Vector2.Distance(pos0, pos1);
                    if (lastPinchDistance > 0f)
                    {
                        float pinchDelta = currentPinchDist - lastPinchDistance;
                        zoomDelta -= pinchDelta * (config.ZoomSensitivity * 0.02f);
                    }
                    lastPinchDistance = currentPinchDist;
                }
            }
            else
            {
                lastPinchDistance = -1f;
            }

            targetYaw += yawDelta;
            targetPitch = Mathf.Clamp(targetPitch + pitchDelta, config.MinPitch, config.MaxPitch);
            targetDistance = Mathf.Clamp(targetDistance + zoomDelta, config.MinDistance, config.MaxDistance);
        }

        public void ResetToDefault()
        {
            if (config == null) return;
            isResetting = true;
            resetTimer = 0f;
            resetStartYaw = currentYaw;
            resetStartPitch = currentPitch;
            resetStartDistance = currentDistance;
            targetYaw = config.DefaultYaw;
            targetPitch = config.DefaultPitch;
            targetDistance = config.DefaultDistance;
        }

        public void ResetCamera() => ResetToDefault();

        public void SetConfig(CameraConfig newConfig)
        {
            config = newConfig;
        }

        public void SetTargetPitch(float pitch)
        {
            float minP = config != null ? config.MinPitch : 25f;
            float maxP = config != null ? config.MaxPitch : 75f;
            targetPitch = Mathf.Clamp(pitch, minP, maxP);
            currentPitch = targetPitch;
        }

        public void SetTargetDistance(float dist)
        {
            float minD = config != null ? config.MinDistance : 10f;
            float maxD = config != null ? config.MaxDistance : 28f;
            targetDistance = Mathf.Clamp(dist, minD, maxD);
            currentDistance = targetDistance;
        }

        public void SetTargetYaw(float yaw)
        {
            targetYaw = yaw;
            currentYaw = yaw;
        }

        private void UpdateResetAnimation()
        {
            float duration = config != null && config.ResetDuration > 0f ? config.ResetDuration : 0.4f;
            resetTimer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(resetTimer / duration);
            float ease = 1f - Mathf.Pow(1f - t, 3f);

            targetYaw = Mathf.LerpAngle(resetStartYaw, config.DefaultYaw, ease);
            targetPitch = Mathf.Lerp(resetStartPitch, config.DefaultPitch, ease);
            targetDistance = Mathf.Lerp(resetStartDistance, config.DefaultDistance, ease);

            if (t >= 1f)
            {
                isResetting = false;
                targetYaw = config.DefaultYaw;
                targetPitch = config.DefaultPitch;
                targetDistance = config.DefaultDistance;
            }
        }

        private void ApplySmoothDamping()
        {
            float smooth = config != null ? config.SmoothingTime : 0.12f;
            currentYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref yawVelocity, smooth, Mathf.Infinity, Time.unscaledDeltaTime);
            currentPitch = Mathf.SmoothDamp(currentPitch, targetPitch, ref pitchVelocity, smooth, Mathf.Infinity, Time.unscaledDeltaTime);
            currentDistance = Mathf.SmoothDamp(currentDistance, targetDistance, ref distanceVelocity, smooth, Mathf.Infinity, Time.unscaledDeltaTime);

            if (config != null)
            {
                currentPitch = Mathf.Clamp(currentPitch, config.MinPitch, config.MaxPitch);
                currentDistance = Mathf.Clamp(currentDistance, config.MinDistance, config.MaxDistance);
            }
        }

        private void ApplyTransformImmediate()
        {
            UpdateCameraTransform();
        }

        private void UpdateCameraTransform()
        {
            if (targetCamera == null)
            {
                ResolveReferences();
                if (targetCamera == null) return;
            }

            Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
            Vector3 offset = rotation * new Vector3(-screenOffset.x, screenOffset.y, -currentDistance);
            Vector3 camPos = orbitCenter + offset;
            Quaternion camRot = rotation;

            targetCamera.position = camPos;
            targetCamera.rotation = camRot;

            var cmFollow = targetCamera.GetComponent<Unity.Cinemachine.CinemachineFollow>();
            if (cmFollow != null)
            {
                cmFollow.FollowOffset = offset;
            }

            if (Camera.main != null && Camera.main.transform != targetCamera)
            {
                var brain = Camera.main.GetComponent<Unity.Cinemachine.CinemachineBrain>();
                if (brain == null)
                {
                    Camera.main.transform.position = camPos;
                    Camera.main.transform.rotation = camRot;
                }
            }
        }
    }
}
