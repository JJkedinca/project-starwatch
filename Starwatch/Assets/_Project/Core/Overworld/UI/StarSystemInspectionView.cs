using System;
using UnityEngine.UIElements;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// Side panel for the selected star system: summary and travel.
    /// </summary>
    internal sealed class StarSystemInspectionView
    {
        public event Action TravelClicked;

        private Label _title;
        private Label _summary;
        private Button _travelButton;

        /// <summary>
        /// Binds the StarSystemInspection.uxml root.
        /// </summary>
        public void Bind(VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            _title = root.Q<Label>("inspection-title");
            _summary = root.Q<Label>("inspection-summary");
            _travelButton = root.Q<Button>("travel-button");
            if (_travelButton != null)
            {
                _travelButton.clicked += () => TravelClicked?.Invoke();
            }
        }

        /// <summary>
        /// Shows empty selection state.
        /// </summary>
        public void ShowEmpty()
        {
            if (_title != null)
            {
                _title.text = "Select a star system";
            }

            if (_summary != null)
            {
                _summary.text = string.Empty;
            }

            SetTravelEnabled(false);
        }

        /// <summary>
        /// Updates labels for the selected node.
        /// </summary>
        public void ShowNode(string title, string summary, bool canTravel)
        {
            if (_title != null)
            {
                _title.text = title ?? "Unknown";
            }

            if (_summary != null)
            {
                _summary.text = summary ?? string.Empty;
            }

            SetTravelEnabled(canTravel);
        }

        private void SetTravelEnabled(bool enabled)
        {
            if (_travelButton != null)
            {
                _travelButton.SetEnabled(enabled);
            }
        }
    }
}
