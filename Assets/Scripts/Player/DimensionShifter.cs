using System;
using UnityEngine;
using UnityEngine.InputSystem;
using DodgeGame.Core;
using DodgeGame.Data;

namespace DodgeGame.Player
{
    public class DimensionShifter : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Dimension shift tuning parameters asset.")]
        [SerializeField] private DimensionConfig config;

        [Header("References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private DashController dashController;

        private PlayerInputActions inputActions;
        private bool isShifted;
        private float shiftTimer;
        private float cooldownTimer;

        public event Action OnShiftStart;
        public event Action OnShiftEnd;

        public bool IsShifted => isShifted;
        public float CooldownRemaining => Mathf.Max(0f, cooldownTimer);
        public float CooldownDuration => config != null ? config.ShiftCooldown : 5.0f;
        public float CooldownProgress => CooldownDuration > 0f ? Mathf.Clamp01(1f - (CooldownRemaining / CooldownDuration)) : 1f;
        public bool CanShift => !isShifted 
            && cooldownTimer <= 0f 
            && (dashController == null || !dashController.IsDashing) 
            && (playerController == null || playerController.CurrentSkillLock != SkillLockState.Dashing);
        public DimensionConfig Config => config;

        private void Awake()
        {
            if (playerController == null) playerController = GetComponent<PlayerController>();
            if (dashController == null) dashController = GetComponent<DashController>();
            InitializeInput();
        }

        private void OnEnable()
        {
            InitializeInput();
            if (inputActions != null)
            {
                inputActions.Player.Enable();
                inputActions.Player.DimensionShift.performed += HandleShiftInput;
            }
        }

        private void InitializeInput()
        {
            if (inputActions == null)
            {
                inputActions = new PlayerInputActions();
            }
        }

        private void OnDisable()
        {
            if (inputActions != null)
            {
                inputActions.Player.DimensionShift.performed -= HandleShiftInput;
                inputActions.Player.Disable();
            }

            if (isShifted)
            {
                isShifted = false;
                OnShiftEnd?.Invoke();
            }
        }

        private void OnDestroy()
        {
            if (inputActions != null)
            {
                inputActions.Player.DimensionShift.performed -= HandleShiftInput;
                inputActions.Player.Disable();
                inputActions.Dispose();
                inputActions = null;
            }
        }

        private void Update()
        {
            if (isShifted)
            {
                shiftTimer -= Time.deltaTime;
                if (shiftTimer <= 0f)
                {
                    EndShift();
                }
            }
            else if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.unscaledDeltaTime;
                if (cooldownTimer < 0f)
                {
                    cooldownTimer = 0f;
                }
            }
        }

        private void HandleShiftInput(InputAction.CallbackContext context)
        {
            TryTriggerShift();
        }

        public bool TryTriggerShift()
        {
            if (!CanShift)
            {
                return false;
            }

            isShifted = true;
            shiftTimer = config != null ? config.ShiftDuration : 1.5f;

            AudioManager.Instance?.PlayDimensionShiftStart();
            OnShiftStart?.Invoke();
            GameEvents.TriggerSkillUsed(SkillType.Phase);
            return true;
        }

        private void EndShift()
        {
            if (!isShifted) return;

            isShifted = false;
            cooldownTimer = config != null ? config.ShiftCooldown : 5.0f;

            AudioManager.Instance?.PlayDimensionShiftEnd();
            OnShiftEnd?.Invoke();
        }
    }
}
