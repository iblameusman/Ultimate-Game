using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DodgeGame.Editor
{
    [InitializeOnLoad]
    public static class PlayModeSceneBootstrapper
    {
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string MainGameScenePath = "Assets/Scenes/MainGame.unity";

        static PlayModeSceneBootstrapper()
        {
            EditorSceneManager.playModeStartScene = null;
            EditorApplication.delayCall += EnsureActiveSceneLoaded;
        }

        [MenuItem("DodgeGame/Open MainMenu Scene", priority = 101)]
        public static void OpenMainMenu()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(MainMenuScenePath);
            }
        }

        [MenuItem("DodgeGame/Open MainGame Scene", priority = 102)]
        public static void OpenMainGame()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(MainGameScenePath);
            }
        }

        private static void EnsureActiveSceneLoaded()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(activeScene.path) && !EditorApplication.isPlaying)
            {
                if (System.IO.File.Exists(MainMenuScenePath))
                {
                    EditorSceneManager.OpenScene(MainMenuScenePath);
                }
            }
        }
    }
}
