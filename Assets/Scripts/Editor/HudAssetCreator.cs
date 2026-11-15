using System.IO;
using UnityEditor;
using UnityEngine;
using DodgeGame.Data;

namespace DodgeGame.Editor
{
    public static class HudAssetCreator
    {
        [MenuItem("Tools/DodgeGame/Create HUD Assets", priority = 100)]
        public static void CreateAllHudAssets()
        {
            CreateHudThemeAssets();
            GeneratePanelSprites();
            GenerateHeartSprites();
            GeneratePauseIcon();
            GeneratePillSprite();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[HudAssetCreator] All HUD assets successfully created.");
        }

        public static void CreateHudThemeAssets()
        {
            string dir = "Assets/Scripts/Data";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string path = $"{dir}/HudTheme.asset";
            var theme = AssetDatabase.LoadAssetAtPath<HudTheme>(path);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<HudTheme>();
                theme.ResetToDefaults();
                AssetDatabase.CreateAsset(theme, path);
            }
            else
            {
                theme.ResetToDefaults();
                EditorUtility.SetDirty(theme);
            }

            string resDir = "Assets/Resources";
            if (!Directory.Exists(resDir)) Directory.CreateDirectory(resDir);
            string resPath = $"{resDir}/HudTheme.asset";
            var resTheme = AssetDatabase.LoadAssetAtPath<HudTheme>(resPath);
            if (resTheme == null)
            {
                resTheme = ScriptableObject.CreateInstance<HudTheme>();
                resTheme.ResetToDefaults();
                AssetDatabase.CreateAsset(resTheme, resPath);
            }
            else
            {
                resTheme.ResetToDefaults();
                EditorUtility.SetDirty(resTheme);
            }

            string soDir = "Assets/ScriptableObjects";
            if (Directory.Exists(soDir))
            {
                string soPath = $"{soDir}/HudTheme.asset";
                var soTheme = AssetDatabase.LoadAssetAtPath<HudTheme>(soPath);
                if (soTheme == null)
                {
                    soTheme = ScriptableObject.CreateInstance<HudTheme>();
                    soTheme.ResetToDefaults();
                    AssetDatabase.CreateAsset(soTheme, soPath);
                }
            }

            Debug.Log($"[HudAssetCreator] Created HudTheme at {path} and {resPath}");
        }

