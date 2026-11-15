using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace DodgeGame.Editor
{
    public static class AudioMixerSetup
    {
        [MenuItem("DodgeGame/Audio/Create Audio Mixer")]
        public static void CreateAudioMixer()
        {
            try
            {
                string dir = "Assets/Audio";
                if (!System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }

                string path = "Assets/Audio/MainAudioMixer.mixer";
                var mixerType = Type.GetType("UnityEditor.Audio.AudioMixerController, UnityEditor");
                if (mixerType == null)
                {
                    Debug.LogError("Could not find AudioMixerController type.");
                    return;
                }

                var createMethod = mixerType.GetMethod("CreateMixerControllerAtPath", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (createMethod == null)
                {
                    Debug.LogError("Could not find CreateMixerControllerAtPath method.");
                    return;
                }

                var mixer = createMethod.Invoke(null, new object[] { path });
                if (mixer == null)
                {
                    Debug.LogError("Failed to create mixer at path.");
                    return;
                }

                Debug.Log($"Created AudioMixerController at {path}!");

                var masterProp = mixerType.GetProperty("masterGroup", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var masterGroup = masterProp?.GetValue(mixer);

                var createNewGroupMethod = mixerType.GetMethod("CreateNewGroup", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(string), typeof(bool) }, null);
                var addChildToParentMethod = mixerType.GetMethod("AddChildToParent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var addGroupToViewMethod = mixerType.GetMethod("AddGroupToCurrentView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                string[] groups = { "Music", "SFX", "Comedy", "UI" };
                foreach (string g in groups)
                {
                    if (createNewGroupMethod != null && addChildToParentMethod != null && masterGroup != null)
                    {
                        var group = createNewGroupMethod.Invoke(mixer, new object[] { g, false });
                        addChildToParentMethod.Invoke(mixer, new object[] { group, masterGroup });
                        Debug.Log($"Added group {g} to master group.");
                    }
                }

                EditorUtility.SetDirty(mixer as UnityEngine.Object);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                if (!System.IO.Directory.Exists("Assets/Resources"))
                {
                    System.IO.Directory.CreateDirectory("Assets/Resources");
                }
                AssetDatabase.CopyAsset(path, "Assets/Resources/MainAudioMixer.mixer");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                var loadedMixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(path);
                if (loadedMixer != null)
                {
                    var allGroups = loadedMixer.FindMatchingGroups(string.Empty);
                    foreach (var grp in allGroups)
                    {
                        Debug.Log($"Successfully found group in mixer: {grp.name}");
                    }
                }
                {
                    var allGroups = loadedMixer.FindMatchingGroups(string.Empty);
                    foreach (var grp in allGroups)
                    {
                        Debug.Log($"Successfully found group in mixer: {grp.name}");
                    }
                }

                Debug.Log("Audio Mixer setup completed successfully!");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
