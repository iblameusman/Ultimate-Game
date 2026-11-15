using System;
using System.Collections.Generic;
using UnityEngine;

namespace DodgeGame.Data
{
    public enum ComedyZone
    {
        CloseDodge,
        Streak,
        DirectHit,
        SpecialSpawn,
        UI
    }

    [Serializable]
    public class ComedyAudioEntry
    {
        [Tooltip("Unique ID for this audio entry (e.g. CD_01, ST_05, DH_01, SB_GUN).")]
        [SerializeField] private string clipId;

        [Tooltip("Gameplay comedy zone.")]
        [SerializeField] private ComedyZone zone;

        [Tooltip("Audio clip variants (1 to 3 clips).")]
        [SerializeField] private AudioClip[] clips;

        [Range(0f, 1f)]
        [SerializeField] private float volume = 0.8f;

        [Range(1, 5)]
        [Tooltip("Priority: 1 = lowest, 5 = highest.")]
        [SerializeField] private int priority = 3;

        [Tooltip("Seconds before this clip ID can be triggered again.")]
        [SerializeField] private float cooldown = 0.25f;

        [Tooltip("Maximum concurrent playing instances of this clip ID.")]
        [SerializeField] private int maxInstances = 2;

        [Tooltip("Whether to apply ±3% random pitch variation on one-shot playback.")]
        [SerializeField] private bool allowPitchVariation = true;

        [Tooltip("Whether playing this clip ducks the Music mixer group.")]
        [SerializeField] private bool duckMusic = false;

        [Tooltip("Decibels to duck music by when duckMusic is true.")]
        [SerializeField] private float duckDb = 4f;

        [Tooltip("Short caption for accessibility display (e.g. [Boing], [Warning!]).")]
        [SerializeField] private string caption = "";

        public string ClipId { get => clipId; set => clipId = value; }
        public ComedyZone Zone { get => zone; set => zone = value; }
        public AudioClip[] Clips { get => clips; set => clips = value; }
        public float Volume { get => volume; set => volume = Mathf.Clamp01(value); }
        public int Priority { get => priority; set => priority = Mathf.Clamp(value, 1, 5); }
        public float Cooldown { get => cooldown; set => cooldown = Mathf.Max(0f, value); }
        public int MaxInstances { get => maxInstances; set => maxInstances = Mathf.Max(1, value); }
        public bool AllowPitchVariation { get => allowPitchVariation; set => allowPitchVariation = value; }
        public bool DuckMusic { get => duckMusic; set => duckMusic = value; }
        public float DuckDb { get => duckDb; set => duckDb = value; }
        public string Caption { get => caption; set => caption = value; }

        public ComedyAudioEntry() { }

        public ComedyAudioEntry(
            string clipId,
            ComedyZone zone,
            float volume,
            int priority,
            float cooldown,
            int maxInstances = 2,
            bool allowPitchVariation = true,
            bool duckMusic = false,
            float duckDb = 4f,
            string caption = "")
        {
            this.clipId = clipId;
            this.zone = zone;
            this.volume = volume;
            this.priority = priority;
            this.cooldown = cooldown;
            this.maxInstances = maxInstances;
            this.allowPitchVariation = allowPitchVariation;
            this.duckMusic = duckMusic;
            this.duckDb = duckDb;
            this.caption = caption;
            this.clips = Array.Empty<AudioClip>();
        }

        public AudioClip GetRandomClip()
        {
            if (clips == null || clips.Length == 0) return null;
            if (clips.Length == 1) return clips[0];
            return clips[UnityEngine.Random.Range(0, clips.Length)];
        }
    }

    [CreateAssetMenu(fileName = "ComedyAudioConfig", menuName = "DodgeGame/Comedy Audio Config")]
    public class ComedyAudioConfig : ScriptableObject
    {
        [SerializeField] private List<ComedyAudioEntry> entries = new List<ComedyAudioEntry>();

