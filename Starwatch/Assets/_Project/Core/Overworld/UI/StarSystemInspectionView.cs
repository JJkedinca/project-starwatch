using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Target card for the selected star map node.
    /// </summary>
    internal sealed class StarSystemInspectionView
    {
        private Label _title;
        private VisualElement _chips;
        private Label _bodies;
        private Label _fleets;
        private Label _relation;
        private Label _hint;

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
            _chips = root.Q<VisualElement>("inspection-chips");
            _bodies = root.Q<Label>("inspection-bodies");
            _fleets = root.Q<Label>("inspection-fleets");
            _relation = root.Q<Label>("inspection-relation");
            _hint = root.Q<Label>("inspection-hint");
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

            _chips?.Clear();
            SetLabel(_bodies, string.Empty);
            SetLabel(_fleets, string.Empty);
            SetLabel(_relation, string.Empty);
            SetLabel(_hint, string.Empty);
        }

        /// <summary>
        /// Applies the selected-node card.
        /// </summary>
        public void Show(StarMapCardModel card)
        {
            if (!card.HasSelection)
            {
                ShowEmpty();
                return;
            }

            if (_title != null)
            {
                _title.text = card.Title;
            }

            ShowChips(card);
            if (card.IsExplored)
            {
                SetLabel(
                    _bodies,
                    card.BodyCount == 0
                        ? "No bodies"
                        : card.ScannedBodyCount + " / " + card.BodyCount + " bodies scanned");
                SetLabel(_fleets, card.HasFleetPresent ? "Fleet present" : "No fleet present");
            }
            else
            {
                SetLabel(_bodies, "Hidden until a fleet arrives.");
                SetLabel(_fleets, string.Empty);
            }

            SetLabel(_relation, RelationText(card));
            SetLabel(
                _hint,
                card.ShowTravelHint
                    ? "Right-click to send " + (string.IsNullOrEmpty(card.FleetName) ? "fleet" : card.FleetName)
                    : string.Empty);
        }

        private void ShowChips(StarMapCardModel card)
        {
            if (_chips == null)
            {
                return;
            }

            _chips.Clear();
            AddChip(card.IsExplored ? "EXPLORED" : "UNEXPLORED", card.IsExplored ? "inspection-chip--good" : "inspection-chip--muted");
            if (card.HasFleetPresent)
            {
                AddChip("FLEET PRESENT", "inspection-chip--info");
            }

            if (card.IsExplored && card.BodyCount > 0)
            {
                AddChip(card.BodyCount + " BODIES", "inspection-chip--info");
            }
        }

        private void AddChip(string text, string stateClass)
        {
            var chip = new Label(text);
            chip.AddToClassList("hud-chip");
            chip.AddToClassList("inspection-chip");
            chip.AddToClassList(stateClass);
            _chips.Add(chip);
        }

        private static string RelationText(StarMapCardModel card)
        {
            var fleet = string.IsNullOrEmpty(card.FleetName) ? "Fleet" : card.FleetName;
            switch (card.FleetRelation)
            {
                case StarMapFleetRelation.AtTarget:
                    return fleet + " is here";
                case StarMapFleetRelation.InTransit:
                    return fleet + " is in transit";
                case StarMapFleetRelation.Reachable:
                    return "From " + fleet + " · " + card.RelationSeconds.ToString("0.#") + "s";
                case StarMapFleetRelation.Unreachable:
                    return "Unreachable";
                default:
                    return "Select a fleet to travel";
            }
        }

        private static void SetLabel(Label label, string text)
        {
            if (label == null)
            {
                return;
            }

            label.text = text ?? string.Empty;
            label.style.display = string.IsNullOrEmpty(text) ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }
}
