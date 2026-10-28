using UnityEditor;
using UnityEngine;

namespace ProjectStarwatch.Editor.DebugTools
{
    /// <summary>
    /// Dockable / detachable editor window for Starwatch debug compile settings.
    /// </summary>
    public sealed class StarwatchDebugWindow : EditorWindow
    {
        private const string kWindowTitle = "Starwatch Debug";

        [MenuItem("Window/Starwatch/Debug Settings")]
        private static void Open()
        {
            var window = GetWindow<StarwatchDebugWindow>();
            window.titleContent = new GUIContent(kWindowTitle);
            window.minSize = new Vector2(280f, 120f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Compile mode", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Debug mode sets scripting define STARWATCH_DEBUG (use #if STARWATCH_DEBUG in code). " +
                "Changing mode forces a script recompile. " +
                "This is not Unity's built-in DEBUG symbol, which the Editor/Development builds already manage.",
                MessageType.Info);

            EditorGUI.BeginChangeCheck();
            var debugMode = EditorGUILayout.Toggle("Debug mode", StarwatchDebugDefines.IsDebugModeEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                StarwatchDebugDefines.IsDebugModeEnabled = debugMode;
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                StarwatchDebugDefines.IsDebugModeEnabled ? "Active: Debug (STARWATCH_DEBUG)" : "Active: Prod",
                EditorStyles.miniLabel);

            EditorGUILayout.Space(16f);
            EditorGUILayout.LabelField("More settings", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Additional debug toggles can land here later.", MessageType.None);
        }

        private void OnFocus()
        {
            Repaint();
        }
    }
}
