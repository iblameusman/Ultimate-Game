using System;
using UnityEngine;
using DodgeGame.Player;

namespace DodgeGame.Hazards
{
    [RequireComponent(typeof(SphereCollider))]
    public class Projectile : MonoBehaviour
    {
        private Action<Projectile> returnToPoolCallback;
        private Vector3 moveDirection;
        private float speed = 15f;
        private float lifeTimer;
        private int damage = 1;

        public void Initialize(Action<Projectile> onReturnToPool, Vector3 direction, float projSpeed, float lifetime, int projDamage)
        {
            returnToPoolCallback = onReturnToPool;
            moveDirection = direction.normalized;
            speed = projSpeed;
            lifeTimer = lifetime;
            damage = projDamage;

            if (moveDirection.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            }
        }

        private void Update()
        {
            transform.position += moveDirection * (speed * Time.deltaTime);

            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0f)
            {
                ReturnToPool();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var health = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                }
                ReturnToPool();
            }
            else if (other.gameObject.layer == LayerMask.NameToLayer("Arena"))
            {
                ReturnToPool();
            }
        }

        private void ReturnToPool()
        {
            if (returnToPoolCallback != null)
            {
                returnToPoolCallback(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
