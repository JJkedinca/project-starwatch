using EndlessHeros.Core.Data;
using EndlessHeros.Core.Run.Timeline;
using UnityEngine.UIElements;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// Bottom bar for the empire stockpile. Whole resources only.
    /// </summary>
    internal sealed class StockpileBarView : IStockpileView
    {
        private Label _industry;
        private Label _energy;
        private Label _science;
        private Label _strategic;

        /// <summary>
        /// Binds the StockpileBar.uxml root.
        /// </summary>
        public void Bind(VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            _industry = root.Q<Label>("stockpile-industry");
            _energy = root.Q<Label>("stockpile-energy");
            _science = root.Q<Label>("stockpile-science");
            _strategic = root.Q<Label>("stockpile-strategic");
            Apply(null);
        }

        /// <summary>
        /// Shows whole stockpile totals. A missing inventory reads as zero.
        /// </summary>
        public void Apply(ResourceInventory stockpile)
        {
            Set(_industry, Whole(stockpile, ResourceKind.Industry));
            Set(_energy, Whole(stockpile, ResourceKind.Energy));
            Set(_science, Whole(stockpile, ResourceKind.Science));
            Set(_strategic, Whole(stockpile, ResourceKind.Strategic));
        }

        private static int Whole(ResourceInventory stockpile, ResourceKind kind)
        {
            return stockpile == null ? 0 : stockpile.GetWhole(kind);
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
