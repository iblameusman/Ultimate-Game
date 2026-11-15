using System;
using DodgeGame.Data;

namespace DodgeGame.Core
{
    public enum PlayerMovementState
    {
        Grounded,
        Airborne,
        Dashing,
        Phased
    }

    public enum BallCategory
    {
        Standard,
        Fire,
        Sticky,
        Nuclear,
        Gun
    }

    public struct BallSpawnInfo
    {
        public BallCategory Category;
        public bool IsFast;
        public bool IsRare;
        public bool IsDun;

        public BallSpawnInfo(BallCategory category, bool isFast = false, bool isRare = false, bool isDun = false)
        {
            Category = category;
            IsFast = isFast;
            IsRare = isRare;
            IsDun = isDun;
        }
    }

    public struct PlayerHitDetails
    {
        public int LivesRemaining;
        public bool IsSticky;
        public bool IsShieldOrPhaseBreak;
        public int StreakBeforeHit;
        public bool IsLastLife;

        public PlayerHitDetails(int livesRemaining, bool isSticky = false, bool isShieldOrPhaseBreak = false, int streakBeforeHit = 0, bool isLastLife = false)
        {
            LivesRemaining = livesRemaining;
            IsSticky = isSticky;
            IsShieldOrPhaseBreak = isShieldOrPhaseBreak;
            StreakBeforeHit = streakBeforeHit;
            IsLastLife = isLastLife;
        }
    }

    public static class GameEvents
    {
        public static event Action<int> OnPlayerHit;
        public static event Action<PlayerHitDetails> OnPlayerHitDetails;
        public static event Action<int> OnPlayerDodged;
        public static event Action<SkillType> OnSkillUsed;
        public static event Action<int> OnGameOver;
        public static event Action OnGameRestart;
        public static event Action<PlayerMovementState> OnNearMiss;
        public static event Action<int> OnStreakChanged;
        public static event Action<int> OnStreakBroken;
        public static event Action<BallSpawnInfo> OnBallSpawned;

        public static void TriggerPlayerHit(int livesRemaining)
        {
            OnPlayerHit?.Invoke(livesRemaining);
        }

        public static void TriggerPlayerHit(PlayerHitDetails hitDetails)
        {
            OnPlayerHitDetails?.Invoke(hitDetails);
            OnPlayerHit?.Invoke(hitDetails.LivesRemaining);
        }

        public static void TriggerPlayerDodged(int count)
        {
            OnPlayerDodged?.Invoke(count);
        }

        public static void TriggerSkillUsed(SkillType skill)
        {
            OnSkillUsed?.Invoke(skill);
        }

        public static void TriggerGameOver(int finalScore)
        {
            OnGameOver?.Invoke(finalScore);
        }

        public static void TriggerGameRestart()
        {
            OnGameRestart?.Invoke();
        }

        public static void TriggerNearMiss(PlayerMovementState state)
        {
            OnNearMiss?.Invoke(state);
        }

        public static void TriggerStreakChanged(int streak)
        {
            OnStreakChanged?.Invoke(streak);
        }

        public static void TriggerStreakBroken(int streak)
        {
            OnStreakBroken?.Invoke(streak);
        }

        public static void TriggerBallSpawned(BallSpawnInfo info)
        {
            OnBallSpawned?.Invoke(info);
        }

        public static void ClearAllListeners()
        {
            OnPlayerHit = null;
            OnPlayerHitDetails = null;
            OnPlayerDodged = null;
            OnSkillUsed = null;
            OnGameOver = null;
            OnGameRestart = null;
            OnNearMiss = null;
            OnStreakChanged = null;
            OnStreakBroken = null;
            OnBallSpawned = null;
        }
    }
}
