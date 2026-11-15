using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using DodgeGame.Core;
using DodgeGame.Data;

namespace DodgeGame.UI
{
    public class SettingsModalController : MonoBehaviour
    {
        [Header("Dimmer & Panel")]
        [SerializeField] private CanvasGroup dimmerCanvasGroup;
        [SerializeField] private Button dimmerButton;
        [SerializeField] private RectTransform modalPanelRect;
        [SerializeField] private CanvasGroup modalPanelCanvasGroup;

        [Header("Header Controls")]
        [SerializeField] private Button closeButton;

        [Header("Audio Sliders")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;

        [Header("Display Controls")]
        [SerializeField] private Slider brightnessSlider;
        [SerializeField] private SettingsSwitchToggle reducedMotionToggle;
        [SerializeField] private SettingsSwitchToggle screenShakeToggle;

        [Header("Camera Controls")]
        [SerializeField] private SettingsSwitchToggle cameraInvertYToggle;
        [SerializeField] private Slider fovSlider;
        [SerializeField] private TextMeshProUGUI fovValueText;
        [SerializeField] private Button cameraDistanceCloseBtn;
        [SerializeField] private Button cameraDistanceNormalBtn;
        [SerializeField] private Button cameraDistanceFarBtn;
        [SerializeField] private TextMeshProUGUI cameraDistanceValueText;

        [Header("Footer Buttons")]
        [SerializeField] private Button resetButton;
        [SerializeField] private Button backButton;

        [Header("Reset Confirmation")]
        [SerializeField] private GameObject resetNormalState;
        [SerializeField] private GameObject resetConfirmState;
        [SerializeField] private Button resetConfirmYesBtn;
        [SerializeField] private Button resetConfirmNoBtn;

        public GameObject ResetNormalState => resetNormalState;
        public GameObject ResetConfirmState => resetConfirmState;
        public Button ResetConfirmYesButton => resetConfirmYesBtn;
        public Button ResetConfirmNoButton => resetConfirmNoBtn;

        [Header("Scroll Area")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Scrollbar verticalScrollbar;

        [Header("Animation Tuning")]
        [SerializeField] private float dimmerFadeDuration = 0.2f;
        [SerializeField] private float panelOpenDuration = 0.25f;
        [SerializeField] private float closeFadeDuration = 0.15f;
        [SerializeField] private float dimmerTargetAlpha = 0.55f;

        public event Action OnClosed;
        private Action onCloseCallback;
        private Coroutine activeTransitionRoutine;
        private bool isOpen = false;
        private bool isRefreshing = false;

        public static bool IsAnyModalOpen { get; private set; }

        public bool IsOpen => isOpen;
        public Button CloseButton => closeButton;
        public Button DimmerButton => dimmerButton;
        public Slider MasterSlider => masterSlider;
        public Slider MusicSlider => musicSlider;
        public Slider SfxSlider => sfxSlider;
        public Slider BrightnessSlider => brightnessSlider;
        public SettingsSwitchToggle ReducedMotionToggle => reducedMotionToggle;
        public SettingsSwitchToggle ScreenShakeToggle => screenShakeToggle;
        public SettingsSwitchToggle CameraInvertYToggle => cameraInvertYToggle;
        public Button ResetButton => resetButton;
        public Button BackButton => backButton;
        public CanvasGroup ModalPanelCanvasGroup => modalPanelCanvasGroup;
        public RectTransform ModalPanelRect => modalPanelRect;
        public ScrollRect ScrollRect => scrollRect;
        public Scrollbar VerticalScrollbar => verticalScrollbar;

        private GameObject lastSelectedObject;
        private GameObject previousSelectedObject;

        private void Awake()
        {
            if (scrollRect == null) scrollRect = GetComponentInChildren<ScrollRect>();
            if (scrollRect != null)
            {
                scrollRect.movementType = ScrollRect.MovementType.Clamped;
            }

            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (backButton != null) backButton.onClick.AddListener(Close);
            if (dimmerButton != null) dimmerButton.onClick.AddListener(Close);
            if (resetButton != null) resetButton.onClick.AddListener(OnResetClicked);
            if (resetConfirmYesBtn != null) resetConfirmYesBtn.onClick.AddListener(OnResetConfirmYes);
            if (resetConfirmNoBtn != null) resetConfirmNoBtn.onClick.AddListener(OnResetConfirmNo);

            if (masterSlider != null) masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            if (musicSlider != null) musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            if (brightnessSlider != null) brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);

            if (reducedMotionToggle != null) reducedMotionToggle.onValueChanged += OnReducedMotionChanged;
            if (screenShakeToggle != null) screenShakeToggle.onValueChanged += OnScreenShakeChanged;
            if (cameraInvertYToggle != null) cameraInvertYToggle.onValueChanged += OnCameraInvertYChanged;
            if (fovSlider != null) fovSlider.onValueChanged.AddListener(OnFovChanged);
            if (cameraDistanceCloseBtn != null) cameraDistanceCloseBtn.onClick.AddListener(() => OnCameraDistanceSelected(CameraDistancePreset.Close));
            if (cameraDistanceNormalBtn != null) cameraDistanceNormalBtn.onClick.AddListener(() => OnCameraDistanceSelected(CameraDistancePreset.Normal));
            if (cameraDistanceFarBtn != null) cameraDistanceFarBtn.onClick.AddListener(() => OnCameraDistanceSelected(CameraDistancePreset.Far));

            if (!isOpen)
            {
                if (modalPanelCanvasGroup != null) modalPanelCanvasGroup.alpha = 0f;
                if (dimmerCanvasGroup != null) dimmerCanvasGroup.alpha = 0f;
            }
        }

        private void OnDestroy()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
            if (backButton != null) backButton.onClick.RemoveListener(Close);
            if (dimmerButton != null) dimmerButton.onClick.RemoveListener(Close);
            if (resetButton != null) resetButton.onClick.RemoveListener(OnResetClicked);
            if (resetConfirmYesBtn != null) resetConfirmYesBtn.onClick.RemoveListener(OnResetConfirmYes);
            if (resetConfirmNoBtn != null) resetConfirmNoBtn.onClick.RemoveListener(OnResetConfirmNo);

            if (masterSlider != null) masterSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            if (musicSlider != null) musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            if (brightnessSlider != null) brightnessSlider.onValueChanged.RemoveListener(OnBrightnessChanged);

            if (reducedMotionToggle != null) reducedMotionToggle.onValueChanged -= OnReducedMotionChanged;
            if (screenShakeToggle != null) screenShakeToggle.onValueChanged -= OnScreenShakeChanged;
            if (cameraInvertYToggle != null) cameraInvertYToggle.onValueChanged -= OnCameraInvertYChanged;
        }

