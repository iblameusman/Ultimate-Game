using System.Collections;
using UnityEngine;
using DodgeGame.Core;
using DodgeGame.Data;
using DodgeGame.Player;

namespace DodgeGame.Arena
{
    public class DimensionVisuals : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Dimension shift configuration providing duration, alpha, and tint colors.")]
        [SerializeField] private DimensionConfig config;

        [Header("Target Renderers")]
        [Tooltip("Renderer of the player capsule.")]
        [SerializeField] private Renderer playerRenderer;
        private Renderer[] allPlayerRenderers;

        [Tooltip("Renderer of the arena floor.")]
        [SerializeField] private Renderer arenaFloorRenderer;
        private Renderer[] allFloorRenderers;

        [Tooltip("Renderers of the arena boundary walls.")]
        [SerializeField] private Renderer[] arenaWallRenderers;

        [Header("Event Source")]
        [Tooltip("DimensionShifter reference to subscribe to shift start and end.")]
        [SerializeField] private DimensionShifter dimensionShifter;

        [Header("Cues & Feedback")]
        [Tooltip("Particle system triggered on dimension shift start and end.")]
        [SerializeField] private ParticleSystem shiftParticleEffect;

        [Tooltip("Optional AudioSource for shift audio cues.")]
        [SerializeField] private AudioSource audioSource;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private MaterialPropertyBlock playerBlock;
        private MaterialPropertyBlock floorBlock;
        private MaterialPropertyBlock wallBlock;

        private Color defaultPlayerColor = Color.white;
        private Color defaultFloorColor = new Color(0.25f, 0.25f, 0.28f, 1f);
        private Color defaultWallColor = new Color(0.35f, 0.35f, 0.4f, 1f);

        private Coroutine transitionCoroutine;
        private float currentTransitionProgress = 0f;

        private void Awake()
        {
            playerBlock = new MaterialPropertyBlock();
            floorBlock = new MaterialPropertyBlock();
            wallBlock = new MaterialPropertyBlock();

            EnsureReferences();
            CacheDefaultColors();
            EnsureAudioSource();
            EnsureParticleSystem();
        }

        private void OnEnable()
        {
            EnsureReferences();
            if (dimensionShifter != null)
            {
                dimensionShifter.OnShiftStart += HandleShiftStart;
                dimensionShifter.OnShiftEnd += HandleShiftEnd;
            }
        }

        private void Start()
        {
            EnsureReferences();
            if (dimensionShifter != null)
            {
                dimensionShifter.OnShiftStart -= HandleShiftStart;
                dimensionShifter.OnShiftStart += HandleShiftStart;
                dimensionShifter.OnShiftEnd -= HandleShiftEnd;
                dimensionShifter.OnShiftEnd += HandleShiftEnd;
            }
        }

        private void OnDisable()
        {
            if (dimensionShifter != null)
            {
                dimensionShifter.OnShiftStart -= HandleShiftStart;
                dimensionShifter.OnShiftEnd -= HandleShiftEnd;
            }

            ResetVisualsInstantly();
        }

        private void EnsureReferences()
        {
            if (dimensionShifter == null)
            {
                dimensionShifter = FindAnyObjectByType<DimensionShifter>();
            }

            if (dimensionShifter != null)
            {
                allPlayerRenderers = dimensionShifter.GetComponentsInChildren<Renderer>(true);
                if (playerRenderer == null)
                {
                    playerRenderer = dimensionShifter.GetComponentInChildren<Renderer>();
                }
            }

            if (arenaFloorRenderer == null)
            {
                var floorGo = GameObject.Find("Arena_Floor");
                if (floorGo != null) arenaFloorRenderer = floorGo.GetComponent<Renderer>();
            }

            var arenaRoot = GameObject.Find("Arena");
            if (arenaRoot != null)
            {
                var floorList = new System.Collections.Generic.List<Renderer>();
                var allRenderers = arenaRoot.GetComponentsInChildren<Renderer>(true);
                foreach (var r in allRenderers)
                {
                    string n = r.gameObject.name;
                    if (n.Contains("Ground") || n.Contains("Road") || n.Contains("Sidewalk") || n.Contains("Curb") || n.Contains("Floor"))
                    {
                        floorList.Add(r);
                    }
                }
                if (floorList.Count > 0)
                {
                    allFloorRenderers = floorList.ToArray();
                    if (arenaFloorRenderer == null) arenaFloorRenderer = allFloorRenderers[0];
                }
            }

            if (arenaWallRenderers == null || arenaWallRenderers.Length == 0)
            {
                string[] wallNames = { "Wall_North", "Wall_South", "Wall_East", "Wall_West" };
                var walls = new System.Collections.Generic.List<Renderer>();
                foreach (var w in wallNames)
                {
                    var wallGo = GameObject.Find(w);
                    if (wallGo != null && wallGo.TryGetComponent<Renderer>(out var r))
                    {
                        walls.Add(r);
                    }
                }

                if (arenaRoot != null)
                {
                    var allRenderers = arenaRoot.GetComponentsInChildren<Renderer>(true);
                    foreach (var r in allRenderers)
                    {
                        string n = r.gameObject.name;
                        if ((n.Contains("BOUNDS") || n.Contains("Wall")) && !walls.Contains(r))
                        {
                            walls.Add(r);
                        }
                    }
                }

                arenaWallRenderers = walls.ToArray();
            }
        }

