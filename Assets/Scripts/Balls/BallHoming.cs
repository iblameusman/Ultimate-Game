using System;
using UnityEngine;
using DodgeGame.Core;
using DodgeGame.Data;
using DodgeGame.Player;

namespace DodgeGame.Balls
{
    [RequireComponent(typeof(BaseBall))]
    [RequireComponent(typeof(Rigidbody))]
    public class BallHoming : MonoBehaviour
    {
        private BaseBall baseBall;
        private Rigidbody rb;
        private BallJuice ballJuice;
        private Transform playerTarget;
        private PlayerHealth cachedPlayerHealth;

        private int floorBounceCount = 0;
        private bool isHoming = false;
        private float homingTimer = 0f;

        public int FloorBounceCount => floorBounceCount;
        public bool IsHoming => isHoming;
        public float HomingTimer => homingTimer;

        public event Action OnHomingActivated;
        public event Action OnHomingDeactivated;

        private void Awake()
        {
            baseBall = GetComponent<BaseBall>();
            rb = GetComponent<Rigidbody>();
            ballJuice = GetComponent<BallJuice>();
        }

        private void Start()
        {
            FindPlayerTarget();
        }

        private void OnEnable()
        {
            ResetHoming();
        }

        public void ResetHoming()
        {
            floorBounceCount = 0;
            if (isHoming)
            {
                StopHoming();
            }
            homingTimer = 0f;
        }

        public void HandleCollision(Collision collision, Vector3 preImpactVelocity)
        {
            if (collision == null) return;

            if (IsFloorCollision(collision))
            {
                if (preImpactVelocity.y < -0.1f)
                {
                    floorBounceCount++;

                    BallConfig config = baseBall != null ? baseBall.Config : null;
                    int threshold = config != null ? config.HomingAfterBounces : 2;
                    bool enabled = config != null && config.HomingEnabled;

                    if (enabled && !isHoming && floorBounceCount >= threshold)
                    {
                        StartHoming();
                    }
                }
            }
        }

        private bool IsFloorCollision(Collision collision)
        {
            if (collision.collider != null)
            {
                if (collision.collider.CompareTag("Floor")) return true;
                string colName = collision.collider.gameObject.name;
                if (colName.IndexOf("Floor", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    colName.IndexOf("Ground", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            if (collision.contactCount > 0)
            {
                if (collision.GetContact(0).normal.y > 0.7f)
                {
                    return true;
                }
            }

            return false;
        }

        public void StartHoming()
        {
            if (isHoming) return;

            BallConfig config = baseBall != null ? baseBall.Config : null;
            if (config != null && !config.HomingEnabled) return;

            isHoming = true;
            homingTimer = 0f;

            SoundManager.Instance?.PlayHomingAlertSound();

            if (ballJuice != null)
            {
                ballJuice.PlayHomingFlash(0.2f);
            }

            OnHomingActivated?.Invoke();
        }

        public void StopHoming()
        {
            if (!isHoming) return;

            isHoming = false;
            homingTimer = 0f;
            OnHomingDeactivated?.Invoke();
        }

        private void FixedUpdate()
        {
            if (!isHoming || rb == null) return;

            BallConfig config = baseBall != null ? baseBall.Config : null;

            float maxLifetime = config != null ? config.HomingLifetime : 6f;
            homingTimer += Time.fixedDeltaTime;
            if (homingTimer >= maxLifetime)
            {
                StopHoming();
                return;
            }

            if (playerTarget == null)
            {
                FindPlayerTarget();
            }

            if (playerTarget == null)
            {
                StopHoming();
                return;
            }

            if (cachedPlayerHealth == null)
            {
                cachedPlayerHealth = playerTarget.GetComponent<PlayerHealth>()
                    ?? playerTarget.GetComponentInParent<PlayerHealth>();
            }

            if (cachedPlayerHealth != null && cachedPlayerHealth.IsDead)
            {
                StopHoming();
                return;
            }

            Vector3 horizVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            float horizSpeed = horizVel.magnitude;
            if (horizSpeed < 0.05f)
            {
                float targetSpeed = baseBall != null ? baseBall.TargetSpeed : 5f;
                horizVel = transform.forward * targetSpeed;
                horizSpeed = targetSpeed;
            }

            Vector3 currentDir = horizVel.normalized;
            Vector3 toPlayer = playerTarget.position - transform.position;
            toPlayer.y = 0f;

            if (toPlayer.sqrMagnitude > 0.01f)
            {
                Vector3 playerDir = toPlayer.normalized;
                float strength = config != null ? Mathf.Clamp01(config.HomingStrength) : 0.6f;
                Vector3 desiredDir = Vector3.Slerp(currentDir, playerDir, strength).normalized;

                float turnRateDeg = config != null ? config.HomingTurnRate : 90f;
                float maxRadiansDelta = turnRateDeg * Mathf.Deg2Rad * Time.fixedDeltaTime;

                Vector3 steeredDir = Vector3.RotateTowards(currentDir, desiredDir, maxRadiansDelta, 0f).normalized;

                rb.linearVelocity = new Vector3(
                    steeredDir.x * horizSpeed,
                    rb.linearVelocity.y,
                    steeredDir.z * horizSpeed
                );
            }
        }

        private void FindPlayerTarget()
        {
            if (playerTarget != null) return;
            var playerGo = GameObject.FindWithTag("Player");
            if (playerGo != null)
            {
                playerTarget = playerGo.transform;
                cachedPlayerHealth = playerTarget.GetComponent<PlayerHealth>()
                    ?? playerTarget.GetComponentInParent<PlayerHealth>();
            }
        }
    }
}
