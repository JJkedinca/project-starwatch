using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Footprint, output multipliers, and deposits for the selected planet.
    /// </summary>
    internal sealed class PlanetDevelopmentView
    {
        private VisualElement _root;
        private Label _summary;

        /// <summary>
        /// Binds the PlanetDevelopment.uxml root.
        /// </summary>
        public void Bind(VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            _root = root;
            _summary = root.Q<Label>("planet-development-summary");
            Hide();
        }

        /// <summary>
        /// Hides planet development.
        /// </summary>
        public void Hide()
        {
            if (_summary != null)
            {
                _summary.text = string.Empty;
            }

            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
            }
        }

        /// <summary>
        /// Shows the development text from the settlement query.
        /// </summary>
        public void Show(string summary)
        {
            if (_root == null)
            {
                return;
            }

            if (_summary != null)
            {
                _summary.text = summary ?? string.Empty;
            }

            _root.style.display = DisplayStyle.Flex;
        }
    }
}
