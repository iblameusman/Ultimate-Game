using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using DodgeGame.Data;

namespace DodgeGame.Core
{
    public enum AudioBusType
    {
        Master,
        Music,
        SFX,
        Comedy,
        UI
    }

    internal class AudioChannelInfo
    {
        public AudioSource Source;
        public int ChannelIndex;
        public string ActiveClipId;
        public ComedyZone? ActiveZone;
        public int Priority;
        public double StartDspTime;
        public double ExpectedEndDspTime;
        public bool IsComedy;
        public bool IsPaused;

        public bool IsFree
        {
            get
            {
                if (Source == null) return true;
                if (IsPaused) return false;
                if (Source.isPlaying) return false;
                return AudioSettings.dspTime >= ExpectedEndDspTime;
            }
        }
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private ComedyAudioConfig comedyConfig;

        [Header("Audio Mixer & Groups")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioMixerGroup masterGroup;
        [SerializeField] private AudioMixerGroup musicGroup;
        [SerializeField] private AudioMixerGroup sfxGroup;
        [SerializeField] private AudioMixerGroup comedyGroup;
        [SerializeField] private AudioMixerGroup uiGroup;

        [Header("Audio Source Pool")]
        [SerializeField] private int poolSize = 16;

        [Header("Fallback Procedural Audio Overrides")]
        [SerializeField] private AudioClip ballBounceClip;
        [SerializeField] private AudioClip fireTileSpawnClip;
        [SerializeField] private AudioClip stickyHitClip;
        [SerializeField] private AudioClip nuclearFuseBeepClip;
        [SerializeField] private AudioClip nuclearExplosionClip;
        [SerializeField] private AudioClip gunFireClip;
        [SerializeField] private AudioClip playerHitClip;
        [SerializeField] private AudioClip dimensionShiftStartClip;
        [SerializeField] private AudioClip dimensionShiftEndClip;
        [SerializeField] private AudioClip slowMoStartClip;
        [SerializeField] private AudioClip slowMoEndClip;
        [SerializeField] private AudioClip gameOverClip;

        private readonly List<AudioChannelInfo> channels = new List<AudioChannelInfo>();

        private readonly Dictionary<string, double> lastPlayedDspTime = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        private int lastDirectHitFrame = -1;
        private double lastDirectHitDspTime = -100.0;

        private readonly List<Coroutine> activeDelayedRoutines = new List<Coroutine>();

        private Coroutine activeDuckRoutine;
        private float originalMusicVolume = 1f;
        private bool isMusicDucked = false;

        private AudioClip synthBounce;
        private AudioClip synthFireTile;
        private AudioClip synthStickyHit;
        private AudioClip synthFuseBeep;
        private AudioClip synthExplosion;
        private AudioClip synthGunFire;
        private AudioClip synthPlayerHit;
        private AudioClip synthShiftStart;
        private AudioClip synthShiftEnd;
        private AudioClip synthSlowMoStart;
        private AudioClip synthSlowMoEnd;
        private AudioClip synthGameOver;
        private AudioClip synthJump;
        private AudioClip synthDash;
        private AudioClip synthSkillReady;

        private bool isMuted = false;

        public ComedyAudioConfig ComedyConfig
        {
            get => comedyConfig;
            set => comedyConfig = value;
        }

        public AudioMixer Mixer => audioMixer;
        public AudioMixerGroup MasterGroup => masterGroup;
        public AudioMixerGroup MusicGroup => musicGroup;
        public AudioMixerGroup SfxGroup => sfxGroup;
        public AudioMixerGroup ComedyGroup => comedyGroup;
        public AudioMixerGroup UiGroup => uiGroup;

        public int PoolSize => poolSize;
        public int ActiveChannelCount
        {
            get
            {
                int c = 0;
                for (int i = 0; i < channels.Count; i++)
                {
                    if (!channels[i].IsFree) c++;
                }
                return c;
            }
        }
        public int ActiveChannels => ActiveChannelCount;

        public bool MuteAudio
        {
            get => isMuted;
            set
            {
                isMuted = value;
                for (int i = 0; i < channels.Count; i++)
                {
                    if (channels[i].Source != null) channels[i].Source.mute = isMuted;
                }
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            ResolveMixerAndConfig();
            InitializePool();
            GenerateProceduralFallbacks();
            EnsureSubsystems();
        }

        private void EnsureSubsystems()
        {
            if (GetComponent<AudioEventRouter>() == null)
            {
                gameObject.AddComponent<AudioEventRouter>();
            }

            if (DodgeStreakTracker.Instance == null && FindAnyObjectByType<DodgeStreakTracker>() == null)
            {
                var streakGo = new GameObject("DodgeStreakTracker");
                streakGo.transform.SetParent(transform);
                streakGo.AddComponent<DodgeStreakTracker>();
            }
        }

        private void OnEnable()
        {
            PauseController.OnGlobalPauseChanged -= HandleGlobalPauseChanged;
            PauseController.OnGlobalPauseChanged += HandleGlobalPauseChanged;

            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= HandleSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            PauseController.OnGlobalPauseChanged -= HandleGlobalPauseChanged;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void HandleSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            ClearQueuedClips();
            StopAllComedyClips();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void ResolveMixerAndConfig()
        {
            if (comedyConfig == null)
            {
                comedyConfig = Resources.Load<ComedyAudioConfig>("ComedyAudioConfig");
            }

            if (audioMixer == null)
            {
                audioMixer = Resources.Load<AudioMixer>("MainAudioMixer");
            }

            if (audioMixer != null)
            {
                if (masterGroup == null)
                {
                    var grps = audioMixer.FindMatchingGroups("Master");
                    if (grps != null && grps.Length > 0) masterGroup = grps[0];
                }
                if (musicGroup == null)
                {
                    var grps = audioMixer.FindMatchingGroups("Music");
                    if (grps != null && grps.Length > 0) musicGroup = grps[0];
                }
                if (sfxGroup == null)
                {
                    var grps = audioMixer.FindMatchingGroups("SFX");
                    if (grps != null && grps.Length > 0) sfxGroup = grps[0];
                }
                if (comedyGroup == null)
                {
                    var grps = audioMixer.FindMatchingGroups("Comedy");
                    if (grps != null && grps.Length > 0) comedyGroup = grps[0];
                }
                if (uiGroup == null)
                {
                    var grps = audioMixer.FindMatchingGroups("UI");
                    if (grps != null && grps.Length > 0) uiGroup = grps[0];
                }
            }
        }

        public void InitializePool()
        {
            channels.Clear();
            for (int i = 0; i < poolSize; i++)
            {
                var channelGo = new GameObject($"AudioChannel_{i}");
                channelGo.transform.SetParent(transform);
                var src = channelGo.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 0f;

                var ch = new AudioChannelInfo
                {
                    Source = src,
                    ChannelIndex = i,
                    ActiveClipId = null,
                    ActiveZone = null,
                    Priority = 1,
                    StartDspTime = 0,
                    ExpectedEndDspTime = 0,
                    IsComedy = false,
                    IsPaused = false
                };
                channels.Add(ch);
            }
        }

        #region Comedy Playback & Conflict Logic

        public bool Play(string clipId)
        {
            if (this == null || string.IsNullOrEmpty(clipId)) return false;

            if (channels.Count == 0)
            {
                InitializePool();
            }

            if (comedyConfig == null)
            {
                ResolveMixerAndConfig();
                if (comedyConfig == null) return false;
            }

            var entry = comedyConfig.GetEntry(clipId);
            if (entry == null)
            {
                Debug.LogWarning($"[AudioManager] ClipId '{clipId}' not found in ComedyAudioConfig.");
                return false;
            }

            if (entry.Zone != ComedyZone.UI)
            {
                var level = GetActiveComedyLevel();
                if (level == ComedyLevel.Off) return false;
                if (level == ComedyLevel.Subtle && entry.Zone == ComedyZone.CloseDodge) return false;
            }

            double currentDsp = AudioSettings.dspTime;
            int currentFrame = Time.frameCount;

            if (entry.Zone == ComedyZone.DirectHit)
            {
                lastDirectHitFrame = currentFrame;
                lastDirectHitDspTime = currentDsp;
                CancelActiveCloseDodges();

                for (int i = 0; i < channels.Count; i++)
                {
                    var ch = channels[i];
                    if (!ch.IsFree && ch.IsComedy && ch.ActiveZone == ComedyZone.SpecialSpawn)
                    {
                        if (Mathf.Abs((float)(ch.StartDspTime - currentDsp)) < 0.05f)
                        {
                            string spawnId = ch.ActiveClipId;
                            if (ch.Source != null && ch.Source.isPlaying)
                            {
                                ch.Source.Stop();
                            }
                            FreeChannel(ch);
                            PlayDelayed(spawnId, 0.5f);
                        }
                    }
                }
            }
            else if (entry.Zone == ComedyZone.Streak)
            {
                if (currentFrame == lastDirectHitFrame || (currentDsp - lastDirectHitDspTime) < 0.05)
                {
                    return false;
                }
            }
            else if (entry.Zone == ComedyZone.SpecialSpawn)
            {
                if (currentFrame == lastDirectHitFrame || (currentDsp - lastDirectHitDspTime) < 0.05)
                {
                    PlayDelayed(clipId, 0.5f);
                    return true;
                }
            }

            if (string.Equals(clipId, "DH_06", StringComparison.OrdinalIgnoreCase))
            {
                StopAllComedyClips();
                ClearQueuedClips();
            }

            if (entry.Cooldown > 0f)
            {
                if (lastPlayedDspTime.TryGetValue(clipId, out double lastTime))
                {
                    if (currentDsp < lastTime + entry.Cooldown)
                    {
                        return false;
                    }
                }
            }

            int activeInstances = GetActiveClipInstanceCount(clipId);
            if (activeInstances >= entry.MaxInstances)
            {
                return false;
            }

            int activeComedy = GetActiveComedyCount();
            if (activeComedy >= 4)
            {
                var lowestComedy = GetLowestPriorityComedyChannel();
                if (lowestComedy != null && lowestComedy.Priority <= entry.Priority)
                {
                    lowestComedy.Source.Stop();
                    FreeChannel(lowestComedy);
                }
                else
                {
                    return false;
                }
            }

            AudioClip clip = entry.GetRandomClip();
            if (clip == null) return false;

            AudioChannelInfo targetChannel = AllocateChannel(entry.Priority);
            if (targetChannel == null) return false;

            float pitch = 1.0f;
            if (entry.AllowPitchVariation &&
                !string.Equals(clipId, "DH_06", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(clipId, "DH_02", StringComparison.OrdinalIgnoreCase))
            {
                pitch = UnityEngine.Random.Range(0.97f, 1.03f);
            }

            EnsureChannelSource(targetChannel);
            var src = targetChannel.Source;
            if (src == null) return false;
            src.clip = clip;
            src.volume = entry.Volume;
            src.pitch = pitch;
            src.spatialBlend = 0f;
            src.outputAudioMixerGroup = (entry.Zone == ComedyZone.UI && uiGroup != null) ? uiGroup : (comedyGroup != null ? comedyGroup : masterGroup);

            targetChannel.ActiveClipId = clipId;
            targetChannel.ActiveZone = entry.Zone;
            targetChannel.Priority = entry.Priority;
            targetChannel.StartDspTime = currentDsp;
            targetChannel.ExpectedEndDspTime = currentDsp + (clip.length / Mathf.Max(0.1f, pitch));
            targetChannel.IsComedy = true;
            targetChannel.IsPaused = false;

            lastPlayedDspTime[clipId] = currentDsp;
            src.Play();

            if (entry.DuckMusic || string.Equals(clipId, "DH_06", StringComparison.OrdinalIgnoreCase))
            {
                float clipDuration = clip.length / Mathf.Max(0.1f, pitch);
                TriggerMusicDuck(clipDuration, entry.DuckDb);
            }

            if (entry.Zone != ComedyZone.CloseDodge && !string.IsNullOrEmpty(entry.Caption))
            {
                DispatchCaption(entry.Caption);
            }

            return true;
        }

        public void PlayDelayed(string clipId, float delaySeconds)
        {
            if (delaySeconds <= 0f)
            {
                Play(clipId);
                return;
            }

            var coroutine = StartCoroutine(PlayDelayedRoutine(clipId, delaySeconds));
            activeDelayedRoutines.Add(coroutine);
        }

        private IEnumerator PlayDelayedRoutine(string clipId, float delaySeconds)
        {
            double targetTime = AudioSettings.dspTime + delaySeconds;
            while (AudioSettings.dspTime < targetTime)
            {
                yield return null;
            }
            Play(clipId);
        }

        public void ClearQueuedClips()
        {
            for (int i = 0; i < activeDelayedRoutines.Count; i++)
            {
                if (activeDelayedRoutines[i] != null)
                {
                    StopCoroutine(activeDelayedRoutines[i]);
                }
            }
            activeDelayedRoutines.Clear();
        }

        public void CancelActiveCloseDodges()
        {
            for (int i = 0; i < channels.Count; i++)
            {
                var ch = channels[i];
                if (!ch.IsFree && ch.IsComedy && ch.ActiveZone == ComedyZone.CloseDodge)
                {
                    if (ch.Source != null && ch.Source.isPlaying)
                    {
                        ch.Source.Stop();
                    }
                    FreeChannel(ch);
                }
            }
        }

        public void CancelClip(string clipId)
        {
            for (int i = 0; i < channels.Count; i++)
            {
                var ch = channels[i];
                if (!ch.IsFree && string.Equals(ch.ActiveClipId, clipId, StringComparison.OrdinalIgnoreCase))
                {
                    if (ch.Source != null && ch.Source.isPlaying)
                    {
                        ch.Source.Stop();
                    }
                    FreeChannel(ch);
                }
            }
        }

        public void StopAllComedyClips()
        {
            for (int i = 0; i < channels.Count; i++)
            {
                var ch = channels[i];
                if (ch.IsComedy)
                {
                    if (ch.Source != null && ch.Source.isPlaying)
                    {
                        ch.Source.Stop();
                    }
                    FreeChannel(ch);
                }
            }
        }

        private int GetActiveClipInstanceCount(string clipId)
        {
            int count = 0;
            for (int i = 0; i < channels.Count; i++)
            {
                var ch = channels[i];
                if (!ch.IsFree && string.Equals(ch.ActiveClipId, clipId, StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }
            return count;
        }

        private int GetActiveComedyCount()
        {
            int count = 0;
            for (int i = 0; i < channels.Count; i++)
            {
                var ch = channels[i];
                if (!ch.IsFree && ch.IsComedy)
                {
                    count++;
                }
            }
            return count;
        }

        private AudioChannelInfo GetLowestPriorityComedyChannel()
        {
            AudioChannelInfo lowest = null;
            int lowestPri = int.MaxValue;

            for (int i = 0; i < channels.Count; i++)
            {
                var ch = channels[i];
                if (!ch.IsFree && ch.IsComedy)
                {
                    if (ch.Priority < lowestPri)
                    {
                        lowestPri = ch.Priority;
                        lowest = ch;
                    }
                }
            }
            return lowest;
        }

        private void EnsureChannelSource(AudioChannelInfo ch)
        {
            if (ch == null) return;
            try
            {
                if (ch.Source == null)
                {
                    var channelGo = new GameObject($"AudioChannel_{ch.ChannelIndex}");
                    if ((UnityEngine.Object)this != null)
                    {
                        channelGo.transform.SetParent(transform);
                    }
                    var src = channelGo.AddComponent<AudioSource>();
                    src.playOnAwake = false;
                    src.spatialBlend = 0f;
                    ch.Source = src;
                }
            }
            catch (Exception)
            {
            }
        }

        private AudioChannelInfo AllocateChannel(int requiredPriority)
        {
            if (channels.Count == 0)
            {
                InitializePool();
            }

            for (int i = 0; i < channels.Count; i++)
            {
                if (channels[i].IsFree)
                {
                    EnsureChannelSource(channels[i]);
                    return channels[i];
                }
            }

            AudioChannelInfo lowest = null;
            int minPriority = int.MaxValue;

            for (int i = 0; i < channels.Count; i++)
            {
                var ch = channels[i];
                if (ch.Priority < minPriority)
                {
                    minPriority = ch.Priority;
                    lowest = ch;
                }
            }

            if (lowest != null && minPriority <= requiredPriority)
            {
                if (lowest.Source != null && lowest.Source.isPlaying)
                {
                    lowest.Source.Stop();
                }
                FreeChannel(lowest);
                EnsureChannelSource(lowest);
                return lowest;
            }

            return null;
        }

        private void FreeChannel(AudioChannelInfo ch)
        {
            ch.ActiveClipId = null;
            ch.ActiveZone = null;
            ch.Priority = 1;
            ch.StartDspTime = 0;
            ch.ExpectedEndDspTime = 0;
            ch.IsComedy = false;
            ch.IsPaused = false;
        }

        private void HandleGlobalPauseChanged(bool isPaused)
        {
            for (int i = 0; i < channels.Count; i++)
            {
                var ch = channels[i];
                if (ch.IsComedy && ch.Source != null)
                {
                    if (isPaused)
                    {
                        if (ch.Source.isPlaying)
                        {
                            ch.Source.Pause();
                            ch.IsPaused = true;
                        }
                    }
                    else
                    {
                        if (ch.IsPaused)
                        {
                            ch.Source.UnPause();
                            ch.IsPaused = false;
                        }
                    }
                }
            }
        }

        #endregion

        #region Music Ducking

        public void TriggerMusicDuck(float clipDuration, float duckDb = 4f)
        {
            if (activeDuckRoutine != null)
            {
                StopCoroutine(activeDuckRoutine);
            }
            activeDuckRoutine = StartCoroutine(MusicDuckRoutine(clipDuration, duckDb));
        }

        private IEnumerator MusicDuckRoutine(float clipDuration, float duckDb)
        {
            float holdTime = clipDuration + 0.2f;
            float easeTime = 0.3f;

            float duckMultiplier = Mathf.Pow(10f, -Mathf.Abs(duckDb) / 20f);

            var soundMgr = SoundManager.Instance;
            if (soundMgr != null)
            {
                if (!isMusicDucked)
                {
                    originalMusicVolume = soundMgr.MusicVolume;
                    isMusicDucked = true;
                }
                soundMgr.MusicVolume = originalMusicVolume * duckMultiplier;
            }

            if (audioMixer != null)
            {
                audioMixer.SetFloat("MusicVolume", -duckDb);
            }

            double targetHold = AudioSettings.dspTime + holdTime;
            while (AudioSettings.dspTime < targetHold)
            {
                yield return null;
            }

            double easeStart = AudioSettings.dspTime;
            double easeEnd = easeStart + easeTime;

            while (AudioSettings.dspTime < easeEnd)
            {
                float t = (float)((AudioSettings.dspTime - easeStart) / easeTime);
                float curMult = Mathf.Lerp(duckMultiplier, 1.0f, t);

                if (soundMgr != null)
                {
                    soundMgr.MusicVolume = originalMusicVolume * curMult;
                }

                if (audioMixer != null)
                {
                    float curDb = Mathf.Lerp(-duckDb, 0f, t);
                    audioMixer.SetFloat("MusicVolume", curDb);
                }

                yield return null;
            }

            if (soundMgr != null)
            {
                soundMgr.MusicVolume = originalMusicVolume;
            }
            if (audioMixer != null)
            {
                audioMixer.SetFloat("MusicVolume", 0f);
            }
            isMusicDucked = false;
            activeDuckRoutine = null;
        }

        #endregion

        #region Menu and UI Sounds

        public bool PlayUI(string uiSoundName)
        {
            if (synthBounce == null)
            {
                GenerateProceduralFallbacks();
            }

            AudioClip clip = null;
            float pitch = 1f;

            switch (uiSoundName.ToUpperInvariant())
            {
                case "UI_HOVER":
                    clip = Resources.Load<AudioClip>("UI/UI_HOVER") ?? Resources.Load<AudioClip>("UI_HOVER") ?? synthBounce;
                    pitch = 1f;
                    break;
                case "UI_SELECT":
                    clip = Resources.Load<AudioClip>("UI/UI_SELECT") ?? Resources.Load<AudioClip>("UI_SELECT") ?? synthSkillReady;
                    pitch = 1f;
                    break;
                case "UI_SELECT_RISE":
                    clip = Resources.Load<AudioClip>("UI/UI_SELECT") ?? Resources.Load<AudioClip>("UI_SELECT") ?? synthSkillReady;
                    pitch = 1.08f;
                    break;
                case "UI_BACK":
                    clip = Resources.Load<AudioClip>("UI/UI_BACK") ?? Resources.Load<AudioClip>("UI_BACK") ?? synthShiftEnd;
                    pitch = 1f;
                    break;
                case "UI_ERROR":
                    clip = Resources.Load<AudioClip>("UI/UI_ERROR") ?? Resources.Load<AudioClip>("UI_ERROR") ?? synthFuseBeep;
                    pitch = 1f;
                    break;
            }

            if (clip != null)
            {
                return PlayOneShotUI(clip, 0.85f, pitch);
            }
            return false;
        }

        public bool PlayOneShotUI(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) return false;
            var ch = AllocateChannel(1);
            if (ch != null)
            {
                EnsureChannelSource(ch);
                var src = ch.Source;
                if (src == null) return false;
                src.clip = clip;
                src.volume = volume;
                src.pitch = pitch;
                src.spatialBlend = 0f;
                src.outputAudioMixerGroup = uiGroup != null ? uiGroup : masterGroup;

                ch.ActiveClipId = clip.name;
                ch.ActiveZone = ComedyZone.UI;
                ch.Priority = 1;
                ch.IsComedy = false;
                ch.IsPaused = false;
                ch.StartDspTime = AudioSettings.dspTime;
                ch.ExpectedEndDspTime = AudioSettings.dspTime + (clip.length / Mathf.Max(0.01f, pitch));
                src.Play();
                return true;
            }
            return false;
        }

        #endregion

        #region Accessibility Captions

        public static event Action<string> OnCaptionRequested;

        private void DispatchCaption(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            if (SettingsService.Instance != null && SettingsService.Instance.CurrentSettings != null)
            {
                if (!SettingsService.Instance.CurrentSettings.CaptionsEnabled) return;
            }
            OnCaptionRequested?.Invoke(text);
        }

        #endregion

        #region Core Gameplay SFX (Backward Compatibility)

        private void PlayGameplaySFX(AudioClip clip, float volume = 1f, float pitch = 1f, float spatialBlend = 0f, int priority = 2)
        {
            if (clip == null) return;
            var ch = AllocateChannel(priority);
            if (ch != null && ch.Source != null)
            {
                var src = ch.Source;
                src.clip = clip;
                src.volume = volume;
                src.pitch = pitch;
                src.spatialBlend = spatialBlend;
                src.outputAudioMixerGroup = sfxGroup != null ? sfxGroup : masterGroup;

                ch.ActiveClipId = clip.name;
                ch.ActiveZone = null;
                ch.Priority = priority;
                ch.IsComedy = false;
                ch.IsPaused = false;
                src.Play();
            }
        }

        public void PlayBallBounce(Vector3 position, float pitch = 1f)
        {
            var clip = ballBounceClip != null ? ballBounceClip : synthBounce;
            PlayGameplaySFX(clip, 0.45f, pitch, 0.3f, 2);
        }

        public void PlayFireTileSpawn(Vector3 position)
        {
            var clip = fireTileSpawnClip != null ? fireTileSpawnClip : synthFireTile;
            PlayGameplaySFX(clip, 0.5f, UnityEngine.Random.Range(0.9f, 1.1f), 0.3f, 2);
        }

        public void PlayStickyHit(Vector3 position)
        {
            var clip = stickyHitClip != null ? stickyHitClip : synthStickyHit;
            PlayGameplaySFX(clip, 0.65f, UnityEngine.Random.Range(0.85f, 1.05f), 0.3f, 2);
        }

        public void PlayNuclearFuseBeep(Vector3 position, float pitch = 1f)
        {
            var clip = nuclearFuseBeepClip != null ? nuclearFuseBeepClip : synthFuseBeep;
            PlayGameplaySFX(clip, 0.5f, pitch, 0.3f, 2);
        }

        public void PlayNuclearExplosion(Vector3 position)
        {
            var clip = nuclearExplosionClip != null ? nuclearExplosionClip : synthExplosion;
            PlayGameplaySFX(clip, 1.0f, UnityEngine.Random.Range(0.9f, 1.05f), 0.3f, 3);
        }

        public void PlayGunFire(Vector3 position)
        {
            var clip = gunFireClip != null ? gunFireClip : synthGunFire;
            PlayGameplaySFX(clip, 0.55f, UnityEngine.Random.Range(0.95f, 1.1f), 0.3f, 2);
        }

        public void PlayPlayerHit()
        {
            var clip = playerHitClip != null ? playerHitClip : synthPlayerHit;
            PlayGameplaySFX(clip, 0.85f, 1.0f, 0f, 5);
        }

        public void PlayDimensionShiftStart()
        {
            var clip = dimensionShiftStartClip != null ? dimensionShiftStartClip : synthShiftStart;
            PlayGameplaySFX(clip, 0.7f, 1.25f, 0f, 3);
        }

        public void PlayDimensionShiftEnd()
        {
            var clip = dimensionShiftEndClip != null ? dimensionShiftEndClip : synthShiftEnd;
            PlayGameplaySFX(clip, 0.65f, 0.9f, 0f, 2);
        }

        public void PlaySlowMoStart()
        {
            var clip = slowMoStartClip != null ? slowMoStartClip : synthSlowMoStart;
            PlayGameplaySFX(clip, 0.8f, 0.7f, 0f, 3);
        }

        public void PlaySlowMoEnd()
        {
            var clip = slowMoEndClip != null ? slowMoEndClip : synthSlowMoEnd;
            PlayGameplaySFX(clip, 0.75f, 1.15f, 0f, 2);
        }

        public bool IsClipPlaying(string clipId)
        {
            if (string.IsNullOrEmpty(clipId)) return false;
            for (int i = 0; i < channels.Count; i++)
            {
                var ch = channels[i];
                if (!ch.IsFree && ch.ActiveClipId != null &&
                    (string.Equals(ch.ActiveClipId, clipId, StringComparison.OrdinalIgnoreCase) ||
                     ch.ActiveClipId.IndexOf(clipId, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    return true;
                }
            }
            return false;
        }

        public float GetActiveClipPitch(string clipId)
        {
            if (string.IsNullOrEmpty(clipId)) return 1f;
            for (int i = 0; i < channels.Count; i++)
            {
                var ch = channels[i];
                if (!ch.IsFree && ch.ActiveClipId != null &&
                    ch.ActiveClipId.IndexOf(clipId, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return ch.Source != null ? ch.Source.pitch : 1f;
                }
            }
            return 1f;
        }

        public AudioMixerGroup GetActiveClipMixerGroup(string clipId)
        {
            if (string.IsNullOrEmpty(clipId)) return null;
            for (int i = 0; i < channels.Count; i++)
            {
                var ch = channels[i];
                if (!ch.IsFree && ch.ActiveClipId != null &&
                    ch.ActiveClipId.IndexOf(clipId, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return ch.Source != null ? ch.Source.outputAudioMixerGroup : null;
                }
            }
            return null;
        }

        public void PlayGameOver()
        {
            if ((UnityEngine.Object)this == null) return;
            try
            {
                if (IsClipPlaying("DH_05"))
                {
                    return;
                }
                Play("DH_06");
            }
            catch (Exception) { }
        }

        public void PlayJump()
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayJumpSound();
            else PlayGameplaySFX(synthJump, 0.7f, UnityEngine.Random.Range(0.95f, 1.05f), 0f, 2);
        }

        public void PlayDash()
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayDashSound();
            else PlayGameplaySFX(synthDash, 0.75f, UnityEngine.Random.Range(0.95f, 1.1f), 0f, 2);
        }

        public void PlaySkillReady()
        {
            PlayGameplaySFX(synthSkillReady, 0.55f, 1.0f, 0f, 2);
        }

        #endregion

        #region Helpers & Settings Access

        public ComedyLevel? OverrideComedyLevel { get; set; } = null;

        private ComedyLevel GetActiveComedyLevel()
        {
            if (OverrideComedyLevel.HasValue)
            {
                return OverrideComedyLevel.Value;
            }
            if (SettingsService.Instance != null && SettingsService.Instance.CurrentSettings != null)
            {
                return SettingsService.Instance.CurrentSettings.ComedyLevel;
            }
            return ComedyLevel.Full;
        }

        #endregion

        #region Procedural Fallback Waveforms

        private void GenerateProceduralFallbacks()
        {
            synthBounce = GenerateTone(220f, 140f, 0.08f);
            synthFireTile = GenerateNoise(0.15f, lowPass: 800f);
            synthStickyHit = GenerateTone(180f, 90f, 0.18f);
            synthFuseBeep = GenerateTone(880f, 880f, 0.05f);
            synthExplosion = GenerateExplosionNoise(0.6f);
            synthGunFire = GenerateTone(640f, 240f, 0.09f);
            synthPlayerHit = GenerateTone(150f, 60f, 0.22f);
            synthShiftStart = GenerateTone(440f, 880f, 0.16f);
            synthShiftEnd = GenerateTone(700f, 320f, 0.16f);
            synthSlowMoStart = GenerateTone(420f, 150f, 0.32f);
            synthSlowMoEnd = GenerateTone(160f, 440f, 0.25f);
            synthGameOver = GenerateMinorChord(0.8f);
            synthJump = GenerateTone(240f, 520f, 0.14f);
            synthDash = GenerateNoise(0.18f, lowPass: 1200f);
            synthSkillReady = GenerateTone(440f, 880f, 0.2f);
        }

        private AudioClip GenerateTone(float startFreq, float endFreq, float duration)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float currentFreq = Mathf.Lerp(startFreq, endFreq, t);
                float envelope = 1f - t;
                float phase = 2f * Mathf.PI * currentFreq * ((float)i / sampleRate);
                samples[i] = Mathf.Sin(phase) * envelope;
            }

            var clip = AudioClip.Create("SynthTone", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateNoise(float duration, float lowPass = 1000f)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            float filterFactor = Mathf.Clamp01(lowPass / (sampleRate * 0.5f));
            float filteredVal = 0f;

            for (int i = 0; i < sampleCount; i++)
            {
                float rawNoise = UnityEngine.Random.Range(-1f, 1f);
                filteredVal += filterFactor * (rawNoise - filteredVal);
                float t = (float)i / sampleCount;
                float envelope = 1f - t;
                samples[i] = filteredVal * envelope;
            }

            var clip = AudioClip.Create("SynthNoise", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateExplosionNoise(float duration)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float envelope = Mathf.Exp(-4f * t);
                float rawNoise = UnityEngine.Random.Range(-1f, 1f);
                float rumble = Mathf.Sin(2f * Mathf.PI * 60f * ((float)i / sampleRate));
                samples[i] = (rawNoise * 0.6f + rumble * 0.4f) * envelope;
            }

            var clip = AudioClip.Create("SynthExplosion", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateMinorChord(float duration)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            float[] chordFreqs = { 220f, 261.63f, 329.63f };

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float envelope = 1f - t;
                float sampleSum = 0f;

                for (int c = 0; c < chordFreqs.Length; c++)
                {
                    float phase = 2f * Mathf.PI * chordFreqs[c] * ((float)i / sampleRate);
                    sampleSum += Mathf.Sin(phase);
                }

                samples[i] = (sampleSum / chordFreqs.Length) * envelope;
            }

            var clip = AudioClip.Create("SynthMinorChord", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        #endregion
    }
}
