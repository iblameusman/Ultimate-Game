using System;
using UnityEngine;
using UnityEngine.InputSystem;
using DodgeGame.Data;
using DodgeGame.Player;

namespace DodgeGame.Core
{
    public class SlowMotionController : MonoBehaviour
    {
        public static SlowMotionController Instance { get; private set; }

        public const string TimeScaleRequesterId = "SlowMo";

        [Header("Configuration")]
        [Tooltip("Slow motion configuration parameters asset.")]
        [SerializeField] private SlowMotionConfig config;

        [Header("References")]
        [Tooltip("Player SlowMotionMeter component.")]
        [SerializeField] private SlowMotionMeter meter;

        private PlayerInputActions inputActions;
        private bool isSlowMoActive = false;
        private float slowMoTimer = 0f;

        public event Action OnSlowMoStart;
        public event Action OnSlowMoEnd;

        public bool IsSlowMoActive => isSlowMoActive;
        public float TimeRemaining => Mathf.Max(0f, slowMoTimer);
        public float Duration => config != null ? config.Duration : 3.0f;
        public SlowMotionConfig Config => config;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            ResolveReferences();
            InitializeInput();
        }

        private void OnEnable()
        {
            ResolveReferences();
            InitializeInput();

            if (inputActions != null)
            {
                inputActions.Player.Enable();
                inputActions.Player.SlowMotion.performed += HandleSlowMotionInput;
            }

            GameEvents.OnPlayerHit += HandlePlayerHit;
            GameEvents.OnGameOver += HandleGameOver;
        }

        private void OnDisable()
        {
            if (inputActions != null)
            {
                inputActions.Player.SlowMotion.performed -= HandleSlowMotionInput;
                inputActions.Player.Disable();
            }

            GameEvents.OnPlayerHit -= HandlePlayerHit;
            GameEvents.OnGameOver -= HandleGameOver;

            if (isSlowMoActive)
            {
                EndSlowMo();
            }
        }

        private void OnDestroy()
        {
            if (inputActions != null)
            {
                inputActions.Player.SlowMotion.performed -= HandleSlowMotionInput;
                inputActions.Player.Disable();
                inputActions.Dispose();
                inputActions = null;
            }

            if (isSlowMoActive)
            {
                EndSlowMo();
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void ResolveReferences()
        {
            if (config == null)
            {
                config = Resources.Load<SlowMotionConfig>("SO_SlowMotionConfig") ?? Resources.Load<SlowMotionConfig>("DefaultSlowMotionConfig");
#if UNITY_EDITOR
                if (config == null)
                {
                    config = UnityEditor.AssetDatabase.LoadAssetAtPath<SlowMotionConfig>("Assets/ScriptableObjects/SO_SlowMotionConfig.asset")
                          ?? UnityEditor.AssetDatabase.LoadAssetAtPath<SlowMotionConfig>("Assets/ScriptableObjects/DefaultSlowMotionConfig.asset");
                }
#endif
            }

            if (meter == null)
            {
                meter = FindAnyObjectByType<SlowMotionMeter>();
            }
        }

        private void InitializeInput()
        {
            if (inputActions == null)
            {
                inputActions = new PlayerInputActions();
            }
        }

        private void HandleSlowMotionInput(InputAction.CallbackContext context)
        {
            TryActivateSlowMo();
        }

        private void Update()
        {
            if (!isSlowMoActive)
            {
                bool manualE = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
                bool manualY = Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame;
                if (manualE || manualY)
                {
                    TryActivateSlowMo();
                }
            }

            if (isSlowMoActive)
            {
                if (PauseController.Instance == null || !PauseController.Instance.IsPaused)
                {
                    slowMoTimer -= Time.unscaledDeltaTime;
                    if (slowMoTimer <= 0f)
                    {
                        EndSlowMo();
                    }
                }
            }
        }

        public bool TryActivateSlowMo()
        {
            if (isSlowMoActive) return false;

            ResolveReferences();
            if (meter == null || !meter.IsReady) return false;

            if (PauseController.Instance != null && PauseController.Instance.IsPaused) return false;
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return false;

            if (!meter.ConsumeMeter()) return false;

            isSlowMoActive = true;
            slowMoTimer = Duration;

            float scale = config != null ? config.TimeScale : 0.4f;
            TimeScaleController.Instance?.RequestTimeScale(scale, Duration, TimeScaleRequesterId);

            AudioManager.Instance?.PlaySlowMoStart();
            GameEvents.TriggerSkillUsed(SkillType.SlowMotion);

            OnSlowMoStart?.Invoke();
            return true;
        }

        public void EndSlowMo()
        {
            if (!isSlowMoActive) return;

            isSlowMoActive = false;
            slowMoTimer = 0f;

            TimeScaleController.Instance?.ReleaseTimeScale(TimeScaleRequesterId);
            AudioManager.Instance?.PlaySlowMoEnd();

            OnSlowMoEnd?.Invoke();
        }

        private void HandlePlayerHit(int livesRemaining)
        {
            if (isSlowMoActive)
            {
                EndSlowMo();
            }
        }

        private void HandleGameOver(int finalScore)
        {
            if (isSlowMoActive)
            {
                EndSlowMo();
            }
        }
    }
}
