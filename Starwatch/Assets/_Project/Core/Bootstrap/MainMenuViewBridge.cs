using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Generation;
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

        public GalaxySizeId GalaxySizeId =>
            SelectedEnum(_sizeDropdown, _sizeIds, GalaxyCatalogSO.DefaultSizeId);

        public GalaxyShapeId GalaxyShapeId =>
            SelectedEnum(_shapeDropdown, _shapeIds, GalaxyCatalogSO.DefaultShapeId);

        [SerializeField]
        private UIDocument _document;

        private VisualElement _titleScreen;
        private VisualElement _newRunPanel;
        private DropdownField _scenarioDropdown;
        private DropdownField _sizeDropdown;
        private DropdownField _shapeDropdown;
        private TextField _seedField;
        private Button _launchButton;
        private readonly List<string> _scenarioIds = new();
        private readonly List<GalaxySizeId> _sizeIds = new();
        private readonly List<GalaxyShapeId> _shapeIds = new();

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
            _sizeDropdown = root.Q<DropdownField>("galaxy-size-dropdown");
            _shapeDropdown = root.Q<DropdownField>("galaxy-shape-dropdown");
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

            if (_scenarioDropdown != null)
            {
                _scenarioDropdown.RegisterValueChangedCallback(_ => RefreshGenerationFieldVisibility());
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

            RefreshGenerationFieldVisibility();
        }

        /// <summary>
        /// Fills size and shape dropdowns from the galaxy catalog.
        /// </summary>
        public void SetGalaxyOptions(
            IReadOnlyList<GalaxySizeOption> sizes,
            IReadOnlyList<GalaxyShapeOption> shapes)
        {
            FillSizeDropdown(sizes);
            FillShapeDropdown(shapes);
            RefreshGenerationFieldVisibility();
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

        private void FillSizeDropdown(IReadOnlyList<GalaxySizeOption> sizes)
        {
            _sizeIds.Clear();
            List<string> choices = new();
            if (sizes != null)
            {
                for (var i = 0; i < sizes.Count; i++)
                {
                    var option = sizes[i];
                    if (option == null || !option.IsValid)
                    {
                        continue;
                    }

                    _sizeIds.Add(option.Id);
                    choices.Add(option.DisplayName);
                }
            }

            ApplyDropdown(_sizeDropdown, choices, IndexOf(_sizeIds, GalaxyCatalogSO.DefaultSizeId));
        }

        private void FillShapeDropdown(IReadOnlyList<GalaxyShapeOption> shapes)
        {
            _shapeIds.Clear();
            List<string> choices = new();
            if (shapes != null)
            {
                for (var i = 0; i < shapes.Count; i++)
                {
                    var option = shapes[i];
                    if (option == null || !option.IsValid)
                    {
                        continue;
                    }

                    _shapeIds.Add(option.Id);
                    choices.Add(option.DisplayName);
                }
            }

            ApplyDropdown(_shapeDropdown, choices, IndexOf(_shapeIds, GalaxyCatalogSO.DefaultShapeId));
        }

        private void RefreshGenerationFieldVisibility()
        {
            var showGeneration = ScenarioId == GalaxyGenerator.ScenarioId;
            SetDropdownVisible(_sizeDropdown, showGeneration && _sizeIds.Count > 0);
            SetDropdownVisible(_shapeDropdown, showGeneration && _shapeIds.Count > 0);
        }

        private static void SetDropdownVisible(DropdownField dropdown, bool visible)
        {
            if (dropdown == null)
            {
                return;
            }

            dropdown.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private static void ApplyDropdown(DropdownField dropdown, List<string> choices, int defaultIndex)
        {
            if (dropdown == null)
            {
                return;
            }

            dropdown.choices = choices;
            if (choices.Count == 0)
            {
                return;
            }

            dropdown.index = Mathf.Clamp(defaultIndex, 0, choices.Count - 1);
        }

        private static int IndexOf<T>(List<T> ids, T defaultId)
            where T : struct
        {
            for (var i = 0; i < ids.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(ids[i], defaultId))
                {
                    return i;
                }
            }

            return 0;
        }

        private static T SelectedEnum<T>(DropdownField dropdown, List<T> ids, T fallback)
            where T : struct
        {
            if (dropdown == null || ids.Count == 0)
            {
                return fallback;
            }

            var index = dropdown.index;
            if (index < 0 || index >= ids.Count)
            {
                return ids[0];
            }

            return ids[index];
        }

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
