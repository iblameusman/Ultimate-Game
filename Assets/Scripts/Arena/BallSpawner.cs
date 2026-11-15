using System.Collections.Generic;
using UnityEngine;
using DodgeGame.Balls;
using DodgeGame.Core;
using DodgeGame.Data;

namespace DodgeGame.Arena
{
    public class BallSpawner : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Game configuration providing spawn intervals, ramp rates, weights, and ball limits.")]
        [SerializeField] private GameConfig config;

        [Header("Ball Prefabs")]
        [Tooltip("Array of ball prefabs available for spawning (Fire, Sticky, Nuclear, Gun).")]
        [SerializeField] private BaseBall[] ballPrefabs;

        [Header("Spawn Perimeter Bounds")]
        [Tooltip("Distance from arena center to perimeter spawning line along X and Z.")]
        [SerializeField] private float perimeterOffset = 28.0f;

        [Tooltip("Y position for spawned balls above the floor.")]
        [SerializeField] private float spawnHeight = 1.2f;

        [Tooltip("Minimum safe distance from the player when choosing a spawn location.")]
        [SerializeField] private float minDistanceFromPlayer = 4.0f;

        [Header("References")]
        [Tooltip("Transform of the player to avoid spawning directly on top of.")]
        [SerializeField] private Transform playerTransform;

        [Tooltip("GameManager reference to synchronize spawning with game state.")]
        [SerializeField] private GameManager gameManager;

        [Header("Audio & Spawn Warning")]
        [Tooltip("Warning lead time before a ball is instantiated and active.")]
        [SerializeField] private float spawnWarningDelay = 0.3f;

        [Tooltip("Flag for rare or bonus spawns (e.g. golden ball).")]
        [SerializeField] private bool hasRareSpawns = false;

        private float lastSpawnRequestTime = -100f;
        private float lastDunPlayTime = -100f;
        private int lastNuclearOrGunFrame = -100;

        public bool HasRareSpawns { get => hasRareSpawns; set => hasRareSpawns = value; }
        public float SpawnWarningDelay { get => spawnWarningDelay; set => spawnWarningDelay = value; }
        public float PerimeterOffset { get => perimeterOffset; set => perimeterOffset = value; }

        private readonly List<BaseBall> activeBalls = new List<BaseBall>();
        private float spawnTimer;
        private float currentInterval;
        private float gameElapsedTime;
        private bool hasSpawnedInitialBalls = false;
        private int pendingSpawnCount;

        public int ActiveBallCount => activeBalls.Count;
        // Includes balls still in their spawn-warning delay so caps aren't exceeded.
        private int TotalBallCount => activeBalls.Count + pendingSpawnCount;
        public float CurrentInterval => currentInterval;
        public int MaxActiveBalls => SettingsService.Instance != null ? SettingsService.Instance.GetCurrentDifficultyPreset().MaxActiveBalls : (config != null ? config.MaxActiveBalls : 6);

