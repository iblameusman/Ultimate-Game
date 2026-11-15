using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using DodgeGame.Core;
using DodgeGame.Data;
using DodgeGame.Player;

namespace DodgeGame.UI
{
    public enum SkillCardType
    {
        Phase,
        Dash,
        SlowMotion
    }

    public enum SkillCardState
    {
        Ready,
        Active,
        Cooldown,
        Locked
    }

    public class SkillCardHud : MonoBehaviour
    {
        [Header("Card Identity")]
        [SerializeField] private SkillCardType skillType = SkillCardType.Phase;

        [Header("UI References")]
        [SerializeField] private Image cardBackground;
        [SerializeField] private Image borderHighlight;
        [SerializeField] private Image cooldownFill;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI keyChipText;
        [SerializeField] private TextMeshProUGUI skillNameText;
        [SerializeField] private TextMeshProUGUI stateLineText;

        [Header("Slow Motion Charge Bar (SlowMotion only)")]
        [SerializeField] private GameObject chargeBarContainer;
        [SerializeField] private Image chargeBarFill;

        [Header("Theme")]
        [SerializeField] private HudTheme theme;

        private SkillCardState currentState = SkillCardState.Ready;
        private bool isGamepadActive = false;
        private TweenHandle chargeTween;
        private TweenHandle pulseTween;
        private float currentDisplayedCharge = 0f;

        private DimensionShifter dimensionShifter;
        private DashController dashController;
        private SlowMotionController slowMoController;
        private SlowMotionMeter slowMoMeter;

        public SkillCardType SkillType { get => skillType; set => skillType = value; }
        public SkillCardState CurrentState => currentState;
        public TextMeshProUGUI KeyChipText => keyChipText;
        public TextMeshProUGUI SkillNameText => skillNameText;
        public TextMeshProUGUI StateLineText => stateLineText;
        public Image CooldownFill => cooldownFill;
        public Image ChargeBarFill => chargeBarFill;
        public CanvasGroup CardCanvasGroup => canvasGroup;
        public HudTheme Theme { get => theme; set { theme = value; RefreshVisualState(); } }

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeEvents();
            InputSystem.onActionChange += HandleActionChange;
            RefreshVisualState();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
            InputSystem.onActionChange -= HandleActionChange;
            chargeTween?.Stop();
            pulseTween?.Stop();
        }

        private void Start()
        {
            ResolveReferences();
            UpdateKeyChip();
            RefreshVisualState();
        }

        private void ResolveReferences()
        {
            if (theme == null)
            {
                theme = Resources.Load<HudTheme>("HudTheme");
#if UNITY_EDITOR
                if (theme == null)
                {
                    theme = UnityEditor.AssetDatabase.LoadAssetAtPath<HudTheme>("Assets/Scripts/Data/HudTheme.asset");
                }
#endif
            }

            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (cardBackground == null) cardBackground = transform.Find("Background")?.GetComponent<Image>();
            if (borderHighlight == null) borderHighlight = transform.Find("BorderHighlight")?.GetComponent<Image>();
            if (cooldownFill == null) cooldownFill = transform.Find("CooldownFill")?.GetComponent<Image>();
            if (keyChipText == null) keyChipText = transform.Find("KeyChip/Text")?.GetComponent<TextMeshProUGUI>();
            if (skillNameText == null) skillNameText = transform.Find("SkillName")?.GetComponent<TextMeshProUGUI>();
            if (stateLineText == null) stateLineText = transform.Find("StateLine")?.GetComponent<TextMeshProUGUI>();

            if (chargeBarContainer == null) chargeBarContainer = transform.Find("ChargeBar")?.gameObject;
            if (chargeBarFill == null && chargeBarContainer != null) chargeBarFill = chargeBarContainer.transform.Find("Fill")?.GetComponent<Image>();

            if (chargeBarContainer != null)
            {
                chargeBarContainer.SetActive(skillType == SkillCardType.SlowMotion);
            }

            if (dimensionShifter == null) dimensionShifter = FindAnyObjectByType<DimensionShifter>();
            if (dashController == null) dashController = FindAnyObjectByType<DashController>();
            if (slowMoController == null) slowMoController = FindAnyObjectByType<SlowMotionController>();
            if (slowMoMeter == null) slowMoMeter = FindAnyObjectByType<SlowMotionMeter>();
        }

        private void SubscribeEvents()
        {
            switch (skillType)
            {
                case SkillCardType.Phase:
                    if (dimensionShifter != null)
                    {
                        dimensionShifter.OnShiftStart += HandlePhaseStart;
                        dimensionShifter.OnShiftEnd += HandlePhaseEnd;
                    }
                    break;

                case SkillCardType.Dash:
                    if (dashController != null)
                    {
                        dashController.OnDashStart += HandleDashStart;
                        dashController.OnDashEnd += HandleDashEnd;
                    }
                    if (dimensionShifter != null)
                    {
                        dimensionShifter.OnShiftStart += HandlePhaseShiftActiveForDash;
                        dimensionShifter.OnShiftEnd += HandlePhaseShiftEndedForDash;
                    }
                    break;

                case SkillCardType.SlowMotion:
                    if (slowMoController != null)
                    {
                        slowMoController.OnSlowMoStart += HandleSlowMoStart;
                        slowMoController.OnSlowMoEnd += HandleSlowMoEnd;
                    }
                    if (slowMoMeter != null)
                    {
                        slowMoMeter.OnChanged += HandleSlowMoMeterChanged;
                    }
                    break;
            }
        }

