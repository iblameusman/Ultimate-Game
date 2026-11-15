using System.Collections;
using UnityEngine;
using DodgeGame.Core;
using DodgeGame.Data;

namespace DodgeGame.Player
{
    [RequireComponent(typeof(DashController))]
    public class DashVisuals : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The DashController owning dash state and events.")]
        [SerializeField] private DashController dashController;

        [Tooltip("Visual transform for mesh squash and stretch. Defaults to this transform.")]
        [SerializeField] private Transform visualTransform;

        [Tooltip("TrailRenderer displaying the high-speed dash trail.")]
        [SerializeField] private TrailRenderer trailRenderer;

        [Header("Stretch Parameters")]
        [SerializeField] private Vector3 stretchedScale = new Vector3(0.85f, 0.85f, 1.25f);
        [SerializeField] private float stretchInDuration = 0.04f;
        [SerializeField] private float stretchOutDuration = 0.08f;

        private Vector3 defaultScale = Vector3.one;
        private TweenHandle currentScaleTween;

        private void Awake()
        {
            if (dashController == null) dashController = GetComponent<DashController>();
            if (visualTransform == null) visualTransform = transform;
            defaultScale = visualTransform.localScale;

            EnsureTrailRenderer();
        }

        private void OnEnable()
        {
            if (dashController != null)
            {
                dashController.OnDashStart -= HandleDashStart;
                dashController.OnDashStart += HandleDashStart;
                dashController.OnDashEnd -= HandleDashEnd;
                dashController.OnDashEnd += HandleDashEnd;
            }
        }

        private void OnDisable()
        {
            if (dashController != null)
            {
                dashController.OnDashStart -= HandleDashStart;
                dashController.OnDashEnd -= HandleDashEnd;
            }

            if (currentScaleTween != null)
            {
                currentScaleTween.Stop();
                currentScaleTween = null;
            }

            if (visualTransform != null)
            {
                visualTransform.localScale = defaultScale;
            }

            if (trailRenderer != null)
            {
                trailRenderer.emitting = false;
            }
        }

        private void EnsureTrailRenderer()
        {
            if (trailRenderer == null)
            {
                trailRenderer = GetComponent<TrailRenderer>();
                if (trailRenderer == null)
                {
                    trailRenderer = gameObject.AddComponent<TrailRenderer>();
                }
            }

            trailRenderer.time = 0.15f;
            trailRenderer.startWidth = 0.65f;
            trailRenderer.endWidth = 0.05f;
            trailRenderer.emitting = false;

            var trailMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit") 
                ?? Shader.Find("Sprites/Default"));
            trailRenderer.material = trailMat;

            Color baseColor = (dashController != null && dashController.Config != null)
                ? dashController.Config.TrailColor
                : new Color(0.2f, 1f, 0.85f, 1f);

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(baseColor, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(0.85f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            trailRenderer.colorGradient = gradient;
        }

        private void HandleDashStart()
        {
            if (trailRenderer != null)
            {
                trailRenderer.Clear();
                trailRenderer.emitting = true;
            }

            VFXManager.Instance?.SpawnDashDust(transform.position);

            bool reducedMotion = SettingsService.Instance?.CurrentSettings != null 
                && SettingsService.Instance.CurrentSettings.ReducedMotion;

            if (!reducedMotion && visualTransform != null)
            {
                currentScaleTween?.Stop();
                currentScaleTween = Tween.Scale(
                    visualTransform,
                    Vector3.Scale(defaultScale, stretchedScale),
                    stretchInDuration,
                    EaseType.EaseOutCubic
                );
            }
        }

        private void HandleDashEnd()
        {
            if (trailRenderer != null)
            {
                trailRenderer.emitting = false;
            }

            VFXManager.Instance?.SpawnDashDust(transform.position);

            if (visualTransform != null)
            {
                currentScaleTween?.Stop();
                currentScaleTween = Tween.Scale(
                    visualTransform,
                    defaultScale,
                    stretchOutDuration,
                    EaseType.EaseOutBack
                );
            }
        }
    }
}
