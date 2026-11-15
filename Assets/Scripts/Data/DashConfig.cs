using UnityEngine;

namespace DodgeGame.Data
{
    [CreateAssetMenu(fileName = "DashConfig", menuName = "DodgeGame/Dash Config")]
    public class DashConfig : ScriptableObject
    {
        [Header("Dash Physics")]
        [Tooltip("Total distance traveled over the duration of the dash in units.")]
        [SerializeField] private float distance = 3.5f;

        [Tooltip("Duration of the dash burst in seconds.")]
        [SerializeField] private float duration = 0.15f;

        [Tooltip("Cooldown period before the dash can be used again in seconds.")]
        [SerializeField] private float cooldown = 2.5f;

        [Header("Invulnerability Window")]
        [Tooltip("Invulnerability duration granted during dash (default 0).")]
        [SerializeField] private float dashInvulnerability = 0f;

        [Header("Visual Tuning")]
        [Tooltip("Color tint applied to the dash trail.")]
        [SerializeField] private Color trailColor = new Color(0.2f, 1f, 0.85f, 1f);

        [Tooltip("Speed multiplier curve evaluated over the normalized dash duration (0 to 1).")]
        [SerializeField] private AnimationCurve easeCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        public float Distance => distance;
        public float Duration => duration;
        public float Cooldown => cooldown;
        public float DashInvulnerability => dashInvulnerability;
        public Color TrailColor => trailColor;
        public AnimationCurve EaseCurve => easeCurve;

        public float BaseSpeed => duration > 0f ? distance / duration : 23.33f;
    }
}
