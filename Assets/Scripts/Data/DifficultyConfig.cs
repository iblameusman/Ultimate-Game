using System;
using UnityEngine;

namespace DodgeGame.Data
{
    [Serializable]
    public struct DifficultyPreset
    {
        [Tooltip("Multiplier applied to ball speed.")]
        public float BallSpeedMultiplier;

        [Tooltip("Multiplier applied to ball spawn intervals (higher = slower spawn rate).")]
        public float SpawnIntervalMultiplier;

        [Tooltip("Maximum active balls allowed simultaneously in the arena.")]
        public int MaxActiveBalls;

        [Tooltip("Starting player lives.")]
        public int StartingLives;

        [Tooltip("Multiplier applied to skill meter gain from dodges.")]
        public float SkillMeterGainMultiplier;

        [Tooltip("Multiplier applied to score accumulation rate.")]
        public float ScoreMultiplier;

        public int Lives => StartingLives;

        public DifficultyPreset(float speed, float interval, int maxBalls, int lives, float meterGain, float scoreMult)
        {
            BallSpeedMultiplier = speed;
            SpawnIntervalMultiplier = interval;
            MaxActiveBalls = maxBalls;
            StartingLives = lives;
            SkillMeterGainMultiplier = meterGain;
            ScoreMultiplier = scoreMult;
        }
    }

    [Serializable]
    public class DifficultyMappingEntry
    {
        [Tooltip("The difficulty displayed to the player.")]
        [SerializeField] private Difficulty displayName;

        [Tooltip("The preset whose gameplay values are actually used.")]
        [SerializeField] private Difficulty effectiveTier;

        [Tooltip("Whether selecting this difficulty is blocked from starting the game.")]
        [SerializeField] private bool blocksStart;

        [Tooltip("Audio clip ID to play when blocked selection is attempted.")]
        [SerializeField] private string rejectVoiceClipId = "";

        public Difficulty DisplayName { get => displayName; set => displayName = value; }
        public Difficulty EffectiveTier { get => effectiveTier; set => effectiveTier = value; }
        public bool BlocksStart { get => blocksStart; set => blocksStart = value; }
        public string RejectVoiceClipId { get => rejectVoiceClipId; set => rejectVoiceClipId = value; }

        public DifficultyMappingEntry() { }

        public DifficultyMappingEntry(Difficulty displayName, Difficulty effectiveTier, bool blocksStart, string rejectVoiceClipId = "")
        {
            this.displayName = displayName;
            this.effectiveTier = effectiveTier;
            this.blocksStart = blocksStart;
            this.rejectVoiceClipId = rejectVoiceClipId;
        }
    }

    [CreateAssetMenu(fileName = "DifficultyConfig", menuName = "DodgeGame/Difficulty Config")]
    public class DifficultyConfig : ScriptableObject
    {
        [Header("Difficulty Presets")]
        [SerializeField] private DifficultyPreset easy = new DifficultyPreset(0.8f, 1.3f, 5, 5, 1.3f, 0.75f);
        [SerializeField] private DifficultyPreset normal = new DifficultyPreset(1.0f, 1.0f, 7, 3, 1.0f, 1.0f);
        [SerializeField] private DifficultyPreset hard = new DifficultyPreset(1.2f, 0.8f, 9, 3, 0.9f, 1.5f);
        [SerializeField] private DifficultyPreset nightmare = new DifficultyPreset(1.4f, 0.65f, 11, 2, 0.8f, 2.0f);

        [Header("Selection Rules & Mapping")]
        [SerializeField] private DifficultyMappingEntry[] mappingTable = new DifficultyMappingEntry[]
        {
            new DifficultyMappingEntry(Difficulty.Easy, Difficulty.Easy, true, "UI_REJECT_EASY"),
            new DifficultyMappingEntry(Difficulty.Normal, Difficulty.Normal, true, "UI_REJECT_NORMAL"),
            new DifficultyMappingEntry(Difficulty.Hard, Difficulty.Easy, false, ""),
            new DifficultyMappingEntry(Difficulty.Nightmare, Difficulty.Normal, false, "")
        };

        [Header("Plot Twist")]
        [Tooltip("If true, shows the plot twist reveal line on Game Over when DisplayName != EffectiveTier.")]
        [SerializeField] private bool revealTwist = true;

        public DifficultyPreset Easy => easy;
        public DifficultyPreset Normal => normal;
        public DifficultyPreset Hard => hard;
        public DifficultyPreset Nightmare => nightmare;

        public DifficultyMappingEntry[] MappingTable => mappingTable;
        public bool RevealTwist { get => revealTwist; set => revealTwist = value; }

        public DifficultyMappingEntry GetMapping(Difficulty displayName)
        {
            if (mappingTable != null)
            {
                for (int i = 0; i < mappingTable.Length; i++)
                {
                    if (mappingTable[i].DisplayName == displayName) return mappingTable[i];
                }
            }

            switch (displayName)
            {
                case Difficulty.Easy:
                    return new DifficultyMappingEntry(Difficulty.Easy, Difficulty.Easy, true, "UI_REJECT_EASY");
                case Difficulty.Normal:
                    return new DifficultyMappingEntry(Difficulty.Normal, Difficulty.Normal, true, "UI_REJECT_NORMAL");
                case Difficulty.Hard:
                    return new DifficultyMappingEntry(Difficulty.Hard, Difficulty.Easy, false, "");
                case Difficulty.Nightmare:
                    return new DifficultyMappingEntry(Difficulty.Nightmare, Difficulty.Normal, false, "");
                default:
                    return new DifficultyMappingEntry(displayName, displayName, false, "");
            }
        }

        public Difficulty GetEffectiveDifficulty(Difficulty displayName)
        {
            var mapping = GetMapping(displayName);
            if (mapping != null)
            {
                return mapping.EffectiveTier;
            }

            switch (displayName)
            {
                case Difficulty.Hard: return Difficulty.Easy;
                case Difficulty.Nightmare: return Difficulty.Normal;
                default: return displayName;
            }
        }

        public float GetScoreMultiplier(Difficulty difficulty)
        {
            return GetPreset(difficulty).ScoreMultiplier;
        }

        public DifficultyPreset GetPreset(Difficulty difficulty)
        {
            switch (difficulty)
            {
                case Difficulty.Easy: return easy;
                case Difficulty.Normal: return normal;
                case Difficulty.Hard: return hard;
                case Difficulty.Nightmare: return nightmare;
                default: return normal;
            }
        }
    }
}