        private Dictionary<string, ComedyAudioEntry> entryLookup;

        public IReadOnlyList<ComedyAudioEntry> Entries => entries;

        private void OnEnable()
        {
            BuildLookup();
        }

        public void BuildLookup()
        {
            if (entryLookup == null)
            {
                entryLookup = new Dictionary<string, ComedyAudioEntry>(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                entryLookup.Clear();
            }

            if (entries != null)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    if (entry != null && !string.IsNullOrEmpty(entry.ClipId))
                    {
                        if (!entryLookup.ContainsKey(entry.ClipId))
                        {
                            entryLookup.Add(entry.ClipId, entry);
                        }
                    }
                }
            }
        }

        public ComedyAudioEntry GetEntry(string clipId)
        {
            if (string.IsNullOrEmpty(clipId)) return null;
            if (entryLookup == null || entryLookup.Count != entries.Count)
            {
                BuildLookup();
            }

            if (entryLookup.TryGetValue(clipId, out var entry))
            {
                return entry;
            }
            return null;
        }

        public void SetEntries(List<ComedyAudioEntry> newEntries)
        {
            entries = newEntries ?? new List<ComedyAudioEntry>();
            BuildLookup();
        }

        public bool ValidateConfig(out string errorMessage)
        {
            var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (entries == null || entries.Count == 0)
            {
                errorMessage = "ComedyAudioConfig has no entries.";
                return false;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null)
                {
                    errorMessage = $"Entry at index {i} is null.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(entry.ClipId))
                {
                    errorMessage = $"Entry at index {i} has empty ClipId.";
                    return false;
                }

                if (!seenIds.Add(entry.ClipId))
                {
                    errorMessage = $"Duplicate ClipId detected: '{entry.ClipId}'. Every ClipId must be unique.";
                    return false;
                }

                if (entry.Clips == null || entry.Clips.Length == 0)
                {
                    errorMessage = $"Entry '{entry.ClipId}' has no AudioClip variants assigned.";
                    return false;
                }

                for (int c = 0; c < entry.Clips.Length; c++)
                {
                    if (entry.Clips[c] == null)
                    {
                        errorMessage = $"Entry '{entry.ClipId}' has null AudioClip at index {c}.";
                        return false;
                    }
                }

                if (entry.Volume < 0f || entry.Volume > 1f)
                {
                    errorMessage = $"Entry '{entry.ClipId}' has invalid Volume: {entry.Volume}. Must be between 0 and 1.";
                    return false;
                }

                if (entry.Cooldown < 0f)
                {
                    errorMessage = $"Entry '{entry.ClipId}' has negative Cooldown: {entry.Cooldown}.";
                    return false;
                }

                if (entry.Priority < 1 || entry.Priority > 5)
                {
                    errorMessage = $"Entry '{entry.ClipId}' has invalid Priority: {entry.Priority}. Must be 1 to 5.";
                    return false;
                }

                if (entry.MaxInstances < 1)
                {
                    errorMessage = $"Entry '{entry.ClipId}' has invalid MaxInstances: {entry.MaxInstances}. Must be >= 1.";
                    return false;
                }
            }

            errorMessage = string.Empty;
            return true;
        }

