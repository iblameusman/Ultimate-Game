using System;
using UnityEngine;
using DodgeGame.Data;

namespace DodgeGame.Core
{
    public class SettingsService : MonoBehaviour
    {
        private static SettingsService instance;
        public static SettingsService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<SettingsService>();
                    if (instance != null)
                    {
                        instance.InitializeSettings();
                    }
                }
                return instance;
            }
            private set => instance = value;
        }

        public const string SETTINGS_PREFS_KEY = "DodgeGame_Settings_v1";
        private const string SettingsPrefKey = SETTINGS_PREFS_KEY;

        [Serializable]
        private class SerializableSettings
        {
            [SerializeField] private float masterVolume = 1f;
            [SerializeField] private float musicVolume = 1f;
            [SerializeField] private float sfxVolume = 1f;
            [SerializeField] private float comedyVolume = 1f;
            [SerializeField] private int comedyLevel = -1;
            [SerializeField] private bool captionsEnabled = false;
            [SerializeField] private bool screenShakeEnabled = true;
            [SerializeField] private bool reducedMotion = false;
            [SerializeField] private float brightness = 1f;
            [SerializeField] private int qualityTier = (int)DodgeGame.Data.QualityTier.High;
            [SerializeField] private bool cameraInvertY = false;
            [SerializeField] private int cameraDistance = (int)DodgeGame.Data.CameraDistancePreset.Normal;
            [SerializeField] private float fieldOfView = 60f;
            [SerializeField] private int difficulty = (int)DodgeGame.Data.Difficulty.Normal;
            [SerializeField] private int effectiveDifficulty = (int)DodgeGame.Data.Difficulty.Normal;

            public float MasterVolume { get => masterVolume; set => masterVolume = value; }
            public float MusicVolume { get => musicVolume; set => musicVolume = value; }
            public float SfxVolume { get => sfxVolume; set => sfxVolume = value; }
            public float ComedyVolume { get => comedyVolume; set => comedyVolume = value; }
            public int ComedyLevelValue { get => comedyLevel; set => comedyLevel = value; }
            public bool CaptionsEnabled { get => captionsEnabled; set => captionsEnabled = value; }
            public bool ScreenShakeEnabled { get => screenShakeEnabled; set => screenShakeEnabled = value; }
            public bool ReducedMotion { get => reducedMotion; set => reducedMotion = value; }
            public float Brightness { get => brightness; set => brightness = value; }
            public int QualityTier { get => qualityTier; set => qualityTier = value; }
            public bool CameraInvertY { get => cameraInvertY; set => cameraInvertY = value; }
            public int CameraDistanceValue { get => cameraDistance; set => cameraDistance = value; }
            public float FieldOfView { get => fieldOfView; set => fieldOfView = value; }
            public int DifficultyValue { get => difficulty; set => difficulty = value; }
            public int EffectiveDifficultyValue { get => effectiveDifficulty; set => effectiveDifficulty = value; }

            public void FromSettings(SettingsData data)
            {
                if (data == null) return;
                masterVolume = data.MasterVolume;
                musicVolume = data.MusicVolume;
                sfxVolume = data.SfxVolume;
                comedyVolume = data.ComedyVolume;
                comedyLevel = (int)data.ComedyLevel;
                captionsEnabled = data.CaptionsEnabled;
                screenShakeEnabled = data.ScreenShakeEnabled;
                reducedMotion = data.ReducedMotion;
                brightness = data.Brightness;
                qualityTier = (int)data.QualityTier;
                cameraInvertY = data.CameraInvertY;
                cameraDistance = (int)data.CameraDistance;
                fieldOfView = data.FieldOfView;
                difficulty = (int)data.CurrentDifficulty;
                effectiveDifficulty = (int)data.EffectiveDifficulty;
            }

            public void ToSettings(SettingsData data)
            {
                if (data == null) return;
                data.MasterVolume = masterVolume;
                data.MusicVolume = musicVolume;
                data.SfxVolume = sfxVolume;
                data.ComedyVolume = comedyVolume;
                data.ComedyLevel = (comedyLevel < 0) ? ComedyLevel.Full : (ComedyLevel)comedyLevel;
                data.CaptionsEnabled = captionsEnabled;
                data.ScreenShakeEnabled = screenShakeEnabled;
                data.ReducedMotion = reducedMotion;
                data.Brightness = brightness;
                data.QualityTier = (QualityTier)qualityTier;
                data.CameraInvertY = cameraInvertY;
                data.CameraDistance = (CameraDistancePreset)cameraDistance;
                data.FieldOfView = fieldOfView;
                data.CurrentDifficulty = (Difficulty)difficulty;
                data.EffectiveDifficulty = (Difficulty)effectiveDifficulty;
            }
        }

        [Header("Settings Data")]
        [Tooltip("Optional reference to DefaultSettings ScriptableObject.")]
        [SerializeField] private SettingsData defaultSettings;

        [Tooltip("Active runtime settings copy.")]
        [SerializeField] private SettingsData runtimeSettings;

        [Header("Difficulty Configuration")]
        [Tooltip("Master difficulty configuration presets.")]
        [SerializeField] private DifficultyConfig difficultyConfig;

        public event Action<SettingsData> OnSettingsChanged;

        public SettingsData CurrentSettings => runtimeSettings;
        public DifficultyConfig DifficultyConfig => difficultyConfig;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }

            InitializeSettings();
        }

        public void InitializeSettings()
        {
            if (Instance == null) Instance = this;
            if (difficultyConfig == null)
            {
                difficultyConfig = Resources.Load<DifficultyConfig>("SO_DifficultyConfig") ?? Resources.Load<DifficultyConfig>("DifficultyConfig");
#if UNITY_EDITOR
                if (difficultyConfig == null)
                {
                    difficultyConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<DifficultyConfig>("Assets/ScriptableObjects/SO_DifficultyConfig.asset")
                                    ?? UnityEditor.AssetDatabase.LoadAssetAtPath<DifficultyConfig>("Assets/ScriptableObjects/DifficultyConfig.asset");
                }
#endif
            }

            if (defaultSettings == null)
            {
                defaultSettings = Resources.Load<SettingsData>("SO_Settings") ?? Resources.Load<SettingsData>("DefaultSettings");
#if UNITY_EDITOR
                if (defaultSettings == null)
                {
                    defaultSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<SettingsData>("Assets/ScriptableObjects/SO_Settings.asset")
                                   ?? UnityEditor.AssetDatabase.LoadAssetAtPath<SettingsData>("Assets/ScriptableObjects/DefaultSettings.asset");
                }
#endif
            }

            if (runtimeSettings == null)
            {
                runtimeSettings = ScriptableObject.CreateInstance<SettingsData>();
                if (defaultSettings != null)
                {
                    runtimeSettings.CopyFrom(defaultSettings);
                }
                else
                {
                    runtimeSettings.ResetToDefaults();
                }
            }

            LoadSettings();
        }

        public void LoadSettings()
        {
            if (runtimeSettings == null)
            {
                InitializeSettings();
                return;
            }

            if (PlayerPrefs.HasKey(SettingsPrefKey))
            {
                try
                {
                    string json = PlayerPrefs.GetString(SettingsPrefKey);
                    var serializable = JsonUtility.FromJson<SerializableSettings>(json);
                    if (serializable != null)
                    {
                        serializable.ToSettings(runtimeSettings);
                    }
                }
                catch (Exception)
                {
                    ResetToDefaults();
                }
            }
            else
            {
                if (defaultSettings != null)
                {
                    runtimeSettings.CopyFrom(defaultSettings);
                }
                else
                {
                    runtimeSettings.ResetToDefaults();
                }
                SaveSettings();
            }

            ApplyCurrentSettings();
        }

        public void SaveSettings()
        {
            if (runtimeSettings == null) return;

            var serializable = new SerializableSettings();
            serializable.FromSettings(runtimeSettings);
            string json = JsonUtility.ToJson(serializable);
            PlayerPrefs.SetString(SettingsPrefKey, json);
            PlayerPrefs.Save();

            ApplyCurrentSettings();
        }

        public void ResetToDefaults()
        {
            if (runtimeSettings == null)
            {
                InitializeSettings();
            }

            if (defaultSettings != null)
            {
                runtimeSettings.CopyFrom(defaultSettings);
            }
            else
            {
                runtimeSettings.ResetToDefaults();
            }

            SaveSettings();
        }

        public void SetMasterVolume(float val)
        {
            if (runtimeSettings == null) return;
            runtimeSettings.MasterVolume = val;
            SaveSettings();
        }

        public void SetMusicVolume(float val)
        {
            if (runtimeSettings == null) return;
            runtimeSettings.MusicVolume = val;
            SaveSettings();
        }

        public void SetSfxVolume(float val)
        {
            if (runtimeSettings == null) return;
            runtimeSettings.SfxVolume = val;
            SaveSettings();
        }

        public void SetComedyVolume(float val)
        {
            if (runtimeSettings == null) return;
            runtimeSettings.ComedyVolume = val;
            SaveSettings();
        }

        public void SetComedyLevel(ComedyLevel level)
        {
            if (runtimeSettings == null) return;
            runtimeSettings.ComedyLevel = level;
            SaveSettings();
        }

        public void SetCaptions(bool enabled)
        {
            if (runtimeSettings == null) return;
            runtimeSettings.CaptionsEnabled = enabled;
            SaveSettings();
        }

        public void SetScreenShake(bool enabled)
        {
            if (runtimeSettings == null) return;
            runtimeSettings.ScreenShakeEnabled = enabled;
            SaveSettings();
        }

        public void SetReducedMotion(bool enabled)
        {
            if (runtimeSettings == null) return;
            runtimeSettings.ReducedMotion = enabled;
            SaveSettings();
        }

        public void SetBrightness(float val)
        {
            if (runtimeSettings == null) return;
            runtimeSettings.Brightness = val;
            SaveSettings();
        }

        public void SetQualityTier(QualityTier tier)
        {
            if (runtimeSettings == null) return;
            runtimeSettings.QualityTier = tier;
            SaveSettings();
        }

        public void SetCameraInvertY(bool invert)
        {
            if (runtimeSettings == null) return;
            runtimeSettings.CameraInvertY = invert;
            SaveSettings();
        }

        public void SetCameraDistance(CameraDistancePreset preset)
        {
            if (runtimeSettings == null) return;
            runtimeSettings.CameraDistance = preset;
            SaveSettings();
        }

        public void SetFieldOfView(float fov)
        {
            if (runtimeSettings == null) return;
            runtimeSettings.FieldOfView = fov;
            SaveSettings();
        }

        public void ResolveDifficultyConfig()
        {
            if (difficultyConfig == null)
            {
                difficultyConfig = Resources.Load<DifficultyConfig>("SO_DifficultyConfig") ?? Resources.Load<DifficultyConfig>("DifficultyConfig");
#if UNITY_EDITOR
                if (difficultyConfig == null)
                {
                    difficultyConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<DifficultyConfig>("Assets/ScriptableObjects/SO_DifficultyConfig.asset")
                                    ?? UnityEditor.AssetDatabase.LoadAssetAtPath<DifficultyConfig>("Assets/ScriptableObjects/DifficultyConfig.asset");
                }
#endif
            }
        }

        public void SetDifficulty(Difficulty diff)
        {
            if (runtimeSettings == null)
            {
                InitializeSettings();
            }
            if (runtimeSettings == null) return;

            runtimeSettings.CurrentDifficulty = diff;
            ResolveDifficultyConfig();

            if (difficultyConfig != null)
            {
                runtimeSettings.EffectiveDifficulty = difficultyConfig.GetEffectiveDifficulty(diff);
            }
            else
            {
                runtimeSettings.EffectiveDifficulty = GetFallbackEffectiveDifficulty(diff);
            }
            SaveSettings();
        }

        public static Difficulty GetFallbackEffectiveDifficulty(Difficulty diff)
        {
            switch (diff)
            {
                case Difficulty.Hard: return Difficulty.Easy;
                case Difficulty.Nightmare: return Difficulty.Normal;
                default: return diff;
            }
        }

        public void SetDifficulty(Difficulty selectedDiff, Difficulty effectiveDiff)
        {
            if (runtimeSettings == null)
            {
                InitializeSettings();
            }
            if (runtimeSettings == null) return;

            runtimeSettings.CurrentDifficulty = selectedDiff;
            runtimeSettings.EffectiveDifficulty = effectiveDiff;
            SaveSettings();
        }

        public DifficultyPreset GetCurrentDifficultyPreset()
        {
            ResolveDifficultyConfig();
            Difficulty diff = runtimeSettings != null ? runtimeSettings.EffectiveDifficulty : Difficulty.Normal;
            if (difficultyConfig != null)
            {
                return difficultyConfig.GetPreset(diff);
            }
            return GetFallbackPreset(diff);
        }

        public static DifficultyPreset GetFallbackPreset(Difficulty diff)
        {
            switch (diff)
            {
                case Difficulty.Easy:
                    return new DifficultyPreset(0.8f, 1.3f, 5, 5, 1.3f, 0.5f);
                case Difficulty.Normal:
                    return new DifficultyPreset(1.0f, 1.0f, 7, 3, 1.0f, 1.0f);
                case Difficulty.Hard:
                    return new DifficultyPreset(1.2f, 0.8f, 9, 3, 0.9f, 1.5f);
                case Difficulty.Nightmare:
                    return new DifficultyPreset(1.4f, 0.65f, 11, 2, 0.8f, 2.0f);
                default:
                    return new DifficultyPreset(1.0f, 1.0f, 7, 3, 1.0f, 1.0f);
            }
        }

        private void ApplyCurrentSettings()
        {
            if (runtimeSettings == null) return;

            AudioListener.volume = runtimeSettings.MasterVolume;

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.MusicVolume = runtimeSettings.MusicVolume;
                SoundManager.Instance.SfxVolume = runtimeSettings.SfxVolume;
            }

            if (AudioManager.Instance != null && AudioManager.Instance.Mixer != null)
            {
                float comedyDb = runtimeSettings.ComedyVolume > 0.0001f ? Mathf.Log10(runtimeSettings.ComedyVolume) * 20f : -80f;
                AudioManager.Instance.Mixer.SetFloat("ComedyVolume", comedyDb);
            }

            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.SetScreenShakeEnabled(runtimeSettings.ScreenShakeEnabled);
            }

            OnSettingsChanged?.Invoke(runtimeSettings);
        }
    }
}
