using EndlessHeros.Core.Data;
using EndlessHeros.Core.Run.Timeline;
using UnityEngine.UIElements;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// Stockpile line for the four settlement resources.
    /// </summary>
    internal sealed class ResourceSummaryView
    {
        private VisualElement _root;
        private Label _industry;
        private Label _energy;
        private Label _science;
        private Label _strategic;

        /// <summary>
        /// Binds the ResourceSummary.uxml root.
        /// </summary>
        public void Bind(VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            _root = root;
            _industry = root.Q<Label>("resource-industry");
            _energy = root.Q<Label>("resource-energy");
            _science = root.Q<Label>("resource-science");
            _strategic = root.Q<Label>("resource-strategic");
            Hide();
        }

        /// <summary>
        /// Hides the stockpile.
        /// </summary>
        public void Hide()
        {
            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
            }
        }

        /// <summary>
        /// Shows whole stockpile totals from the inventory.
        /// </summary>
        public void Show(ResourceInventory resources)
        {
            if (_root == null)
            {
                return;
            }

            Set(_industry, Whole(resources, ResourceKind.Industry));
            Set(_energy, Whole(resources, ResourceKind.Energy));
            Set(_science, Whole(resources, ResourceKind.Science));
            Set(_strategic, Whole(resources, ResourceKind.Strategic));
            _root.style.display = DisplayStyle.Flex;
        }

        private static int Whole(ResourceInventory resources, ResourceKind kind)
        {
            return resources == null ? 0 : resources.GetWhole(kind);
        }

        private static void Set(Label label, int amount)
        {
            if (label != null)
            {
                label.text = amount.ToString();
            }
        }
    }
}
