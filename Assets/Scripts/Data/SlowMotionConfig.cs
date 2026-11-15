using UnityEngine;

namespace DodgeGame.Data
{
    [CreateAssetMenu(fileName = "SlowMotionConfig", menuName = "DodgeGame/SlowMotionConfig", order = 4)]
    public class SlowMotionConfig : ScriptableObject
    {
        [Header("Time Dilation")]
        [Tooltip("Target time scale applied to balls, hazards, projectiles, and game systems (0.1 to 0.9).")]
        [Range(0.1f, 0.9f)]
        [SerializeField] private float timeScale = 0.4f;

        [Tooltip("Active slow motion duration in real unscaled seconds.")]
        [Range(1f, 10f)]
        [SerializeField] private float duration = 3.0f;

        [Header("Meter Economy")]
        [Tooltip("Amount of meter awarded per successful close dodge.")]
        [Range(1f, 50f)]
        [SerializeField] private float meterPerDodge = 8.0f;

        [Tooltip("Amount of meter subtracted when taking a hit.")]
        [Range(1f, 50f)]
        [SerializeField] private float meterPenalty = 15.0f;

        [Tooltip("Maximum meter capacity required to activate Slow Motion.")]
        [Range(50f, 200f)]
        [SerializeField] private float meterMax = 100.0f;

        [Header("Visual Feedback")]
        [Tooltip("Vignette color overlaid around screen borders during slow motion.")]
        [SerializeField] private Color vignetteColor = new Color(0.05f, 0.25f, 0.65f, 0.65f);

        [Tooltip("Full-screen ambient tint color applied during slow motion.")]
        [SerializeField] private Color tintColor = new Color(0.1f, 0.45f, 0.95f, 0.18f);

        public float TimeScale => Mathf.Clamp(timeScale, 0.05f, 0.95f);
        public float Duration => Mathf.Max(0.5f, duration);
        public float MeterPerDodge => Mathf.Max(0f, meterPerDodge);
        public float MeterPenalty => Mathf.Max(0f, meterPenalty);
        public float MeterMax => Mathf.Max(1f, meterMax);
        public Color VignetteColor => vignetteColor;
        public Color TintColor => tintColor;
    }
}
