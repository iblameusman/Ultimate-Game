using UnityEngine;

namespace DodgeGame.Core
{
    public class VFXManager : MonoBehaviour
    {
        public static VFXManager Instance { get; private set; }

        [Header("Particle Systems")]
        [SerializeField] private ParticleSystem bounceSparksFx;
        [SerializeField] private ParticleSystem fireTileSpawnFx;
        [SerializeField] private ParticleSystem nuclearExplosionFx;
        [SerializeField] private ParticleSystem dimensionShiftFx;
        [SerializeField] private ParticleSystem dashDustFx;
        [SerializeField] private ParticleSystem fireBallImpactFx;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            EnsureParticleSystems();
        }

        public void EnsureParticleSystems()
        {
            var particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (particleShader == null) particleShader = Shader.Find("Particles/Standard Unlit");

            if (bounceSparksFx == null)
            {
                var go = new GameObject("VFX_BounceSparks");
                go.transform.SetParent(transform);
                bounceSparksFx = go.AddComponent<ParticleSystem>();
                bounceSparksFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                var main = bounceSparksFx.main;
                main.playOnAwake = false;
                main.loop = false;
                main.duration = 0.25f;
                main.startLifetime = 0.25f;
                main.startSpeed = 8f;
                main.startSize = 0.18f;
                main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.85f, 0.4f), Color.white);

