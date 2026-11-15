using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using DodgeGame.Data;

namespace DodgeGame.Editor
{
    public static class ComedyAudioImporter
    {
        [MenuItem("DodgeGame/Audio/Configure Audio Importers and Build Config")]
        public static void ConfigureAndBuildConfig()
        {
            ConfigureAudioImporters();
            BuildComedyAudioConfigAsset();
        }

        public static void ConfigureAudioImporters()
        {
            string[] audioFolders = { "Assets/Audio/Comedy", "Assets/Audio/UI" };

            foreach (var folder in audioFolders)
            {
                if (!Directory.Exists(folder)) continue;

                string[] files = Directory.GetFiles(folder, "*.wav", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    string assetPath = file.Replace("\\", "/");
                    var importer = AssetImporter.GetAtPath(assetPath) as AudioImporter;
                    if (importer == null) continue;

                    var settings = importer.defaultSampleSettings;
                    var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
                    float length = clip != null ? clip.length : 0.5f;

                    if (length < 1.0f)
                    {
                        settings.loadType = AudioClipLoadType.DecompressOnLoad;
                    }
                    else
                    {
                        settings.loadType = AudioClipLoadType.CompressedInMemory;
                    }

                    settings.sampleRateSetting = AudioSampleRateSetting.OverrideSampleRate;
                    settings.sampleRateOverride = 48000;

                    importer.defaultSampleSettings = settings;
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ComedyAudioImporter] Configured Audio Importers to 48kHz and correct LoadType.");
        }

        public static void BuildComedyAudioConfigAsset()
        {
            string comedyFolder = "Assets/Audio/Comedy";
            var defaultEntries = ComedyAudioConfig.CreateDefaultEntries();

            foreach (var entry in defaultEntries)
            {
                string id = entry.ClipId;
                var matchingClips = new List<AudioClip>();
                string[] files = Directory.GetFiles(comedyFolder, $"{id}_*.wav");
                Array.Sort(files);

                foreach (string f in files)
                {
                    string path = f.Replace("\\", "/");
                    var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                    if (clip != null)
                    {
                        matchingClips.Add(clip);
                    }
                }

                if (matchingClips.Count > 0)
                {
                    entry.Clips = matchingClips.ToArray();
                }
                else
                {
                    Debug.LogWarning($"[ComedyAudioImporter] No clips found for entry {id}!");
                }
            }

            string dataPath = "Assets/Scripts/Data/ComedyAudioConfig.asset";
            var config = AssetDatabase.LoadAssetAtPath<ComedyAudioConfig>(dataPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<ComedyAudioConfig>();
                AssetDatabase.CreateAsset(config, dataPath);
            }

            config.SetEntries(defaultEntries);
            EditorUtility.SetDirty(config);

            if (config.ValidateConfig(out string err))
            {
                Debug.Log("[ComedyAudioImporter] ComedyAudioConfig validated successfully! 22 unique entries.");
            }
            else
            {
                Debug.LogError($"[ComedyAudioImporter] ComedyAudioConfig validation failed: {err}");
            }

            if (!Directory.Exists("Assets/Resources")) Directory.CreateDirectory("Assets/Resources");
            if (!Directory.Exists("Assets/ScriptableObjects")) Directory.CreateDirectory("Assets/ScriptableObjects");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            AssetDatabase.CopyAsset(dataPath, "Assets/Resources/ComedyAudioConfig.asset");
            AssetDatabase.CopyAsset(dataPath, "Assets/ScriptableObjects/ComedyAudioConfig.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ComedyAudioImporter] ComedyAudioConfig saved to Scripts/Data, Resources, and ScriptableObjects.");
        }
    }
}
