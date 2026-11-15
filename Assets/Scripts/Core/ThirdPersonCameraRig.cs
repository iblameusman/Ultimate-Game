using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using DodgeGame.Data;
using DodgeGame.Player;

namespace DodgeGame.Core
{
    [DefaultExecutionOrder(150)]
    public class ThirdPersonCameraRig : MonoBehaviour
    {
        public static ThirdPersonCameraRig Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private CameraConfig config;

        [Header("Cinemachine References")]
        [Tooltip("The Cinemachine 3 camera used for third-person gameplay.")]
        [SerializeField] private Unity.Cinemachine.CinemachineCamera gameplayCamera;

        [Tooltip("The Cinemachine 3 camera used for overview / menu / game over.")]
        [SerializeField] private Unity.Cinemachine.CinemachineCamera overviewCamera;

        [Tooltip("Main Cinemachine Brain.")]
        [SerializeField] private Unity.Cinemachine.CinemachineBrain cinemachineBrain;

        [Header("Targets")]
        [Tooltip("Player root transform. Resolved automatically if null.")]
        [SerializeField] private Transform playerTransform;

        [Tooltip("Follow pivot (at player feet + followHeight).")]
        [SerializeField] private Transform followPivot;

        [Tooltip("LookAt target (at player feet + lookAtHeight).")]
        [SerializeField] private Transform lookAtTarget;

        [Header("Camera Mode")]
        [SerializeField] private CameraMode currentMode = CameraMode.ThirdPerson;

        private float currentYaw;
        private float currentPitch;
        private float currentDistance;
        private float currentFov;

        private float targetYaw;
        private float targetPitch;
        private float targetDistance;
        private float baseFov = 60f;

        private float yawVelocity;
        private float pitchVelocity;
        private float distanceVelocity;
        private float fovVelocity;
        private Vector3 horizontalPosVelocity;
        private float verticalPosVelocity;

        private Vector3 smoothedPivotPos;
        private Vector3 smoothedLookAtPos;

        private float timeSinceLastOrbitInput = 0f;

        private bool isResetting = false;
        private float resetTimer = 0f;
        private float resetStartYaw;
        private float resetStartPitch;
        private float resetStartDistance;
        private const float ResetDuration = 0.4f;

        private float dashFovBonus = 0f;
        private Coroutine dashFovCoroutine;

        private float landingNudgeY = 0f;
        private Coroutine landingNudgeCoroutine;

        private float occlusionPitchOffset = 0f;
        private float occlusionPitchVelocity = 0f;

        private readonly RaycastHit[] hitBuffer = new RaycastHit[8];

        private PlayerController playerController;
        private DashController dashController;
        private ArenaCameraController arenaCameraController;

        private float lastClickTime = -1f;
        private const float DoubleClickThreshold = 0.35f;

        private float lastPinchDistance = -1f;

        private bool cursorLocked = false;
        private bool skipNextMouseDelta = false;
        private bool lastInputWasMouse = false;

