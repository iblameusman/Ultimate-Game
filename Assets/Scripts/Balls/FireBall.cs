using UnityEngine;
using DodgeGame.Core;
using DodgeGame.Data;
using DodgeGame.Hazards;
using DodgeGame.Player;

namespace DodgeGame.Balls
{
    public class FireBall : BaseBall
    {
        [Header("Fire Ball Hazards")]
        [Tooltip("Prefab of the FireTile spawned upon bouncing.")]
        [SerializeField] private FireTile fireTilePrefab;

        [Tooltip("Hazard config for spawned fire tiles.")]
        [SerializeField] private HazardConfig hazardConfig;

        [Tooltip("Parent transform for pooled tiles (optional, keeps hierarchy clean).")]
        [SerializeField] private Transform poolParent;

        private ObjectPool<FireTile> tilePool;

        protected override void Awake()
        {
            base.Awake();
            InitializePool();
        }

        private void InitializePool()
        {
            if (fireTilePrefab != null && tilePool == null)
            {
                Transform parent = poolParent != null ? poolParent : transform.parent;
                tilePool = new ObjectPool<FireTile>(fireTilePrefab, initialCapacity: 15, parent: parent);
            }
        }

        protected override void OnBounce(Collision collision)
        {
            base.OnBounce(collision);

            if (tilePool == null)
            {
                InitializePool();
            }

            if (tilePool != null && collision.contactCount > 0)
            {
                ContactPoint contact = collision.GetContact(0);
                Vector3 spawnPos = contact.point;

                spawnPos.y = 0.02f;

                FireTile tile = tilePool.Get();
                tile.transform.position = spawnPos;
                tile.transform.rotation = Quaternion.identity;
                tile.Initialize(OnTileExpired, hazardConfig);

                AudioManager.Instance?.PlayFireTileSpawn(spawnPos);
                VFXManager.Instance?.SpawnFireTileBurst(spawnPos);
                VFXManager.Instance?.SpawnFireBallBounceImpact(contact.point, contact.normal);
            }
        }

        private void OnTileExpired(FireTile tile)
        {
            if (tilePool != null && tile != null)
            {
                tilePool.Release(tile);
            }
        }

        protected override void OnHitPlayer(PlayerHealth player)
        {
            base.OnHitPlayer(player);
        }
    }
}
