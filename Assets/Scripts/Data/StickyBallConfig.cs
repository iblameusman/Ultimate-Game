using UnityEngine;

namespace DodgeGame.Data
{
    [CreateAssetMenu(fileName = "StickyBallConfig", menuName = "DodgeGame/Sticky Ball Config")]
    public class StickyBallConfig : BallConfig
    {
        [Header("Sticky & Goo Tuning")]
        [Tooltip("Multiplier applied to player move speed when slowed (e.g. 0.5 = 50% speed).")]
        [SerializeField] private float slowMultiplier = 0.5f;

        [Tooltip("Duration of the slow effect upon direct ball impact (seconds).")]
        [SerializeField] private float slowDuration = 2f;

        [Tooltip("Lifetime of spawned goo patches on the floor (seconds).")]
        [SerializeField] private float gooDuration = 4f;

        public float SlowMultiplier => slowMultiplier;
        public float SlowDuration => slowDuration;
        public float GooDuration => gooDuration;
    }
}
