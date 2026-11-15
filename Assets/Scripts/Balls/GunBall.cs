using UnityEngine;
using DodgeGame.Core;
using DodgeGame.Data;
using DodgeGame.Hazards;

namespace DodgeGame.Balls
{
    public class GunBall : BaseBall
    {
        [Header("Gun Tuning")]
        [Tooltip("Configuration containing fire interval, projectile speed, spread, etc.")]
        [SerializeField] private GunBallConfig gunConfig;

        [Header("Projectile Hazard")]
        [Tooltip("Prefab of the Projectile fired at the player.")]
        [SerializeField] private Projectile projectilePrefab;

        [Tooltip("Parent transform for pooled projectiles.")]
        [SerializeField] private Transform poolParent;

        private ObjectPool<Projectile> projectilePool;
        private float fireTimer;

        protected override void Awake()
        {
            if (gunConfig != null && ballConfig == null)
            {
                ballConfig = gunConfig;
            }

            base.Awake();
            InitializeProjectilePool();
        }

        protected override void Start()
        {
            base.Start();
            fireTimer = gunConfig != null ? gunConfig.FireInterval : 2f;

            var playerGo = GameObject.FindWithTag("Player");
            if (playerGo != null)
            {
                playerTarget = playerGo.transform;
            }
        }

        private void InitializeProjectilePool()
        {
            if (projectilePrefab != null && projectilePool == null)
            {
                Transform parent = poolParent != null ? poolParent : transform.parent;
                projectilePool = new ObjectPool<Projectile>(projectilePrefab, initialCapacity: 15, parent: parent);
            }
        }

        protected override void Update()
        {
            base.Update();
            fireTimer -= Time.deltaTime;

            if (fireTimer <= 0f)
            {
                FireAtPlayer();
                fireTimer = gunConfig != null ? gunConfig.FireInterval : 2f;
            }
        }

        private void FireAtPlayer()
        {
            if (projectilePool == null)
            {
                InitializeProjectilePool();
            }

            if (projectilePool == null) return;

            if (playerTarget == null)
            {
                var playerGo = GameObject.FindWithTag("Player");
                if (playerGo != null) playerTarget = playerGo.transform;
            }

            Vector3 targetPos = playerTarget != null ? playerTarget.position : Vector3.zero;
            Vector3 aimDirection = (targetPos - transform.position).normalized;

            float maxSpread = gunConfig != null ? gunConfig.Spread : 5f;
            float spreadAngle = Random.Range(-maxSpread, maxSpread);
            Quaternion spreadRotation = Quaternion.Euler(0f, spreadAngle, 0f);
            Vector3 finalDirection = spreadRotation * aimDirection;

            Projectile proj = projectilePool.Get();
            proj.transform.position = transform.position + (finalDirection * (sphereCollider.radius + 0.3f));

            float speed = gunConfig != null ? gunConfig.ProjectileSpeed : 15f;
            float lifetime = gunConfig != null ? gunConfig.ProjectileLifetime : 5f;
            int damage = gunConfig != null ? gunConfig.ProjectileDamage : 1;

            proj.Initialize(OnProjectileRecycled, finalDirection, speed, lifetime, damage);
            AudioManager.Instance?.PlayGunFire(transform.position);
        }

        private void OnProjectileRecycled(Projectile proj)
        {
            if (projectilePool != null && proj != null)
            {
                projectilePool.Release(proj);
            }
        }
    }
}
