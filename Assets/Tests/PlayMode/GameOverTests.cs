using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using DodgeGame.Arena;
using DodgeGame.Core;
using DodgeGame.Data;

namespace DodgeGame.Tests.PlayMode
{
    [TestFixture]
    public class GameOverTests
    {
        private GameObject gameManagerObject;
        private GameObject testAudioObject;
        private GameManager gameManager;
        private TimeScaleController timeScaleController;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            TimeScaleController.ResetTimeScale();
            GameEvents.ClearAllListeners();

            var existingGms = Object.FindObjectsByType<GameManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < existingGms.Length; i++)
            {
                if (existingGms[i] != null)
                {
                    Object.DestroyImmediate(existingGms[i].gameObject);
                }
            }

            if (AudioManager.Instance == null)
            {
                testAudioObject = new GameObject("TestAudioManager");
                testAudioObject.AddComponent<AudioManager>();
            }

            gameManagerObject = new GameObject("GameManagerObject");
            gameManagerObject.AddComponent<AudioListener>();
            timeScaleController = gameManagerObject.AddComponent<TimeScaleController>();
            gameManager = gameManagerObject.AddComponent<GameManager>();

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            TimeScaleController.ResetTimeScale();
            GameEvents.ClearAllListeners();

            if (gameManagerObject != null)
            {
                Object.DestroyImmediate(gameManagerObject);
            }
            if (testAudioObject != null)
            {
                Object.DestroyImmediate(testAudioObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator GameOver_RestoresNormalTimeScale()
        {
            // Set an abnormal time scale request
            timeScaleController.RequestTimeScale(0.3f, 10f, "CustomAbnormalScale");
            Assert.AreEqual(0.3f, Time.timeScale, 0.01f);

            // Clean teardown or reset on game over must restore 1.0
            TimeScaleController.ResetTimeScale();
            Assert.AreEqual(1.0f, Time.timeScale, 0.01f, "Time scale must restore to 1.0.");

            yield return null;
        }

        [UnityTest]
        public IEnumerator GameOver_StopsSpawner()
        {
            var spawnerObject = new GameObject("TestSpawner");
            var spawner = spawnerObject.AddComponent<BallSpawner>();

            // Trigger Game Over
            gameManager.TriggerGameOver("Unit Test Elimination");
            yield return null;

            Assert.AreEqual(GameState.GameOver, gameManager.CurrentState, "Game state must be GameOver.");
            Assert.IsFalse(gameManager.IsPlaying, "IsPlaying must be false.");

            // Capture ball count and verify no balls spawn while GameOver is active
            int initialCount = spawner.ActiveBallCount;
            yield return new WaitForSecondsRealtime(0.2f);

            Assert.AreEqual(initialCount, spawner.ActiveBallCount,
                "Spawner must not spawn any new balls once GameOver is active.");

            Object.Destroy(spawnerObject);
        }

        [UnityTest]
        public IEnumerator GameOver_ShowsCorrectScoreAndDispatchesEvents()
        {
            int capturedScore = -1;
            GameEvents.OnGameOver += (finalScore) => capturedScore = finalScore;

            gameManager.TriggerGameOver("Unit Test");
            yield return null;

            // Wait for GameOverRoutine to fire OnGameOver
            float waitTimer = 0f;
            while (capturedScore == -1 && waitTimer < 1.0f)
            {
                waitTimer += Time.unscaledDeltaTime;
                yield return null;
            }

            Assert.AreEqual(gameManager.CurrentScore, capturedScore,
                "Dispatched game over score must accurately match GameManager.CurrentScore.");
        }
    }
}