        public CameraMode CurrentMode => currentMode;
        public CameraConfig Config => config;
        public float CurrentYaw => currentYaw;
        public float CurrentPitch => currentPitch;
        public float CurrentDistance => currentDistance;
        public float TargetYaw => targetYaw;
        public float TargetPitch => targetPitch;
        public float TargetDistance => targetDistance;
        public float TimeSinceLastOrbitInput => timeSinceLastOrbitInput;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize()
        {
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (activeScene.name == "MainGame")
            {
                if (Instance == null)
                {
                    var rigGo = new GameObject("ThirdPersonCameraRig");
                    rigGo.AddComponent<ThirdPersonCameraRig>();
                }
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            ResolveReferences();
            InitializeTargetsAndState();
        }

        private void Start()
        {
            ResolveReferences();
            HookEvents();
            ApplyMode(currentMode, immediate: true);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            UnhookEvents();
            SetCursorLocked(false);
        }

        private void OnDisable()
        {
            SetCursorLocked(false);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            // Force a re-lock check when the window regains focus.
            if (!hasFocus) cursorLocked = false;
            skipNextMouseDelta = true;
        }

        private void SetCursorLocked(bool locked)
        {
            if (cursorLocked == locked && Cursor.lockState == (locked ? CursorLockMode.Locked : CursorLockMode.None)) return;
            cursorLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
            // The first delta after locking often contains the warp to screen center.
            if (locked) skipNextMouseDelta = true;
        }

        private void ResolveReferences()
        {
            if (config == null)
            {
                config = Resources.Load<CameraConfig>("SO_CameraConfig") ?? Resources.Load<CameraConfig>("CameraConfig");
#if UNITY_EDITOR
                if (config == null)
                {
                    config = UnityEditor.AssetDatabase.LoadAssetAtPath<CameraConfig>("Assets/Resources/SO_CameraConfig.asset")
                          ?? UnityEditor.AssetDatabase.LoadAssetAtPath<CameraConfig>("Assets/ScriptableObjects/SO_CameraConfig.asset");
                }
#endif
            }

            if (playerTransform == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                }
                else
                {
                    var pc = FindAnyObjectByType<PlayerController>();
                    if (pc != null) playerTransform = pc.transform;
                }
            }

            if (playerTransform != null)
            {
                if (playerController == null) playerController = playerTransform.GetComponent<PlayerController>();
                if (dashController == null) dashController = playerTransform.GetComponent<DashController>();
            }

            if (cinemachineBrain == null && Camera.main != null)
            {
                cinemachineBrain = Camera.main.GetComponent<Unity.Cinemachine.CinemachineBrain>();
            }

            if (gameplayCamera == null)
            {
                var vcams = FindObjectsByType<Unity.Cinemachine.CinemachineCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var vcam in vcams)
                {
                    if (vcam.gameObject.name.Contains("ThirdPerson") || vcam.gameObject.name.Equals("CinemachineCamera"))
                    {
                        gameplayCamera = vcam;
                        break;
                    }
                }
                if (gameplayCamera == null && vcams.Length > 0)
                {
                    gameplayCamera = vcams[0];
                }
            }

            if (overviewCamera == null)
            {
                var vcams = FindObjectsByType<Unity.Cinemachine.CinemachineCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var vcam in vcams)
                {
                    if (vcam != gameplayCamera && (vcam.gameObject.name.Contains("Overview") || vcam.gameObject.name.Contains("Menu")))
                    {
                        overviewCamera = vcam;
                        break;
                    }
                }

                if (overviewCamera == null)
                {
                    var overviewGo = new GameObject("CinemachineCamera_Overview");
                    overviewCamera = overviewGo.AddComponent<Unity.Cinemachine.CinemachineCamera>();
                    overviewCamera.Priority.Value = 10;
                    overviewGo.transform.position = new Vector3(0f, 21f, -18f);
                    overviewGo.transform.rotation = Quaternion.Euler(42f, 0f, 0f);
                }
            }

            if (gameplayCamera != null)
            {
                var cmFollow = gameplayCamera.GetComponent<Unity.Cinemachine.CinemachineFollow>();
                if (cmFollow != null)
                {
                    cmFollow.enabled = false;
                }
            }

