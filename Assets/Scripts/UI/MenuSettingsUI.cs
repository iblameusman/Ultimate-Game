using System;
using UnityEngine;
using UnityEngine.UI;
using DodgeGame.Core;
using DodgeGame.Data;

namespace DodgeGame.UI
{
    public class MenuSettingsUI : MonoBehaviour
    {
        [Header("Audio Sliders")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider comedySlider;

        [Header("Comedy Level Controls")]
        [SerializeField] private Button comedyOffButton;
        [SerializeField] private Button comedySubtleButton;
        [SerializeField] private Button comedyFullButton;
        [SerializeField] private TMPro.TextMeshProUGUI comedyLabel;

        [Header("Accessibility Toggles")]
        [SerializeField] private Toggle screenShakeToggle;
        [SerializeField] private Toggle reducedMotionToggle;
        [SerializeField] private Toggle captionsToggle;

        [Header("Camera Controls")]
        [SerializeField] private Toggle cameraInvertYToggle;
        [SerializeField] private Slider fovSlider;
        [SerializeField] private TMPro.TextMeshProUGUI fovLabel;
        [SerializeField] private Button cameraDistanceCloseBtn;
        [SerializeField] private Button cameraDistanceNormalBtn;
        [SerializeField] private Button cameraDistanceFarBtn;
        [SerializeField] private TMPro.TextMeshProUGUI cameraDistanceLabel;

        [Header("Display Slider")]
        [SerializeField] private Slider brightnessSlider;

        [Header("Graphics Quality")]
        [SerializeField] private TMPro.TMP_Dropdown qualityDropdown;
        [SerializeField] private Button qualityNextButton;
        [SerializeField] private TMPro.TextMeshProUGUI qualityLabel;

        [Header("Action Buttons")]
        [SerializeField] private Button resetButton;
        [SerializeField] private Button backButton;

        public Slider MasterVolumeSlider => masterSlider;
        public Button ResetButton => resetButton;
        public Button BackButton => backButton;
        public event Action OnBackRequested;

        private bool isRefreshing = false;

        private void Awake()
        {
            if (masterSlider != null) masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            if (musicSlider != null) musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            if (comedySlider != null) comedySlider.onValueChanged.AddListener(OnComedyVolumeChanged);
            if (comedyOffButton != null) comedyOffButton.onClick.AddListener(() => SetComedyLevel(ComedyLevel.Off));
            if (comedySubtleButton != null) comedySubtleButton.onClick.AddListener(() => SetComedyLevel(ComedyLevel.Subtle));
            if (comedyFullButton != null) comedyFullButton.onClick.AddListener(() => SetComedyLevel(ComedyLevel.Full));
            if (screenShakeToggle != null) screenShakeToggle.onValueChanged.AddListener(OnScreenShakeChanged);
            if (reducedMotionToggle != null) reducedMotionToggle.onValueChanged.AddListener(OnReducedMotionChanged);
            if (captionsToggle != null) captionsToggle.onValueChanged.AddListener(OnCaptionsChanged);
            if (cameraInvertYToggle != null) cameraInvertYToggle.onValueChanged.AddListener(OnCameraInvertYChanged);
            if (fovSlider != null) fovSlider.onValueChanged.AddListener(OnFovChanged);
            if (cameraDistanceCloseBtn != null) cameraDistanceCloseBtn.onClick.AddListener(() => OnCameraDistanceSelected(CameraDistancePreset.Close));
            if (cameraDistanceNormalBtn != null) cameraDistanceNormalBtn.onClick.AddListener(() => OnCameraDistanceSelected(CameraDistancePreset.Normal));
            if (cameraDistanceFarBtn != null) cameraDistanceFarBtn.onClick.AddListener(() => OnCameraDistanceSelected(CameraDistancePreset.Far));
            if (brightnessSlider != null) brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
            if (qualityDropdown != null) qualityDropdown.onValueChanged.AddListener(OnQualityDropdownChanged);
            if (qualityNextButton != null) qualityNextButton.onClick.AddListener(OnQualityNextClicked);

            if (resetButton != null) resetButton.onClick.AddListener(OnResetClicked);
            if (backButton != null) backButton.onClick.AddListener(OnBackClicked);
        }

        private void OnEnable()
        {
            if (SettingsService.Instance != null)
            {
                SettingsService.Instance.OnSettingsChanged += HandleExternalSettingsChange;
            }
            RefreshUI();
        }

        private void OnDisable()
        {
            if (SettingsService.Instance != null)
            {
                SettingsService.Instance.OnSettingsChanged -= HandleExternalSettingsChange;
            }
        }

        private void OnDestroy()
        {
            if (masterSlider != null) masterSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            if (musicSlider != null) musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            if (comedySlider != null) comedySlider.onValueChanged.RemoveListener(OnComedyVolumeChanged);
            if (comedyOffButton != null) comedyOffButton.onClick.RemoveAllListeners();
            if (comedySubtleButton != null) comedySubtleButton.onClick.RemoveAllListeners();
            if (comedyFullButton != null) comedyFullButton.onClick.RemoveAllListeners();
            if (screenShakeToggle != null) screenShakeToggle.onValueChanged.RemoveListener(OnScreenShakeChanged);
            if (reducedMotionToggle != null) reducedMotionToggle.onValueChanged.RemoveListener(OnReducedMotionChanged);
            if (captionsToggle != null) captionsToggle.onValueChanged.RemoveListener(OnCaptionsChanged);
            if (cameraInvertYToggle != null) cameraInvertYToggle.onValueChanged.RemoveListener(OnCameraInvertYChanged);
            if (brightnessSlider != null) brightnessSlider.onValueChanged.RemoveListener(OnBrightnessChanged);
            if (qualityDropdown != null) qualityDropdown.onValueChanged.RemoveListener(OnQualityDropdownChanged);
            if (qualityNextButton != null) qualityNextButton.onClick.RemoveListener(OnQualityNextClicked);

            if (resetButton != null) resetButton.onClick.RemoveListener(OnResetClicked);
            if (backButton != null) backButton.onClick.RemoveListener(OnBackClicked);
        }

        public void RefreshUI()
        {
            if (SettingsService.Instance == null || SettingsService.Instance.CurrentSettings == null)
            {
                return;
            }

            isRefreshing = true;
            SettingsData s = SettingsService.Instance.CurrentSettings;

            if (masterSlider != null) masterSlider.value = s.MasterVolume;
            if (musicSlider != null) musicSlider.value = s.MusicVolume;
            if (sfxSlider != null) sfxSlider.value = s.SfxVolume;
            if (comedySlider != null) comedySlider.value = s.ComedyVolume;

            if (screenShakeToggle != null) screenShakeToggle.isOn = s.ScreenShakeEnabled;
            if (reducedMotionToggle != null) reducedMotionToggle.isOn = s.ReducedMotion;
            if (captionsToggle != null) captionsToggle.isOn = s.CaptionsEnabled;
            if (cameraInvertYToggle != null) cameraInvertYToggle.isOn = s.CameraInvertY;
            if (fovSlider != null)
            {
                fovSlider.minValue = 50f;
                fovSlider.maxValue = 75f;
                fovSlider.value = s.FieldOfView;
            }
            if (fovLabel != null) fovLabel.text = $"{Mathf.RoundToInt(s.FieldOfView)}°";
            UpdateCameraDistanceSegmentVisuals(s.CameraDistance);

            UpdateComedySegmentVisuals(s.ComedyLevel);

            if (brightnessSlider != null) brightnessSlider.value = s.Brightness;

            if (qualityDropdown != null)
            {
                qualityDropdown.value = (int)s.QualityTier;
            }
            if (qualityLabel != null)
            {
                qualityLabel.text = s.QualityTier.ToString();
            }

            isRefreshing = false;
        }

        private void HandleExternalSettingsChange(SettingsData settings)
        {
            if (!isRefreshing)
            {
                RefreshUI();
            }
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

        private void OnComedyVolumeChanged(float val)
        {
            if (isRefreshing) return;
            SettingsService.Instance?.SetComedyVolume(val);
        }

        public void SetComedyLevel(ComedyLevel level)
        {
            if (isRefreshing) return;
            SettingsService.Instance?.SetComedyLevel(level);
            UpdateComedySegmentVisuals(level);
        }

        private void OnCaptionsChanged(bool val)
        {
            if (isRefreshing) return;
            SettingsService.Instance?.SetCaptions(val);
        }

        public void UpdateComedySegmentVisuals(ComedyLevel level)
        {
            Color activeColor = new Color(0.239f, 0.863f, 0.165f, 1f);
            Color inactiveColor = new Color(0.184f, 0.220f, 0.243f, 1f);

            SetButtonVisual(comedyOffButton, level == ComedyLevel.Off, activeColor, inactiveColor);
            SetButtonVisual(comedySubtleButton, level == ComedyLevel.Subtle, activeColor, inactiveColor);
            SetButtonVisual(comedyFullButton, level == ComedyLevel.Full, activeColor, inactiveColor);

            if (comedyLabel != null)
            {
                comedyLabel.text = $"Comedy: {level}";
            }
        }

        private void SetButtonVisual(Button btn, bool isActive, Color activeCol, Color inactiveCol)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.color = isActive ? activeCol : inactiveCol;
            }
            var tmp = btn.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.color = isActive ? Color.black : Color.white;
                tmp.fontStyle = isActive ? TMPro.FontStyles.Bold : TMPro.FontStyles.Normal;
            }
        }

