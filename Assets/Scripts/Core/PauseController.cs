using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DodgeGame.Core
{
    public class PauseController : MonoBehaviour
    {
        public const string PAUSE_REQUESTER_ID = "Pause";

        public static PauseController Instance { get; private set; }

        [Header("State")]
        [SerializeField] private bool isPaused = false;

        public event Action<bool> OnPauseChanged;
        public static event Action<bool> OnGlobalPauseChanged;

        public bool IsPaused => isPaused;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (isPaused && TimeScaleController.Instance != null)
            {
                TimeScaleController.Instance.ReleaseTimeScale(PAUSE_REQUESTER_ID);
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.GameOver)
            {
                return;
            }

            bool toggleRequested = false;

            var keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame))
            {
                toggleRequested = true;
            }

            var gamepad = Gamepad.current;
            if (gamepad != null && gamepad.startButton.wasPressedThisFrame)
            {
                toggleRequested = true;
            }

            if (toggleRequested)
            {
                TogglePause();
            }
        }

        public void TogglePause()
        {
            SetPaused(!isPaused);
        }

        public void SetPaused(bool paused)
        {
            if (isPaused == paused) return;

            if (paused)
            {
                if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing && !isPaused)
                {
                    return;
                }

                isPaused = true;
                if (TimeScaleController.Instance != null)
                {
                    TimeScaleController.Instance.RequestTimeScale(0f, 0f, PAUSE_REQUESTER_ID);
                }
                OnPauseChanged?.Invoke(true);
                OnGlobalPauseChanged?.Invoke(true);
            }
            else
            {
                isPaused = false;
                if (TimeScaleController.Instance != null)
                {
                    TimeScaleController.Instance.ReleaseTimeScale(PAUSE_REQUESTER_ID);
                }
                OnPauseChanged?.Invoke(false);
                OnGlobalPauseChanged?.Invoke(false);
            }
        }
    }
}
