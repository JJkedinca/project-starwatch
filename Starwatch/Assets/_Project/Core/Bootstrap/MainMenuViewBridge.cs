using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Core.Bootstrap
{
    /// <summary>
    /// Thin UI Toolkit bridge for the main menu title screen and New Run panel.
    /// </summary>
    public sealed class MainMenuViewBridge : MonoBehaviour
    {
        public event Action StartRunRequested;

        public event Action QuitRequested;

        public string ScenarioId
        {
            get
            {
                if (_scenarioDropdown == null || _scenarioIds.Count == 0)
                {
                    return Run.RunStartConfiguration.DefaultScenarioId;
                }

                var index = _scenarioDropdown.index;
                if (index < 0 || index >= _scenarioIds.Count)
                {
                    return _scenarioIds[0];
                }

                return _scenarioIds[index];
            }
        }

        [SerializeField]
        private UIDocument _document;

        private VisualElement _titleScreen;
        private VisualElement _newRunPanel;
        private DropdownField _scenarioDropdown;
        private TextField _seedField;
        private Button _launchButton;
        private readonly List<string> _scenarioIds = new();

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
            _titleScreen = root.Q<VisualElement>("title-screen");
            _newRunPanel = root.Q<VisualElement>("new-run-panel");
            _scenarioDropdown = root.Q<DropdownField>("scenario-dropdown");
            _seedField = root.Q<TextField>("seed-field");

            var newRun = root.Q<Button>("new-run-button");
            if (newRun != null)
            {
                newRun.clicked += ShowNewRun;
            }

            var back = root.Q<Button>("back-button");
            if (back != null)
            {
                back.clicked += ShowTitle;
            }

            var dice = root.Q<Button>("dice-button");
            if (dice != null)
            {
                dice.clicked += RollSeed;
            }

            _launchButton = root.Q<Button>("launch-button");
            if (_launchButton != null)
            {
                _launchButton.clicked += () => StartRunRequested?.Invoke();
            }
            else
            {
                Debug.LogError("MainMenuViewBridge could not find launch-button.");
            }

            var quitButton = root.Q<Button>("quit-button");
            if (quitButton != null)
            {
                quitButton.clicked += () => QuitRequested?.Invoke();
            }

            ShowTitle();
        }

        /// <summary>
        /// Fills the scenario dropdown from registered scenario ids.
        /// </summary>
        public void SetScenarios(IReadOnlyList<string> scenarioIds)
        {
            _scenarioIds.Clear();
            List<string> choices = new();
            if (scenarioIds != null)
            {
                for (var i = 0; i < scenarioIds.Count; i++)
                {
                    var id = scenarioIds[i];
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        continue;
                    }

                    _scenarioIds.Add(id);
                    choices.Add(id);
                }
            }

            if (_scenarioDropdown == null)
            {
                return;
            }

            _scenarioDropdown.choices = choices;
            if (choices.Count > 0)
            {
                var defaultIndex = 0;
                for (var i = 0; i < _scenarioIds.Count; i++)
                {
                    if (_scenarioIds[i] == Run.RunStartConfiguration.DefaultScenarioId)
                    {
                        defaultIndex = i;
                        break;
                    }
                }

                _scenarioDropdown.index = defaultIndex;
            }
        }

        /// <summary>
        /// Enables or disables the Launch button during transitions.
        /// </summary>
        public void SetStartRunInteractable(bool interactable)
        {
            if (_launchButton != null)
            {
                _launchButton.SetEnabled(interactable);
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

        /// <summary>
        /// Fills the seed field with a random value the player can note before launch.
        /// </summary>
        public void RollSeed()
        {
            if (_seedField == null)
            {
                return;
            }

            _seedField.value = UnityEngine.Random.Range(1, int.MaxValue).ToString();
        }

        /// <summary>
        /// Exposes the current seed field text for tests.
        /// </summary>
        public string SeedFieldText => _seedField != null ? _seedField.value : string.Empty;

        private void ShowNewRun()
        {
            if (_titleScreen != null)
            {
                _titleScreen.style.display = DisplayStyle.None;
            }

            if (_newRunPanel != null)
            {
                _newRunPanel.style.display = DisplayStyle.Flex;
            }
        }

        private void ShowTitle()
        {
            if (_titleScreen != null)
            {
                _titleScreen.style.display = DisplayStyle.Flex;
            }

            if (_newRunPanel != null)
            {
                _newRunPanel.style.display = DisplayStyle.None;
            }
        }
    }
}