        private void CacheDefaultColors()
        {
            if (playerRenderer != null && playerRenderer.sharedMaterial != null)
            {
                if (playerRenderer.sharedMaterial.HasProperty(BaseColorId))
                {
                    defaultPlayerColor = playerRenderer.sharedMaterial.GetColor(BaseColorId);
                }
            }

            if (arenaFloorRenderer != null && arenaFloorRenderer.sharedMaterial != null)
            {
                if (arenaFloorRenderer.sharedMaterial.HasProperty(BaseColorId))
                {
                    defaultFloorColor = arenaFloorRenderer.sharedMaterial.GetColor(BaseColorId);
                }
            }

            if (arenaWallRenderers != null && arenaWallRenderers.Length > 0 && arenaWallRenderers[0] != null)
            {
                if (arenaWallRenderers[0].sharedMaterial.HasProperty(BaseColorId))
                {
                    defaultWallColor = arenaWallRenderers[0].sharedMaterial.GetColor(BaseColorId);
                }
            }
        }

        private void EnsureAudioSource()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 0f;
                }
            }
        }

        private void EnsureParticleSystem()
        {
            if (shiftParticleEffect == null)
            {
                var psGo = new GameObject("Shift_Flash_FX");
                psGo.transform.SetParent(transform, false);
                shiftParticleEffect = psGo.AddComponent<ParticleSystem>();
                shiftParticleEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                var main = shiftParticleEffect.main;
                main.playOnAwake = false;
                main.loop = false;
                main.duration = 0.35f;
                main.startLifetime = 0.35f;
                main.startSpeed = 7f;
                main.startSize = 0.5f;
                main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.4f, 0.85f, 1f, 0.9f), Color.white);

                var emission = shiftParticleEffect.emission;
                emission.rateOverTime = 0;
                emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 30) });

                var shape = shiftParticleEffect.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.8f;

                var renderer = psGo.GetComponent<ParticleSystemRenderer>();
                if (renderer != null)
                {
                    renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
                }
            }
        }

        private void HandleShiftStart()
        {
            PlayShiftCue(isEntering: true);
            StartTransition(targetProgress: 1.0f);
        }

        private void HandleShiftEnd()
        {
            PlayShiftCue(isEntering: false);
            StartTransition(targetProgress: 0.0f);
        }

        private void StartTransition(float targetProgress)
        {
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }
            transitionCoroutine = StartCoroutine(TransitionRoutine(targetProgress));
        }

        private IEnumerator TransitionRoutine(float target)
        {
            float duration = config != null ? config.TransitionDuration : 0.2f;
            float start = currentTransitionProgress;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                currentTransitionProgress = Mathf.Lerp(start, target, t);
                ApplyVisualTints(currentTransitionProgress);
                yield return null;
            }

            currentTransitionProgress = target;
            ApplyVisualTints(currentTransitionProgress);
            transitionCoroutine = null;
        }

        private void ApplyVisualTints(float t)
        {
            float shiftAlpha = config != null ? config.ShiftAlpha : 0.4f;
            Color shiftTint = config != null ? config.ShiftTint : new Color(0.25f, 0.75f, 1.0f, 1.0f);

            if (allPlayerRenderers != null && allPlayerRenderers.Length > 0)
            {
                for (int i = 0; i < allPlayerRenderers.Length; i++)
                {
                    var r = allPlayerRenderers[i];
                    if (r != null && !(r is ParticleSystemRenderer || r is TrailRenderer))
                    {
                        r.GetPropertyBlock(playerBlock);
                        Color baseCol = (r.sharedMaterial != null && r.sharedMaterial.HasProperty(BaseColorId))
                            ? r.sharedMaterial.GetColor(BaseColorId)
                            : defaultPlayerColor;
                        Color targetPlayerColor = Color.Lerp(baseCol, shiftTint, 0.4f);
                        targetPlayerColor.a = Mathf.Lerp(baseCol.a, shiftAlpha, t);
                        playerBlock.SetColor(BaseColorId, targetPlayerColor);

                        Color emission = Color.Lerp(Color.black, shiftTint * 1.5f, t);
                        playerBlock.SetColor(EmissionColorId, emission);
                        r.SetPropertyBlock(playerBlock);
                    }
                }
            }
            else if (playerRenderer != null)
            {
                playerRenderer.GetPropertyBlock(playerBlock);
                Color targetPlayerColor = Color.Lerp(defaultPlayerColor, shiftTint, 0.4f);
                targetPlayerColor.a = Mathf.Lerp(defaultPlayerColor.a, shiftAlpha, t);
                playerBlock.SetColor(BaseColorId, targetPlayerColor);

                Color emission = Color.Lerp(Color.black, shiftTint * 1.5f, t);
                playerBlock.SetColor(EmissionColorId, emission);
                playerRenderer.SetPropertyBlock(playerBlock);
            }

            if (allFloorRenderers != null && allFloorRenderers.Length > 0)
            {
                Color targetFloorColor = Color.Lerp(defaultFloorColor, defaultFloorColor * shiftTint * 1.8f, t);
                Color floorEmission = Color.Lerp(Color.black, shiftTint * 0.4f, t);
                for (int i = 0; i < allFloorRenderers.Length; i++)
                {
                    if (allFloorRenderers[i] != null)
                    {
                        allFloorRenderers[i].GetPropertyBlock(floorBlock);
                        floorBlock.SetColor(BaseColorId, targetFloorColor);
                        floorBlock.SetColor(EmissionColorId, floorEmission);
                        allFloorRenderers[i].SetPropertyBlock(floorBlock);
                    }
                }
            }
            else if (arenaFloorRenderer != null)
            {
                arenaFloorRenderer.GetPropertyBlock(floorBlock);
                Color targetFloorColor = Color.Lerp(defaultFloorColor, defaultFloorColor * shiftTint * 1.8f, t);
                floorBlock.SetColor(BaseColorId, targetFloorColor);

                Color floorEmission = Color.Lerp(Color.black, shiftTint * 0.4f, t);
                floorBlock.SetColor(EmissionColorId, floorEmission);
                arenaFloorRenderer.SetPropertyBlock(floorBlock);
            }

            if (arenaWallRenderers != null)
            {
                Color targetWallColor = Color.Lerp(defaultWallColor, defaultWallColor * shiftTint * 1.8f, t);
                Color wallEmission = Color.Lerp(Color.black, shiftTint * 0.4f, t);

                for (int i = 0; i < arenaWallRenderers.Length; i++)
                {
                    if (arenaWallRenderers[i] != null)
                    {
                        arenaWallRenderers[i].GetPropertyBlock(wallBlock);
                        wallBlock.SetColor(BaseColorId, targetWallColor);
                        wallBlock.SetColor(EmissionColorId, wallEmission);
                        arenaWallRenderers[i].SetPropertyBlock(wallBlock);
                    }
                }
            }
        }

        private void PlayShiftCue(bool isEntering)
        {
            if (shiftParticleEffect != null && playerRenderer != null)
            {
                shiftParticleEffect.transform.position = playerRenderer.transform.position + Vector3.up * 0.8f;
                shiftParticleEffect.Play();
            }

            if (playerRenderer != null)
            {
                VFXManager.Instance?.SpawnDimensionShift(playerRenderer.transform.position);
            }

            if (audioSource != null)
            {
                audioSource.pitch = isEntering ? 1.3f : 0.85f;
                audioSource.PlayOneShot(CreateChimeClip(isEntering));
            }
        }

        private AudioClip CreateChimeClip(bool isEntering)
        {
            int sampleRate = 44100;
            float duration = 0.18f;
            int sampleCount = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            float baseFreq = isEntering ? 520f : 340f;
            float targetFreq = isEntering ? 880f : 220f;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float currentFreq = Mathf.Lerp(baseFreq, targetFreq, t);
                float env = Mathf.Sin(t * Mathf.PI);
                samples[i] = Mathf.Sin(2f * Mathf.PI * currentFreq * ((float)i / sampleRate)) * env * 0.35f;
            }

            var clip = AudioClip.Create(isEntering ? "ShiftInCue" : "ShiftOutCue", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void ResetVisualsInstantly()
        {
            currentTransitionProgress = 0f;
            ApplyVisualTints(0f);
        }
    }
}