        private void Update()
        {
            if (!isOpen) return;

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Close();
                return;
            }

            if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
            {
                Close();
                return;
            }

            if (EventSystem.current != null)
            {
                var curSelected = EventSystem.current.currentSelectedGameObject;
                if (curSelected != lastSelectedObject)
                {
                    lastSelectedObject = curSelected;
                    if (curSelected != null && scrollRect != null && scrollRect.content != null)
                    {
                        if (curSelected.transform.IsChildOf(scrollRect.content))
                        {
                            ScrollToEnsureVisible(curSelected.GetComponent<RectTransform>());
                        }
                    }
                }
            }
        }

        public void Open(Action onClose = null)
        {
            onCloseCallback = onClose;
            isOpen = true;
            IsAnyModalOpen = true;
            gameObject.SetActive(true);

            ResetScrollPosition();
            RefreshUI();
            HideResetConfirmation();

            if (!gameObject.activeInHierarchy)
            {
                Debug.LogError($"[SettingsModalController.Open] gameObject '{gameObject.name}' is NOT active in hierarchy! activeSelf={gameObject.activeSelf}, parent={transform.parent?.name}, parentActiveInHierarchy={transform.parent?.gameObject.activeInHierarchy}");
            }

            if (activeTransitionRoutine != null) StopCoroutine(activeTransitionRoutine);
            if (gameObject.activeInHierarchy)
            {
                activeTransitionRoutine = StartCoroutine(OpenRoutine());
            }

            if (EventSystem.current != null)
            {
                if (masterSlider != null)
                {
                    EventSystem.current.SetSelectedGameObject(masterSlider.gameObject);
                }
                else if (closeButton != null)
                {
                    EventSystem.current.SetSelectedGameObject(closeButton.gameObject);
                }
            }
        }

