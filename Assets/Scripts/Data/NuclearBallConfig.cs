using UnityEngine;

namespace DodgeGame.Data
{
    [CreateAssetMenu(fileName = "NuclearBallConfig", menuName = "DodgeGame/Nuclear Ball Config")]
    public class NuclearBallConfig : BallConfig
    {
        [Header("Nuclear Explosion")]
        [Tooltip("Countdown in seconds from spawn until detonation.")]
        [SerializeField] private float fuseTime = 3f;

        [Tooltip("Maximum radius reached by the expanding shockwave ring.")]
        [SerializeField] private float shockwaveMaxRadius = 15f;

        [Tooltip("Time in seconds for the shockwave ring to expand to max radius.")]
        [SerializeField] private float shockwaveDuration = 1.5f;

        [Tooltip("Damage dealt to player on contact with the shockwave ring.")]
        [SerializeField] private int shockwaveDamage = 1;

        public float FuseTime => fuseTime;
        public float ShockwaveMaxRadius => shockwaveMaxRadius;
        public float ShockwaveDuration => shockwaveDuration;
        public int ShockwaveDamage => shockwaveDamage;
    }
}
