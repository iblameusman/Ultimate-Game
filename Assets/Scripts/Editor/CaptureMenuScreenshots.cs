using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using DodgeGame.UI;

namespace DodgeGame.EditorTools
{
    public static class CaptureMenuScreenshots
    {
        public static void BuildAndCaptureAll()
        {
            DodgeGame.Editor.MainMenuBuilder.BuildMainMenuScene();

            var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);

            var camGo = GameObject.Find("Main Camera");
            if (camGo == null)
            {
                Debug.LogError("Main Camera not found!");
                return;
            }
            var cam = camGo.GetComponent<Camera>();

            var camCtrl = camGo.GetComponent<DodgeGame.Core.ArenaCameraController>();
            if (camCtrl != null)
            {
                camCtrl.InitializeFromDefaults();
            }

            int[,] resolutions = new int[,] {
                { 1280, 720 },
                { 1920, 1080 },
                { 2560, 1440 }
            };

            if (!Directory.Exists("Screenshots"))
            {
                Directory.CreateDirectory("Screenshots");
            }

            var menuUI = Object.FindAnyObjectByType<MainMenuUI>();
            if (menuUI != null && menuUI.SettingsModalController != null)
            {
                menuUI.SettingsModalController.CloseImmediate();
            }
            Canvas.ForceUpdateCanvases();

            for (int i = 0; i < resolutions.GetLength(0); i++)
            {
                int w = resolutions[i, 0];
                int h = resolutions[i, 1];
                Capture(cam, w, h, $"Screenshots/menu_{w}x{h}.png");
            }
            Capture(cam, 1280, 720, "Screenshots/after_mainmenu_1280x720.png");

            if (menuUI != null && menuUI.SettingsModalController != null)
            {
                menuUI.SettingsModalController.OpenImmediate();
                Canvas.ForceUpdateCanvases();

                for (int i = 0; i < resolutions.GetLength(0); i++)
                {
                    int w = resolutions[i, 0];
                    int h = resolutions[i, 1];
                    Capture(cam, w, h, $"Screenshots/settings_{w}x{h}.png");
                }

                Capture(cam, 1280, 720, "Screenshots/after_settings_1280x720.png");

                menuUI.SettingsModalController.CloseImmediate();
                Canvas.ForceUpdateCanvases();
            }

            if (menuUI != null && menuUI.DifficultyModal != null)
            {
                menuUI.DifficultyModal.gameObject.SetActive(true);
                var cg = menuUI.DifficultyModal.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 1f;
                var panelCg = menuUI.DifficultyModal.transform.Find("Panel")?.GetComponent<CanvasGroup>();
                if (panelCg != null) panelCg.alpha = 1f;
                var dimmerCg = menuUI.DifficultyModal.transform.Find("Dimmer")?.GetComponent<CanvasGroup>();
                if (dimmerCg != null) dimmerCg.alpha = 0.55f;
                Canvas.ForceUpdateCanvases();

                for (int i = 0; i < resolutions.GetLength(0); i++)
                {
                    int w = resolutions[i, 0];
                    int h = resolutions[i, 1];
                    Capture(cam, w, h, $"Screenshots/difficulty_{w}x{h}.png");
                }

                menuUI.DifficultyModal.gameObject.SetActive(false);
                Canvas.ForceUpdateCanvases();
            }

            Debug.Log("[Capture] All menu, settings, and difficulty screenshots captured successfully.");
        }

        private static void Capture(Camera cam, int w, int h, string path)
        {
            var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
            var prevTarget = cam.targetTexture;
            var prevActive = RenderTexture.active;

            cam.targetTexture = rt;
            RenderTexture.active = rt;

            cam.Render();

            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();

            cam.targetTexture = prevTarget;
            RenderTexture.active = prevActive;
            Object.DestroyImmediate(rt);

            byte[] bytes = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);

            File.WriteAllBytes(path, bytes);
            Debug.Log($"[Capture] Saved {path} ({bytes.Length} bytes)");
        }
    }
}
