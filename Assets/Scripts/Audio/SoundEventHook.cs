using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_WEBGL && !UNITY_EDITOR
using UnityEngine.InputSystem;
#endif
using DodgeGame.Core;

namespace DodgeGame.Audio
{
    public enum SoundEventId
    {
        MenuPlayHover,
        EasySelected,
        MediumSelected,
        GameOver,
        GameplayStart
    }

    [DisallowMultipleComponent]
    public class SoundEventHook : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private SoundEventId eventId;
        [SerializeField] private AudioClip clip;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;

        private bool armed = true;
        private int lastPlayFrame = -1;

        private static AudioSource sharedSource;
        private static SoundEventHook currentOwner;

        public SoundEventId EventId => eventId;
        public AudioClip Clip => clip;

        public static SoundEventHook Attach(GameObject owner, SoundEventId id)
        {
            if (owner == null) return null;

            var hook = owner.GetComponent<SoundEventHook>();
            if (hook == null)
            {
                hook = owner.AddComponent<SoundEventHook>();
                hook.eventId = id;
            }
            else if (hook.eventId != id)
            {
                return null;
            }

            if (hook.clip == null)
            {
                var entry = SoundEventLibrary.Load()?.Get(id);
                if (entry != null)
                {
                    hook.clip = entry.clip;
                    hook.volume = entry.volume;
                }
            }
            return hook;
        }

        public static bool Play(GameObject owner, SoundEventId id)
        {
            if (!Application.isPlaying) return false;
            var hook = Attach(owner, id);
            return hook != null && hook.Play();
        }

        public bool Play()
        {
            if (clip == null || !Application.isPlaying) return false;
            if (!armed) return true;
            if (lastPlayFrame == Time.frameCount) return true;
            if (currentOwner == this && sharedSource != null && sharedSource.isPlaying) return true;

            lastPlayFrame = Time.frameCount;
            if (eventId != SoundEventId.EasySelected && eventId != SoundEventId.MediumSelected)
            {
                armed = false;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            if (!SoundEventHookRunner.UserHasInteracted)
            {
                SoundEventHookRunner.Queue(this);
                return true;
            }
#endif
            PlayNow();
            return true;
        }

        internal void PlayNow()
        {
            var source = GetSharedSource();
            if (source == null) return;

            source.Stop();
            source.clip = clip;
            source.volume = volume;
            source.Play();
            currentOwner = this;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventId == SoundEventId.MenuPlayHover) Play();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (eventId == SoundEventId.MenuPlayHover) armed = true;
        }

        private void OnEnable()
        {
            GameEvents.OnGameRestart += Rearm;
        }

        private void OnDisable()
        {
            GameEvents.OnGameRestart -= Rearm;
        }

        private void Rearm()
        {
            armed = true;
        }

        private static AudioSource GetSharedSource()
        {
            if (sharedSource != null) return sharedSource;

            var go = new GameObject("SoundEventHookAudio");
            DontDestroyOnLoad(go);
            sharedSource = go.AddComponent<AudioSource>();
            sharedSource.playOnAwake = false;
            sharedSource.loop = false;
            sharedSource.spatialBlend = 0f;
            sharedSource.ignoreListenerPause = true;
#if UNITY_WEBGL && !UNITY_EDITOR
            go.AddComponent<SoundEventHookRunner>();
#endif
            return sharedSource;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private class SoundEventHookRunner : MonoBehaviour
        {
            public static bool UserHasInteracted { get; private set; }
            private static SoundEventHook pending;

            public static void Queue(SoundEventHook hook)
            {
                pending = hook;
                GetSharedSource();
            }

            private void Update()
            {
                if (UserHasInteracted) return;

                bool pressed =
                    (Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)) ||
                    (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) ||
                    (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
                    (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
                if (!pressed) return;

                UserHasInteracted = true;
                if (pending != null) pending.PlayNow();
                pending = null;
            }
        }
#endif
    }
}
