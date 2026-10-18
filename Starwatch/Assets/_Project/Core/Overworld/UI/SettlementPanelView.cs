using System;
using ProjectStarwatch.Core.Identifiers;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Settlement panel. Planet base, current production, and the other planet sections appear for the selected planet.
    /// </summary>
    internal sealed class SettlementPanelView : ISettlementView
    {
        public event Action FoundOutpostRequested;

        public event Action FoundColonyRequested;

        public event Action AbandonRequested;

        public event Action<ContentId> PlaceRequested;

        public event Action<ContentId> UpgradeRequested;

        private readonly ResourceSummaryView _baseStats = new();
        private readonly ResourceSummaryView _production = new();
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

            _baseStats.Bind(panel.Q<VisualElement>("planet-base"));
            _production.Bind(panel.Q<VisualElement>("planet-production"));
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
            if (!model.ShowPlanet)
            {
                _baseStats.Hide();
                _production.Hide();
                _development.Hide();
                _status.Hide();
                _commitment.Hide();
                _buildings.Hide();
                return;
            }

            if (!model.Economy.IsRevealed)
            {
                _baseStats.Hide();
                _production.Hide();
            }
            else
            {
                _baseStats.Show(model.Economy.Base);
                _production.ShowRate(model.Economy.Production, model.Economy.Upkeep);
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
            _baseStats.Hide();
            _production.Hide();
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
