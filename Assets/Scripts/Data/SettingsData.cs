using UnityEngine;

namespace DodgeGame.Data
{
    public enum ComedyLevel
    {
        Off,
        Subtle,
        Full
    }

    [CreateAssetMenu(fileName = "SettingsData", menuName = "DodgeGame/Settings Data")]
    public class SettingsData : ScriptableObject
    {
        [Header("Audio Settings (0 to 1)")]
        [Range(0f, 1f)]
        [SerializeField] private float masterVolume = 1f;

        [Range(0f, 1f)]
        [SerializeField] private float musicVolume = 1f;

        [Range(0f, 1f)]
        [SerializeField] private float sfxVolume = 1f;

        [Range(0f, 1f)]
        [SerializeField] private float comedyVolume = 1f;

        [Header("Comedy Level")]
        [SerializeField] private ComedyLevel comedyLevel = ComedyLevel.Full;

        [Header("Accessibility & Visuals")]
        [SerializeField] private bool screenShakeEnabled = true;
        [SerializeField] private bool reducedMotion = false;
        [SerializeField] private bool captionsEnabled = false;

        [Range(0.8f, 1.2f)]
        [SerializeField] private float brightness = 1f;

        [Header("Graphics Quality Tier")]
        [SerializeField] private QualityTier qualityTier = QualityTier.High;

        [Header("Camera Controls")]
        [SerializeField] private bool cameraInvertY = false;
        [SerializeField] private CameraDistancePreset cameraDistance = CameraDistancePreset.Normal;
        [Range(50f, 75f)]
        [SerializeField] private float fieldOfView = 60f;

        [Header("Gameplay Difficulty")]
        [SerializeField] private Difficulty difficulty = Difficulty.Normal;
        [SerializeField] private Difficulty effectiveDifficulty = Difficulty.Normal;

        public float MasterVolume
        {
            get => masterVolume;
            set => masterVolume = Mathf.Clamp01(value);
        }

        public float MusicVolume
        {
            get => musicVolume;
            set => musicVolume = Mathf.Clamp01(value);
        }

        public float SfxVolume
        {
            get => sfxVolume;
            set => sfxVolume = Mathf.Clamp01(value);
        }

        public float ComedyVolume
        {
            get => comedyVolume;
            set => comedyVolume = Mathf.Clamp01(value);
        }

        public ComedyLevel ComedyLevel
        {
            get => comedyLevel;
            set => comedyLevel = value;
        }

        public bool CaptionsEnabled
        {
            get => captionsEnabled;
            set => captionsEnabled = value;
        }

        public bool ScreenShakeEnabled
        {
            get => screenShakeEnabled;
            set => screenShakeEnabled = value;
        }

        public bool ReducedMotion
        {
            get => reducedMotion;
            set => reducedMotion = value;
        }

        public float Brightness
        {
            get => brightness;
            set => brightness = Mathf.Clamp(value, 0.8f, 1.2f);
        }

        public QualityTier QualityTier
        {
            get => qualityTier;
            set => qualityTier = value;
        }

        public bool CameraInvertY
        {
            get => cameraInvertY;
            set => cameraInvertY = value;
        }

        public CameraDistancePreset CameraDistance
        {
            get => cameraDistance;
            set => cameraDistance = value;
        }

        public float FieldOfView
        {
            get => fieldOfView;
            set => fieldOfView = Mathf.Clamp(value, 50f, 75f);
        }

        public Difficulty CurrentDifficulty
        {
            get => difficulty;
            set => difficulty = value;
        }

        public Difficulty SelectedDifficulty
        {
            get => difficulty;
            set => difficulty = value;
        }

        public Difficulty EffectiveDifficulty
        {
            get => effectiveDifficulty;
            set => effectiveDifficulty = value;
        }

        public void CopyFrom(SettingsData other)
        {
            if (other == null) return;
            masterVolume = other.masterVolume;
            musicVolume = other.musicVolume;
            sfxVolume = other.sfxVolume;
            comedyVolume = other.comedyVolume;
            comedyLevel = other.comedyLevel;
            captionsEnabled = other.captionsEnabled;
            screenShakeEnabled = other.screenShakeEnabled;
            reducedMotion = other.reducedMotion;
            brightness = other.brightness;
            qualityTier = other.qualityTier;
            cameraInvertY = other.cameraInvertY;
            cameraDistance = other.cameraDistance;
            fieldOfView = other.fieldOfView;
            difficulty = other.difficulty;
            effectiveDifficulty = other.effectiveDifficulty;
        }

        public void ResetToDefaults()
        {
            masterVolume = 1f;
            musicVolume = 1f;
            sfxVolume = 1f;
            comedyVolume = 1f;
            comedyLevel = ComedyLevel.Full;
            captionsEnabled = false;
            screenShakeEnabled = true;
            reducedMotion = false;
            brightness = 1f;
            qualityTier = QualityTier.High;
            cameraInvertY = false;
            cameraDistance = CameraDistancePreset.Normal;
            fieldOfView = 60f;
            difficulty = Difficulty.Normal;
            effectiveDifficulty = Difficulty.Normal;
        }
    }
}
