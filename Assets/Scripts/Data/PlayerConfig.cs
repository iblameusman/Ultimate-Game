using UnityEngine;

namespace DodgeGame.Data
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "DodgeGame/Player Config")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Movement")]
        [Tooltip("Horizontal movement speed in units/second.")]
        [SerializeField] private float moveSpeed = 11f;

        [Tooltip("Ground acceleration in units/second^2.")]
        [SerializeField] private float acceleration = 88f;

        [Tooltip("Ground deceleration (braking) in units/second^2.")]
        [SerializeField] private float deceleration = 75f;

        [Tooltip("Air acceleration in units/second^2.")]
        [SerializeField] private float airAcceleration = 50f;

        [Tooltip("Air deceleration (braking) in units/second^2.")]
        [SerializeField] private float airDeceleration = 15f;

        [Tooltip("Rotation smoothing speed towards movement direction.")]
        [SerializeField] private float rotationSpeed = 20f;

        [Header("Jumping & Gravity")]
        [Tooltip("Upward impulse velocity applied when jumping.")]
        [SerializeField] private float jumpForce = 8.5f;

        [Tooltip("Multiplier applied to gravity when falling downward.")]
        [SerializeField] private float fallGravityMultiplier = 2.2f;

        [Tooltip("Multiplier applied to gravity when rising if jump button is released early.")]
        [SerializeField] private float lowJumpGravityMultiplier = 2.5f;

        [Tooltip("Maximum allowed falling speed (terminal velocity).")]
        [SerializeField] private float maxFallSpeed = 25f;

        [Tooltip("Time window in seconds to buffer a jump press before touching ground.")]
        [SerializeField] private float jumpBufferDuration = 0.15f;

        [Tooltip("Grace period in seconds where jump is permitted after leaving ground.")]
        [SerializeField] private float coyoteTimeDuration = 0.15f;

        [Header("Ground Check")]
        [Tooltip("Distance below the player's collider to check for ground.")]
        [SerializeField] private float groundCheckDistance = 0.15f;

        [Tooltip("Radius of the ground check sphere cast relative to capsule radius.")]
        [SerializeField] private float groundCheckRadius = 0.45f;

        [Tooltip("Layers recognized as walkable ground.")]
        [SerializeField] private LayerMask groundLayerMask;

        [Tooltip("Maximum walkable slope angle in degrees.")]
        [SerializeField] private float maxSlopeAngle = 50f;

        [Header("Health & Defense")]
        [Tooltip("Starting and maximum lives for the player.")]
        [SerializeField] private int maxLives = 3;

        [Tooltip("Duration of invulnerability window after receiving damage (seconds).")]
        [SerializeField] private float invulnerabilityDuration = 1.5f;

        public float MoveSpeed => moveSpeed;
        public float Acceleration => acceleration;
        public float Deceleration => deceleration;
        public float AirAcceleration => airAcceleration;
        public float AirDeceleration => airDeceleration;
        public float RotationSpeed => rotationSpeed;
        public float JumpForce => jumpForce;
        public float FallGravityMultiplier => fallGravityMultiplier;
        public float LowJumpGravityMultiplier => lowJumpGravityMultiplier;
        public float MaxFallSpeed => maxFallSpeed;
        public float JumpBufferDuration => jumpBufferDuration;
        public float CoyoteTimeDuration => coyoteTimeDuration;
        public float GroundCheckDistance => groundCheckDistance;
        public float GroundCheckRadius => groundCheckRadius;
        public LayerMask GroundLayerMask => groundLayerMask;
        public float MaxSlopeAngle => maxSlopeAngle;
        public int MaxLives => maxLives;
        public float InvulnerabilityDuration => invulnerabilityDuration;
    }
}