        private void UnsubscribeEvents()
        {
            if (dimensionShifter != null)
            {
                dimensionShifter.OnShiftStart -= HandlePhaseStart;
                dimensionShifter.OnShiftEnd -= HandlePhaseEnd;
                dimensionShifter.OnShiftStart -= HandlePhaseShiftActiveForDash;
                dimensionShifter.OnShiftEnd -= HandlePhaseShiftEndedForDash;
            }

            if (dashController != null)
            {
                dashController.OnDashStart -= HandleDashStart;
                dashController.OnDashEnd -= HandleDashEnd;
            }

            if (slowMoController != null)
            {
                slowMoController.OnSlowMoStart -= HandleSlowMoStart;
                slowMoController.OnSlowMoEnd -= HandleSlowMoEnd;
            }

            if (slowMoMeter != null)
            {
                slowMoMeter.OnChanged -= HandleSlowMoMeterChanged;
            }
        }

        private void Update()
        {
            switch (skillType)
            {
                case SkillCardType.Phase:
                    if (dimensionShifter != null)
                    {
                        if (dimensionShifter.IsShifted)
                        {
                            SetState(SkillCardState.Active);
                            float rem = dimensionShifter.CooldownRemaining;
                            UpdateActiveText(rem);
                        }
                        else if (dimensionShifter.CooldownRemaining > 0f)
                        {
                            SetState(SkillCardState.Cooldown);
                            float rem = dimensionShifter.CooldownRemaining;
                            float prog = dimensionShifter.CooldownProgress;
                            UpdateCooldown(rem, prog);
                        }
                        else
                        {
                            SetState(SkillCardState.Ready);
                        }
                    }
                    break;

                case SkillCardType.Dash:
                    if (dimensionShifter != null && dimensionShifter.IsShifted)
                    {
                        SetState(SkillCardState.Locked);
                    }
                    else if (dashController != null)
                    {
                        if (dashController.IsDashing)
                        {
                            SetState(SkillCardState.Active);
                            UpdateActiveText(0.15f);
                        }
                        else if (dashController.CooldownTimeRemaining > 0f)
                        {
                            SetState(SkillCardState.Cooldown);
                            float rem = dashController.CooldownTimeRemaining;
                            float prog = dashController.CooldownProgress;
                            UpdateCooldown(rem, prog);
                        }
                        else
                        {
                            SetState(SkillCardState.Ready);
                        }
                    }
                    break;

                case SkillCardType.SlowMotion:
                    if (slowMoController != null && slowMoController.IsSlowMoActive)
                    {
                        SetState(SkillCardState.Active);
                        UpdateActiveText(slowMoController.TimeRemaining);
                    }
                    else if (slowMoMeter != null)
                    {
                        if (slowMoMeter.IsReady)
                        {
                            SetState(SkillCardState.Ready);
                        }
                        else
                        {
                            currentState = SkillCardState.Cooldown;
                            RefreshVisualState();
                            int pct = Mathf.Clamp(Mathf.RoundToInt(slowMoMeter.Charge * 100f), 0, 99);
                            if (stateLineText != null) stateLineText.text = $"{pct}%";
                        }
                    }
                    break;
            }
        }

        public void SetState(SkillCardState newState, bool forceRefresh = false)
        {
            if (currentState == newState && !forceRefresh) return;
            currentState = newState;
            RefreshVisualState();
        }

        public void RefreshVisualState()
        {
            Color readyCol = theme != null ? theme.AccentReady : new Color(0.239f, 0.863f, 0.165f, 1f);
            Color activeCol = theme != null ? theme.AccentActive : new Color(0.184f, 0.608f, 1.0f, 1f);
            Color cooldownCol = theme != null ? theme.CooldownFill : new Color(0.604f, 0.651f, 0.686f, 1f);

            switch (currentState)
            {
                case SkillCardState.Ready:
                    if (canvasGroup != null) canvasGroup.alpha = 1f;
                    if (borderHighlight != null)
                    {
                        borderHighlight.gameObject.SetActive(true);
                        borderHighlight.color = readyCol;
                    }
                    if (cooldownFill != null) cooldownFill.fillAmount = 0f;
                    if (stateLineText != null)
                    {
                        stateLineText.text = "READY";
                        stateLineText.color = readyCol;
                    }
                    break;

                case SkillCardState.Active:
                    if (canvasGroup != null) canvasGroup.alpha = 1f;
                    if (borderHighlight != null)
                    {
                        borderHighlight.gameObject.SetActive(true);
                        borderHighlight.color = activeCol;
                    }
                    if (cooldownFill != null) cooldownFill.fillAmount = 0f;
                    if (stateLineText != null) stateLineText.color = activeCol;
                    break;

                case SkillCardState.Cooldown:
                    if (canvasGroup != null) canvasGroup.alpha = 1f;
                    if (borderHighlight != null) borderHighlight.gameObject.SetActive(false);
                    if (cooldownFill != null) cooldownFill.color = cooldownCol;
                    if (stateLineText != null) stateLineText.color = cooldownCol;
                    break;

                case SkillCardState.Locked:
                    if (canvasGroup != null) canvasGroup.alpha = 0.4f;
                    if (borderHighlight != null) borderHighlight.gameObject.SetActive(false);
                    if (cooldownFill != null) cooldownFill.fillAmount = 0f;
                    if (stateLineText != null)
                    {
                        stateLineText.text = "LOCKED";
                        stateLineText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                    }
                    break;
            }
        }

