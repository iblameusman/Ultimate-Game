using System;
using UnityEngine;
using DodgeGame.Core;
using DodgeGame.Data;

namespace DodgeGame.Player
{
    [ExecuteAlways]
    public class SlowMotionMeter : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Slow motion configuration parameters asset.")]
        [SerializeField] private SlowMotionConfig config;

        [Header("State Inspection")]
        [SerializeField] private float currentMeter = 0f;

        public event Action<float, float> OnChanged;

        public event Action OnMeterFull;

        public float CurrentMeter => currentMeter;
        public float MeterMax => config != null ? config.MeterMax : 100f;
        public float Charge => MeterMax > 0f ? Mathf.Clamp01(currentMeter / MeterMax) : 0f;
        public bool IsReady => currentMeter >= (MeterMax - 0.001f);
        public SlowMotionConfig Config => config;

        public void Initialize(SlowMotionConfig customConfig = null)
        {
            if (customConfig != null) config = customConfig;
            ResolveConfig();
            currentMeter = 0f;
            SubscribeEvents();
        }

        private void Awake()
        {
            ResolveConfig();
            currentMeter = 0f;
        }

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        public void SubscribeEvents()
        {
            UnsubscribeEvents();
            GameEvents.OnPlayerDodged += HandlePlayerDodged;
            GameEvents.OnPlayerHit += HandlePlayerHit;
            GameEvents.OnGameOver += HandleGameOver;
        }

        public void UnsubscribeEvents()
        {
            GameEvents.OnPlayerDodged -= HandlePlayerDodged;
            GameEvents.OnPlayerHit -= HandlePlayerHit;
            GameEvents.OnGameOver -= HandleGameOver;
        }

        private void Start()
        {
            ResolveConfig();
            currentMeter = 0f;
            OnChanged?.Invoke(currentMeter, MeterMax);
        }

        private void ResolveConfig()
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
        }

        private void HandlePlayerDodged(int totalDodges)
        {
            ResolveConfig();
            float gain = config != null ? config.MeterPerDodge : 8f;
            if (SettingsService.Instance != null)
            {
                gain *= SettingsService.Instance.GetCurrentDifficultyPreset().SkillMeterGainMultiplier;
            }
            bool wasReady = IsReady;

            currentMeter = Mathf.Clamp(currentMeter + gain, 0f, MeterMax);
            OnChanged?.Invoke(currentMeter, MeterMax);

            if (!wasReady && IsReady)
            {
                OnMeterFull?.Invoke();
            }
        }

        private void HandlePlayerHit(int livesRemaining)
        {
            ResolveConfig();
            float penalty = config != null ? config.MeterPenalty : 15f;

            currentMeter = Mathf.Clamp(currentMeter - penalty, 0f, MeterMax);
            OnChanged?.Invoke(currentMeter, MeterMax);
        }

        private void HandleGameOver(int finalScore)
        {
            ResetMeter();
        }

        public bool ConsumeMeter()
        {
            if (!IsReady) return false;

            currentMeter = 0f;
            OnChanged?.Invoke(currentMeter, MeterMax);
            return true;
        }

        public void ResetMeter()
        {
            currentMeter = 0f;
            OnChanged?.Invoke(currentMeter, MeterMax);
        }

        public void AddMeter(float amount)
        {
            bool wasReady = IsReady;
            currentMeter = Mathf.Clamp(currentMeter + amount, 0f, MeterMax);
            OnChanged?.Invoke(currentMeter, MeterMax);

            if (!wasReady && IsReady)
            {
                OnMeterFull?.Invoke();
            }
        }
    }
}
