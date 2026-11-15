using System;
using UnityEngine;
using DodgeGame.Data;

namespace DodgeGame.Core
{
    public class QualityManager : MonoBehaviour
    {
        public static QualityManager Instance { get; private set; }

        private const string QUALITY_DETECTED_KEY = "DodgeGame_Quality_AutoDetected";

        [Header("State Inspection")]
        [SerializeField] private QualityTier currentTier = QualityTier.High;

        public event Action<QualityTier> OnQualityChanged;

        public QualityTier CurrentTier => currentTier;

        public float MaxLightDistance => currentTier switch
        {
            QualityTier.High => 25.0f,
            QualityTier.Medium => 15.0f,
            _ => 0.0f
        };

        public bool EnablePointLight => currentTier != QualityTier.Low;

        public int EmberEmissionRate => currentTier switch
        {
            QualityTier.High => 8,
            QualityTier.Medium => 4,
            _ => 0
        };

        public float TrailLifetime => currentTier == QualityTier.Low ? 0.18f : 0.35f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            InitializeQuality();
        }

        private void OnEnable()
        {
            if (SettingsService.Instance != null)
            {
                SettingsService.Instance.OnSettingsChanged += HandleSettingsChanged;
            }
        }

        private void OnDisable()
        {
            if (SettingsService.Instance != null)
            {
                SettingsService.Instance.OnSettingsChanged -= HandleSettingsChanged;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void InitializeQuality()
        {
            if (!PlayerPrefs.HasKey(QUALITY_DETECTED_KEY))
            {
                QualityTier detected = DetectHardwareTier();
                PlayerPrefs.SetInt(QUALITY_DETECTED_KEY, 1);
                PlayerPrefs.Save();

                if (SettingsService.Instance != null)
                {
                    SettingsService.Instance.SetQualityTier(detected);
                }
                currentTier = detected;
            }
            else if (SettingsService.Instance != null && SettingsService.Instance.CurrentSettings != null)
            {
                currentTier = SettingsService.Instance.CurrentSettings.QualityTier;
            }
        }

        public static QualityTier DetectHardwareTier()
        {
            int vramMB = SystemInfo.graphicsMemorySize;
            int cpuCores = SystemInfo.processorCount;

            if (vramMB > 0 && vramMB < 2048 || cpuCores <= 4)
            {
                return QualityTier.Low;
            }
            if (vramMB > 0 && vramMB < 4096)
            {
                return QualityTier.Medium;
            }
            return QualityTier.High;
        }

        public void SetQuality(QualityTier tier)
        {
            if (currentTier == tier) return;

            currentTier = tier;
            if (SettingsService.Instance != null && SettingsService.Instance.CurrentSettings != null)
            {
                if (SettingsService.Instance.CurrentSettings.QualityTier != tier)
                {
                    SettingsService.Instance.SetQualityTier(tier);
                }
            }

            OnQualityChanged?.Invoke(currentTier);
        }

        private void HandleSettingsChanged(SettingsData settings)
        {
            if (settings != null && settings.QualityTier != currentTier)
            {
                currentTier = settings.QualityTier;
                OnQualityChanged?.Invoke(currentTier);
            }
        }
    }
}