        private static void GeneratePanelSprites()
        {
            string dir = "Assets/Art/UI";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            int size = 64;
            float radius = 8f;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color fill = new Color(0.9137f, 0.9294f, 0.9412f, 0.85f);
            Color border = new Color(0.7216f, 0.7608f, 0.7922f, 1f);
            Color bevel = Color.white;
            Color clear = new Color(0, 0, 0, 0);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = SignedDistanceRoundedRect(x + 0.5f, y + 0.5f, size, size, radius);
                    if (d > 0.5f)
                    {
                        tex.SetPixel(x, y, clear);
                    }
                    else if (d > -0.5f)
                    {
                        float a = Mathf.Clamp01(0.5f - d);
                        tex.SetPixel(x, y, new Color(border.r, border.g, border.b, a));
                    }
                    else if (d > -1.5f)
                    {
                        tex.SetPixel(x, y, border);
                    }
                    else if (d > -3.5f)
                    {
                        tex.SetPixel(x, y, bevel);
                    }
                    else
                    {
                        tex.SetPixel(x, y, fill);
                    }
                }
            }

            tex.Apply();
            string pngPath = $"{dir}/Panel_Hud.png";
            File.WriteAllBytes(pngPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate);
            ConfigureSprite(pngPath, new Vector4(16, 16, 16, 16));
        }

        private static void GenerateHeartSprites()
        {
            string dir = "Assets/Art/UI";
            int size = 96;

            var fullTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color danger = new Color(0.8980f, 0.2824f, 0.3020f, 1f);
            Color white = Color.white;
            Color clear = new Color(0, 0, 0, 0);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x + 0.5f - size * 0.5f) / (size * 0.42f);
                    float ny = (y + 0.5f - size * 0.45f) / (size * 0.42f);

                    float d = HeartDistance(nx, ny);
                    if (d > 0.05f)
                    {
                        fullTex.SetPixel(x, y, clear);
                    }
                    else if (d > -0.08f)
                    {
                        float alpha = Mathf.Clamp01((0.05f - d) / 0.04f);
                        fullTex.SetPixel(x, y, new Color(white.r, white.g, white.b, alpha));
                    }
                    else
                    {
                        fullTex.SetPixel(x, y, danger);
                    }
                }
            }
            fullTex.Apply();
            string fullPath = $"{dir}/Icon_Heart_Full.png";
            File.WriteAllBytes(fullPath, fullTex.EncodeToPNG());
            Object.DestroyImmediate(fullTex);

            AssetDatabase.ImportAsset(fullPath, ImportAssetOptions.ForceUpdate);
            ConfigureSprite(fullPath, Vector4.zero);

            var emptyTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color grey = new Color(0.6039f, 0.6510f, 0.6863f, 1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x + 0.5f - size * 0.5f) / (size * 0.42f);
                    float ny = (y + 0.5f - size * 0.45f) / (size * 0.42f);

                    float d = HeartDistance(nx, ny);
                    if (d > 0.05f || d < -0.12f)
                    {
                        emptyTex.SetPixel(x, y, clear);
                    }
                    else
                    {
                        float alpha = 1f;
                        if (d > 0.01f) alpha = Mathf.Clamp01((0.05f - d) / 0.04f);
                        else if (d < -0.09f) alpha = Mathf.Clamp01((d - (-0.12f)) / 0.03f);
                        emptyTex.SetPixel(x, y, new Color(grey.r, grey.g, grey.b, alpha));
                    }
                }
            }
            emptyTex.Apply();
            string emptyPath = $"{dir}/Icon_Heart_Empty.png";
            File.WriteAllBytes(emptyPath, emptyTex.EncodeToPNG());
            Object.DestroyImmediate(emptyTex);

            AssetDatabase.ImportAsset(emptyPath, ImportAssetOptions.ForceUpdate);
            ConfigureSprite(emptyPath, Vector4.zero);
        }

        private static void GeneratePauseIcon()
        {
            string dir = "Assets/Art/UI";
            int size = 96;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color navy = new Color(0.1176f, 0.1647f, 0.2000f, 1f);
            Color clear = new Color(0, 0, 0, 0);

            int barWidth = 14;
            int barHeight = 44;
            int gap = 16;
            int startX1 = (size - (barWidth * 2 + gap)) / 2;
            int startX2 = startX1 + barWidth + gap;
            int startY = (size - barHeight) / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool inBar1 = (x >= startX1 && x < startX1 + barWidth && y >= startY && y < startY + barHeight);
                    bool inBar2 = (x >= startX2 && x < startX2 + barWidth && y >= startY && y < startY + barHeight);
                    if (inBar1 || inBar2)
                    {
                        tex.SetPixel(x, y, navy);
                    }
                    else
                    {
                        tex.SetPixel(x, y, clear);
                    }
                }
            }
            tex.Apply();
            string path = $"{dir}/Icon_Pause.png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            ConfigureSprite(path, Vector4.zero);
        }

        private static void GeneratePillSprite()
        {
            string dir = "Assets/Art/UI";
            int size = 32;
            float radius = 6f;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color pillGrey = new Color(0.85f, 0.88f, 0.90f, 1f);
            Color clear = new Color(0, 0, 0, 0);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = SignedDistanceRoundedRect(x + 0.5f, y + 0.5f, size, size, radius);
                    if (d > 0.5f)
                    {
                        tex.SetPixel(x, y, clear);
                    }
                    else if (d > -0.5f)
                    {
                        float a = Mathf.Clamp01(0.5f - d);
                        tex.SetPixel(x, y, new Color(pillGrey.r, pillGrey.g, pillGrey.b, a));
                    }
                    else
                    {
                        tex.SetPixel(x, y, pillGrey);
                    }
                }
            }
            tex.Apply();
            string path = $"{dir}/Pill_Light.png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            ConfigureSprite(path, new Vector4(8, 8, 8, 8));
        }

        private static float SignedDistanceRoundedRect(float x, float y, float w, float h, float r)
        {
            float halfW = w * 0.5f - r;
            float halfH = h * 0.5f - r;
            float cx = Mathf.Abs(x - w * 0.5f) - halfW;
            float cy = Mathf.Abs(y - h * 0.5f) - halfH;

            float outsideDist = Mathf.Sqrt(Mathf.Max(cx, 0f) * Mathf.Max(cx, 0f) + Mathf.Max(cy, 0f) * Mathf.Max(cy, 0f)) - r;
            float insideDist = Mathf.Min(Mathf.Max(cx, cy), 0f);
            return outsideDist + insideDist;
        }

        private static float HeartDistance(float x, float y)
        {
            float yShifted = y - 0.2f;
            float a = x * x + yShifted * yShifted - 0.7f;
            return a * a * a - x * x * yShifted * yShifted * yShifted;
        }

        private static void ConfigureSprite(string path, Vector4 border)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.spriteBorder = border;
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }
        }
    }
}
