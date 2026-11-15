using UnityEngine;
using UnityEngine.SceneManagement;

namespace DodgeGame.Core
{
    public static class SceneBootstrapper
    {
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnAfterSceneLoad()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(activeScene.path) && string.IsNullOrEmpty(activeScene.name))
            {
                if (SceneManager.sceneCountInBuildSettings > 0)
                {
                    SceneManager.LoadScene(0);
                }
            }
        }
#endif
    }
}
