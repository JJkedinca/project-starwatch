using UnityEngine.UIElements;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// Presence and the quoted output and upkeep for the selected planet.
    /// </summary>
    internal sealed class ColonyStatusView
    {
        private VisualElement _root;
        private Label _summary;

        /// <summary>
        /// Binds the ColonyStatus.uxml root.
        /// </summary>
        public void Bind(VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            _root = root;
            _summary = root.Q<Label>("colony-status-summary");
            Hide();
        }

        /// <summary>
        /// Hides colony status.
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
        /// Shows presence and rates. The text is already quoted by the settlement query.
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
