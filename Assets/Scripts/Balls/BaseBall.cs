using System;
using UnityEngine;
using DodgeGame.Core;
using DodgeGame.Data;
using DodgeGame.Player;

namespace DodgeGame.Balls
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(BallHoming))]
    public abstract class BaseBall : MonoBehaviour
    {
        [Header("Ball Configuration")]
        [Tooltip("ScriptableObject containing speed, damage, mass, physics, and homing tuning.")]
        [SerializeField] protected BallConfig ballConfig;

        [Header("Visual Feedback")]
        [Tooltip("Optional TrailRenderer to visualize ball trajectory. Created automatically if missing.")]
        [SerializeField] protected TrailRenderer trailRenderer;

        protected Rigidbody rb;
        protected SphereCollider sphereCollider;
        protected MeshRenderer meshRenderer;
        protected MaterialPropertyBlock propertyBlock;
        protected Transform playerTarget;
        protected BallHoming homing;
        protected BallJuice ballJuice;

        protected Vector3 prePhysicsVelocity;
        private float lowSpeedDuration = 0f;

        public BallConfig Config => ballConfig;
        public BallHoming Homing => homing;
        public int BounceCount => homing != null ? homing.FloorBounceCount : 0;
        public bool IsHoming => homing != null && homing.IsHoming;

        public float TargetSpeed
        {
            get
            {
                float baseSpeed = ballConfig != null ? ballConfig.Speed : 5f;
                float multiplier = SettingsService.Instance != null ? SettingsService.Instance.GetCurrentDifficultyPreset().BallSpeedMultiplier : 1f;
                return baseSpeed * multiplier;
            }
        }

        public static float ArenaLimitX = 29.2f;
        public static float ArenaLimitZ = 29.2f;
        protected const float MinHeightY = 0.35f;
        protected const float MaxHeightY = 5.0f;

        private bool hasEnteredDangerZone = false;
        private static int totalPlayerDodges = 0;
        private const float DodgeDangerRadius = 2.8f;
        private const float DodgeClearRadius = 4.0f;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody>();
            sphereCollider = GetComponent<SphereCollider>();
            meshRenderer = GetComponent<MeshRenderer>();
            ballJuice = GetComponent<BallJuice>();
            homing = GetComponent<BallHoming>() ?? gameObject.AddComponent<BallHoming>();
            propertyBlock = new MaterialPropertyBlock();

            EnsureTrailRenderer();
            ApplyConfigProperties();
        }

        protected virtual void Start()
        {
            FindPlayerTarget();
            LaunchInitialVelocity();
        }

        protected virtual void OnEnable()
        {
            lowSpeedDuration = 0f;
            if (homing != null)
            {
                homing.ResetHoming();
            }
        }

        protected virtual void FixedUpdate()
        {
            prePhysicsVelocity = rb != null ? rb.linearVelocity : Vector3.zero;

            EnforceSpeedAndVerticalLimits();

            EnforceAntiSticking();

            ApplyRollingPhysics();

            EnforceArenaBoundsSafety();

            CheckDodgeProximity();
        }

        protected virtual void Update()
        {
        }

        protected virtual void EnforceSpeedAndVerticalLimits()
        {
            if (rb == null || ballConfig == null) return;

            Vector3 vel = rb.linearVelocity;

            float maxVert = ballConfig.MaxVerticalSpeed > 0f ? ballConfig.MaxVerticalSpeed : 4f;
            vel.y = Mathf.Clamp(vel.y, -maxVert, maxVert);

            float targetSpeed = TargetSpeed;
            float currentSpeed = vel.magnitude;

            if (targetSpeed > 0.001f && currentSpeed > 0.001f)
            {
                float driftRatio = Mathf.Abs(currentSpeed - targetSpeed) / targetSpeed;
                if (driftRatio > 0.02f)
                {
                    vel = vel.normalized * targetSpeed;
                }
            }

            rb.linearVelocity = vel;
        }

        protected virtual void EnforceAntiSticking()
        {
            if (rb == null || ballConfig == null) return;

            float targetSpeed = TargetSpeed;
            float currentSpeed = rb.linearVelocity.magnitude;
            float stuckThreshold = targetSpeed * 0.4f;

            if (currentSpeed < stuckThreshold)
            {
                lowSpeedDuration += Time.fixedDeltaTime;
                if (lowSpeedDuration >= 0.2f)
                {
                    UnstickBall(targetSpeed);
                    lowSpeedDuration = 0f;
                }
            }
            else
            {
                lowSpeedDuration = 0f;
            }
        }

        private void UnstickBall(float targetSpeed)
        {
            Vector3 toCenter = (Vector3.zero - transform.position);
            toCenter.y = 0f;
            if (toCenter.sqrMagnitude < 0.04f)
            {
                toCenter = Vector3.forward;
            }
            toCenter.Normalize();

            float randomYaw = UnityEngine.Random.Range(-30f, 30f);
            Vector3 impulseDir = Quaternion.Euler(0f, randomYaw, 0f) * toCenter;
            impulseDir.y = 0.25f;
            impulseDir.Normalize();

            rb.linearVelocity = impulseDir * targetSpeed;
        }

        protected virtual void ApplyRollingPhysics()
        {
            if (rb == null || sphereCollider == null) return;
            Vector3 horiz = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            float speed = horiz.magnitude;
            if (speed > 0.05f)
            {
                Vector3 rollAxis = Vector3.Cross(Vector3.up, horiz.normalized);
                float radius = Mathf.Max(0.1f, sphereCollider.radius);
                rb.angularVelocity = rollAxis * (speed / radius);
            }
        }

        protected virtual void EnforceArenaBoundsSafety()
        {
            if (rb == null) return;

            Vector3 pos = transform.position;
            Vector3 vel = rb.linearVelocity;
            bool adjusted = false;

            if (pos.x > ArenaLimitX)
            {
                pos.x = ArenaLimitX;
                vel.x = -Mathf.Abs(vel.x);
                adjusted = true;
            }
            else if (pos.x < -ArenaLimitX)
            {
                pos.x = -ArenaLimitX;
                vel.x = Mathf.Abs(vel.x);
                adjusted = true;
            }

            if (pos.z > ArenaLimitZ)
            {
                pos.z = ArenaLimitZ;
                vel.z = -Mathf.Abs(vel.z);
                adjusted = true;
            }
            else if (pos.z < -ArenaLimitZ)
            {
                pos.z = -ArenaLimitZ;
                vel.z = Mathf.Abs(vel.z);
                adjusted = true;
            }

            if (pos.y < MinHeightY)
            {
                pos.y = MinHeightY;
                if (vel.y < 0f) vel.y = Mathf.Abs(vel.y) * 0.8f;
                adjusted = true;
            }
            else if (pos.y > MaxHeightY)
            {
                pos.y = MaxHeightY;
                if (vel.y > 0f) vel.y = -Mathf.Abs(vel.y) * 0.5f;
                adjusted = true;
            }

            if (adjusted)
            {
                rb.position = pos;
                rb.linearVelocity = vel;
            }
        }

        public virtual void LaunchInitialVelocity()
        {
            if (rb == null || ballConfig == null) return;

            float randomAngle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 randomHorizontalDir = new Vector3(Mathf.Cos(randomAngle), 0f, Mathf.Sin(randomAngle)).normalized;

            rb.linearVelocity = randomHorizontalDir * TargetSpeed;
        }

        protected virtual void ApplyConfigProperties()
        {
            if (ballConfig == null) return;

            if (sphereCollider != null)
            {
                sphereCollider.radius = ballConfig.Radius;
                if (ballConfig.BounceMaterial != null)
                {
                    sphereCollider.material = ballConfig.BounceMaterial;
                }
            }

            if (rb != null)
            {
                rb.mass = ballConfig.Mass;
                rb.useGravity = true;
                rb.linearDamping = 0f;
                rb.angularDamping = 0f;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }

        protected virtual void EnsureTrailRenderer()
        {
            if (trailRenderer == null)
            {
                trailRenderer = GetComponent<TrailRenderer>();
            }

            if (trailRenderer == null)
            {
                trailRenderer = gameObject.AddComponent<TrailRenderer>();
                trailRenderer.time = 0.35f;
                trailRenderer.startWidth = 0.4f;
                trailRenderer.endWidth = 0.02f;
                trailRenderer.autodestruct = false;

                var trailMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit")
                    ?? Shader.Find("Particles/Standard Unlit")
                    ?? Shader.Find("Sprites/Default"));
                trailRenderer.material = trailMat;

                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.7f, 0.2f), 0.0f), new GradientColorKey(Color.red, 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(0.8f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
                );
                trailRenderer.colorGradient = gradient;
            }
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            int arenaLayer = LayerMask.NameToLayer("Arena");
            int playerLayer = LayerMask.NameToLayer("Player");
            int ballLayer = LayerMask.NameToLayer("Ball");

            if (collision.gameObject.layer == arenaLayer)
            {
                OnBounce(collision);
            }
            else if (collision.gameObject.layer == ballLayer)
            {
                OnBallBounce(collision);
            }
            else if (collision.gameObject.layer == playerLayer || collision.gameObject.CompareTag("Player"))
            {
                hasEnteredDangerZone = false;
                var playerHealth = collision.gameObject.GetComponent<PlayerHealth>()
                    ?? collision.gameObject.GetComponentInParent<PlayerHealth>();

                if (playerHealth != null)
                {
                    OnHitPlayer(playerHealth);
                }
            }
        }

        protected virtual void OnBallBounce(Collision collision)
        {
            if (collision.contactCount > 0)
            {
                ContactPoint contact = collision.GetContact(0);
                AudioManager.Instance?.PlayBallBounce(contact.point, 1.25f);
                VFXManager.Instance?.SpawnBounceSparks(contact.point, contact.normal);
                TriggerSquashAndStretch(contact.normal);
            }
        }

        protected virtual void OnBounce(Collision collision)
        {
            if (hasEnteredDangerZone)
            {
                hasEnteredDangerZone = false;
                totalPlayerDodges++;
                GameEvents.TriggerPlayerDodged(totalPlayerDodges);
            }

            Vector3 preVel = prePhysicsVelocity.sqrMagnitude > 0.01f ? prePhysicsVelocity : rb.linearVelocity;

            if (collision.contactCount > 0)
            {
                ContactPoint contact = collision.GetContact(0);
                AudioManager.Instance?.PlayBallBounce(contact.point);
                VFXManager.Instance?.SpawnBounceSparks(contact.point, contact.normal);
                TriggerSquashAndStretch(contact.normal);

                bool isWall = Mathf.Abs(contact.normal.y) < 0.6f;
                if (isWall)
                {
                    HandleWallReflection(contact.normal, preVel, contact.point);
                }
            }

            if (homing != null)
            {
                homing.HandleCollision(collision, preVel);
            }

            if (ballConfig != null && ballConfig.BounceForce > 0f && collision.contactCount > 0)
            {
                Vector3 contactNormal = collision.GetContact(0).normal;
                rb.AddForce(contactNormal * ballConfig.BounceForce, ForceMode.Impulse);
            }
        }

        protected virtual void HandleWallReflection(Vector3 wallNormal, Vector3 inVelocity, Vector3 contactPoint)
        {
            Vector3 reflected = Vector3.Reflect(inVelocity, wallNormal);
            Vector3 reflectedHoriz = new Vector3(reflected.x, 0f, reflected.z);

            if (reflectedHoriz.sqrMagnitude > 0.01f)
            {
                Vector3 reflDir = reflectedHoriz.normalized;

                if (playerTarget == null)
                {
                    FindPlayerTarget();
                }

                if (playerTarget != null)
                {
                    Vector3 toPlayer = playerTarget.position - contactPoint;
                    toPlayer.y = 0f;

                    if (toPlayer.sqrMagnitude > 0.01f)
                    {
                        Vector3 toPlayerDir = toPlayer.normalized;
                        float alignment = Vector3.Dot(reflDir, toPlayerDir);

                        if (alignment > 0.90f)
                        {
                            Vector3 cross = Vector3.Cross(reflDir, toPlayerDir);
                            float sign = cross.y >= 0f ? -1f : 1f;
                            Quaternion deflectRot = Quaternion.Euler(0f, 30f * sign, 0f);
                            reflDir = (deflectRot * reflDir).normalized;

                            float horizSpeed = reflectedHoriz.magnitude;
                            reflected.x = reflDir.x * horizSpeed;
                            reflected.z = reflDir.z * horizSpeed;
                        }
                    }
                }

                rb.linearVelocity = reflected.normalized * TargetSpeed;
            }
        }

        public void TriggerSquashAndStretch(Vector3 contactNormal)
        {
            if (ballJuice != null)
            {
                ballJuice.TriggerBounceSquash(contactNormal);
            }
        }

        public virtual void StartHoming()
        {
            if (homing != null)
            {
                homing.StartHoming();
            }
        }

        public virtual void ResetBounceCounter()
        {
            if (homing != null)
            {
                homing.ResetHoming();
            }
        }

        protected virtual void CheckDodgeProximity()
        {
            if (playerTarget == null)
            {
                FindPlayerTarget();
                if (playerTarget == null) return;
            }

            float distSqr = (transform.position - playerTarget.position).sqrMagnitude;
            if (!hasEnteredDangerZone)
            {
                if (distSqr <= DodgeDangerRadius * DodgeDangerRadius)
                {
                    hasEnteredDangerZone = true;
                }
            }
            else
            {
                if (distSqr >= DodgeClearRadius * DodgeClearRadius)
                {
                    hasEnteredDangerZone = false;
                    totalPlayerDodges++;
                    GameEvents.TriggerPlayerDodged(totalPlayerDodges);
                }
            }
        }

        protected virtual void FindPlayerTarget()
        {
            if (playerTarget != null) return;
            var playerGo = GameObject.FindWithTag("Player");
            if (playerGo != null)
            {
                playerTarget = playerGo.transform;
            }
        }

        protected virtual void OnHitPlayer(PlayerHealth player)
        {
            if (player != null)
            {
                int damage = ballConfig != null ? ballConfig.Damage : 1;
                player.TakeDamage(damage, this);
            }
        }
    }
}