        public void OpenImmediate(Action onClose = null)
        {
            onCloseCallback = onClose;
            isOpen = true;
            IsAnyModalOpen = true;
            gameObject.SetActive(true);

            ResetScrollPosition();
            RefreshUI();
            HideResetConfirmation();

            if (activeTransitionRoutine != null) StopCoroutine(activeTransitionRoutine);

            if (dimmerCanvasGroup != null)
            {
                dimmerCanvasGroup.alpha = dimmerTargetAlpha;
                dimmerCanvasGroup.interactable = true;
                dimmerCanvasGroup.blocksRaycasts = true;
            }
            if (modalPanelCanvasGroup != null)
            {
                modalPanelCanvasGroup.alpha = 1f;
                modalPanelCanvasGroup.interactable = true;
                modalPanelCanvasGroup.blocksRaycasts = true;
            }
            if (modalPanelRect != null) modalPanelRect.localScale = Vector3.one;

            if (EventSystem.current != null)
            {
                if (masterSlider != null) EventSystem.current.SetSelectedGameObject(masterSlider.gameObject);
                else if (closeButton != null) EventSystem.current.SetSelectedGameObject(closeButton.gameObject);
            }
        }

        public void Close()
        {
            if (!isOpen) return;
            isOpen = false;
            IsAnyModalOpen = false;

            SettingsService.Instance?.SaveSettings();

            SoundManager.Instance?.PlayButtonClick();
            AudioManager.Instance?.PlayUI("UI_BACK");

            var callback = onCloseCallback;
            onCloseCallback = null;
            callback?.Invoke();
            OnClosed?.Invoke();

            if (activeTransitionRoutine != null) StopCoroutine(activeTransitionRoutine);
            activeTransitionRoutine = StartCoroutine(CloseRoutine());
        }

        public void CloseImmediate()
        {
            if (!isOpen && !gameObject.activeSelf) return;
            isOpen = false;
            IsAnyModalOpen = false;

            SettingsService.Instance?.SaveSettings();

            if (activeTransitionRoutine != null) StopCoroutine(activeTransitionRoutine);

            if (dimmerCanvasGroup != null)
            {
                dimmerCanvasGroup.alpha = 0f;
                dimmerCanvasGroup.interactable = false;
                dimmerCanvasGroup.blocksRaycasts = false;
            }
            if (modalPanelCanvasGroup != null)
            {
                modalPanelCanvasGroup.alpha = 0f;
                modalPanelCanvasGroup.interactable = false;
                modalPanelCanvasGroup.blocksRaycasts = false;
            }

            gameObject.SetActive(false);
            OnClosed?.Invoke();
            onCloseCallback?.Invoke();
            onCloseCallback = null;
        }

        public void ResetScrollPosition()
        {
            if (scrollRect == null) scrollRect = GetComponentInChildren<ScrollRect>();
            if (scrollRect != null)
            {
                scrollRect.movementType = ScrollRect.MovementType.Clamped;
                scrollRect.StopMovement();
                scrollRect.verticalNormalizedPosition = 1f;
                if (scrollRect.content != null)
                {
                    var pos = scrollRect.content.anchoredPosition;
                    pos.y = 0f;
                    scrollRect.content.anchoredPosition = pos;
                }
            }
        }

