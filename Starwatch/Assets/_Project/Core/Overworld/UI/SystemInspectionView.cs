using System;
using UnityEngine.UIElements;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// Side panel for the selected map node: summary and explore action.
    /// </summary>
    internal sealed class SystemInspectionView
    {
        public event Action ExploreClicked;

        private Label _title;
        private Label _summary;
        private Button _exploreButton;

        /// <summary>
        /// Binds to inspection controls from Overworld UXML.
        /// </summary>
        public void Bind(VisualElement inspectionPanel)
        {
            if (inspectionPanel == null)
            {
                return;
            }

            _title = inspectionPanel.Q<Label>("inspection-title");
            _summary = inspectionPanel.Q<Label>("inspection-summary");
            _exploreButton = inspectionPanel.Q<Button>("explore-button");
            if (_exploreButton != null)
            {
                _exploreButton.clicked += () => ExploreClicked?.Invoke();
            }
        }

        /// <summary>
        /// Shows empty selection state.
        /// </summary>
        public void ShowEmpty()
        {
            if (_title != null)
            {
                _title.text = "Select a node";
            }

            if (_summary != null)
            {
                _summary.text = string.Empty;
            }

            SetExploreEnabled(false);
        }

        /// <summary>
        /// Updates labels for the selected node.
        /// </summary>
        public void ShowNode(string title, string summary, bool canExplore)
        {
            if (_title != null)
            {
                _title.text = title ?? "Unknown";
            }

            if (_summary != null)
            {
                _summary.text = summary ?? string.Empty;
            }

            SetExploreEnabled(canExplore);
        }

        private void SetExploreEnabled(bool enabled)
        {
            if (_exploreButton != null)
            {
                _exploreButton.SetEnabled(enabled);
            }
        }
    }
}
