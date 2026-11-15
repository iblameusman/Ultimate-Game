using System;
using UnityEngine;
using DodgeGame.Data;

namespace DodgeGame.Core
{
    public class AudioEventRouter : MonoBehaviour
    {
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
            GameEvents.OnNearMiss -= HandleNearMiss;
            GameEvents.OnNearMiss += HandleNearMiss;

            GameEvents.OnStreakChanged -= HandleStreakChanged;
            GameEvents.OnStreakChanged += HandleStreakChanged;

            GameEvents.OnStreakBroken -= HandleStreakBroken;
            GameEvents.OnStreakBroken += HandleStreakBroken;

            GameEvents.OnPlayerHitDetails -= HandlePlayerHitDetails;
            GameEvents.OnPlayerHitDetails += HandlePlayerHitDetails;

            GameEvents.OnPlayerHit -= HandlePlayerHit;
            GameEvents.OnPlayerHit += HandlePlayerHit;

            GameEvents.OnBallSpawned -= HandleBallSpawned;
            GameEvents.OnBallSpawned += HandleBallSpawned;

            GameEvents.OnGameOver -= HandleGameOver;
            GameEvents.OnGameOver += HandleGameOver;

            GameEvents.OnSkillUsed -= HandleSkillUsed;
            GameEvents.OnSkillUsed += HandleSkillUsed;

            GameEvents.OnPlayerDodged -= HandlePlayerDodged;
            GameEvents.OnPlayerDodged += HandlePlayerDodged;
        }

        public void UnsubscribeEvents()
        {
            GameEvents.OnNearMiss -= HandleNearMiss;
            GameEvents.OnStreakChanged -= HandleStreakChanged;
            GameEvents.OnStreakBroken -= HandleStreakBroken;
            GameEvents.OnPlayerHitDetails -= HandlePlayerHitDetails;
            GameEvents.OnPlayerHit -= HandlePlayerHit;
            GameEvents.OnBallSpawned -= HandleBallSpawned;
            GameEvents.OnGameOver -= HandleGameOver;
            GameEvents.OnSkillUsed -= HandleSkillUsed;
            GameEvents.OnPlayerDodged -= HandlePlayerDodged;
        }

        private void HandleNearMiss(PlayerMovementState state)
        {
            if (AudioManager.Instance == null) return;

            string clipId = state switch
            {
                PlayerMovementState.Airborne => "CD_02",
                PlayerMovementState.Dashing => "CD_03",
                PlayerMovementState.Phased => "CD_04",
                _ => "CD_01"
            };

            AudioManager.Instance.Play(clipId);
        }

        private void HandleStreakChanged(int streak)
        {
            if (AudioManager.Instance == null) return;

            switch (streak)
            {
                case 5:
                    AudioManager.Instance.Play("ST_05");
                    break;
                case 10:
                    AudioManager.Instance.Play("ST_10");
                    break;
                case 15:
                    AudioManager.Instance.Play("ST_15");
                    break;
                case 20:
                    AudioManager.Instance.Play("ST_20");
                    break;
                default:
                    if (streak > 20 && streak % 5 == 0)
                    {
                        AudioManager.Instance.PlayDelayed("ST_20", 0.15f);
                    }
                    break;
            }
        }

        private void HandleStreakBroken(int streak)
        {
            if (AudioManager.Instance == null) return;

            if (streak >= 5)
            {
                AudioManager.Instance.Play("ST_BREAK");
            }
        }

        private void HandlePlayerHitDetails(PlayerHitDetails hit)
        {
            if (AudioManager.Instance == null) return;

            if (hit.IsLastLife)
            {
                AudioManager.Instance.Play("DH_05");
                AudioManager.Instance.PlayDelayed("DH_06", 1.5f);
            }
            else if (hit.IsSticky)
            {
                AudioManager.Instance.Play("DH_04");
            }
            else if (hit.IsShieldOrPhaseBreak)
            {
                AudioManager.Instance.Play("DH_03");
            }
            else if (hit.StreakBeforeHit >= 10)
            {
                AudioManager.Instance.Play("DH_02");
                AudioManager.Instance.PlayDelayed("DH_01", 1.2f);
            }
            else
            {
                AudioManager.Instance.Play("DH_01");
            }
        }

        private void HandlePlayerHit(int livesRemaining)
        {
        }

        private void HandleBallSpawned(BallSpawnInfo info)
        {
            if (AudioManager.Instance == null) return;

            if (info.IsRare)
            {
                AudioManager.Instance.Play("SB_RARE");
            }
            else if (info.Category == BallCategory.Nuclear)
            {
                AudioManager.Instance.Play("SB_NUKE");
            }
            else if (info.Category == BallCategory.Gun)
            {
                AudioManager.Instance.Play("SB_GUN");
            }
            else if (info.IsDun)
            {
                AudioManager.Instance.Play("SB_DUN");
            }
            else if (info.Category == BallCategory.Fire)
            {
                AudioManager.Instance.Play("SB_FIRE");
            }
            else if (info.Category == BallCategory.Sticky)
            {
                AudioManager.Instance.Play("SB_STICKY");
            }
            else if (info.IsFast)
            {
                AudioManager.Instance.Play("SB_FAST");
            }
        }

        private void HandleGameOver(int finalScore)
        {
            if (AudioManager.Instance == null) return;
            AudioManager.Instance.CancelClip("DH_05");
            AudioManager.Instance.Play("DH_06");
        }

        private void HandleSkillUsed(SkillType skill)
        {
        }

        private void HandlePlayerDodged(int count)
        {
        }
    }
}
