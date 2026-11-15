using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace DodgeGame.Editor
{
    /// <summary>
    /// One-click Web build with settings that work on any static host
    /// (itch.io, GitHub Pages, Netlify, a plain web server).
    /// Command line:
    ///   Unity.exe -batchmode -quit -buildTarget WebGL -projectPath . -executeMethod DodgeGame.Editor.WebGLBuilder.BuildFromCommandLine
    /// </summary>
    public static class WebGLBuilder
    {
        private const string OutputPath = "Builds/WebGL";

        // Editor-only test scenes that should never ship.
        private static readonly string[] ExcludedScenes = { "Assets/Scenes/FontCheck.unity" };

        [MenuItem("DodgeGame/Build/Web (WebGL)", priority = 200)]
        public static void BuildFromMenu()
        {
            var report = Build();
            if (report != null && report.summary.result == BuildResult.Succeeded)
            {
                EditorUtility.RevealInFinder(Path.Combine(OutputPath, "index.html"));
            }
        }

        public static void BuildFromCommandLine()
        {
            var report = Build();
            EditorApplication.Exit(report != null && report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        private static BuildReport Build()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
            {
                Debug.LogError("[WebGLBuilder] Web Build Support module is not installed for this editor. Add it from Unity Hub > Installs.");
                return null;
            }

            ApplyWebSettings();

            string[] scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled && !ExcludedScenes.Contains(s.path))
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                Debug.LogError("[WebGLBuilder] No enabled scenes in Build Settings.");
                return null;
            }

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputPath,
                target = BuildTarget.WebGL,
                targetGroup = BuildTargetGroup.WebGL,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[WebGLBuilder] Build succeeded: {summary.outputPath} ({summary.totalSize / (1024f * 1024f):0.0} MB, {summary.totalTime.TotalSeconds:0}s)");
            }
            else
            {
                Debug.LogError($"[WebGLBuilder] Build {summary.result} with {summary.totalErrors} error(s). See the Console / log for details.");
            }
            return report;
        }

        private static void ApplyWebSettings()
        {
            // Brotli is smallest; the fallback lets the loader decompress in JS when the
            // host doesn't send Content-Encoding headers, so the build runs anywhere.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
            PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
            PlayerSettings.WebGL.memoryGrowthMode = WebGLMemoryGrowthMode.Geometric;
            PlayerSettings.WebGL.maximumMemorySize = 2048;
            PlayerSettings.runInBackground = true;
            AssetDatabase.SaveAssets();
        }
    }
}