        public static List<ComedyAudioEntry> CreateDefaultEntries()
        {
            return new List<ComedyAudioEntry>
            {
                new ComedyAudioEntry("CD_01", ComedyZone.CloseDodge, 0.7f, 2, 0.25f, 2, true, false, 4f, "[Whoosh]"),
                new ComedyAudioEntry("CD_02", ComedyZone.CloseDodge, 0.7f, 2, 0.25f, 2, true, false, 4f, "[Boing]"),
                new ComedyAudioEntry("CD_03", ComedyZone.CloseDodge, 0.7f, 2, 0.25f, 2, true, false, 4f, "[Zip]"),
                new ComedyAudioEntry("CD_04", ComedyZone.CloseDodge, 0.7f, 2, 0.25f, 2, true, false, 4f, "[Blink]"),

                new ComedyAudioEntry("ST_05", ComedyZone.Streak, 0.85f, 4, 0.5f, 2, true, false, 4f, "[5x Streak!]"),
                new ComedyAudioEntry("ST_10", ComedyZone.Streak, 0.85f, 4, 0.5f, 2, true, false, 4f, "[10x Mega Streak!]"),
                new ComedyAudioEntry("ST_15", ComedyZone.Streak, 0.85f, 4, 0.5f, 2, true, false, 4f, "[15x Ultra Streak!]"),
                new ComedyAudioEntry("ST_20", ComedyZone.Streak, 0.85f, 4, 0.5f, 2, true, false, 4f, "[20x GODLIKE STREAK!]"),
                new ComedyAudioEntry("ST_BREAK", ComedyZone.Streak, 0.85f, 4, 0.5f, 2, true, false, 4f, "[Streak Lost!]"),

                new ComedyAudioEntry("DH_01", ComedyZone.DirectHit, 0.9f, 5, 0.4f, 2, true, false, 4f, "[Bonk!]"),
                new ComedyAudioEntry("DH_02", ComedyZone.DirectHit, 0.9f, 5, 0.4f, 2, false, false, 4f, "[Record Scratch]"),
                new ComedyAudioEntry("DH_03", ComedyZone.DirectHit, 0.9f, 5, 0.4f, 2, true, false, 4f, "[Glass Shatter]"),
                new ComedyAudioEntry("DH_04", ComedyZone.DirectHit, 0.9f, 5, 0.4f, 2, true, false, 4f, "[Wah-Wah]"),
                new ComedyAudioEntry("DH_05", ComedyZone.DirectHit, 0.9f, 5, 0.4f, 2, true, false, 4f, "[Heartbeat...]"),
                new ComedyAudioEntry("DH_06", ComedyZone.DirectHit, 0.9f, 5, 0.0f, 2, false, true, 4f, "[Dramatic Failure]"),

                new ComedyAudioEntry("SB_GUN", ComedyZone.SpecialSpawn, 0.8f, 3, 1.0f, 2, true, false, 4f, "[Gun Ball Warning!]"),
                new ComedyAudioEntry("SB_NUKE", ComedyZone.SpecialSpawn, 0.8f, 3, 1.0f, 2, true, true, 4f, "[Nuclear Alarm!]"),
                new ComedyAudioEntry("SB_FIRE", ComedyZone.SpecialSpawn, 0.8f, 3, 1.0f, 2, true, false, 4f, "[Fire Warning!]"),
                new ComedyAudioEntry("SB_STICKY", ComedyZone.SpecialSpawn, 0.8f, 3, 1.0f, 2, true, false, 4f, "[Sticky Warning!]"),
                new ComedyAudioEntry("SB_FAST", ComedyZone.SpecialSpawn, 0.8f, 3, 1.0f, 2, true, false, 4f, "[Speed Warning!]"),
                new ComedyAudioEntry("SB_RARE", ComedyZone.SpecialSpawn, 0.8f, 3, 1.0f, 2, true, true, 4f, "[Rare Ball Spawn!]"),
                new ComedyAudioEntry("SB_DUN", ComedyZone.SpecialSpawn, 0.8f, 3, 1.0f, 2, true, true, 4f, "[Dun Dun Dun!]"),

                new ComedyAudioEntry("UI_REJECT_EASY", ComedyZone.UI, 0.9f, 5, 0.5f, 1, false, false, 0f, "[Nope! Not that one!]"),
                new ComedyAudioEntry("UI_REJECT_NORMAL", ComedyZone.UI, 0.9f, 5, 0.5f, 1, false, false, 0f, "[Nah, that's too easy for you!]")
            };
        }
    }
}