        public void UpdateCooldown(float remainingSeconds, float progress)
        {
            if (cooldownFill != null)
            {
                cooldownFill.fillAmount = Mathf.Clamp01(progress);
            }
            if (stateLineText != null)
            {
                stateLineText.text = $"{Mathf.Max(0f, remainingSeconds):F1}s";
            }
        }

        public void UpdateActiveText(float remainingSeconds)
        {
            if (stateLineText != null)
            {
                stateLineText.text = $"{Mathf.Max(0f, remainingSeconds):F1}s";
            }
        }

        public void UpdateSlowMoCharge(float current, float max)
        {
            float targetCharge = max > 0f ? Mathf.Clamp01(current / max) : 0f;

            if (chargeBarFill != null)
            {
                chargeTween?.Stop();
                float startCharge = currentDisplayedCharge;
                chargeTween = Tween.Value(
                    startCharge,
                    targetCharge,
                    0.2f,
                    val =>
                    {
                        currentDisplayedCharge = val;
                        chargeBarFill.fillAmount = val;
                    },
                    EaseType.EaseOutCubic,
                    useUnscaledTime: true,
                    onComplete: () =>
                    {
                        currentDisplayedCharge = targetCharge;
                        chargeBarFill.fillAmount = targetCharge;
                        if (targetCharge >= 0.999f)
                        {
                            PulseChargeBarFull();
                        }
                    }
                );
            }

            if (targetCharge >= 0.999f)
            {
                SetState(SkillCardState.Ready, forceRefresh: true);
            }
            else if (currentState != SkillCardState.Active)
            {
                SetState(SkillCardState.Cooldown);
                int pct = Mathf.Clamp(Mathf.RoundToInt(targetCharge * 100f), 0, 99);
                if (stateLineText != null) stateLineText.text = $"{pct}%";
            }
        }

        private void PulseChargeBarFull()
        {
            if (chargeBarFill == null) return;
            pulseTween?.Stop();
            Color readyCol = theme != null ? theme.AccentReady : new Color(0.239f, 0.863f, 0.165f, 1f);
            Color originalCol = chargeBarFill.color;

            chargeBarFill.color = readyCol;
            pulseTween = Tween.Color(chargeBarFill, originalCol, 0.35f, EaseType.EaseOutCubic, useUnscaledTime: true);
        }

        private void HandleActionChange(object obj, InputActionChange change)
        {
            if (change == InputActionChange.ActionPerformed)
            {
                var action = (InputAction)obj;
                var control = action?.activeControl;
                if (control != null)
                {
                    bool gamepad = control.device is Gamepad;
                    if (gamepad != isGamepadActive)
                    {
                        isGamepadActive = gamepad;
                        UpdateKeyChip();
                    }
                }
            }
        }

        public void SetInputDevice(bool isGamepad)
        {
            isGamepadActive = isGamepad;
            UpdateKeyChip();
        }

        public void UpdateKeyChip()
        {
            if (keyChipText == null) return;

            switch (skillType)
            {
                case SkillCardType.Phase:
                    keyChipText.text = isGamepadActive ? "LB" : "[Q]";
                    break;
                case SkillCardType.Dash:
                    keyChipText.text = isGamepadActive ? "B" : "[Shift]";
                    break;
                case SkillCardType.SlowMotion:
                    keyChipText.text = isGamepadActive ? "Y" : "[E]";
                    break;
            }
        }

        private void HandlePhaseStart()
        {
            SetState(SkillCardState.Active);
        }

        private void HandlePhaseEnd()
        {
            SetState(SkillCardState.Cooldown);
        }

        private void HandleDashStart()
        {
            SetState(SkillCardState.Active);
        }

        private void HandleDashEnd()
        {
            SetState(SkillCardState.Cooldown);
        }

        private void HandlePhaseShiftActiveForDash()
        {
            SetState(SkillCardState.Locked);
        }

        private void HandlePhaseShiftEndedForDash()
        {
            SetState(SkillCardState.Ready);
        }

        private void HandleSlowMoStart()
        {
            SetState(SkillCardState.Active);
        }

        private void HandleSlowMoEnd()
        {
            SetState(SkillCardState.Cooldown);
        }

        private void HandleSlowMoMeterChanged(float current, float max)
        {
            UpdateSlowMoCharge(current, max);
        }
    }
}
