using System;
using UnityEngine;
using UnityEngine.InputSystem;
using DodgeGame.Core;
using DodgeGame.Data;

namespace DodgeGame.Player
{
    public enum DashState
    {
        Ready,
        Dashing,
        Cooldown
    }

    [RequireComponent(typeof(Rigidbody))]
    public class DashController : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Tuning parameters asset for the Dash ability.")]
        [SerializeField] private DashConfig config;

        [Header("References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private DimensionShifter dimensionShifter;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private Rigidbody rb;

        private DashState currentState = DashState.Ready;
        private float dashTimer = 0f;
        private float cooldownTimer = 0f;
        private Vector3 dashDirection = Vector3.forward;
        private PlayerInputActions inputActions;

        public event Action OnDashStart;
        public event Action OnDashEnd;

        public DashState CurrentState => currentState;
        public bool IsDashing => currentState == DashState.Dashing;
        public bool IsReady => currentState == DashState.Ready;

        public float CooldownRemaining
        {
            get
            {
                float totalCooldown = config != null ? config.Cooldown : 2.5f;
                return totalCooldown > 0f ? Mathf.Clamp01(cooldownTimer / totalCooldown) : 0f;
            }
        }

        public float CooldownTimeRemaining => Mathf.Max(0f, cooldownTimer);
        public float CooldownProgress => 1f - CooldownRemaining;
        public DashConfig Config => config;
        public Vector3 DashDirection => dashDirection;

        private void Awake()
        {
            if (rb == null) rb = GetComponent<Rigidbody>();
            if (playerController == null) playerController = GetComponent<PlayerController>();
            if (dimensionShifter == null) dimensionShifter = GetComponent<DimensionShifter>();
            if (playerHealth == null) playerHealth = GetComponent<PlayerHealth>();

            ResolveConfig();
            InitializeInput();
        }

        private void OnEnable()
        {
            InitializeInput();
            if (inputActions != null)
            {
                inputActions.Player.Enable();
                inputActions.Player.Dash.performed += HandleDashInput;
            }
        }

        private void OnDisable()
        {
            if (inputActions != null)
            {
                inputActions.Player.Dash.performed -= HandleDashInput;
                inputActions.Player.Disable();
            }

            if (currentState == DashState.Dashing)
            {
                EndDash();
            }
        }

        private void OnDestroy()
        {
            if (inputActions != null)
            {
                inputActions.Player.Dash.performed -= HandleDashInput;
                inputActions.Player.Disable();
                inputActions.Dispose();
                inputActions = null;
            }
        }

        private void ResolveConfig()
        {
            if (config == null)
            {
                config = Resources.Load<DashConfig>("SO_DashConfig") ?? Resources.Load<DashConfig>("DefaultDashConfig");
#if UNITY_EDITOR
                if (config == null)
                {
                    config = UnityEditor.AssetDatabase.LoadAssetAtPath<DashConfig>("Assets/ScriptableObjects/SO_DashConfig.asset")
                          ?? UnityEditor.AssetDatabase.LoadAssetAtPath<DashConfig>("Assets/ScriptableObjects/DefaultDashConfig.asset");
                }
#endif
            }
        }

        private void InitializeInput()
        {
            if (inputActions == null)
            {
                inputActions = new PlayerInputActions();
            }
        }

        private void HandleDashInput(InputAction.CallbackContext context)
        {
            TryDash();
        }

        private void Update()
        {
            if (currentState == DashState.Ready)
            {
                bool manualShift = Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame;
                bool manualGamepadB = Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame;
                if (manualShift || manualGamepadB)
                {
                    TryDash();
                }
            }

            if (currentState == DashState.Cooldown)
            {
                if (cooldownTimer > 0f)
                {
                    cooldownTimer -= Time.unscaledDeltaTime;
                    if (cooldownTimer <= 0f)
                    {
                        cooldownTimer = 0f;
                        currentState = DashState.Ready;
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            if (currentState != DashState.Dashing || rb == null) return;

            ResolveConfig();
            float duration = config != null ? config.Duration : 0.15f;
            float baseSpeed = config != null ? config.BaseSpeed : (3.5f / 0.15f);

            float remaining = Mathf.Max(0f, duration - dashTimer);
            float step = Mathf.Min(Time.fixedDeltaTime, remaining);
            float ratio = Time.fixedDeltaTime > 0f ? (step / Time.fixedDeltaTime) : 1f;

            float t = duration > 0f ? Mathf.Clamp01(dashTimer / duration) : 1f;
            float speedFactor = (config != null && config.EaseCurve != null && config.EaseCurve.length > 0)
                ? config.EaseCurve.Evaluate(t)
                : 1f;

            float currentSpeed = baseSpeed * speedFactor * ratio;

            rb.linearVelocity = new Vector3(dashDirection.x * currentSpeed, rb.linearVelocity.y, dashDirection.z * currentSpeed);

            dashTimer += Time.fixedDeltaTime;
            if (dashTimer >= duration)
            {
                EndDash();
            }
        }

        public bool TryDash()
        {
            if (currentState != DashState.Ready) return false;

            if (dimensionShifter != null && dimensionShifter.IsShifted)
            {
                return false;
            }

            if (playerController != null && playerController.CurrentSkillLock == SkillLockState.Phasing)
            {
                return false;
            }

            if (playerHealth != null && playerHealth.IsDead) return false;
            if (PauseController.Instance != null && PauseController.Instance.IsPaused) return false;

            ResolveConfig();
            DetermineDashDirection();

            currentState = DashState.Dashing;
            dashTimer = 0f;

            if (dashDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(dashDirection);
            }

            AudioManager.Instance?.PlayDash();
            GameEvents.TriggerSkillUsed(SkillType.Dash);

            OnDashStart?.Invoke();
            return true;
        }

        private void DetermineDashDirection()
        {
            Vector2 moveInput = playerController != null ? playerController.MoveInput : Vector2.zero;

            if (moveInput.sqrMagnitude > 0.01f && playerController != null)
            {
                Vector3 camRel = playerController.GetCameraRelativeDirection(moveInput);
                camRel.y = 0f;
                if (camRel.sqrMagnitude > 0.001f)
                {
                    dashDirection = camRel.normalized;
                    return;
                }
            }

            Vector3 forward = transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude > 0.001f)
            {
                dashDirection = forward.normalized;
            }
            else
            {
                dashDirection = Vector3.forward;
            }
        }

        private void EndDash()
        {
            currentState = DashState.Cooldown;
            dashTimer = 0f;
            cooldownTimer = config != null ? config.Cooldown : 2.5f;

            OnDashEnd?.Invoke();
        }
    }
}
