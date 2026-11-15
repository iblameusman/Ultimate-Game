using UnityEngine;
using UnityEngine.InputSystem;
using DodgeGame.Data;
using DodgeGame.Core;
using DodgeGame.Balls;

namespace DodgeGame.Player
{
    public enum SkillLockState
    {
        None,
        Phasing,
        Dashing
    }

    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("ScriptableObject containing tunable movement and jump parameters.")]
        [SerializeField] private PlayerConfig config;

        [Header("Abilities")]
        [Tooltip("Reference to the player DimensionShifter component.")]
        [SerializeField] private DimensionShifter dimensionShifter;

        [Tooltip("Reference to the player DashController component.")]
        [SerializeField] private DashController dashController;

        [Header("Visual Feedback (Optional)")]
        [Tooltip("Optional visual mesh transform to rotate toward movement direction. If null, rotates Rigidbody directly.")]
        [SerializeField] private Transform visualTransform;

        private Rigidbody rb;
        private CapsuleCollider capsuleCollider;
        private PlayerHealth playerHealth;
        private PlayerInputActions inputActions;

        private Vector2 moveInput;
        private bool jumpButtonHeld;
        private float jumpBufferTimer;

        private bool isGrounded;
        private float coyoteTimer;
        private float jumpCooldownTimer;
        private RaycastHit currentGroundHit;
        private bool hasGroundHit;

        private float activeSlowMultiplier = 1f;
        private float slowDurationTimer = 0f;
        private int gooOverlapCount = 0;
        private float gooSlowMultiplier = 1f;

        public bool IsGrounded => isGrounded;
        public Vector3 Velocity => rb != null ? rb.linearVelocity : Vector3.zero;
        public bool IsSlowed => slowDurationTimer > 0f || gooOverlapCount > 0;
        public bool IsDashing => dashController != null && dashController.IsDashing;
        public float DashCooldownProgress => dashController != null ? dashController.CooldownProgress : 1f;
        public SkillLockState CurrentSkillLock { get; private set; } = SkillLockState.None;
        public Vector2 MoveInput => moveInput;
        public event System.Action OnJump;
        public event System.Action OnLanded;

        public float CurrentMoveSpeed
        {
            get
            {
                float baseSpeed = config != null ? config.MoveSpeed : 11f;
                if (slowDurationTimer > 0f) baseSpeed *= activeSlowMultiplier;
                if (gooOverlapCount > 0) baseSpeed *= gooSlowMultiplier;
                if (dimensionShifter != null && dimensionShifter.IsShifted)
                {
                    float shiftMult = dimensionShifter.Config != null ? dimensionShifter.Config.ShiftMoveMultiplier : 0.7f;
                    baseSpeed *= shiftMult;
                }

                if (Time.timeScale > 0.01f && Time.timeScale < 0.99f)
                {
                    baseSpeed /= Time.timeScale;
                }

                return baseSpeed;
            }
        }

        private void Awake()
        {
            EnsureComponents();
            InitializeInput();
        }

        private void Start()
        {
            EnsureComponents();
            SubscribeEvents();
        }

        private void OnEnable()
        {
            EnsureComponents();
            InitializeInput();

            if (inputActions != null)
            {
                inputActions.Player.Enable();
                inputActions.Player.Jump.performed += OnJumpPerformed;
            }

            SubscribeEvents();
        }

        private void OnDisable()
        {
            if (inputActions != null)
            {
                inputActions.Player.Jump.performed -= OnJumpPerformed;
                inputActions.Player.Disable();
            }

            UnsubscribeEvents();

            CurrentSkillLock = SkillLockState.None;
            gameObject.layer = LayerMask.NameToLayer("Player");
        }

        private void OnDestroy()
        {
            if (inputActions != null)
            {
                inputActions.Player.Jump.performed -= OnJumpPerformed;
                inputActions.Player.Disable();
                inputActions.Dispose();
                inputActions = null;
            }

            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            if (dimensionShifter != null)
            {
                dimensionShifter.OnShiftStart -= HandleShiftStart;
                dimensionShifter.OnShiftStart += HandleShiftStart;
                dimensionShifter.OnShiftEnd -= HandleShiftEnd;
                dimensionShifter.OnShiftEnd += HandleShiftEnd;
            }

            if (dashController != null)
            {
                dashController.OnDashStart -= HandleDashStart;
                dashController.OnDashStart += HandleDashStart;
                dashController.OnDashEnd -= HandleDashEnd;
                dashController.OnDashEnd += HandleDashEnd;
            }
        }

        private void UnsubscribeEvents()
        {
            if (dimensionShifter != null)
            {
                dimensionShifter.OnShiftStart -= HandleShiftStart;
                dimensionShifter.OnShiftEnd -= HandleShiftEnd;
            }

            if (dashController != null)
            {
                dashController.OnDashStart -= HandleDashStart;
                dashController.OnDashEnd -= HandleDashEnd;
            }
        }

        private void HandleShiftStart()
        {
            CurrentSkillLock = SkillLockState.Phasing;
            int shiftedLayer = LayerMask.NameToLayer("PlayerShifted");
            if (shiftedLayer >= 0)
            {
                gameObject.layer = shiftedLayer;
            }
        }

        private void HandleShiftEnd()
        {
            if (CurrentSkillLock == SkillLockState.Phasing)
            {
                CurrentSkillLock = SkillLockState.None;
            }
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer >= 0)
            {
                gameObject.layer = playerLayer;
            }
        }

        private void HandleDashStart()
        {
            CurrentSkillLock = SkillLockState.Dashing;
        }

        private void HandleDashEnd()
        {
            if (CurrentSkillLock == SkillLockState.Dashing)
            {
                CurrentSkillLock = SkillLockState.None;
            }
        }

        private void InitializeInput()
        {
            if (inputActions == null)
            {
                inputActions = new PlayerInputActions();
            }
        }

        public void EnsureComponents()
        {
            if (rb == null) rb = GetComponent<Rigidbody>();
            if (capsuleCollider == null) capsuleCollider = GetComponent<CapsuleCollider>();
            if (playerHealth == null) playerHealth = GetComponent<PlayerHealth>();
            if (dimensionShifter == null) dimensionShifter = GetComponent<DimensionShifter>();
            if (dashController == null) dashController = GetComponent<DashController>();
            if (GetComponent<NearMissDetector>() == null) gameObject.AddComponent<NearMissDetector>();

            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }

            if (capsuleCollider != null && capsuleCollider.sharedMaterial == null)
            {
                var frictionless = new PhysicsMaterial("Player_Frictionless")
                {
                    dynamicFriction = 0f,
                    staticFriction = 0f,
                    bounciness = 0f,
                    frictionCombine = PhysicsMaterialCombine.Minimum,
                    bounceCombine = PhysicsMaterialCombine.Minimum
                };
                capsuleCollider.sharedMaterial = frictionless;
            }
        }

        private void Update()
        {
            if (slowDurationTimer > 0f)
            {
                slowDurationTimer -= Time.deltaTime;
            }

            if (inputActions == null) return;

            if (IsDashing)
            {
                moveInput = Vector2.zero;
                return;
            }

            if (playerHealth != null && playerHealth.IsDead)
            {
                moveInput = Vector2.zero;
                jumpButtonHeld = false;
                jumpBufferTimer = 0f;
                return;
            }

            moveInput = inputActions.Player.Move.ReadValue<Vector2>();

            if (inputActions.Player.Jump.WasPressedThisFrame())
            {
                QueueJump();
            }

            jumpButtonHeld = inputActions.Player.Jump.IsPressed();

            if (jumpBufferTimer > 0f)
            {
                jumpBufferTimer -= Time.deltaTime;
            }
        }

        private void FixedUpdate()
        {
            EnsureComponents();
            if (rb == null) return;

            if (IsDashing) return;

            UpdateTimers();
            UpdateGroundStatus();
            ApplyJump();
            ApplyVerticalGravityModifiers();
            ApplyHorizontalMovement();
            ApplyRotation();
            EnforceArenaBoundsSafety();
        }

        private void EnforceArenaBoundsSafety()
        {
            Vector3 pos = rb != null ? rb.position : transform.position;
            bool clamped = false;
            float limitX = BaseBall.ArenaLimitX - 0.4f;
            float limitZ = BaseBall.ArenaLimitZ - 0.4f;
            if (pos.x > limitX) { pos.x = limitX; clamped = true; }
            else if (pos.x < -limitX) { pos.x = -limitX; clamped = true; }
            if (pos.z > limitZ) { pos.z = limitZ; clamped = true; }
            else if (pos.z < -limitZ) { pos.z = -limitZ; clamped = true; }

            if (clamped)
            {
                if (rb != null) rb.position = pos;
                else transform.position = pos;
            }
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            QueueJump();
        }

        public void QueueJump()
        {
            float bufferTime = config != null ? config.JumpBufferDuration : 0.15f;
            jumpBufferTimer = bufferTime;
        }

        private void UpdateTimers()
        {
            if (jumpCooldownTimer > 0f)
            {
                jumpCooldownTimer -= Time.fixedDeltaTime;
            }
        }

        public void ApplySlow(float multiplier, float duration)
        {
            activeSlowMultiplier = Mathf.Clamp(multiplier, 0.05f, 1f);
            slowDurationTimer = Mathf.Max(slowDurationTimer, duration);
        }

        public void SetGooSlow(bool inside, float multiplier = 0.5f)
        {
            if (inside)
            {
                gooOverlapCount++;
                gooSlowMultiplier = Mathf.Clamp(multiplier, 0.05f, 1f);
            }
            else
            {
                gooOverlapCount = Mathf.Max(0, gooOverlapCount - 1);
            }
        }

        private void UpdateGroundStatus()
        {
            if (capsuleCollider == null) return;
            bool wasGrounded = isGrounded;

            if (jumpCooldownTimer > 0f)
            {
                isGrounded = false;
                coyoteTimer = 0f;
                hasGroundHit = false;
                return;
            }

            float radius = capsuleCollider.radius * 0.9f;
            float checkDist = config != null ? config.GroundCheckDistance : 0.15f;
            float maxSlope = config != null ? config.MaxSlopeAngle : 50f;

            Vector3 center = transform.position + capsuleCollider.center;
            float halfHeight = Mathf.Max(capsuleCollider.height * 0.5f, radius);
            Vector3 castOrigin = center + (Vector3.down * (halfHeight - radius));

            int playerLayerMask = 1 << gameObject.layer;
            int groundMask = (config != null && config.GroundLayerMask.value != 0)
                ? (config.GroundLayerMask.value & ~playerLayerMask)
                : ~LayerMask.GetMask("Player", "Ignore Raycast", "TransparentFX");

            bool hit = Physics.SphereCast(
                castOrigin,
                radius,
                Vector3.down,
                out RaycastHit hitInfo,
                checkDist + 0.05f,
                groundMask,
                QueryTriggerInteraction.Ignore
            );

            if (!hit)
            {
                hit = Physics.Raycast(
                    center,
                    Vector3.down,
                    out hitInfo,
                    halfHeight + checkDist,
                    groundMask,
                    QueryTriggerInteraction.Ignore
                );
            }

            if (hit && hitInfo.collider != capsuleCollider && !hitInfo.collider.transform.IsChildOf(transform))
            {
                float slopeAngle = Vector3.Angle(Vector3.up, hitInfo.normal);
                if (slopeAngle <= maxSlope)
                {
                    isGrounded = true;
                    coyoteTimer = config != null ? config.CoyoteTimeDuration : 0.15f;
                    currentGroundHit = hitInfo;
                    hasGroundHit = true;
                    if (!wasGrounded)
                    {
                        OnLanded?.Invoke();
                    }
                    return;
                }
            }

            isGrounded = false;
            coyoteTimer -= Time.fixedDeltaTime;
            hasGroundHit = false;
        }

        private void ApplyJump()
        {
            if (jumpBufferTimer > 0f && coyoteTimer > 0f)
            {
                float jumpVel = config != null ? config.JumpForce : 8.5f;

                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpVel, rb.linearVelocity.z);
                AudioManager.Instance?.PlayJump();
                OnJump?.Invoke();

                jumpBufferTimer = 0f;
                coyoteTimer = 0f;
                jumpCooldownTimer = 0.15f;
                isGrounded = false;
                hasGroundHit = false;
            }
        }

        private void ApplyVerticalGravityModifiers()
        {
            float fallMult = config != null ? config.FallGravityMultiplier : 2.2f;
            float lowJumpMult = config != null ? config.LowJumpGravityMultiplier : 2.5f;
            float maxFallSpeed = config != null ? config.MaxFallSpeed : 25f;

            if (rb.linearVelocity.y < 0f)
            {
                float extraGravity = Physics.gravity.y * (fallMult - 1f) * Time.fixedDeltaTime;
                rb.linearVelocity += Vector3.up * extraGravity;
            }
            else if (rb.linearVelocity.y > 0f && !jumpButtonHeld)
            {
                float extraGravity = Physics.gravity.y * (lowJumpMult - 1f) * Time.fixedDeltaTime;
                rb.linearVelocity += Vector3.up * extraGravity;
            }

            if (rb.linearVelocity.y < -maxFallSpeed)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, -maxFallSpeed, rb.linearVelocity.z);
            }
        }

        private void ApplyHorizontalMovement()
        {
            if (IsDashing) return;

            Vector3 desiredDirection = GetCameraRelativeDirection(moveInput);

            if (isGrounded && hasGroundHit)
            {
                float slopeAngle = Vector3.Angle(Vector3.up, currentGroundHit.normal);
                if (slopeAngle > 0.1f)
                {
                    desiredDirection = Vector3.ProjectOnPlane(desiredDirection, currentGroundHit.normal).normalized;
                }
            }

            float targetSpeed = CurrentMoveSpeed;
            Vector3 targetHorizontalVelocity = desiredDirection * targetSpeed;

            float accel;
            bool isMoving = moveInput.sqrMagnitude > 0.001f;

            if (isGrounded)
            {
                accel = isMoving
                    ? (config != null ? config.Acceleration : 60f)
                    : (config != null ? config.Deceleration : 50f);
            }
            else
            {
                accel = isMoving
                    ? (config != null ? config.AirAcceleration : 35f)
                    : (config != null ? config.AirDeceleration : 10f);
            }

            if (Time.timeScale > 0.01f && Time.timeScale < 0.99f)
            {
                accel /= Time.timeScale;
            }

            Vector3 currentHorizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            Vector3 newHorizontalVelocity = Vector3.MoveTowards(
                currentHorizontalVelocity,
                targetHorizontalVelocity,
                accel * Time.fixedDeltaTime
            );

            rb.linearVelocity = new Vector3(newHorizontalVelocity.x, rb.linearVelocity.y, newHorizontalVelocity.z);
        }

        public Vector3 GetCameraRelativeDirection(Vector2 input)
        {
            if (input.sqrMagnitude < 0.0001f) return Vector3.zero;

            Camera cam = Camera.main;
            Vector3 camForward, camRight;

            if (cam != null)
            {
                camForward = cam.transform.forward;
                camRight = cam.transform.right;
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();
            }
            else
            {
                camForward = Vector3.forward;
                camRight = Vector3.right;
            }

            Vector3 dir = (camForward * input.y) + (camRight * input.x);
            if (dir.sqrMagnitude > 1f)
            {
                dir.Normalize();
            }

            return dir;
        }

        private void ApplyRotation()
        {
            Vector3 horizontalMove = new Vector3(moveInput.x, 0f, moveInput.y);
            if (horizontalMove.sqrMagnitude < 0.001f) return;

            Vector3 lookDirection = GetCameraRelativeDirection(moveInput);
            lookDirection.y = 0f;

            if (lookDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
                const float turnSpeed = 720f;

                if (visualTransform != null)
                {
                    visualTransform.rotation = Quaternion.RotateTowards(
                        visualTransform.rotation,
                        targetRotation,
                        turnSpeed * Time.unscaledDeltaTime
                    );
                }
                else
                {
                    rb.MoveRotation(Quaternion.RotateTowards(
                        rb.rotation,
                        targetRotation,
                        turnSpeed * Time.unscaledDeltaTime
                    ));
                }
            }
        }

        public void AddKnockback(Vector3 force, ForceMode mode = ForceMode.Impulse)
        {
            if (rb != null)
            {
                rb.AddForce(force, mode);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (capsuleCollider == null) capsuleCollider = GetComponent<CapsuleCollider>();
            if (capsuleCollider == null) return;

            Gizmos.color = isGrounded ? Color.green : Color.red;

            float radius = capsuleCollider.radius * 0.9f;
            float checkDist = config != null ? config.GroundCheckDistance : 0.15f;
            Vector3 center = transform.position + capsuleCollider.center;
            float halfHeight = Mathf.Max(capsuleCollider.height * 0.5f, radius);
            Vector3 castOrigin = center + (Vector3.down * (halfHeight - radius));
            Vector3 castEnd = castOrigin + (Vector3.down * checkDist);

            Gizmos.DrawWireSphere(castOrigin, radius);
            Gizmos.DrawWireSphere(castEnd, radius);
            Gizmos.DrawLine(castOrigin, castEnd);

            if (hasGroundHit)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(currentGroundHit.point, currentGroundHit.normal);
            }
        }
    }
}
