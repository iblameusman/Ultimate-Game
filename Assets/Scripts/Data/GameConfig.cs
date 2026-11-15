using UnityEngine;

namespace DodgeGame.Data
{
    [CreateAssetMenu(fileName = "NewGameConfig", menuName = "DodgeGame/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Scoring")]
        [Tooltip("Points awarded per second while the player is alive.")]
        [SerializeField] private float scorePerSecond = 10f;

        [Header("Spawn Settings")]
        [Tooltip("Initial interval in seconds between ball spawns.")]
        [SerializeField] private float spawnInterval = 5f;

        [Tooltip("Minimum allowable interval in seconds between ball spawns.")]
        [SerializeField] private float minSpawnInterval = 1.5f;

        [Tooltip("How much the spawn interval shrinks per minute of gameplay.")]
        [SerializeField] private float difficultyRampRate = 0.5f;

        [Tooltip("Maximum number of concurrently active balls in the arena.")]
        [SerializeField] private int maxActiveBalls = 8;

        [Header("Ball Spawn Weights")]
        [Tooltip("Relative spawn weight for Fire Ball.")]
        [SerializeField] private float fireBallWeight = 30f;

        [Tooltip("Relative spawn weight for Sticky Ball.")]
        [SerializeField] private float stickyBallWeight = 25f;

        [Tooltip("Relative spawn weight for Nuclear Ball.")]
        [SerializeField] private float nuclearBallWeight = 20f;

        [Tooltip("Relative spawn weight for Gun Ball.")]
        [SerializeField] private float gunBallWeight = 25f;

        public float ScorePerSecond => scorePerSecond;
        public float SpawnInterval => spawnInterval;
        public float MinSpawnInterval => minSpawnInterval;
        public float DifficultyRampRate => difficultyRampRate;
        public int MaxActiveBalls => maxActiveBalls;

        public float FireBallWeight => fireBallWeight;
        public float StickyBallWeight => stickyBallWeight;
        public float NuclearBallWeight => nuclearBallWeight;
        public float GunBallWeight => gunBallWeight;
    }
}
