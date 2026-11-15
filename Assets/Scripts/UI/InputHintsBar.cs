using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace DodgeGame.UI
{
    public class InputHintsBar : MonoBehaviour
    {
        [Header("Target References")]
        [SerializeField] private TMP_Text hintText;

        [Header("State")]
        [SerializeField] private bool isGamepadActive = false;
        [SerializeField] private bool isPanelOpen = false;

        [Header("Debouncing")]
        [SerializeField] private float switchDebounceTime = 0.05f;
        private float lastSwitchTime = -1f;

        public bool IsGamepadActive => isGamepadActive;
        public bool IsPanelOpen => isPanelOpen;

        public void SetGamepadActive(bool active)
        {
            if (isGamepadActive == active) return;
            if (Time.unscaledTime - lastSwitchTime < switchDebounceTime) return;

            lastSwitchTime = Time.unscaledTime;
            isGamepadActive = active;
            UpdateHintsDisplay();
        }

        private void Awake()
        {
            if (hintText == null) hintText = GetComponent<TMP_Text>();
            UpdateHintsDisplay();
        }

        private void OnEnable()
        {
            InputSystem.onActionChange += HandleActionChange;
            UpdateHintsDisplay();
        }

        private void OnDisable()
        {
            InputSystem.onActionChange -= HandleActionChange;
        }

        private void Update()
        {
            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                if (gamepad.buttonSouth.wasPressedThisFrame ||
                    gamepad.buttonEast.wasPressedThisFrame ||
                    gamepad.dpad.left.wasPressedThisFrame ||
                    gamepad.dpad.right.wasPressedThisFrame ||
                    gamepad.dpad.up.wasPressedThisFrame ||
                    gamepad.dpad.down.wasPressedThisFrame ||
                    gamepad.leftStick.ReadValue().sqrMagnitude > 0.2f)
                {
                    SetGamepadActive(true);
                }
            }

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.anyKey.wasPressedThisFrame)
            {
                SetGamepadActive(false);
            }

            var mouse = Mouse.current;
            if (mouse != null && (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame))
            {
                SetGamepadActive(false);
            }
        }

        private void HandleActionChange(object obj, InputActionChange change)
        {
            if (change == InputActionChange.ActionPerformed)
            {
                var action = obj as InputAction;
                if (action != null && action.activeControl != null)
                {
                    bool gamepad = action.activeControl.device is Gamepad;
                    SetGamepadActive(gamepad);
                }
            }
        }

        public void SetPanelOpen(bool open)
        {
            isPanelOpen = open;
            UpdateHintsDisplay();
        }

        private void UpdateHintsDisplay()
        {
            if (hintText == null) return;

            if (isGamepadActive)
            {
                hintText.text = isPanelOpen
                    ? "[A] Select    [B] Back"
                    : "[A] Select    [B] Quit  |  R-Stick Orbit  LB/RB Zoom  R3 Reset";
            }
            else
            {
                hintText.text = isPanelOpen
                    ? "[Enter] Select    [Esc] Back"
                    : "[Enter] Select    [Esc] Quit  |  Hold RMB to orbit camera  Scroll to zoom  Home to reset view";
            }
        }
    }
}
