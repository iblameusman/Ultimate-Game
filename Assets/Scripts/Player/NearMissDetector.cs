using System;
using System.Collections.Generic;
using UnityEngine;
using DodgeGame.Balls;
using DodgeGame.Core;

namespace DodgeGame.Player
{
    public class NearMissDetector : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Trigger distance around player collider center to detect near-misses.")]
        [SerializeField] private float nearMissDistance = 0.5f;

        [Header("References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private DashController dashController;
        [SerializeField] private DimensionShifter dimensionShifter;
        [SerializeField] private Collider playerCollider;

        private SphereCollider triggerCollider;
        private readonly HashSet<Collider> contactingBallColliders = new HashSet<Collider>();
        private float lastNearMissTime = -10f;

        public float NearMissDistance
        {
            get => nearMissDistance;
            set
            {
                nearMissDistance = value;
                if (triggerCollider != null)
                {
                    triggerCollider.radius = nearMissDistance;
                }
            }
        }

        private void Awake()
        {
            if (playerController == null) playerController = GetComponent<PlayerController>();
            if (dashController == null) dashController = GetComponent<DashController>();
            if (dimensionShifter == null) dimensionShifter = GetComponent<DimensionShifter>();
            if (playerCollider == null) playerCollider = GetComponent<Collider>();

            SetupTriggerCollider();
        }

        private void SetupTriggerCollider()
        {
            var triggerGo = new GameObject("NearMissTriggerZone");
            triggerGo.transform.SetParent(transform, false);
            triggerGo.transform.localPosition = playerCollider != null ? playerCollider.bounds.center - transform.position : Vector3.up;

            triggerCollider = triggerGo.AddComponent<SphereCollider>();
            triggerCollider.isTrigger = true;
            float baseRadius = 0.5f;
            if (playerCollider is CapsuleCollider capsule)
            {
                baseRadius = capsule.radius;
                triggerCollider.center = capsule.center;
            }
            triggerCollider.radius = baseRadius + nearMissDistance;

            var proxy = triggerGo.AddComponent<NearMissTriggerProxy>();
            proxy.Initialize(this);
        }

        internal void HandleTriggerEnter(Collider other)
        {
            var ball = other.GetComponentInParent<BaseBall>();
            if (ball == null) return;

            if (contactingBallColliders.Contains(other)) return;

            if (Time.unscaledTime < lastNearMissTime + 0.25f) return;

            lastNearMissTime = Time.unscaledTime;

            PlayerMovementState state = EvaluatePlayerState();
            GameEvents.TriggerNearMiss(state);
        }

        public PlayerMovementState EvaluatePlayerState()
        {
            if (dimensionShifter != null && dimensionShifter.IsShifted)
            {
                return PlayerMovementState.Phased;
            }
            if (dashController != null && dashController.IsDashing)
            {
                return PlayerMovementState.Dashing;
            }
            if (playerController != null && !playerController.IsGrounded)
            {
                return PlayerMovementState.Airborne;
            }
            return PlayerMovementState.Grounded;
        }

        private void OnCollisionEnter(Collision collision)
        {
            var ball = collision.gameObject.GetComponentInParent<BaseBall>();
            if (ball != null)
            {
                contactingBallColliders.Add(collision.collider);
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            contactingBallColliders.Remove(collision.collider);
        }
    }

    public class NearMissTriggerProxy : MonoBehaviour
    {
        private NearMissDetector detector;

        public void Initialize(NearMissDetector owner)
        {
            detector = owner;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (detector != null)
            {
                detector.HandleTriggerEnter(other);
            }
        }
    }
}
