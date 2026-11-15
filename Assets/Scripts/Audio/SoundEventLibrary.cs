using System;
using UnityEngine;

namespace DodgeGame.Audio
{
    [CreateAssetMenu(fileName = "SoundEventLibrary", menuName = "DodgeGame/Audio/Sound Event Library")]
    public class SoundEventLibrary : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public SoundEventId eventId;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 1f;
        }

        public const string ResourcePath = "Audio/SoundEventLibrary";

        [SerializeField] private Entry[] entries = Array.Empty<Entry>();

        private static SoundEventLibrary cached;

        public static SoundEventLibrary Load()
        {
            if (cached == null) cached = Resources.Load<SoundEventLibrary>(ResourcePath);
            return cached;
        }

        public Entry Get(SoundEventId id)
        {
            foreach (var e in entries)
            {
                if (e != null && e.eventId == id) return e;
            }
            return null;
        }
    }
}
