using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DodgeGame.Core
{
    public class DodgeStreakTracker : MonoBehaviour
    {
        public static DodgeStreakTracker Instance { get; private set; }

        public const float DodgeConfirmationDelay = 0.5f;

        [Header("Runtime State")]
        [SerializeField] private int currentStreak = 0;

        private readonly HashSet<int> reachedThresholds = new HashSet<int>();
        private Coroutine pendingConfirmationRoutine;
        private int pendingNearMissId = 0;

        public int CurrentStreak => currentStreak;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            GameEvents.OnNearMiss -= HandleNearMiss;
            GameEvents.OnNearMiss += HandleNearMiss;

            GameEvents.OnPlayerHit -= HandlePlayerHit;
            GameEvents.OnPlayerHit += HandlePlayerHit;

            GameEvents.OnGameOver -= HandleGameOver;
            GameEvents.OnGameOver += HandleGameOver;
        }

        private void OnDisable()
        {
            GameEvents.OnNearMiss -= HandleNearMiss;
            GameEvents.OnPlayerHit -= HandlePlayerHit;
            GameEvents.OnGameOver -= HandleGameOver;
        }

        public void HandleNearMiss(PlayerMovementState state)
        {
            pendingNearMissId++;
            int thisId = pendingNearMissId;
            if (pendingConfirmationRoutine != null)
            {
            }
            pendingConfirmationRoutine = StartCoroutine(ConfirmDodgeRoutine(thisId));
        }

        private IEnumerator ConfirmDodgeRoutine(int id)
        {
            float elapsed = 0f;
            while (elapsed < DodgeConfirmationDelay)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            ConfirmDodge();
        }

        public void ConfirmDodge()
        {
            currentStreak++;
            GameEvents.TriggerPlayerDodged(currentStreak);

            CheckStreakThreshold(currentStreak);
        }

        private void CheckStreakThreshold(int streak)
        {
            bool isThreshold = false;
            if (streak == 5 || streak == 10 || streak == 15 || (streak >= 20 && streak % 5 == 0))
            {
                isThreshold = true;
            }

            if (isThreshold && !reachedThresholds.Contains(streak))
            {
                reachedThresholds.Add(streak);
                GameEvents.TriggerStreakChanged(streak);
            }
        }

        public void HandlePlayerHit(int livesRemaining)
        {
            if (pendingConfirmationRoutine != null)
            {
                StopCoroutine(pendingConfirmationRoutine);
                pendingConfirmationRoutine = null;
            }
            pendingNearMissId++;

            BreakStreak();
        }

        public void BreakStreak()
        {
            int oldStreak = currentStreak;
            currentStreak = 0;
            reachedThresholds.Clear();

            if (oldStreak >= 5)
            {
                GameEvents.TriggerStreakBroken(oldStreak);
            }
        }

        private void HandleGameOver(int score)
        {
            if (pendingConfirmationRoutine != null)
            {
                StopCoroutine(pendingConfirmationRoutine);
                pendingConfirmationRoutine = null;
            }
            currentStreak = 0;
            reachedThresholds.Clear();
        }

        public void ResetTracker()
        {
            if (pendingConfirmationRoutine != null)
            {
                StopCoroutine(pendingConfirmationRoutine);
                pendingConfirmationRoutine = null;
            }
            currentStreak = 0;
            reachedThresholds.Clear();
        }
    }
}
