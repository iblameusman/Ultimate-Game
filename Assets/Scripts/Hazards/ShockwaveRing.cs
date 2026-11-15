using System.Collections.Generic;
using UnityEngine;
using DodgeGame.Player;

namespace DodgeGame.Hazards
{
    [RequireComponent(typeof(SphereCollider))]
    public class ShockwaveRing : MonoBehaviour
    {
        [Header("Visual Components")]
        [Tooltip("MeshRenderer of the shockwave sphere/ring.")]
        [SerializeField] private MeshRenderer meshRenderer;

        private SphereCollider sphereCollider;
        private float maxRadius = 15f;
        private float duration = 1.5f;
        private int damage = 1;

        private float elapsed;
        private readonly HashSet<GameObject> damagedTargets = new HashSet<GameObject>();
        private MaterialPropertyBlock propertyBlock;
        private Color initialColor;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private void Awake()
        {
            sphereCollider = GetComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();

            propertyBlock = new MaterialPropertyBlock();

            if (meshRenderer != null && meshRenderer.sharedMaterial != null)
            {
                initialColor = meshRenderer.sharedMaterial.HasProperty(BaseColorId)
                    ? meshRenderer.sharedMaterial.GetColor(BaseColorId)
                    : new Color(1f, 0.9f, 0.2f, 0.75f);
            }
            else
            {
                initialColor = new Color(1f, 0.9f, 0.2f, 0.75f);
            }
        }

        public void Initialize(float targetRadius, float expandDuration, int ringDamage)
        {
            maxRadius = Mathf.Max(1f, targetRadius);
            duration = Mathf.Max(0.1f, expandDuration);
            damage = ringDamage;
            elapsed = 0f;
            damagedTargets.Clear();

            transform.localScale = Vector3.one * 0.2f;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);

            float currentDiameter = Mathf.Lerp(0.5f, maxRadius * 2f, progress);
            transform.localScale = new Vector3(currentDiameter, currentDiameter * 0.3f, currentDiameter);

            if (meshRenderer != null)
            {
                meshRenderer.GetPropertyBlock(propertyBlock);
                Color c = initialColor;
                c.a = initialColor.a * (1f - progress);
                propertyBlock.SetColor(BaseColorId, c);
                propertyBlock.SetColor(EmissionColorId, initialColor * (1f - progress) * 2f);
                meshRenderer.SetPropertyBlock(propertyBlock);
            }

            if (progress >= 1f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            CheckDamage(other);
        }

        private void OnTriggerStay(Collider other)
        {
            CheckDamage(other);
        }

        private void CheckDamage(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            GameObject rootGo = other.gameObject;
            if (damagedTargets.Contains(rootGo)) return;

            var health = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
            if (health != null)
            {
                if (health.TakeDamage(damage))
                {
                    damagedTargets.Add(rootGo);
                }
            }
        }
    }
}
