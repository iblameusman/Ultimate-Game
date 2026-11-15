using UnityEngine;
using UnityEngine.Serialization;

namespace DodgeGame.Data
{
    [CreateAssetMenu(fileName = "BallConfig", menuName = "DodgeGame/Ball Config")]
    public class BallConfig : ScriptableObject
    {
        [Header("Movement & Physics")]
        [Tooltip("Constant speed at which the ball travels.")]
        [SerializeField] private float speed = 10f;

        [Tooltip("Maximum vertical speed (Y velocity) to keep vertical component under control.")]
        [SerializeField] private float maxVerticalSpeed = 4f;

        [Tooltip("Sphere collider radius.")]
        [SerializeField] private float radius = 0.5f;

        [Tooltip("Rigidbody mass.")]
        [SerializeField] private float mass = 1f;

        [Tooltip("Optional bouncy physics material assigned to collider.")]
        [SerializeField] private PhysicsMaterial bounceMaterial;

        [Tooltip("Optional extra impulse force applied on bounce.")]
        [SerializeField] private float bounceForce = 0f;

        [Header("Homing / Player Tracking")]
        [Tooltip("Per-type flag: whether this ball can home in on the player.")]
        [FormerlySerializedAs("enableHoming")]
        [SerializeField] private bool homingEnabled = false;

        [Tooltip("Number of arena floor bounces before ball starts homing into the player.")]
        [FormerlySerializedAs("bouncesBeforeHoming")]
        [SerializeField] private int homingAfterBounces = 2;

        [Tooltip("Steering rate towards player in degrees/second on the horizontal plane when homing.")]
        [SerializeField] private float homingTurnRate = 90f;

        [Tooltip("Blending strength (0 to 1) for homing direction. Default 0.6 blends steering with current direction.")]
        [Range(0f, 1f)]
        [SerializeField] private float homingStrength = 0.6f;

        [Tooltip("Maximum duration in seconds that homing persists before expiring.")]
        [SerializeField] private float homingLifetime = 6f;

        [Header("Combat")]
        [Tooltip("Damage dealt to the player on direct collision.")]
        [SerializeField] private int damage = 1;

        public float Speed => speed;
        public float MaxVerticalSpeed => maxVerticalSpeed;
        public float Radius => radius;
        public float Mass => mass;
        public PhysicsMaterial BounceMaterial => bounceMaterial;
        public float BounceForce => bounceForce;
        public bool HomingEnabled => homingEnabled;
        public bool EnableHoming => homingEnabled;
        public int HomingAfterBounces => homingAfterBounces;
        public int BouncesBeforeHoming => homingAfterBounces;
        public float HomingTurnRate => homingTurnRate;
        public float HomingStrength => homingStrength;
        public float HomingLifetime => homingLifetime;
        public float HomingSpeedMultiplier => 1f;
        public int Damage => damage;
    }
}
