using UnityEditor;
using UnityEngine;
using TMPro;
using DodgeGame.Data;
using DodgeGame.UI;

namespace DodgeGame.Editor
{
    public class FontCheckWindow : EditorWindow
    {
        private TypographyConfig config;
        private Vector2 scrollPos;

        [MenuItem("Tools/DodgeGame/Font Check Panel")]
        public static void ShowWindow()
        {
            var win = GetWindow<FontCheckWindow>("Font Check");
            win.minSize = new Vector2(650, 600);
            win.Show();
        }

        private void OnEnable()
        {
            LoadConfig();
        }

        private void LoadConfig()
        {
            if (config == null)
            {
                config = AssetDatabase.LoadAssetAtPath<TypographyConfig>("Assets/ScriptableObjects/DefaultTypography.asset");
                if (config == null)
                {
                    config = Resources.Load<TypographyConfig>("DefaultTypography");
                }
            }
        }

        private void OnGUI()
        {
            LoadConfig();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Typography System Visual Inspector", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Exo 2 Typography Hierarchy (All 6 Roles)", EditorStyles.miniLabel);
            EditorGUILayout.Space(5);

            config = (TypographyConfig)EditorGUILayout.ObjectField("Typography Config", config, typeof(TypographyConfig), false);

            if (config == null)
            {
                EditorGUILayout.HelpBox("DefaultTypography asset not found at Assets/ScriptableObjects/DefaultTypography.asset", MessageType.Warning);
                return;
            }

            EditorGUILayout.Space(10);
            if (GUILayout.Button("Reapply Styles to All Text in Active Scene", GUILayout.Height(30)))
            {
                var stylers = FindObjectsByType<TextStyler>(FindObjectsInactive.Include);
                Undo.RecordObjects(stylers, "Reapply Text Styles");
                foreach (var s in stylers)
                {
                    s.ApplyStyle();
                    EditorUtility.SetDirty(s);
                }
                Debug.Log($"[FontCheck] Reapplied typography styles to {stylers.Length} text elements in scene.");
            }

            EditorGUILayout.Space(10);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            DrawRolePreview(TypographyRole.Display, config.Display, "Display (96pt)", "ULTIMATE DODGE 2026");
            DrawRolePreview(TypographyRole.Heading, config.Heading, "Heading (56pt)", "SURVIVE THE HAZARD ARENA");
            DrawRolePreview(TypographyRole.Button, config.Button, "Button (44pt)", "START GAME [SPACE]");
            DrawRolePreview(TypographyRole.Label, config.Label, "Label (28pt)", "LIVES: 3 | SHIFT: READY [Q] | BEST: 12,450");
            DrawRolePreview(TypographyRole.Body, config.Body, "Body (30pt)", "Phase through hazards and dodge ballistic attacks using your three active abilities.");
            DrawRolePreview(TypographyRole.Caption, config.Caption, "Caption (22pt)", "v1.0.0 - Exo 2 Unified Typography System | Clean Dark Theme");

            EditorGUILayout.EndScrollView();
        }

        private void DrawRolePreview(TypographyRole role, TextStyle style, string label, string sampleText)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Role: {role.ToString().ToUpper()}", EditorStyles.boldLabel, GUILayout.Width(140));
            EditorGUILayout.LabelField($"Font: {(style.Font != null ? style.Font.name : "None")} | Size: {style.Size} | Outline: {style.OutlineWidth}", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            Rect rect = GUILayoutUtility.GetRect(new GUIContent(sampleText), EditorStyles.label, GUILayout.MinHeight(Mathf.Max(35f, style.Size * 0.7f)));
            EditorGUI.DrawRect(rect, new Color(14f / 255f, 20f / 255f, 23f / 255f, 1f));

            var guiStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = Mathf.Clamp(Mathf.RoundToInt(style.Size * 0.55f), 12, 48),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = style.Color }
            };

            EditorGUI.LabelField(rect, sampleText, guiStyle);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(6);
        }
    }
}
