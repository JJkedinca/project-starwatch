using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EndlessHeros.Core.Bootstrap
{
    /// <summary>
    /// Thin UI Toolkit bridge for the main menu. Raises intents and exposes
    /// optional development scenario/seed fields without owning run logic.
    /// </summary>
    public sealed class MainMenuViewBridge : MonoBehaviour
    {
        [SerializeField]
        private UIDocument _document;

        private TextField _scenarioField;
        private TextField _seedField;

        public event Action StartRunRequested;

        public event Action QuitRequested;

        public string ScenarioId =>
            _scenarioField != null && !string.IsNullOrWhiteSpace(_scenarioField.value)
                ? _scenarioField.value
                : Run.RunStartConfiguration.DefaultScenarioId;

        /// <summary>
        /// Queries the visual tree and hooks button callbacks to intent events.
        /// </summary>
        public void Bind()
        {
            if (_document == null)
            {
                _document = GetComponent<UIDocument>();
            }

            if (_document == null || _document.rootVisualElement == null)
            {
                Debug.LogError("MainMenuViewBridge requires a UIDocument with a visual tree.");
                return;
            }

            var root = _document.rootVisualElement;
            _scenarioField = root.Q<TextField>("scenario-field");
            _seedField = root.Q<TextField>("seed-field");

            var startButton = root.Q<Button>("start-run-button");
            if (startButton != null)
            {
                startButton.clicked += () => StartRunRequested?.Invoke();
            }
            else
            {
                Debug.LogError("MainMenuViewBridge could not find start-run-button.");
            }

            var quitButton = root.Q<Button>("quit-button");
            if (quitButton != null)
            {
                quitButton.clicked += () => QuitRequested?.Invoke();
            }
        }

        /// <summary>
        /// Parses an optional seed from the menu field when the player provided one.
        /// </summary>
        public bool TryGetSeed(out int seed)
        {
            seed = 0;
            if (_seedField == null || string.IsNullOrWhiteSpace(_seedField.value))
            {
                return false;
            }

            return int.TryParse(_seedField.value, out seed);
        }
    }
}