        private void OnScreenShakeChanged(bool val)
        {
            if (isRefreshing) return;
            SettingsService.Instance?.SetScreenShake(val);
        }

        private void OnReducedMotionChanged(bool val)
        {
            if (isRefreshing) return;
            SettingsService.Instance?.SetReducedMotion(val);
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
            if (fovLabel != null) fovLabel.text = $"{Mathf.RoundToInt(val)}°";
        }

        private void OnCameraDistanceSelected(CameraDistancePreset preset)
        {
            if (isRefreshing) return;
            if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
            SettingsService.Instance?.SetCameraDistance(preset);
            UpdateCameraDistanceSegmentVisuals(preset);
        }

        private void UpdateCameraDistanceSegmentVisuals(CameraDistancePreset preset)
        {
            Color activeColor = new Color(0.239f, 0.863f, 0.165f, 1f);
            Color inactiveColor = new Color(0.184f, 0.220f, 0.243f, 1f);

            SetButtonVisual(cameraDistanceCloseBtn, preset == CameraDistancePreset.Close, activeColor, inactiveColor);
            SetButtonVisual(cameraDistanceNormalBtn, preset == CameraDistancePreset.Normal, activeColor, inactiveColor);
            SetButtonVisual(cameraDistanceFarBtn, preset == CameraDistancePreset.Far, activeColor, inactiveColor);

            if (cameraDistanceLabel != null)
            {
                cameraDistanceLabel.text = $"Camera: {preset}";
            }
        }

        private void OnBrightnessChanged(float val)
        {
            if (isRefreshing) return;
            SettingsService.Instance?.SetBrightness(val);
        }

        private void OnQualityDropdownChanged(int index)
        {
            if (isRefreshing) return;
            SettingsService.Instance?.SetQualityTier((QualityTier)index);
            if (qualityLabel != null) qualityLabel.text = ((QualityTier)index).ToString();
        }

        private void OnQualityNextClicked()
        {
            if (isRefreshing || SettingsService.Instance == null || SettingsService.Instance.CurrentSettings == null) return;
            if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();

            int current = (int)SettingsService.Instance.CurrentSettings.QualityTier;
            int next = (current + 1) % 3;
            SettingsService.Instance.SetQualityTier((QualityTier)next);
            RefreshUI();
        }

        private void OnResetClicked()
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
            SettingsService.Instance?.ResetToDefaults();
            RefreshUI();
        }

        private void OnBackClicked()
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
            AudioManager.Instance?.PlayUI("UI_BACK");
            OnBackRequested?.Invoke();
        }
    }
}
