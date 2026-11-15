using System.Collections.Generic;
using UnityEngine;

namespace DodgeGame.Core
{
    public class TimeScaleController : MonoBehaviour
    {
        public static TimeScaleController Instance { get; private set; }

        [System.Serializable]
        private class TimeScaleRequest
        {
            [SerializeField] private string requesterId;
            [SerializeField] private float scale;
            [SerializeField] private float duration;
            [SerializeField] private float timeRemaining;

            public string RequesterId => requesterId;
            public float Scale => scale;
            public float Duration => duration;
            public float TimeRemaining
            {
                get => timeRemaining;
                set => timeRemaining = value;
            }

            public TimeScaleRequest(string id, float s, float d)
            {
                requesterId = id;
                scale = s;
                duration = d;
                timeRemaining = d;
            }

            public void Refresh(float s, float d)
            {
                scale = s;
                duration = d;
                timeRemaining = d;
            }
        }

        [Header("State Inspection")]
        [SerializeField] private float currentTimeScale = 1.0f;
        [SerializeField] private List<TimeScaleRequest> activeRequests = new List<TimeScaleRequest>();

        public float CurrentTimeScale => currentTimeScale;
        public int ActiveRequestCount => activeRequests.Count;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            RestoreNormalTime();
        }

        private void OnEnable()
        {
            GameEvents.OnGameOver += HandleGameOver;
        }

        private void OnDisable()
        {
            GameEvents.OnGameOver -= HandleGameOver;
            RestoreNormalTime();
        }

        private void OnDestroy()
        {
            RestoreNormalTime();
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (activeRequests.Count == 0) return;

            bool requestsModified = false;
            float unscaledDelta = Time.unscaledDeltaTime;

            for (int i = activeRequests.Count - 1; i >= 0; i--)
            {
                var req = activeRequests[i];
                if (req.Duration > 0f)
                {
                    req.TimeRemaining -= unscaledDelta;
                    if (req.TimeRemaining <= 0f)
                    {
                        activeRequests.RemoveAt(i);
                        requestsModified = true;
                    }
                }
            }

            if (requestsModified)
            {
                RecalculateScale();
            }
        }

        public void RequestTimeScale(float scale, float duration, string requesterId)
        {
            if (string.IsNullOrEmpty(requesterId)) return;

            scale = Mathf.Max(0f, scale);
            bool found = false;

            for (int i = 0; i < activeRequests.Count; i++)
            {
                if (activeRequests[i].RequesterId == requesterId)
                {
                    activeRequests[i].Refresh(scale, duration);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                activeRequests.Add(new TimeScaleRequest(requesterId, scale, duration));
            }

            RecalculateScale();
        }

        public void ReleaseTimeScale(string requesterId)
        {
            if (string.IsNullOrEmpty(requesterId)) return;

            bool removed = false;
            for (int i = activeRequests.Count - 1; i >= 0; i--)
            {
                if (activeRequests[i].RequesterId == requesterId)
                {
                    activeRequests.RemoveAt(i);
                    removed = true;
                }
            }

            if (removed)
            {
                RecalculateScale();
            }
        }

        public void RestoreNormalTime()
        {
            activeRequests.Clear();
            ApplyScale(1.0f);
        }

        public static void ResetTimeScale()
        {
            if (Instance != null)
            {
                Instance.RestoreNormalTime();
            }
            else
            {
                Time.timeScale = 1.0f;
                Time.fixedDeltaTime = 0.02f;
            }
        }

        private void RecalculateScale()
        {
            if (activeRequests.Count == 0)
            {
                ApplyScale(1.0f);
                return;
            }

            float lowest = activeRequests[0].Scale;
            for (int i = 1; i < activeRequests.Count; i++)
            {
                if (activeRequests[i].Scale < lowest)
                {
                    lowest = activeRequests[i].Scale;
                }
            }

            ApplyScale(lowest);
        }

        private void ApplyScale(float targetScale)
        {
            currentTimeScale = targetScale;
            Time.timeScale = targetScale;
            Time.fixedDeltaTime = 0.02f * targetScale;
        }

        private void HandleGameOver(int finalScore)
        {
            for (int i = activeRequests.Count - 1; i >= 0; i--)
            {
                string id = activeRequests[i].RequesterId;
                if (id != "GameOverSlowMo" && id != "GameOverFreeze")
                {
                    activeRequests.RemoveAt(i);
                }
            }
            RecalculateScale();
        }
    }
}
