using System;
using UnityEngine;
using DodgeGame.Player;

namespace DodgeGame.Hazards
{
    [RequireComponent(typeof(BoxCollider))]
    public class GooPatch : MonoBehaviour
    {
        [Header("Visual Components")]
        [Tooltip("MeshRenderer of the goo quad/cube to fade over its final second.")]
        [SerializeField] private MeshRenderer meshRenderer;

        private Action<GooPatch> returnToPoolCallback;
        private float lifeTimer;
        private float duration = 4f;
        private float slowMultiplier = 0.5f;
        private PlayerController overlappingPlayer;

        private MaterialPropertyBlock propertyBlock;
        private Color originalBaseColor;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private void Awake()
        {
            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }

            propertyBlock = new MaterialPropertyBlock();

            if (meshRenderer != null && meshRenderer.sharedMaterial != null)
            {
                originalBaseColor = meshRenderer.sharedMaterial.HasProperty(BaseColorId)
                    ? meshRenderer.sharedMaterial.GetColor(BaseColorId)
                    : new Color(0.2f, 0.9f, 0.2f, 0.6f);
            }
            else
            {
                originalBaseColor = new Color(0.2f, 0.9f, 0.2f, 0.6f);
            }
        }

        public void Initialize(Action<GooPatch> onReturnToPool, float patchDuration, float slowMult)
        {
            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }
            if (propertyBlock == null)
            {
                propertyBlock = new MaterialPropertyBlock();
            }

            returnToPoolCallback = onReturnToPool;
            duration = patchDuration;
            lifeTimer = patchDuration;
            slowMultiplier = slowMult;

            ResetVisuals();
        }

        private void Update()
        {
            lifeTimer -= Time.deltaTime;

            UpdateFadeVisuals();

            if (lifeTimer <= 0f)
            {
                ReturnToPool();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var player = other.GetComponent<PlayerController>() ?? other.GetComponentInParent<PlayerController>();
                if (player != null)
                {
                    overlappingPlayer = player;
                    player.SetGooSlow(true, slowMultiplier);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var player = other.GetComponent<PlayerController>() ?? other.GetComponentInParent<PlayerController>();
                if (player != null)
                {
                    player.SetGooSlow(false);
                    if (overlappingPlayer == player)
                    {
                        overlappingPlayer = null;
                    }
                }
            }
        }

        private void UpdateFadeVisuals()
        {
            if (meshRenderer == null) return;
            if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();

            float fadeThreshold = 1.0f;
            if (lifeTimer < fadeThreshold)
            {
                float progress = Mathf.Clamp01(lifeTimer / fadeThreshold);
                Color current = originalBaseColor;
                current.a = originalBaseColor.a * progress;

                meshRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(BaseColorId, current);
                meshRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void ResetVisuals()
        {
            if (meshRenderer == null) return;
            if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();

            meshRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorId, originalBaseColor);
            meshRenderer.SetPropertyBlock(propertyBlock);
        }

        private void ReturnToPool()
        {
            if (overlappingPlayer != null)
            {
                overlappingPlayer.SetGooSlow(false);
                overlappingPlayer = null;
            }

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