            if (arenaCameraController == null)
            {
                arenaCameraController = ArenaCameraController.Instance ?? FindAnyObjectByType<ArenaCameraController>();
            }
        }

        private void InitializeTargetsAndState()
        {
            if (config != null)
            {
                targetYaw = 0f;
                targetPitch = config.ThirdPersonDefaultPitch;
                targetDistance = config.FollowDistance;
                baseFov = config.FieldOfView;
            }
            else
            {
                targetYaw = 0f;
                targetPitch = 22f;
                targetDistance = 9f;
                baseFov = 60f;
            }

            currentYaw = targetYaw;
            currentPitch = targetPitch;
            currentDistance = targetDistance;
            currentFov = baseFov;

            if (playerTransform != null)
            {
                float fHeight = config != null ? config.FollowHeight : 2.2f;
                float lHeight = config != null ? config.LookAtHeight : 1.2f;

                smoothedPivotPos = playerTransform.position + Vector3.up * fHeight;
                smoothedLookAtPos = playerTransform.position + Vector3.up * lHeight;
            }

            if (followPivot == null)
            {
                var pivotGo = new GameObject("Camera_FollowPivot");
                followPivot = pivotGo.transform;
                followPivot.position = smoothedPivotPos;
            }

            if (lookAtTarget == null)
            {
                var lookAtGo = new GameObject("Camera_LookAtTarget");
                lookAtTarget = lookAtGo.transform;
                lookAtTarget.position = smoothedLookAtPos;
            }
        }

        private void HookEvents()
        {
            if (dashController != null)
            {
                dashController.OnDashStart += HandleDashStart;
            }

            if (playerController != null)
            {
                playerController.OnLanded += HandlePlayerLanded;
            }

            GameEvents.OnGameOver += HandleGameOver;

            if (SettingsService.Instance != null)
            {
                SettingsService.Instance.OnSettingsChanged += HandleSettingsChanged;
                HandleSettingsChanged(SettingsService.Instance.CurrentSettings);
            }
        }

        private void UnhookEvents()
        {
            if (dashController != null)
            {
                dashController.OnDashStart -= HandleDashStart;
            }

            if (playerController != null)
            {
                playerController.OnLanded -= HandlePlayerLanded;
            }

            GameEvents.OnGameOver -= HandleGameOver;

            if (SettingsService.Instance != null)
            {
                SettingsService.Instance.OnSettingsChanged -= HandleSettingsChanged;
            }
        }

        public void SetMode(CameraMode newMode, float blendDuration = 0.4f)
        {
            currentMode = newMode;
            ApplyMode(currentMode, immediate: false, blendDuration);
        }

        private void ApplyMode(CameraMode mode, bool immediate, float blendDuration = 0.4f)
        {
            if (cinemachineBrain != null && !immediate)
            {
                cinemachineBrain.DefaultBlend = new Unity.Cinemachine.CinemachineBlendDefinition(
                    Unity.Cinemachine.CinemachineBlendDefinition.Styles.EaseInOut,
                    blendDuration
                );
            }

            if (mode == CameraMode.ThirdPerson)
            {
                if (gameplayCamera != null) gameplayCamera.Priority.Value = 20;
                if (overviewCamera != null) overviewCamera.Priority.Value = 10;

                if (arenaCameraController != null)
                {
                    arenaCameraController.enabled = false;
                }
            }
            else
            {
                if (overviewCamera != null) overviewCamera.Priority.Value = 20;
                if (gameplayCamera != null) gameplayCamera.Priority.Value = 10;

                if (arenaCameraController != null)
                {
                    arenaCameraController.enabled = true;
                }
            }
        }

        private void HandleDashStart()
        {
            if (dashFovCoroutine != null) StopCoroutine(dashFovCoroutine);
            dashFovCoroutine = StartCoroutine(DashFovKickRoutine());

            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.ShakeDash();
            }
        }

        private IEnumerator DashFovKickRoutine()
        {
            float kick = config != null ? config.DashFovKick : 6f;
            float duration = config != null ? config.DashFovReturnDuration : 0.3f;

            if (SettingsService.Instance?.CurrentSettings != null && SettingsService.Instance.CurrentSettings.ReducedMotion)
            {
                kick *= 0.5f;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float ease = 1f - Mathf.Pow(t, 2f);
                dashFovBonus = kick * ease;
                yield return null;
            }

            dashFovBonus = 0f;
            dashFovCoroutine = null;
        }

        private void HandlePlayerLanded()
        {
            if (SettingsService.Instance?.CurrentSettings != null && SettingsService.Instance.CurrentSettings.ReducedMotion)
            {
                return;
            }

            if (landingNudgeCoroutine != null) StopCoroutine(landingNudgeCoroutine);
            landingNudgeCoroutine = StartCoroutine(LandingNudgeRoutine());
        }

        private IEnumerator LandingNudgeRoutine()
        {
            float amount = config != null ? config.LandingNudgeAmount : 0.05f;
            float duration = config != null ? config.LandingNudgeDuration : 0.2f;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float ease = Mathf.Sin(t * Mathf.PI);
                landingNudgeY = -amount * ease;
                yield return null;
            }

            landingNudgeY = 0f;
            landingNudgeCoroutine = null;
        }

        private void HandleGameOver(int finalScore = 0)
        {
            SetMode(CameraMode.Overview, blendDuration: 0.6f);
        }

        private void HandleGameRestart()
        {
            ResetBehindPlayerImmediate();
            SetMode(CameraMode.ThirdPerson, blendDuration: 0.4f);
        }

        private void HandleSettingsChanged(SettingsData settings)
        {
            if (settings == null) return;

            if (config != null)
            {
                config.ApplyDistancePreset(settings.CameraDistance);
                targetDistance = config.FollowDistance;
                baseFov = settings.FieldOfView;
                config.FieldOfView = baseFov;
                config.SetInvertY(settings.CameraInvertY);
            }
        }

        private void LateUpdate()
        {
            if (currentMode != CameraMode.ThirdPerson)
            {
                SetCursorLocked(false);
                return;
            }

            ResolveReferences();
            if (playerTransform == null) return;

            float dt = Time.unscaledDeltaTime;

            bool inputBlocked = IsInputBlocked();
            bool wantLock = !inputBlocked && config != null && config.LockCursorInGameplay && Application.isFocused;
            SetCursorLocked(wantLock);

            if (isResetting)
            {
                UpdateResetAnimation(dt);
            }
            else
            {
                if (!inputBlocked)
                {
                    HandleInput(dt);
                    HandleAutoRecenter(dt);
                }
                else
                {
                    timeSinceLastOrbitInput = 0f;
                }
            }

            ApplySmoothing(dt);

            UpdatePivots(dt);

            Vector3 desiredCamPos = ComputeDesiredPosition(smoothedPivotPos, currentYaw, currentPitch, currentDistance);

            Vector3 finalCamPos = SolveCollisionAndBounds(smoothedPivotPos, desiredCamPos, dt);

            ApplyToCamera(finalCamPos, smoothedLookAtPos, dt);
        }

        private bool IsInputBlocked()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return true;
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

        private void HandleInput(float dt)
        {
            if (config == null) return;

            bool invertY = config.InvertY;
            if (SettingsService.Instance?.CurrentSettings != null)
            {
                invertY = SettingsService.Instance.CurrentSettings.CameraInvertY;
            }

            float yawDelta = 0f;
            float pitchDelta = 0f;
            float zoomDelta = 0f;
            bool hadOrbitInput = false;

            if (Mouse.current != null)
            {
                // With a locked cursor the pointer sits at screen center, so HUD hits are meaningless.
                bool overUI = !cursorLocked &&
                              UnityEngine.EventSystems.EventSystem.current != null &&
                              UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

                if (!overUI && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame))
                {
                    float now = Time.unscaledTime;
                    if (now - lastClickTime < DoubleClickThreshold)
                    {
                        ResetBehindPlayer();
                        return;
                    }
                    lastClickTime = now;
                }

                if (!overUI)
                {
                    // Minecraft-style: locked cursor = free mouse look. Unlocked fallback = hold RMB to orbit.
                    Vector2 mouseDelta = Mouse.current.delta.ReadValue();
                    if (skipNextMouseDelta)
                    {
                        mouseDelta = Vector2.zero;
                        skipNextMouseDelta = false;
                    }

                    if ((cursorLocked || Mouse.current.rightButton.isPressed) && mouseDelta.sqrMagnitude > 0.001f)
                    {
                        float sens = config.MouseSensitivity;
                        yawDelta += mouseDelta.x * sens;
                        float ySign = invertY ? 1f : -1f;
                        pitchDelta += mouseDelta.y * ySign * sens;
                        hadOrbitInput = true;
                        lastInputWasMouse = true;
                    }

                    float scroll = Mouse.current.scroll.ReadValue().y;
                    if (Mathf.Abs(scroll) > 0.01f)
                    {
                        zoomDelta -= Mathf.Sign(scroll) * config.ZoomSpeed;
                        hadOrbitInput = true;
                    }
                }
            }

            if (Gamepad.current != null)
            {
                if (Gamepad.current.startButton.wasPressedThisFrame || Gamepad.current.rightStickButton.wasPressedThisFrame)
                {
                    ResetBehindPlayer();
                    return;
                }

                Vector2 stick = Gamepad.current.rightStick.ReadValue();
                if (stick.sqrMagnitude > 0.01f)
                {
                    yawDelta += stick.x * config.OrbitSpeedGamepad * dt;
                    float ySign = invertY ? 1f : -1f;
                    pitchDelta += stick.y * ySign * config.OrbitSpeedGamepad * dt;
                    hadOrbitInput = true;
                    lastInputWasMouse = false;
                }

                if (Gamepad.current.rightShoulder.isPressed || Gamepad.current.dpad.down.isPressed)
                {
                    zoomDelta -= config.ZoomSpeed * dt * 2f;
                    hadOrbitInput = true;
                }
                if (Gamepad.current.leftShoulder.isPressed || Gamepad.current.dpad.up.isPressed)
                {
                    zoomDelta += config.ZoomSpeed * dt * 2f;
                    hadOrbitInput = true;
                }
            }

            if (Keyboard.current != null)
            {
                if (Keyboard.current.homeKey.wasPressedThisFrame)
                {
                    ResetBehindPlayer();
                    return;
                }

                if (Keyboard.current.leftArrowKey.isPressed) { yawDelta -= config.OrbitSpeedGamepad * dt; hadOrbitInput = true; }
                if (Keyboard.current.rightArrowKey.isPressed) { yawDelta += config.OrbitSpeedGamepad * dt; hadOrbitInput = true; }
                if (Keyboard.current.upArrowKey.isPressed) { pitchDelta += (invertY ? -1f : 1f) * config.OrbitSpeedGamepad * dt; hadOrbitInput = true; }
                if (Keyboard.current.downArrowKey.isPressed) { pitchDelta += (invertY ? 1f : -1f) * config.OrbitSpeedGamepad * dt; hadOrbitInput = true; }

                if (Keyboard.current.pageUpKey.isPressed) { zoomDelta -= config.ZoomSpeed * dt * 2f; hadOrbitInput = true; }
                if (Keyboard.current.pageDownKey.isPressed) { zoomDelta += config.ZoomSpeed * dt * 2f; hadOrbitInput = true; }
            }

            if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
            {
                int activeTouches = 0;
                for (int i = 0; i < Touchscreen.current.touches.Count; i++)
                {
                    if (Touchscreen.current.touches[i].isInProgress) activeTouches++;
                }

                if (activeTouches == 1)
                {
                    Vector2 delta = Touchscreen.current.touches[0].delta.ReadValue();
                    if (delta.sqrMagnitude > 0.01f)
                    {
                        yawDelta += delta.x * (config.OrbitSensitivity * 0.05f);
                        float ySign = invertY ? 1f : -1f;
                        pitchDelta += delta.y * ySign * (config.OrbitSensitivity * 0.05f);
                        hadOrbitInput = true;
                    }
                    lastPinchDistance = -1f;
                }
                else if (activeTouches >= 2)
                {
                    Vector2 pos0 = Touchscreen.current.touches[0].position.ReadValue();
                    Vector2 pos1 = Touchscreen.current.touches[1].position.ReadValue();
                    float currentPinch = Vector2.Distance(pos0, pos1);
                    if (lastPinchDistance > 0f)
                    {
                        float deltaPinch = currentPinch - lastPinchDistance;
                        zoomDelta -= deltaPinch * (config.ZoomSensitivity * 0.02f);
                        hadOrbitInput = true;
                    }
                    lastPinchDistance = currentPinch;
                }
            }
            else
            {
                lastPinchDistance = -1f;
            }

            if (hadOrbitInput)
            {
                timeSinceLastOrbitInput = 0f;
            }
            else
            {
                timeSinceLastOrbitInput += dt;
            }

            targetYaw += yawDelta;
            float minP = config != null ? config.ThirdPersonMinPitch : 8f;
            float maxP = config != null ? config.ThirdPersonMaxPitch : 55f;
            targetPitch = Mathf.Clamp(targetPitch + pitchDelta, minP, maxP);

            float minD = config != null ? config.MinFollowDistance : 5f;
            float maxD = config != null ? config.MaxFollowDistance : 14f;
            targetDistance = Mathf.Clamp(targetDistance + zoomDelta, minD, maxD);
        }

        private void HandleAutoRecenter(float dt)
        {
            if (config == null || playerController == null) return;

            // Mouse-look players aim the camera themselves; recentering would fight them.
            if (lastInputWasMouse) return;

            if (SettingsService.Instance?.CurrentSettings != null && SettingsService.Instance.CurrentSettings.ReducedMotion)
            {
                return;
            }

            if (playerController.MoveInput.sqrMagnitude > 0.01f && timeSinceLastOrbitInput >= config.AutoRecenterDelay)
            {
                Vector3 moveDir = playerController.GetCameraRelativeDirection(playerController.MoveInput);
                moveDir.y = 0f;

                if (moveDir.sqrMagnitude > 0.01f)
                {
                    float desiredYaw = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
                    targetYaw = Mathf.MoveTowardsAngle(targetYaw, desiredYaw, config.RecenterSpeed * dt);
                }
            }
        }

        public void ResetBehindPlayer()
        {
            if (playerTransform == null) return;
            isResetting = true;
            resetTimer = 0f;
            resetStartYaw = currentYaw;
            resetStartPitch = currentPitch;
            resetStartDistance = currentDistance;

            float playerYaw = playerTransform.eulerAngles.y;
            targetYaw = playerYaw;
            targetPitch = config != null ? config.ThirdPersonDefaultPitch : 22f;
            targetDistance = config != null ? config.FollowDistance : 9f;
        }

        public void ResetBehindPlayerImmediate()
        {
            if (playerTransform == null) return;
            isResetting = false;
            float playerYaw = playerTransform.eulerAngles.y;
            targetYaw = playerYaw;
            targetPitch = config != null ? config.ThirdPersonDefaultPitch : 22f;
            targetDistance = config != null ? config.FollowDistance : 9f;

            currentYaw = targetYaw;
            currentPitch = targetPitch;
            currentDistance = targetDistance;
        }

        private void UpdateResetAnimation(float dt)
        {
            resetTimer += dt;
            float t = Mathf.Clamp01(resetTimer / ResetDuration);
            float ease = 1f - Mathf.Pow(1f - t, 3f);

            float playerYaw = playerTransform != null ? playerTransform.eulerAngles.y : 0f;
            float goalPitch = config != null ? config.ThirdPersonDefaultPitch : 22f;
            float goalDist = config != null ? config.FollowDistance : 9f;

            targetYaw = Mathf.LerpAngle(resetStartYaw, playerYaw, ease);
            targetPitch = Mathf.Lerp(resetStartPitch, goalPitch, ease);
            targetDistance = Mathf.Lerp(resetStartDistance, goalDist, ease);

            if (t >= 1f)
            {
                isResetting = false;
                targetYaw = playerYaw;
                targetPitch = goalPitch;
                targetDistance = goalDist;
            }
        }

        private void ApplySmoothing(float dt)
        {
            float rotDamp = config != null ? config.RotationDamping : 0.08f;
            if (lastInputWasMouse && !isResetting && config != null)
            {
                rotDamp = config.MouseSmoothing;
            }
            currentYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref yawVelocity, rotDamp, Mathf.Infinity, dt);
            currentPitch = Mathf.SmoothDamp(currentPitch, targetPitch, ref pitchVelocity, rotDamp, Mathf.Infinity, dt);

            float minP = config != null ? config.ThirdPersonMinPitch : 8f;
            float maxP = config != null ? config.ThirdPersonMaxPitch : 55f;
            currentPitch = Mathf.Clamp(currentPitch, minP, maxP);

            float targetFovWithKick = baseFov + dashFovBonus;
            currentFov = Mathf.SmoothDamp(currentFov, targetFovWithKick, ref fovVelocity, 0.05f, Mathf.Infinity, dt);
        }

        private void UpdatePivots(float dt)
        {
            float fHeight = config != null ? config.FollowHeight : 2.2f;
            float lHeight = config != null ? config.LookAtHeight : 1.2f;
            float lookAhead = config != null ? config.LookAheadDistance : 0.6f;
            float posDamp = config != null ? config.PositionDamping : 0.12f;
            float vertDamp = config != null ? config.VerticalDamping : 0.35f;

            Vector3 moveLead = Vector3.zero;
            if (playerController != null && playerController.Velocity.sqrMagnitude > 0.01f)
            {
                Vector3 horizontalVel = new Vector3(playerController.Velocity.x, 0f, playerController.Velocity.z);
                moveLead = horizontalVel.normalized * lookAhead;
            }

            Vector3 targetPivotPos = playerTransform.position + Vector3.up * (fHeight + landingNudgeY) + moveLead;
            Vector3 targetLookAtPos = playerTransform.position + Vector3.up * lHeight + moveLead;

            Vector3 currentHorizPivot = new Vector3(smoothedPivotPos.x, 0f, smoothedPivotPos.z);
            Vector3 targetHorizPivot = new Vector3(targetPivotPos.x, 0f, targetPivotPos.z);
            Vector3 newHorizPivot = Vector3.SmoothDamp(currentHorizPivot, targetHorizPivot, ref horizontalPosVelocity, posDamp, Mathf.Infinity, dt);

            float newVertPivotY = Mathf.SmoothDamp(smoothedPivotPos.y, targetPivotPos.y, ref verticalPosVelocity, vertDamp, Mathf.Infinity, dt);
            smoothedPivotPos = new Vector3(newHorizPivot.x, newVertPivotY, newHorizPivot.z);

            smoothedLookAtPos = targetLookAtPos;

            if (followPivot != null) followPivot.position = smoothedPivotPos;
            if (lookAtTarget != null) lookAtTarget.position = smoothedLookAtPos;
        }

        public Vector3 ComputeDesiredPosition(Vector3 pivot, float yaw, float pitch, float distance)
        {
            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 offset = rot * new Vector3(0f, 0f, -distance);
            return pivot + offset;
        }

        public Vector3 SolveCollisionAndBounds(Vector3 pivot, Vector3 desiredPosition, float dt)
        {
            if (config == null) return desiredPosition;

            Vector3 castDir = desiredPosition - pivot;
            float targetDist = castDir.magnitude;
            if (targetDist < 0.001f) return desiredPosition;
            castDir /= targetDist;

            float radius = config.SphereCastRadius;
            LayerMask mask = config.ArenaLayerMask;
            float resolvedDistance = targetDist;

            int hitCount = Physics.SphereCastNonAlloc(
                pivot,
                radius,
                castDir,
                hitBuffer,
                targetDist,
                mask,
                QueryTriggerInteraction.Ignore
            );

            float closestHitDist = float.MaxValue;
            RaycastHit closestHit = default;
            bool didHit = false;

            for (int i = 0; i < hitCount; i++)
            {
                var hit = hitBuffer[i];
                if (hit.collider == null || hit.collider.isTrigger) continue;
                if (hit.distance < closestHitDist)
                {
                    closestHitDist = hit.distance;
                    closestHit = hit;
                    didHit = true;
                }
            }

            if (didHit)
            {
                resolvedDistance = Mathf.Max(closestHitDist - config.CollisionOffset, config.MinFollowDistance);

                if (config.WallOcclusionPitchRaise)
                {
                    float targetOcclusionPitch = Mathf.Min(config.ThirdPersonMaxPitch - currentPitch, 12f);
                    occlusionPitchOffset = Mathf.SmoothDamp(occlusionPitchOffset, targetOcclusionPitch, ref occlusionPitchVelocity, 0.15f, Mathf.Infinity, dt);
                }
            }
            else
            {
                occlusionPitchOffset = Mathf.SmoothDamp(occlusionPitchOffset, 0f, ref occlusionPitchVelocity, 0.25f, Mathf.Infinity, dt);
            }

            if (currentDistance < 0.001f)
            {
                currentDistance = resolvedDistance;
            }

            float dampTime = resolvedDistance < currentDistance ? config.PullInDamping : config.ReturnDamping;
            currentDistance = Mathf.SmoothDamp(currentDistance, resolvedDistance, ref distanceVelocity, dampTime, Mathf.Infinity, dt);

            Vector3 finalPos = pivot + (castDir * currentDistance);
            if (config.WallOcclusionPitchRaise && occlusionPitchOffset > 0.01f)
            {
                finalPos.y += Mathf.Tan(occlusionPitchOffset * Mathf.Deg2Rad) * currentDistance * 0.25f;
            }

            if (finalPos.y < config.MinFloorHeight)
            {
                finalPos.y = config.MinFloorHeight;
            }

            finalPos.x = Mathf.Clamp(finalPos.x, config.ArenaBoundsMin.x, config.ArenaBoundsMax.x);
            finalPos.y = Mathf.Clamp(finalPos.y, config.ArenaBoundsMin.y, config.ArenaBoundsMax.y);
            finalPos.z = Mathf.Clamp(finalPos.z, config.ArenaBoundsMin.z, config.ArenaBoundsMax.z);

            return finalPos;
        }

        private void ApplyToCamera(Vector3 cameraPosition, Vector3 lookAtPosition, float dt)
        {
            if (gameplayCamera == null) return;

            Transform camTransform = gameplayCamera.transform;
            camTransform.position = cameraPosition;

            Vector3 toLookAt = lookAtPosition - cameraPosition;
            if (toLookAt.sqrMagnitude > 0.001f)
            {
                camTransform.rotation = Quaternion.LookRotation(toLookAt.normalized, Vector3.up);
            }

            gameplayCamera.Lens.FieldOfView = currentFov;
        }

        public void SetConfig(CameraConfig cfg) => config = cfg;
        public void SetTargetYaw(float yaw) => targetYaw = yaw;
        public void SetTargetPitch(float pitch) => targetPitch = pitch;
        public void SetTargetDistance(float dist) => targetDistance = dist;
        public void SetBaseFov(float fov) => baseFov = fov;
    }
}