        private void Awake()
        {
            if (gameManager == null)
            {
                gameManager = FindAnyObjectByType<GameManager>();
            }

            if (playerTransform == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                }
            }
        }

        private void Start()
        {
            if (perimeterOffset < 15f) perimeterOffset = 28.0f;
            float spawnMult = SettingsService.Instance != null ? SettingsService.Instance.GetCurrentDifficultyPreset().SpawnIntervalMultiplier : 1f;
            currentInterval = (config != null ? config.SpawnInterval : 4f) * spawnMult;
            spawnTimer = 1f;
            gameElapsedTime = 0f;
            hasSpawnedInitialBalls = false;
            pendingSpawnCount = 0;
        }

        private void Update()
        {
            if (gameManager != null && !gameManager.IsPlaying)
            {
                return;
            }

            if (config == null || ballPrefabs == null || ballPrefabs.Length == 0)
            {
                return;
            }

            activeBalls.RemoveAll(ball => ball == null || !ball.gameObject.activeInHierarchy);

            if (!hasSpawnedInitialBalls)
            {
                hasSpawnedInitialBalls = true;
                SpawnBall();
                SpawnBall();
                spawnTimer = currentInterval;
            }
            else if (TotalBallCount == 0)
            {
                SpawnBall();
                spawnTimer = currentInterval;
            }

            gameElapsedTime += Time.deltaTime;
            float minutesPassed = gameElapsedTime / 60f;
            float spawnMult = SettingsService.Instance != null ? SettingsService.Instance.GetCurrentDifficultyPreset().SpawnIntervalMultiplier : 1f;
            float baseInterval = (config != null ? config.SpawnInterval : 4f) * spawnMult;
            float minInterval = (config != null ? config.MinSpawnInterval : 1.5f) * spawnMult;

            currentInterval = Mathf.Max(
                minInterval,
                baseInterval - (minutesPassed * config.DifficultyRampRate)
            );

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                spawnTimer = currentInterval;

                int maxAllowed = MaxActiveBalls;
                if (TotalBallCount < maxAllowed)
                {
                    SpawnBall();
                }
            }
        }

        public void SpawnBall()
        {
            BaseBall selectedPrefab = SelectWeightedBallPrefab();
            if (selectedPrefab == null) return;

            Vector3 spawnPosition = CalculateSafeEdgePosition();

            BallCategory category = BallCategory.Standard;
            if (selectedPrefab is NuclearBall) category = BallCategory.Nuclear;
            else if (selectedPrefab is GunBall) category = BallCategory.Gun;
            else if (selectedPrefab is FireBall) category = BallCategory.Fire;
            else if (selectedPrefab is StickyBall) category = BallCategory.Sticky;

            if (category == BallCategory.Nuclear || category == BallCategory.Gun)
            {
                lastNuclearOrGunFrame = Time.frameCount;
            }

            float speedMult = SettingsService.Instance != null
                ? SettingsService.Instance.GetCurrentDifficultyPreset().BallSpeedMultiplier
                : 1f;
            bool isFast = speedMult >= 1.4f;

            bool isRare = hasRareSpawns;

            bool isDun = false;
            float currentTime = Time.time;
            if ((currentTime - lastSpawnRequestTime) <= 2.0f && (currentTime - lastDunPlayTime) >= 10.0f)
            {
                if (category != BallCategory.Nuclear && category != BallCategory.Gun && Time.frameCount != lastNuclearOrGunFrame)
                {
                    isDun = true;
                    lastDunPlayTime = currentTime;
                }
            }
            lastSpawnRequestTime = currentTime;

            var spawnInfo = new BallSpawnInfo(category, isFast, isRare, isDun);
            GameEvents.TriggerBallSpawned(spawnInfo);

            if (spawnWarningDelay > 0f && gameObject.activeInHierarchy)
            {
                pendingSpawnCount++;
                StartCoroutine(SpawnBallDelayedRoutine(selectedPrefab, spawnPosition, spawnWarningDelay));
            }
            else
            {
                BaseBall newBall = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
                activeBalls.Add(newBall);
            }
        }

        private System.Collections.IEnumerator SpawnBallDelayedRoutine(BaseBall prefab, Vector3 position, float delay)
        {
            yield return new WaitForSeconds(delay);
            pendingSpawnCount = Mathf.Max(0, pendingSpawnCount - 1);

            if (gameManager != null && !gameManager.IsPlaying) yield break;
            if (prefab == null) yield break;

            BaseBall newBall = Instantiate(prefab, position, Quaternion.identity);
            activeBalls.Add(newBall);
        }

        private BaseBall SelectWeightedBallPrefab()
        {
            if (ballPrefabs == null || ballPrefabs.Length == 0) return null;

            float totalWeight = 0f;
            float[] weights = new float[ballPrefabs.Length];

            for (int i = 0; i < ballPrefabs.Length; i++)
            {
                BaseBall prefab = ballPrefabs[i];
                if (prefab == null) continue;

                float weight = GetWeightForPrefab(prefab);
                weights[i] = weight;
                totalWeight += weight;
            }

            if (totalWeight <= 0f)
            {
                return ballPrefabs[0];
            }

            float randomRoll = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            for (int i = 0; i < ballPrefabs.Length; i++)
            {
                cumulative += weights[i];
                if (randomRoll <= cumulative)
                {
                    return ballPrefabs[i];
                }
            }

            return ballPrefabs[0];
        }

        private float GetWeightForPrefab(BaseBall prefab)
        {
            if (config == null) return 1f;

            if (prefab is FireBall) return config.FireBallWeight;
            if (prefab is StickyBall) return config.StickyBallWeight;
            if (prefab is NuclearBall) return config.NuclearBallWeight;
            if (prefab is GunBall) return config.GunBallWeight;

            return 1f;
        }

        private Vector3 CalculateSafeEdgePosition()
        {
            Vector3 playerPos = playerTransform != null ? playerTransform.position : Vector3.zero;
            Vector3 bestPos = Vector3.zero;
            float bestDistance = -1f;

            for (int attempt = 0; attempt < 8; attempt++)
            {
                Vector3 candidate = GetRandomPerimeterPoint();
                float dist = Vector3.Distance(new Vector3(candidate.x, 0f, candidate.z), new Vector3(playerPos.x, 0f, playerPos.z));

                if (dist >= minDistanceFromPlayer)
                {
                    return candidate;
                }

                if (dist > bestDistance)
                {
                    bestDistance = dist;
                    bestPos = candidate;
                }
            }

            return bestPos;
        }

        private Vector3 GetRandomPerimeterPoint()
        {
            int wallIndex = Random.Range(0, 4);
            float randomOffset = Random.Range(-perimeterOffset, perimeterOffset);

            return wallIndex switch
            {
                0 => new Vector3(randomOffset, spawnHeight, perimeterOffset),
                1 => new Vector3(randomOffset, spawnHeight, -perimeterOffset),
                2 => new Vector3(perimeterOffset, spawnHeight, randomOffset),
                _ => new Vector3(-perimeterOffset, spawnHeight, randomOffset)
            };
        }

        public void ClearAllSpawnedBalls()
        {
            StopAllCoroutines();
            pendingSpawnCount = 0;
            for (int i = 0; i < activeBalls.Count; i++)
            {
                if (activeBalls[i] != null)
                {
                    Destroy(activeBalls[i].gameObject);
                }
            }
            activeBalls.Clear();
        }

        private void OnDestroy()
        {
            ClearAllSpawnedBalls();
        }
    }
}
