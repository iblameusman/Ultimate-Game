using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using DodgeGame.Core;
using DodgeGame.Data;
using DodgeGame.Player;

namespace DodgeGame.Editor
{
    public static class CaptureGameScreenshots
    {
        [MenuItem("Tools/DodgeGame/Capture Camera Scenarios", priority = 210)]
        public static void CaptureCameraScenarios()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainGame.unity", OpenSceneMode.Single);

            var camGo = GameObject.Find("Main Camera") ?? GameObject.FindWithTag("MainCamera");
            if (camGo == null)
            {
                Debug.LogError("Main Camera not found in MainGame!");
                return;
            }
            var cam = camGo.GetComponent<Camera>();

            var playerGo = GameObject.FindWithTag("Player") ?? GameObject.Find("Player");
            var rig = Object.FindAnyObjectByType<ThirdPersonCameraRig>();
            if (rig == null)
            {
                var rigGo = new GameObject("ThirdPersonCameraRig");
                rig = rigGo.AddComponent<ThirdPersonCameraRig>();
            }

            var config = rig.Config ?? Resources.Load<CameraConfig>("SO_CameraConfig");
            if (config != null) rig.SetConfig(config);

            if (!Directory.Exists("Screenshots"))
            {
                Directory.CreateDirectory("Screenshots");
            }

            int[,] resolutions = new int[,] {
                { 1280, 720 },
                { 1920, 1080 },
                { 2560, 1080 }
            };

            if (playerGo != null)
            {
                playerGo.transform.position = new Vector3(8.5f, 0.5f, 0f);
                playerGo.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            }
            rig.SetTargetYaw(-90f);
            rig.SetTargetPitch(28f);
            Vector3 pivot = playerGo != null ? playerGo.transform.position + Vector3.up * 2.2f : new Vector3(8.5f, 2.7f, 0f);
            Vector3 desired = rig.ComputeDesiredPosition(pivot, -90f, 28f, 9f);
            Vector3 solved = rig.SolveCollisionAndBounds(pivot, desired, 0.016f);
            cam.transform.position = solved;
            cam.transform.rotation = Quaternion.LookRotation((pivot - solved).normalized, Vector3.up);

            for (int i = 0; i < resolutions.GetLength(0); i++)
            {
                int w = resolutions[i, 0];
                int h = resolutions[i, 1];
                Capture(cam, w, h, $"Screenshots/after_camera_wall_{w}x{h}.png");
            }

            if (playerGo != null)
            {
                playerGo.transform.position = new Vector3(0f, 2.8f, 0f);
                playerGo.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
            rig.SetTargetYaw(0f);
            rig.SetTargetPitch(22f);
            pivot = playerGo != null ? playerGo.transform.position + Vector3.up * 2.2f : new Vector3(0f, 5.0f, 0f);
            desired = rig.ComputeDesiredPosition(pivot, 0f, 22f, 9f);
            solved = rig.SolveCollisionAndBounds(pivot, desired, 0.016f);
            cam.transform.position = solved;
            cam.transform.rotation = Quaternion.LookRotation((pivot - solved).normalized, Vector3.up);

            for (int i = 0; i < resolutions.GetLength(0); i++)
            {
                int w = resolutions[i, 0];
                int h = resolutions[i, 1];
                Capture(cam, w, h, $"Screenshots/after_camera_jumping_{w}x{h}.png");
            }

            if (playerGo != null)
            {
                playerGo.transform.position = new Vector3(-2f, 0.5f, 0f);
                playerGo.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            }
            float originalFov = cam.fieldOfView;
            cam.fieldOfView = 66f;
            pivot = playerGo != null ? playerGo.transform.position + Vector3.up * 2.2f : new Vector3(-2f, 2.7f, 0f);
            desired = rig.ComputeDesiredPosition(pivot, 0f, 22f, 9f);
            solved = rig.SolveCollisionAndBounds(pivot, desired, 0.016f);
            cam.transform.position = solved;
            cam.transform.rotation = Quaternion.LookRotation((pivot - solved).normalized, Vector3.up);

            for (int i = 0; i < resolutions.GetLength(0); i++)
            {
                int w = resolutions[i, 0];
                int h = resolutions[i, 1];
                Capture(cam, w, h, $"Screenshots/after_camera_dashing_{w}x{h}.png");
            }
            cam.fieldOfView = originalFov;

            if (playerGo != null)
            {
                playerGo.transform.position = new Vector3(1f, 0.5f, 1f);
                playerGo.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
            }
            pivot = playerGo != null ? playerGo.transform.position + Vector3.up * 2.2f : new Vector3(1f, 2.7f, 1f);
            desired = rig.ComputeDesiredPosition(pivot, 30f, 22f, 9f);
            solved = rig.SolveCollisionAndBounds(pivot, desired, 0.016f);
            Vector3 shakeOffset = new Vector3(0.18f, -0.09f, 0.12f);
            cam.transform.position = solved + shakeOffset;
            cam.transform.rotation = Quaternion.LookRotation((pivot - solved).normalized, Vector3.up);

            for (int i = 0; i < resolutions.GetLength(0); i++)
            {
                int w = resolutions[i, 0];
                int h = resolutions[i, 1];
                Capture(cam, w, h, $"Screenshots/after_camera_hitstop_{w}x{h}.png");
            }

            Debug.Log("[CaptureGame] All camera scenario screenshots captured successfully!");
        }

        private static void Capture(Camera cam, int w, int h, string path)
        {
            var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
            var prevTarget = cam.targetTexture;
            var prevActive = RenderTexture.active;

            var canvas = Object.FindAnyObjectByType<Canvas>();
            RenderMode prevMode = RenderMode.ScreenSpaceOverlay;
            Camera prevWorldCam = null;
            float prevPlaneDist = 100f;

            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                prevMode = canvas.renderMode;
                prevWorldCam = canvas.worldCamera;
                prevPlaneDist = canvas.planeDistance;

                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = cam;
                canvas.planeDistance = 1f;
            }

            try
            {
                cam.targetTexture = rt;
                RenderTexture.active = rt;

                cam.Render();

                var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                tex.Apply();

                byte[] bytes = tex.EncodeToPNG();
                Object.DestroyImmediate(tex);

                File.WriteAllBytes(path, bytes);
                Debug.Log($"[CaptureGame] Saved {path} ({bytes.Length} bytes)");
            }
            finally
            {
                cam.targetTexture = prevTarget;
                RenderTexture.active = prevActive;
                Object.DestroyImmediate(rt);

                if (canvas != null && prevMode == RenderMode.ScreenSpaceOverlay)
                {
                    canvas.renderMode = prevMode;
                    canvas.worldCamera = prevWorldCam;
                    canvas.planeDistance = prevPlaneDist;
                }
            }
        }
    }
}
