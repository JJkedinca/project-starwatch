using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Side panel for the selected star system: summary and arrival time.
    /// </summary>
    internal sealed class StarSystemInspectionView
    {
        private Label _title;
        private Label _summary;
        private Label _arrival;

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
            _arrival = root.Q<Label>("travel-arrival");
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

            SetArrival(false, 0f);
        }

        /// <summary>
        /// Updates labels for the selected node.
        /// </summary>
        public void ShowNode(string title, string summary, bool hasArrival, float arrivalSeconds)
        {
            if (_title != null)
            {
                _title.text = title ?? "Unknown";
            }

            if (_summary != null)
            {
                _summary.text = summary ?? string.Empty;
            }

            SetArrival(hasArrival, arrivalSeconds);
        }

        private void SetArrival(bool hasArrival, float arrivalSeconds)
        {
            if (_arrival == null)
            {
                return;
            }

            _arrival.text = hasArrival ? "Arrives in " + arrivalSeconds.ToString("0.#") + "s" : string.Empty;
            _arrival.style.display = hasArrival ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
