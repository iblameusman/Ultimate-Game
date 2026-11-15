using System.Collections;
using UnityEngine;
using DodgeGame.Data;

namespace DodgeGame.Core
{
    public class HitStop : MonoBehaviour
    {
        public static HitStop Instance { get; private set; }

        private const string RequesterId = "HitStop";

        [Header("Configuration")]
        [Tooltip("Juice configuration providing hit stop freeze duration.")]
        [SerializeField] private JuiceConfig juiceConfig;

        private bool isHitStopping = false;
        private Coroutine activeHitStopRoutine;

        public bool IsHitStopping => isHitStopping;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            ResolveConfig();
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerHit += HandlePlayerHit;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerHit -= HandlePlayerHit;
            CleanupHitStop();
        }

        private void OnDestroy()
        {
            CleanupHitStop();
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void ResolveConfig()
        {
            if (juiceConfig == null)
            {
                juiceConfig = Resources.Load<JuiceConfig>("SO_Juice") ?? Resources.Load<JuiceConfig>("DefaultJuice");
#if UNITY_EDITOR
                if (juiceConfig == null)
                {
                    juiceConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<JuiceConfig>("Assets/ScriptableObjects/SO_Juice.asset")
                               ?? UnityEditor.AssetDatabase.LoadAssetAtPath<JuiceConfig>("Assets/ScriptableObjects/DefaultJuice.asset");
                }
#endif
            }
        }

        private void HandlePlayerHit(int livesRemaining)
        {
            TriggerHitStop();
        }

        public void TriggerHitStop()
        {
            if (isHitStopping) return;

            ResolveConfig();
            float duration = juiceConfig != null ? juiceConfig.HitStopDuration : 0.05f;

            if (duration <= 0f) return;

            activeHitStopRoutine = StartCoroutine(HitStopRoutine(duration));
        }

        private IEnumerator HitStopRoutine(float duration)
        {
            isHitStopping = true;

            if (TimeScaleController.Instance != null)
            {
                TimeScaleController.Instance.RequestTimeScale(0f, 0f, RequesterId);
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (TimeScaleController.Instance != null)
            {
                TimeScaleController.Instance.ReleaseTimeScale(RequesterId);
            }

            isHitStopping = false;
            activeHitStopRoutine = null;
        }

        private void CleanupHitStop()
        {
            if (activeHitStopRoutine != null)
            {
                StopCoroutine(activeHitStopRoutine);
                activeHitStopRoutine = null;
            }

            if (isHitStopping)
            {
                if (TimeScaleController.Instance != null)
                {
                    TimeScaleController.Instance.ReleaseTimeScale(RequesterId);
                }
                isHitStopping = false;
            }
        }
    }
}
