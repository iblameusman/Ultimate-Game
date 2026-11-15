using UnityEditor;
using UnityEditor.SceneManagement;

namespace DodgeGame.Editor
{
    [InitializeOnLoad]
    public static class PlayModeStartScene
    {
        private const string MainMenuPath = "Assets/Scenes/MainMenu.unity";
        private const string MenuPath = "Tools/Dodge Game/Start Play Mode From Main Menu";
        private const string PrefKey = "DodgeGame_PlayFromMainMenu";

        static PlayModeStartScene()
        {
            EditorApplication.delayCall += Apply;
        }

        private static bool Enabled
        {
            get => EditorPrefs.GetBool(PrefKey, true);
            set => EditorPrefs.SetBool(PrefKey, value);
        }

        private static void Apply()
        {
            EditorSceneManager.playModeStartScene = Enabled
                ? AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuPath)
                : null;
        }

        [MenuItem(MenuPath)]
        private static void Toggle()
        {
            Enabled = !Enabled;
            Apply();
        }

        [MenuItem(MenuPath, true)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, Enabled);
            return true;
        }
    }
}
