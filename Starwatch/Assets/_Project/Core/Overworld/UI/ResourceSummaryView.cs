using System;
using EndlessHeros.Core.Data;
using EndlessHeros.Core.Run.Timeline;
using UnityEngine.UIElements;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// Four resource amounts for one planet panel.
    /// </summary>
    internal sealed class ResourceSummaryView
    {
        private VisualElement _root;
        private Label _industry;
        private Label _energy;
        private Label _science;
        private Label _strategic;

        /// <summary>
        /// Binds the panel that contains a resource summary. Hiding this root hides the panel title with the amounts.
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
        /// Hides the planet resources.
        /// </summary>
        public void Hide()
        {
            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
            }
        }

        /// <summary>
        /// Shows unsigned totals. Fractions stay visible.
        /// </summary>
        public void Show(ResourceInventory output)
        {
            if (_root == null)
            {
                return;
            }

            Set(_industry, Format(Amount(output, ResourceKind.Industry)));
            Set(_energy, Format(Amount(output, ResourceKind.Energy)));
            Set(_science, Format(Amount(output, ResourceKind.Science)));
            Set(_strategic, Format(Amount(output, ResourceKind.Strategic)));
            _root.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// Shows output minus upkeep. A positive rate is marked with +.
        /// </summary>
        public void ShowRate(ResourceInventory output, ResourceInventory upkeep)
        {
            if (_root == null)
            {
                return;
            }

            Set(_industry, FormatRate(Amount(output, ResourceKind.Industry) - Amount(upkeep, ResourceKind.Industry)));
            Set(_energy, FormatRate(Amount(output, ResourceKind.Energy) - Amount(upkeep, ResourceKind.Energy)));
            Set(_science, FormatRate(Amount(output, ResourceKind.Science) - Amount(upkeep, ResourceKind.Science)));
            Set(_strategic, FormatRate(Amount(output, ResourceKind.Strategic) - Amount(upkeep, ResourceKind.Strategic)));
            _root.style.display = DisplayStyle.Flex;
        }

        private static float Amount(ResourceInventory output, ResourceKind kind)
        {
            return output == null ? 0f : output.Get(kind);
        }

        private static string FormatRate(float amount)
        {
            var text = Format(amount);
            if (amount > 0f && text != "0")
            {
                return "+" + text;
            }

            return text;
        }

        private static string Format(float amount)
        {
            var nearest = MathF.Round(amount);
            if (MathF.Abs(amount - nearest) < 0.001f)
            {
                return ((int)nearest).ToString();
            }

            return amount.ToString("0.#");
        }

        private static void Set(Label label, string amount)
        {
            if (label != null)
            {
                label.text = amount ?? string.Empty;
            }
        }
    }
}
