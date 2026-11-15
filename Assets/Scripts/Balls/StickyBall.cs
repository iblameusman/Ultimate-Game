using UnityEngine;
using DodgeGame.Core;
using DodgeGame.Data;
using DodgeGame.Hazards;
using DodgeGame.Player;

namespace DodgeGame.Balls
{
    public class StickyBall : BaseBall
    {
        [Header("Sticky Tuning")]
        [Tooltip("Configuration containing slow multiplier, slow duration, and goo duration.")]
        [SerializeField] private StickyBallConfig stickyConfig;

        [Header("Goo Hazard")]
        [Tooltip("Prefab of the GooPatch spawned on floor bounces.")]
        [SerializeField] private GooPatch gooPatchPrefab;

        [Tooltip("Parent transform for pooled goo patches.")]
        [SerializeField] private Transform poolParent;

        private ObjectPool<GooPatch> gooPool;

        protected override void Awake()
        {
            if (stickyConfig != null && ballConfig == null)
            {
                ballConfig = stickyConfig;
            }

            base.Awake();
            InitializeGooPool();
        }

        private void InitializeGooPool()
        {
            if (gooPatchPrefab != null && gooPool == null)
            {
                Transform parent = poolParent != null ? poolParent : transform.parent;
                gooPool = new ObjectPool<GooPatch>(gooPatchPrefab, initialCapacity: 12, parent: parent);
            }
        }

        protected override void OnBounce(Collision collision)
        {
            base.OnBounce(collision);

            if (gooPool == null)
            {
                InitializeGooPool();
            }

            if (gooPool != null && collision.contactCount > 0)
            {
                ContactPoint contact = collision.GetContact(0);
                Vector3 spawnPos = contact.point;

                spawnPos.y = 0.02f;

                float duration = stickyConfig != null ? stickyConfig.GooDuration : 4f;
                float slowMult = stickyConfig != null ? stickyConfig.SlowMultiplier : 0.5f;

                GooPatch patch = gooPool.Get();
                patch.transform.position = spawnPos;
                patch.transform.rotation = Quaternion.identity;
                patch.Initialize(OnGooExpired, duration, slowMult);
            }
        }

        private void OnGooExpired(GooPatch patch)
        {
            if (gooPool != null && patch != null)
            {
                gooPool.Release(patch);
            }
        }

        protected override void OnHitPlayer(PlayerHealth player)
        {
            base.OnHitPlayer(player);

            if (player != null)
            {
                AudioManager.Instance?.PlayStickyHit(player.transform.position);

                var controller = player.GetComponent<PlayerController>()
                    ?? player.GetComponentInParent<PlayerController>();

                if (controller != null && stickyConfig != null)
                {
                    controller.ApplySlow(stickyConfig.SlowMultiplier, stickyConfig.SlowDuration);
                }
            }
        }
    }
}
