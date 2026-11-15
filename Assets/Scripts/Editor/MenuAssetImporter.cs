using UnityEditor;
using UnityEngine;
using System.IO;
using TMPro;
using DodgeGame.Data;

namespace DodgeGame.Editor
{
    public static class MenuAssetImporter
    {
        [MenuItem("DodgeGame/Setup UI Sprites")]
        public static void SetupUISprites()
        {
            string uiDir = "Assets/Art/UI";
            if (!Directory.Exists(uiDir)) return;

            string[] files = Directory.GetFiles(uiDir, "*.png");
            foreach (string file in files)
            {
                string assetPath = file.Replace('\\', '/');
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null) continue;

                bool modified = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    modified = true;
                }

                if (!importer.alphaIsTransparency)
                {
                    importer.alphaIsTransparency = true;
                    modified = true;
                }

                if (importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = false;
                    modified = true;
                }

                if (assetPath.Contains("Panel_Header"))
                {
                    importer.spriteBorder = new Vector4(16, 16, 16, 16);
                    modified = true;
                }
                else if (assetPath.Contains("Button_"))
                {
                    importer.spriteBorder = new Vector4(16, 16, 24, 16);
                    modified = true;
                }
                else if (assetPath.Contains("HexPattern"))
                {
                    importer.wrapMode = TextureWrapMode.Repeat;
                    modified = true;
                }

                if (modified)
                {
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("[MenuAssetImporter] UI sprites successfully configured.");
        }

        [MenuItem("DodgeGame/Create MenuTheme Asset")]
        public static void CreateMenuThemeAsset()
        {
            string assetPath = "Assets/ScriptableObjects/MenuTheme.asset";
            var existing = AssetDatabase.LoadAssetAtPath<MenuTheme>(assetPath);
            if (existing == null)
            {
                var theme = ScriptableObject.CreateInstance<MenuTheme>();
                AssetDatabase.CreateAsset(theme, assetPath);
                AssetDatabase.SaveAssets();
                Debug.Log("[MenuAssetImporter] Created MenuTheme.asset at " + assetPath);
            }
        }

        [MenuItem("DodgeGame/Refresh Typography Assets")]
        public static void RefreshTypographyAssets()
        {
            var fontDisplay = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Art/Fonts/FontDisplay.asset");
            var fontUI = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Art/Fonts/FontUI.asset");
            var fontBody = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Art/Fonts/FontBody.asset");

            string[] paths = {
                "Assets/ScriptableObjects/SO_Typography.asset",
                "Assets/Resources/SO_Typography.asset",
                "Assets/ScriptableObjects/DefaultTypography.asset",
                "Assets/Resources/DefaultTypography.asset"
            };

            foreach (string p in paths)
            {
                var typo = AssetDatabase.LoadAssetAtPath<TypographyConfig>(p);
                if (typo != null)
                {
                    typo.InitializeDefaults(fontDisplay, fontUI, fontBody);
                    EditorUtility.SetDirty(typo);
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[MenuAssetImporter] Typography assets updated to Section 2 target sizes.");
        }

        [MenuItem("DodgeGame/Create Camera and Difficulty Configs")]
        public static void CreateCameraAndDifficultyConfigs()
        {
            string camPath = "Assets/ScriptableObjects/SO_CameraConfig.asset";
            var camConfig = AssetDatabase.LoadAssetAtPath<CameraConfig>(camPath);
            if (camConfig == null)
            {
                camConfig = ScriptableObject.CreateInstance<CameraConfig>();
                AssetDatabase.CreateAsset(camConfig, camPath);
            }
            if (!Directory.Exists("Assets/Resources")) Directory.CreateDirectory("Assets/Resources");
            AssetDatabase.CopyAsset(camPath, "Assets/Resources/SO_CameraConfig.asset");

            string diffPath = "Assets/ScriptableObjects/SO_DifficultyConfig.asset";
            var diffConfig = AssetDatabase.LoadAssetAtPath<DifficultyConfig>(diffPath);
            if (diffConfig == null)
            {
                diffConfig = ScriptableObject.CreateInstance<DifficultyConfig>();
                AssetDatabase.CreateAsset(diffConfig, diffPath);
            }
            AssetDatabase.CopyAsset(diffPath, "Assets/Resources/SO_DifficultyConfig.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[MenuAssetImporter] CameraConfig and DifficultyConfig created and synced to Resources.");
        }
    }
}