                var emission = bounceSparksFx.emission;
                emission.rateOverTime = 0;
                emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 15) });

                var shape = bounceSparksFx.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.2f;

                var renderer = go.GetComponent<ParticleSystemRenderer>();
                if (renderer != null && particleShader != null)
                {
                    renderer.material = new Material(particleShader);
                }
            }

            if (fireTileSpawnFx == null)
            {
                var go = new GameObject("VFX_FireTileSpawn");
                go.transform.SetParent(transform);
                fireTileSpawnFx = go.AddComponent<ParticleSystem>();
                fireTileSpawnFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                var main = fireTileSpawnFx.main;
                main.playOnAwake = false;
                main.loop = false;
                main.duration = 0.45f;
                main.startLifetime = 0.45f;
                main.startSpeed = 4f;
                main.startSize = 0.35f;
                main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.45f, 0.1f), new Color(1f, 0.8f, 0.2f));

                var emission = fireTileSpawnFx.emission;
                emission.rateOverTime = 0;
                emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 25) });

                var shape = fireTileSpawnFx.shape;
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = 35f;
                shape.radius = 0.5f;

                var renderer = go.GetComponent<ParticleSystemRenderer>();
                if (renderer != null && particleShader != null)
                {
                    renderer.material = new Material(particleShader);
                }
            }

            if (nuclearExplosionFx == null)
            {
                var go = new GameObject("VFX_NuclearExplosion");
                go.transform.SetParent(transform);
                nuclearExplosionFx = go.AddComponent<ParticleSystem>();
                nuclearExplosionFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                var main = nuclearExplosionFx.main;
                main.playOnAwake = false;
                main.loop = false;
                main.duration = 0.85f;
                main.startLifetime = 0.85f;
                main.startSpeed = 16f;
                main.startSize = 1.2f;
                main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.3f, 0.05f), new Color(1f, 0.95f, 0.3f));

                var emission = nuclearExplosionFx.emission;
                emission.rateOverTime = 0;
                emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 75) });

                var shape = nuclearExplosionFx.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 1.5f;

                var renderer = go.GetComponent<ParticleSystemRenderer>();
                if (renderer != null && particleShader != null)
                {
                    renderer.material = new Material(particleShader);
                }
            }

            if (dimensionShiftFx == null)
            {
                var go = new GameObject("VFX_DimensionShift");
                go.transform.SetParent(transform);
                dimensionShiftFx = go.AddComponent<ParticleSystem>();
                dimensionShiftFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                var main = dimensionShiftFx.main;
                main.playOnAwake = false;
                main.loop = false;
                main.duration = 0.35f;
                main.startLifetime = 0.35f;
                main.startSpeed = 7f;
                main.startSize = 0.5f;
                main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.3f, 0.9f, 1f, 0.9f), Color.white);

                var emission = dimensionShiftFx.emission;
                emission.rateOverTime = 0;
                emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 35) });

                var shape = dimensionShiftFx.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.8f;

                var renderer = go.GetComponent<ParticleSystemRenderer>();
                if (renderer != null && particleShader != null)
                {
                    renderer.material = new Material(particleShader);
                }
            }

            if (dashDustFx == null)
            {
                var go = new GameObject("VFX_DashDust");
                go.transform.SetParent(transform);
                dashDustFx = go.AddComponent<ParticleSystem>();
                dashDustFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                var main = dashDustFx.main;
                main.playOnAwake = false;
                main.loop = false;
                main.duration = 0.25f;
                main.startLifetime = 0.25f;
                main.startSpeed = 3.5f;
                main.startSize = 0.35f;
                main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.85f, 0.95f, 1f, 0.6f), new Color(1f, 1f, 1f, 0.1f));

                var emission = dashDustFx.emission;
                emission.rateOverTime = 0;
                emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 16) });

                var shape = dashDustFx.shape;
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = 0.4f;

                var renderer = go.GetComponent<ParticleSystemRenderer>();
                if (renderer != null && particleShader != null)
                {
                    renderer.material = new Material(particleShader);
                }
            }

            if (fireBallImpactFx == null)
            {
                var go = new GameObject("VFX_FireBallImpact");
                go.transform.SetParent(transform);
                fireBallImpactFx = go.AddComponent<ParticleSystem>();
                fireBallImpactFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                var main = fireBallImpactFx.main;
                main.playOnAwake = false;
                main.loop = false;
                main.duration = 0.35f;
                main.startLifetime = 0.32f;
                main.startSpeed = 4.5f;
                main.startSize = 0.16f;
                main.gravityModifier = 1.2f;
                main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.8f, 0.2f), new Color(1f, 0.3f, 0.05f));

                var emission = fireBallImpactFx.emission;
                emission.rateOverTime = 0;
                emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 8) });

                var shape = fireBallImpactFx.shape;
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = 45f;
                shape.radius = 0.2f;

                var renderer = go.GetComponent<ParticleSystemRenderer>();
                if (renderer != null && particleShader != null)
                {
                    renderer.material = new Material(particleShader);
                }

                var ringGo = new GameObject("FlashRing");
                ringGo.transform.SetParent(go.transform, false);
                var ringPs = ringGo.AddComponent<ParticleSystem>();
                ringPs.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                var rMain = ringPs.main;
                rMain.playOnAwake = false;
                rMain.loop = false;
                rMain.duration = 0.18f;
                rMain.startLifetime = 0.18f;
                rMain.startSpeed = 0f;
                rMain.startSize = 1.1f;
                rMain.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.65f, 0.15f, 0.8f), new Color(1f, 0.2f, 0.02f, 0f));

                var rEmission = ringPs.emission;
                rEmission.rateOverTime = 0;
                rEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 1) });

                var rRenderer = ringGo.GetComponent<ParticleSystemRenderer>();
                if (rRenderer != null && particleShader != null)
                {
                    rRenderer.material = new Material(particleShader);
                }
            }
        }

        public void SpawnBounceSparks(Vector3 position, Vector3 normal)
        {
            if (bounceSparksFx == null) return;
            bounceSparksFx.transform.position = position;
            if (normal != Vector3.zero)
            {
                bounceSparksFx.transform.rotation = Quaternion.LookRotation(normal);
            }
            bounceSparksFx.Play();
        }

        public void SpawnFireBallBounceImpact(Vector3 position, Vector3 normal)
        {
            if (fireBallImpactFx == null) return;
            fireBallImpactFx.transform.position = position + Vector3.up * 0.02f;
            if (normal != Vector3.zero)
            {
                fireBallImpactFx.transform.rotation = Quaternion.LookRotation(normal);
            }
            else
            {
                fireBallImpactFx.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            }
            fireBallImpactFx.Play(true);
        }

        public void SpawnFireTileBurst(Vector3 position)
        {
            if (fireTileSpawnFx == null) return;
            fireTileSpawnFx.transform.position = position + Vector3.up * 0.1f;
            fireTileSpawnFx.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            fireTileSpawnFx.Play();
        }

        public void SpawnNuclearExplosion(Vector3 position)
        {
            if (nuclearExplosionFx == null) return;
            nuclearExplosionFx.transform.position = position + Vector3.up * 0.5f;
            nuclearExplosionFx.Play();
        }

        public void SpawnDimensionShift(Vector3 position)
        {
            if (dimensionShiftFx == null) return;
            dimensionShiftFx.transform.position = position + Vector3.up * 1f;
            dimensionShiftFx.Play();
        }

        public void SpawnDashDust(Vector3 position)
        {
            if (dashDustFx == null) return;
            dashDustFx.transform.position = position + Vector3.up * 0.05f;
            dashDustFx.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            dashDustFx.Play();
        }
    }
}