        public void ScrollTo(float normalizedY)
        {
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = Mathf.Clamp01(normalizedY);
            }
        }

        public void ScrollToEnsureVisible(RectTransform target)
        {
            if (scrollRect == null || scrollRect.content == null || scrollRect.viewport == null || target == null)
                return;

            RectTransform viewport = scrollRect.viewport;
            RectTransform content = scrollRect.content;

            Vector3[] targetCorners = new Vector3[4];
            target.GetWorldCorners(targetCorners);

            Vector3[] viewportCorners = new Vector3[4];
            viewport.GetWorldCorners(viewportCorners);

            float targetBottom = float.MaxValue;
            float targetTop = float.MinValue;
            for (int i = 0; i < 4; i++)
            {
                Vector3 localPos = viewport.InverseTransformPoint(targetCorners[i]);
                if (localPos.y < targetBottom) targetBottom = localPos.y;
                if (localPos.y > targetTop) targetTop = localPos.y;
            }

            float viewportBottom = float.MaxValue;
            float viewportTop = float.MinValue;
            for (int i = 0; i < 4; i++)
            {
                Vector3 localPos = viewport.InverseTransformPoint(viewportCorners[i]);
                if (localPos.y < viewportBottom) viewportBottom = localPos.y;
                if (localPos.y > viewportTop) viewportTop = localPos.y;
            }

            float margin = 20f;
            float delta = 0f;
            if (targetTop > viewportTop - margin)
            {
                delta = targetTop - (viewportTop - margin);
            }
            else if (targetBottom < viewportBottom + margin)
            {
                delta = targetBottom - (viewportBottom + margin);
            }

            if (Mathf.Abs(delta) > 0.5f)
            {
                Vector2 contentPos = content.anchoredPosition;
                contentPos.y -= delta;
                float contentHeight = content.rect.height;
                float viewportHeight = viewport.rect.height;
                float maxPos = Mathf.Max(0f, contentHeight - viewportHeight);
                contentPos.y = Mathf.Clamp(contentPos.y, 0f, maxPos);
                content.anchoredPosition = contentPos;
            }
        }

        private IEnumerator OpenRoutine()
        {
            bool reducedMotion = IsReducedMotionActive();

            if (dimmerCanvasGroup != null) dimmerCanvasGroup.alpha = 0f;
            if (modalPanelCanvasGroup != null)
            {
                modalPanelCanvasGroup.alpha = 0f;
                modalPanelCanvasGroup.interactable = false;
                modalPanelCanvasGroup.blocksRaycasts = false;
            }

            Vector3 startScale = reducedMotion ? Vector3.one : Vector3.one * 0.95f;
            if (modalPanelRect != null) modalPanelRect.localScale = startScale;

            float elapsed = 0f;
            float duration = panelOpenDuration;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                if (dimmerCanvasGroup != null)
                {
                    float dt = Mathf.Clamp01(elapsed / dimmerFadeDuration);
                    dimmerCanvasGroup.alpha = Mathf.Lerp(0f, dimmerTargetAlpha, dt);
                }

                if (modalPanelCanvasGroup != null) modalPanelCanvasGroup.alpha = t;
                if (modalPanelRect != null && !reducedMotion)
                {
                    float ease = Tween.EvaluateEase(t, EaseType.EaseOutBack);
                    modalPanelRect.localScale = Vector3.LerpUnclamped(startScale, Vector3.one, ease);
                }

                yield return null;
            }

            if (dimmerCanvasGroup != null) dimmerCanvasGroup.alpha = dimmerTargetAlpha;
            if (modalPanelCanvasGroup != null)
            {
                modalPanelCanvasGroup.alpha = 1f;
                modalPanelCanvasGroup.interactable = true;
                modalPanelCanvasGroup.blocksRaycasts = true;
            }
            if (modalPanelRect != null) modalPanelRect.localScale = Vector3.one;

            activeTransitionRoutine = null;
        }

        private IEnumerator CloseRoutine()
        {
            float elapsed = 0f;
            float duration = closeFadeDuration;

            float startDimmer = dimmerCanvasGroup != null ? dimmerCanvasGroup.alpha : 0.5f;

            if (modalPanelCanvasGroup != null)
            {
                modalPanelCanvasGroup.interactable = false;
                modalPanelCanvasGroup.blocksRaycasts = false;
            }

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                if (modalPanelCanvasGroup != null) modalPanelCanvasGroup.alpha = 1f - t;
                if (dimmerCanvasGroup != null) dimmerCanvasGroup.alpha = Mathf.Lerp(startDimmer, 0f, t);

                yield return null;
            }

            if (modalPanelCanvasGroup != null) modalPanelCanvasGroup.alpha = 0f;
            if (dimmerCanvasGroup != null) dimmerCanvasGroup.alpha = 0f;

            HideResetConfirmation();
            gameObject.SetActive(false);
            activeTransitionRoutine = null;

            onCloseCallback?.Invoke();
            OnClosed?.Invoke();
        }

        public void RefreshUI()
        {
            if (SettingsService.Instance == null || SettingsService.Instance.CurrentSettings == null) return;

            isRefreshing = true;
            SettingsData s = SettingsService.Instance.CurrentSettings;

            if (masterSlider != null) masterSlider.value = s.MasterVolume;
            if (musicSlider != null) musicSlider.value = s.MusicVolume;
            if (sfxSlider != null) sfxSlider.value = s.SfxVolume;

            if (brightnessSlider != null) brightnessSlider.value = s.Brightness;
            if (reducedMotionToggle != null) reducedMotionToggle.SetIsOn(s.ReducedMotion, false);
            if (screenShakeToggle != null) screenShakeToggle.SetIsOn(s.ScreenShakeEnabled, false);

            if (cameraInvertYToggle != null) cameraInvertYToggle.SetIsOn(s.CameraInvertY, false);
            if (fovSlider != null)
            {
                fovSlider.minValue = 50f;
                fovSlider.maxValue = 75f;
                fovSlider.value = s.FieldOfView;
            }
            if (fovValueText != null) fovValueText.text = $"{Mathf.RoundToInt(s.FieldOfView)}°";
            UpdateCameraDistanceSegmentVisuals(s.CameraDistance);

            isRefreshing = false;
        }

        private bool IsReducedMotionActive()
        {
            return SettingsService.Instance != null &&
                   SettingsService.Instance.CurrentSettings != null &&
                   SettingsService.Instance.CurrentSettings.ReducedMotion;
        }

        private void OnMasterVolumeChanged(float val)
        {
            if (isRefreshing) return;
            SettingsService.Instance?.SetMasterVolume(val);
        }

        private void OnMusicVolumeChanged(float val)
        {
            if (isRefreshing) return;
            SettingsService.Instance?.SetMusicVolume(val);
        }

        private void OnSfxVolumeChanged(float val)
        {
            if (isRefreshing) return;
            SettingsService.Instance?.SetSfxVolume(val);
        }

        private void OnBrightnessChanged(float val)
        {
            if (isRefreshing) return;
            SettingsService.Instance?.SetBrightness(val);
        }

        private void OnReducedMotionChanged(bool val)
        {
            if (isRefreshing) return;
            SettingsService.Instance?.SetReducedMotion(val);
        }

        private void OnScreenShakeChanged(bool val)
        {
            if (isRefreshing) return;
            SettingsService.Instance?.SetScreenShake(val);
        }

        private void OnCameraInvertYChanged(bool val)
        {
            if (isRefreshing) return;
            SettingsService.Instance?.SetCameraInvertY(val);
        }

        private void OnFovChanged(float val)
        {
            if (isRefreshing) return;
            SettingsService.Instance?.SetFieldOfView(val);
            if (fovValueText != null) fovValueText.text = $"{Mathf.RoundToInt(val)}°";
        }

        private void OnCameraDistanceSelected(CameraDistancePreset preset)
        {
            if (isRefreshing) return;
            SoundManager.Instance?.PlayButtonClick();
            SettingsService.Instance?.SetCameraDistance(preset);
            UpdateCameraDistanceSegmentVisuals(preset);
        }

        private void UpdateCameraDistanceSegmentVisuals(CameraDistancePreset preset)
        {
            Color activeColor = new Color(0.239f, 0.863f, 0.165f, 1f);
            Color inactiveColor = new Color(0.184f, 0.220f, 0.243f, 1f);

            SetButtonSegmentVisual(cameraDistanceCloseBtn, preset == CameraDistancePreset.Close, activeColor, inactiveColor);
            SetButtonSegmentVisual(cameraDistanceNormalBtn, preset == CameraDistancePreset.Normal, activeColor, inactiveColor);
            SetButtonSegmentVisual(cameraDistanceFarBtn, preset == CameraDistancePreset.Far, activeColor, inactiveColor);

            if (cameraDistanceValueText != null)
            {
                cameraDistanceValueText.text = preset.ToString();
            }
        }

        private void SetButtonSegmentVisual(Button btn, bool isActive, Color activeCol, Color inactiveCol)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null) img.color = isActive ? activeCol : inactiveCol;

            var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.color = isActive ? new Color(0.12f, 0.16f, 0.20f, 1f) : Color.white;
                tmp.fontStyle = isActive ? FontStyles.Bold : FontStyles.Normal;
            }
        }

        private void OnResetClicked()
        {
            SoundManager.Instance?.PlayButtonClick();
            ShowResetConfirmation();
        }

        public void ShowResetConfirmation()
        {
            if (resetNormalState != null) resetNormalState.SetActive(false);
            if (resetConfirmState != null)
            {
                resetConfirmState.SetActive(true);
                if (EventSystem.current != null && resetConfirmYesBtn != null)
                {
                    EventSystem.current.SetSelectedGameObject(resetConfirmYesBtn.gameObject);
                }
            }
        }

        public void HideResetConfirmation()
        {
            if (resetConfirmState != null) resetConfirmState.SetActive(false);
            if (resetNormalState != null) resetNormalState.SetActive(true);
        }

        private void OnResetConfirmYes()
        {
            SoundManager.Instance?.PlayButtonClick();
            SettingsService.Instance?.ResetToDefaults();
            RefreshUI();
            HideResetConfirmation();
            if (EventSystem.current != null && resetButton != null)
            {
                EventSystem.current.SetSelectedGameObject(resetButton.gameObject);
            }
        }

        private void OnResetConfirmNo()
        {
            SoundManager.Instance?.PlayButtonClick();
            HideResetConfirmation();
            if (EventSystem.current != null && resetButton != null)
            {
                EventSystem.current.SetSelectedGameObject(resetButton.gameObject);
            }
        }

        public void SetReferences(
            CanvasGroup dimmerCg, Button dimmerBtn,
            RectTransform panelRt, CanvasGroup panelCg,
            Button closeBtn,
            Slider master, Slider music, Slider sfx,
            Slider brightness,
            SettingsSwitchToggle motion, SettingsSwitchToggle shake,
            SettingsSwitchToggle invertY,
            Button resetBtn, Button backBtn,
            ScrollRect sRect = null,
            Scrollbar vScrollbar = null,
            GameObject normalState = null,
            GameObject confirmState = null,
            Button confirmYes = null,
            Button confirmNo = null)
        {
            dimmerCanvasGroup = dimmerCg;
            dimmerButton = dimmerBtn;
            modalPanelRect = panelRt;
            modalPanelCanvasGroup = panelCg;
            closeButton = closeBtn;
            masterSlider = master;
            musicSlider = music;
            sfxSlider = sfx;
            brightnessSlider = brightness;
            reducedMotionToggle = motion;
            screenShakeToggle = shake;
            cameraInvertYToggle = invertY;
            resetButton = resetBtn;
            backButton = backBtn;
            if (sRect != null) scrollRect = sRect;
            if (vScrollbar != null) verticalScrollbar = vScrollbar;
            if (normalState != null) resetNormalState = normalState;
            if (confirmState != null) resetConfirmState = confirmState;
            if (confirmYes != null) resetConfirmYesBtn = confirmYes;
            if (confirmNo != null) resetConfirmNoBtn = confirmNo;
        }
    }
}
