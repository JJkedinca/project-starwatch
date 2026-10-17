using System;
using EndlessHeros.Core.Identifiers;
using UnityEngine.UIElements;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// Settlement panel. Shows the stockpile while a star system is open, and the
    /// planet sections when a planet is selected.
    /// </summary>
    internal sealed class SettlementPanelView : ISettlementView
    {
        public event Action FoundOutpostRequested;

        public event Action FoundColonyRequested;

        public event Action AbandonRequested;

        public event Action<ContentId> PlaceRequested;

        public event Action<ContentId> UpgradeRequested;

        private readonly ResourceSummaryView _resources = new();
        private readonly PlanetDevelopmentView _development = new();
        private readonly ColonyStatusView _status = new();
        private readonly SettlementCommitmentView _commitment = new();
        private readonly BuildingListView _buildings = new();
        private VisualElement _panel;

        internal void Bind(VisualElement panel)
        {
            _panel = panel;
            if (panel == null)
            {
                return;
            }

            _resources.Bind(panel.Q<VisualElement>("resource-summary"));
            _development.Bind(panel.Q<VisualElement>("planet-development"));
            _status.Bind(panel.Q<VisualElement>("colony-status"));
            _commitment.Bind(panel.Q<VisualElement>("settlement-commitment"));
            _commitment.FoundOutpostClicked += ClickFoundOutpost;
            _commitment.FoundColonyClicked += ClickFoundColony;
            _commitment.AbandonClicked += ClickAbandon;
            _buildings.Bind(panel.Q<VisualElement>("building-list"));
            _buildings.PlaceClicked += ClickPlace;
            _buildings.UpgradeClicked += ClickUpgrade;
            Hide();
        }

        public void Apply(SettlementPanelModel model)
        {
            if (_panel == null || !model.ShowPanel)
            {
                Hide();
                return;
            }

            _panel.style.display = DisplayStyle.Flex;
            _resources.Show(model.Resources);
            if (!model.ShowPlanet)
            {
                _development.Hide();
                _status.Hide();
                _commitment.Hide();
                _buildings.Hide();
                return;
            }

            _development.Show(model.Development);
            _status.Show(model.Status);
            _commitment.Show(model.Commitment);
            _buildings.Show(model.Buildings);
        }

        internal void ClickFoundOutpost()
        {
            FoundOutpostRequested?.Invoke();
        }

        internal void ClickFoundColony()
        {
            FoundColonyRequested?.Invoke();
        }

        internal void ClickAbandon()
        {
            AbandonRequested?.Invoke();
        }

        internal void ClickPlace(ContentId buildingId)
        {
            PlaceRequested?.Invoke(buildingId);
        }

        internal void ClickUpgrade(ContentId buildingId)
        {
            UpgradeRequested?.Invoke(buildingId);
        }

        private void Hide()
        {
            _resources.Hide();
            _development.Hide();
            _status.Hide();
            _commitment.Hide();
            _buildings.Hide();
            if (_panel != null)
            {
                _panel.style.display = DisplayStyle.None;
            }
        }
    }
}
