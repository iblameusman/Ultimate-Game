using UnityEngine;

namespace DodgeGame.Data
{
    [CreateAssetMenu(fileName = "GunBallConfig", menuName = "DodgeGame/Gun Ball Config")]
    public class GunBallConfig : BallConfig
    {
        [Header("Gun & Projectile Tuning")]
        [Tooltip("Seconds between successive projectile shots.")]
        [SerializeField] private float fireInterval = 2f;

        [Tooltip("Travel speed of the fired projectile.")]
        [SerializeField] private float projectileSpeed = 15f;

        [Tooltip("Maximum lifetime in seconds before a missing projectile despawns.")]
        [SerializeField] private float projectileLifetime = 5f;

        [Tooltip("Damage dealt to the player on projectile impact.")]
        [SerializeField] private int projectileDamage = 1;

        [Tooltip("Random angular spread in degrees applied to firing direction.")]
        [SerializeField] private float spread = 5f;

        public float FireInterval => fireInterval;
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileLifetime => projectileLifetime;
        public int ProjectileDamage => projectileDamage;
        public float Spread => spread;
    }
}
