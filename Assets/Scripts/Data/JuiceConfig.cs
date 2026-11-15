using UnityEngine;

namespace DodgeGame.Data
{
    [CreateAssetMenu(fileName = "JuiceConfig", menuName = "DodgeGame/Juice Config")]
    public class JuiceConfig : ScriptableObject
    {
        [Header("Screen Shake")]
        [Tooltip("Base displacement amplitude for camera shake.")]
        [SerializeField] private float shakeAmplitude = 0.35f;

        [Tooltip("Duration in seconds for camera shake decay.")]
        [SerializeField] private float shakeDuration = 0.25f;

        [Tooltip("Perlin noise frequency in Hz.")]
        [SerializeField] private float shakeFrequency = 22f;

        [Header("Hit Stop")]
        [Tooltip("Freeze duration in seconds applied to gameplay systems on player damage.")]
        [SerializeField] private float hitStopDuration = 0.05f;

        [Header("Deformation & Squash")]
        [Tooltip("Deformation ratio applied to balls on bounce contact (0.12 = 12%).")]
        [SerializeField] private float squashAndStretchAmount = 0.12f;

        [Header("Camera Punch")]
        [Tooltip("Camera punch backward impulse strength on hit.")]
        [SerializeField] private float cameraPunchStrength = 0.25f;

        [Header("Ball Spawn Pop")]
        [Tooltip("Duration in seconds for balls to scale pop from 0 to 1 on spawn.")]
        [SerializeField] private float ballSpawnPopDuration = 0.25f;

        public float ShakeAmplitude => shakeAmplitude;
        public float ShakeDuration => shakeDuration;
        public float ShakeFrequency => shakeFrequency;
        public float HitStopDuration => hitStopDuration;
        public float SquashAndStretchAmount => squashAndStretchAmount;
        public float CameraPunchStrength => cameraPunchStrength;
        public float BallSpawnPopDuration => ballSpawnPopDuration;
    }
}
