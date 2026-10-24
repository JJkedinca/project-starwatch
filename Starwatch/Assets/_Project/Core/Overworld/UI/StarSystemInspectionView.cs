using System;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Target card for the selected star map node. Hidden while a star system is open.
    /// </summary>
    internal sealed class StarSystemInspectionView
    {
        public event Action TravelClicked;

        private VisualElement _root;
        private Label _title;
        private VisualElement _chips;
        private Label _bodies;
        private Label _fleets;
        private Label _relation;
        private Label _hint;
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

            _root = root;
            _title = root.Q<Label>("inspection-title");
            _chips = root.Q<VisualElement>("inspection-chips");
            _bodies = root.Q<Label>("inspection-bodies");
            _fleets = root.Q<Label>("inspection-fleets");
            _relation = root.Q<Label>("inspection-relation");
            _hint = root.Q<Label>("inspection-hint");
            _travelButton = root.Q<Button>("inspection-travel");
            if (_travelButton != null)
            {
                _travelButton.clicked -= OnTravelClicked;
                _travelButton.clicked += OnTravelClicked;
            }
        }

        /// <summary>
        /// Shows empty selection state on the star map.
        /// </summary>
        public void ShowEmpty()
        {
            if (_root != null)
            {
                _root.style.display = DisplayStyle.Flex;
            }

            if (_title != null)
            {
                _title.text = "Select a star system";
            }

            _chips?.Clear();
            SetLabel(_bodies, string.Empty);
            SetLabel(_fleets, string.Empty);
            SetLabel(_relation, string.Empty);
            SetLabel(_hint, string.Empty);
            SetTravelVisible(false);
        }

        /// <summary>
        /// Applies the selected-node card, or hides while a star system is open.
        /// </summary>
        public void Show(StarMapCardModel card)
        {
            if (!card.IsShown)
            {
                if (_root != null)
                {
                    _root.style.display = DisplayStyle.None;
                }

                return;
            }

            if (!card.HasSelection)
            {
                ShowEmpty();
                return;
            }

            if (_root != null)
            {
                _root.style.display = DisplayStyle.Flex;
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
                    ? "Right-click the map or Travel to send "
                        + (string.IsNullOrEmpty(card.FleetName) ? "fleet" : card.FleetName)
                    : string.Empty);
            SetTravelVisible(card.ShowTravelHint);
        }

        private void OnTravelClicked()
        {
            TravelClicked?.Invoke();
        }

        private void SetTravelVisible(bool visible)
        {
            if (_travelButton == null)
            {
                return;
            }

            _travelButton.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
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
