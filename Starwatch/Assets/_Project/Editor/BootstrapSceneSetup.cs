using System.IO;
using EndlessHeros.Core.Bootstrap;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace EndlessHeros.Editor
{
    /// <summary>
    /// Editor utility that creates Bootstrap/MainMenu/Overworld/Battle scenes
    /// and registers them in Build Settings.
    /// </summary>
    public static class BootstrapSceneSetup
    {
        private const string kScenesRoot = "Assets/_Project/Scenes";
        private const string kMainMenuUxml = "Assets/_Project/Runtime/UI/MainMenu/MainMenu.uxml";

        [MenuItem("Starwatch/Setup Bootstrap Scenes")]
        public static void SetupFromMenu()
        {
            Setup();
        }

        /// <summary>
        /// Creates the bootstrap scene set and updates Editor Build Settings.
        /// </summary>
        public static void Setup()
        {
            Directory.CreateDirectory(
                Path.Combine(Application.dataPath, "_Project", "Scenes"));

            CreateBootstrapScene();
            CreateMainMenuScene();
            CreateOverworldScene();
            CreateBattleScene();
            RegisterBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Starwatch bootstrap scenes are ready. Enter Play Mode from Bootstrap.");
        }

        /// <summary>
        /// Creates Bootstrap.unity with a BootstrapLoader entry object.
        /// </summary>
        private static void CreateBootstrapScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var bootstrap = new GameObject("Bootstrap");
            bootstrap.AddComponent<BootstrapLoader>();
            EditorSceneManager.SaveScene(scene, $"{kScenesRoot}/Bootstrap.unity");
        }

        /// <summary>
        /// Creates MainMenu.unity with scope, UIDocument, and view bridge wiring.
        /// </summary>
        private static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var scopeGo = new GameObject("MainMenuLifetimeScope");
            scopeGo.AddComponent<MainMenuLifetimeScope>();

            var uiGo = new GameObject("MainMenuUI");
            var document = uiGo.AddComponent<UIDocument>();
            var bridge = uiGo.AddComponent<MainMenuViewBridge>();

            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(kMainMenuUxml);
            if (uxml != null)
            {
                document.visualTreeAsset = uxml;
            }
            else
            {
                Debug.LogWarning($"Missing MainMenu UXML at {kMainMenuUxml}");
            }

            var panelSettings = FindOrCreatePanelSettings();
            document.panelSettings = panelSettings;

            var serializedBridge = new SerializedObject(bridge);
            serializedBridge.FindProperty("_document").objectReferenceValue = document;
            serializedBridge.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, $"{kScenesRoot}/MainMenu.unity");
        }

        /// <summary>
        /// Creates Overworld.unity with an empty scene scope and placeholder camera.
        /// </summary>
        private static void CreateOverworldScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var scopeGo = new GameObject("OverworldLifetimeScope");
            scopeGo.AddComponent<OverworldLifetimeScope>();

            var cameraGo = new GameObject("OverworldCamera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.05f, 0.07f, 0.1f);
            camera.transform.position = new Vector3(0f, 20f, 0f);
            camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            EditorSceneManager.SaveScene(scene, $"{kScenesRoot}/Overworld.unity");
        }

        /// <summary>
        /// Creates Battle.unity as an additive presentation scene shell.
        /// </summary>
        private static void CreateBattleScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var scopeGo = new GameObject("BattleLifetimeScope");
            scopeGo.AddComponent<BattleLifetimeScope>();
            EditorSceneManager.SaveScene(scene, $"{kScenesRoot}/Battle.unity");
        }

        /// <summary>
        /// Sets Bootstrap as build index 0 and registers the remaining gameplay scenes.
        /// </summary>
        private static void RegisterBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene($"{kScenesRoot}/Bootstrap.unity", true),
                new EditorBuildSettingsScene($"{kScenesRoot}/MainMenu.unity", true),
                new EditorBuildSettingsScene($"{kScenesRoot}/Overworld.unity", true),
                new EditorBuildSettingsScene($"{kScenesRoot}/Battle.unity", true),
            };
        }

        /// <summary>
        /// Loads or creates PanelSettings required by the main-menu UIDocument.
        /// </summary>
        private static PanelSettings FindOrCreatePanelSettings()
        {
            const string path = "Assets/_Project/Runtime/UI/MainMenu/MainMenuPanelSettings.asset";
            var existing = AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            if (existing != null)
            {
                return existing;
            }

            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            AssetDatabase.CreateAsset(settings, path);
            return settings;
        }

        [MenuItem("Starwatch/Setup Bootstrap Scenes", true)]
        private static bool ValidateSetupFromMenu()
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode;
        }
    }
}
