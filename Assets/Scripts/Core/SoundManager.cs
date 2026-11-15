using System;
using UnityEngine;

namespace DodgeGame.Core
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Audio Settings")]
        [Range(0f, 1f)] [SerializeField] private float sfxVolume = 0.85f;
        [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.45f;
        [SerializeField] private bool musicEnabled = true;
        [SerializeField] private UnityEngine.Audio.AudioMixerGroup uiMixerGroup;

        private AudioSource sfxSource;
        private AudioSource musicSource;
        private AudioSource uiSource;

        private AudioClip bounceClip;
        private AudioClip homingAlertClip;
        private AudioClip jumpClip;
        private AudioClip dashClip;
        private AudioClip hitExplosionClip;
        private AudioClip countdownBeepClip;
        private AudioClip countdownGoClip;
        private AudioClip buttonClickClip;
        private AudioClip buttonHoverClip;
        private AudioClip buttonPressClip;
        private AudioClip playStartJingleClip;
        private AudioClip difficultySwitchClip;
        private AudioClip bgmLoopClip;

        public float SfxVolume
        {
            get => sfxVolume;
            set
            {
                sfxVolume = Mathf.Clamp01(value);
                if (sfxSource != null) sfxSource.volume = sfxVolume;
            }
        }

        public float MusicVolume
        {
            get => musicVolume;
            set
            {
                musicVolume = Mathf.Clamp01(value);
                if (musicSource != null) musicSource.volume = musicVolume;
            }
        }

        public bool MusicEnabled
        {
            get => musicEnabled;
            set
            {
                musicEnabled = value;
                if (musicSource != null)
                {
                    if (musicEnabled && !musicSource.isPlaying) musicSource.Play();
                    else if (!musicEnabled && musicSource.isPlaying) musicSource.Pause();
                }
            }
        }

        private bool muteAudio = false;

        public bool MuteAudio
        {
            get => muteAudio;
            set
            {
                muteAudio = value;
                if (sfxSource != null) sfxSource.mute = muteAudio;
                if (musicSource != null) musicSource.mute = muteAudio;
                if (AudioManager.Instance != null) AudioManager.Instance.MuteAudio = muteAudio;
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

            InitializeAudioSources();
            GenerateProceduralClips();
            StartMusic();
        }

        private void InitializeAudioSources()
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.volume = sfxVolume;

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.volume = musicVolume;

            uiSource = gameObject.AddComponent<AudioSource>();
            uiSource.playOnAwake = false;
            uiSource.volume = sfxVolume;
            if (uiMixerGroup != null)
            {
                uiSource.outputAudioMixerGroup = uiMixerGroup;
            }
        }

        private void GenerateProceduralClips()
        {
            bounceClip = CreateBounceTone(frequency: 260f, duration: 0.12f);
            homingAlertClip = CreateAlertTone(duration: 0.35f);
            jumpClip = CreateSweepTone(startFreq: 220f, endFreq: 480f, duration: 0.16f);
            dashClip = CreateNoiseTone(duration: 0.18f);
            hitExplosionClip = CreateExplosionTone(duration: 0.6f);
            countdownBeepClip = CreateBeepTone(frequency: 440f, duration: 0.12f);
            countdownGoClip = CreateBeepTone(frequency: 880f, duration: 0.25f);
            buttonClickClip = CreateBeepTone(frequency: 600f, duration: 0.05f);
            buttonHoverClip = CreateBeepTone(frequency: 950f, duration: 0.03f);
            buttonPressClip = CreateSweepTone(startFreq: 240f, endFreq: 90f, duration: 0.08f);
            playStartJingleClip = CreateSweepTone(startFreq: 330f, endFreq: 880f, duration: 0.35f);
            difficultySwitchClip = CreateBeepTone(frequency: 720f, duration: 0.04f);
            bgmLoopClip = CreateBgmArpeggioLoop(duration: 4.0f);
        }

        private void StartMusic()
        {
            if (musicSource != null && bgmLoopClip != null)
            {
                musicSource.clip = bgmLoopClip;
                musicSource.volume = musicVolume;
                if (musicEnabled)
                {
                    musicSource.Play();
                }
            }
        }

        public void PlayBounceSound(float pitch = 1f)
        {
            if (sfxSource == null || bounceClip == null) return;
            sfxSource.pitch = Mathf.Clamp(pitch, 0.7f, 1.4f);
            sfxSource.PlayOneShot(bounceClip, sfxVolume * 0.75f);
        }

        public void PlayHomingAlertSound()
        {
            if (sfxSource == null || homingAlertClip == null) return;
            sfxSource.pitch = 1f;
            sfxSource.PlayOneShot(homingAlertClip, sfxVolume);
        }

        public void PlayJumpSound()
        {
            if (sfxSource == null || jumpClip == null) return;
            sfxSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
            sfxSource.PlayOneShot(jumpClip, sfxVolume * 0.8f);
        }

        public void PlayDashSound()
        {
            if (sfxSource == null || dashClip == null) return;
            sfxSource.pitch = UnityEngine.Random.Range(0.95f, 1.1f);
            sfxSource.PlayOneShot(dashClip, sfxVolume * 0.85f);
        }

        public void PlayHitExplosion()
        {
            if (sfxSource == null || hitExplosionClip == null) return;
            sfxSource.pitch = 1f;
            sfxSource.PlayOneShot(hitExplosionClip, sfxVolume);
        }

        public void PlayCountdownBeep(bool isGo = false)
        {
            if (sfxSource == null) return;
            sfxSource.pitch = 1f;
            sfxSource.PlayOneShot(isGo ? countdownGoClip : countdownBeepClip, sfxVolume);
        }

        public void PlayButtonClick()
        {
            PlayButtonPress();
        }

        public void PlayButtonHover()
        {
            var src = uiSource != null ? uiSource : sfxSource;
            if (src == null || buttonHoverClip == null) return;
            src.pitch = UnityEngine.Random.Range(1.05f, 1.15f);
            src.PlayOneShot(buttonHoverClip, sfxVolume * 0.6f);
        }

        public void PlayButtonPress()
        {
            var src = uiSource != null ? uiSource : sfxSource;
            if (src == null || buttonPressClip == null) return;
            src.pitch = 1.0f;
            src.PlayOneShot(buttonPressClip, sfxVolume * 0.9f);
        }

        public void PlayPlayButtonPress()
        {
            var src = uiSource != null ? uiSource : sfxSource;
            if (src == null || playStartJingleClip == null) return;
            src.pitch = 1.0f;
            src.PlayOneShot(playStartJingleClip, sfxVolume);
        }

        public void PlayDifficultySwitch()
        {
            var src = uiSource != null ? uiSource : sfxSource;
            if (src == null || difficultySwitchClip == null) return;
            src.pitch = 1.0f;
            src.PlayOneShot(difficultySwitchClip, sfxVolume * 0.8f);
        }

        private AudioClip CreateBounceTone(float frequency, float duration)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * duration);
            float[] data = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float env = 1f - (t / duration);
                float f = frequency * (1f + (env * 0.5f));
                float wave = Mathf.Sin(2f * Mathf.PI * f * t);
                data[i] = wave * env * env;
            }

            AudioClip clip = AudioClip.Create("BounceSynth", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateAlertTone(float duration)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * duration);
            float[] data = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Sin(t / duration * Mathf.PI);
                float f = 520f + (Mathf.Sin(t * 30f) * 180f);
                float wave = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * f * t)) * 0.5f;
                data[i] = wave * env;
            }

            AudioClip clip = AudioClip.Create("HomingAlertSynth", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateSweepTone(float startFreq, float endFreq, float duration)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * duration);
            float[] data = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float norm = t / duration;
                float env = 1f - norm;
                float currentFreq = Mathf.Lerp(startFreq, endFreq, norm);
                float wave = Mathf.Sin(2f * Mathf.PI * currentFreq * t);
                data[i] = wave * env;
            }

            AudioClip clip = AudioClip.Create("SweepSynth", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateNoiseTone(float duration)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * duration);
            float[] data = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Sin(t / duration * Mathf.PI);
                float noise = (UnityEngine.Random.value * 2f) - 1f;
                data[i] = noise * env * 0.7f;
            }

            AudioClip clip = AudioClip.Create("NoiseSynth", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateExplosionTone(float duration)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * duration);
            float[] data = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Pow(1f - (t / duration), 2.5f);
                float bass = Mathf.Sin(2f * Mathf.PI * (70f * (1f - (t / duration))) * t);
                float noise = (UnityEngine.Random.value * 2f) - 1f;
                data[i] = ((bass * 0.6f) + (noise * 0.4f)) * env;
            }

            AudioClip clip = AudioClip.Create("ExplosionSynth", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateBeepTone(float frequency, float duration)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * duration);
            float[] data = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Sin(t / duration * Mathf.PI);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * env;
            }

            AudioClip clip = AudioClip.Create("BeepSynth", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateBgmArpeggioLoop(float duration)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * duration);
            float[] data = new float[sampleCount];

            float[] notes = { 130.81f, 155.56f, 174.61f, 196.00f, 233.08f, 196.00f, 174.61f, 155.56f };
            float stepDuration = duration / 16f;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                int step = (int)(t / stepDuration) % 8;
                float stepT = t % stepDuration;
                float env = Mathf.Exp(-stepT * 12f);

                float freq = notes[step];
                float tone = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.5f;
                tone += Mathf.Sin(4f * Mathf.PI * freq * t) * 0.25f;

                data[i] = tone * env * 0.35f;
            }

            AudioClip clip = AudioClip.Create("BgmSynthLoop", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}