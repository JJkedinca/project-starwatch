using System;
using System.Globalization;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Settlement;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Settlement dossier: header, tabs (Overview / Buildings / Shipyard), commitment card.
    /// </summary>
    internal sealed class SettlementPanelView : ISettlementView
    {
        private enum Tab
        {
            Overview = 0,
            Buildings = 1,
            Shipyard = 2,
        }

        public event Action FoundOutpostRequested;

        public event Action FoundColonyRequested;

        public event Action AbandonRequested;

        public event Action ScanRequested;

        public event Action<ContentId> PlaceRequested;

        public event Action<ContentId> UpgradeRequested;

        private readonly ResourceSummaryView _baseStats = new();
        private readonly ResourceSummaryView _production = new();
        private readonly SettlementCommitmentView _commitment = new();
        private readonly BuildingListView _buildings = new();
        private VisualElement _panel;
        private VisualElement _header;
        private Label _planetName;
        private Label _scanChip;
        private Button _scanButton;
        private Label _presence;
        private Label _footprint;
        private VisualElement _chips;
        private Button _abandonButton;
        private Button _abandonConfirm;
        private Button _tabOverview;
        private Button _tabBuildings;
        private Button _tabShipyard;
        private VisualElement _overviewBody;
        private VisualElement _buildingsBody;
        private VisualElement _shipyardHost;
        private Label _netLine;
        private ContentId _boundPlanetId;
        private Tab _tab = Tab.Overview;
        private bool _showShipyardTab;
        private bool _confirmAbandon;

        internal void Bind(VisualElement panel)
        {
            _panel = panel;
            if (panel == null)
            {
                return;
            }

            _header = panel.Q<VisualElement>("settlement-header");
            _planetName = panel.Q<Label>("settlement-planet-name");
            _scanChip = panel.Q<Label>("settlement-scan-chip");
            _scanButton = panel.Q<Button>("settlement-scan");
            _presence = panel.Q<Label>("settlement-presence");
            _footprint = panel.Q<Label>("settlement-footprint");
            _chips = panel.Q<VisualElement>("settlement-chips");
            _abandonButton = panel.Q<Button>("settlement-abandon");
            _abandonConfirm = panel.Q<Button>("settlement-abandon-confirm");
            _tabOverview = panel.Q<Button>("tab-overview");
            _tabBuildings = panel.Q<Button>("tab-buildings");
            _tabShipyard = panel.Q<Button>("tab-shipyard");
            _overviewBody = panel.Q<VisualElement>("tab-overview-body");
            _buildingsBody = panel.Q<VisualElement>("tab-buildings-body");
            _shipyardHost = panel.Q<VisualElement>("tab-shipyard-body");
            _netLine = panel.Q<Label>("settlement-net-line");

            _baseStats.Bind(panel.Q<VisualElement>("planet-base"));
            _production.Bind(panel.Q<VisualElement>("planet-production"));
            _commitment.Bind(panel.Q<VisualElement>("settlement-commitment"));
            _commitment.FoundOutpostClicked += ClickFoundOutpost;
            _commitment.FoundColonyClicked += ClickFoundColony;
            _buildings.Bind(panel.Q<VisualElement>("building-list"));
            _buildings.PlaceClicked += ClickPlace;
            _buildings.UpgradeClicked += ClickUpgrade;

            if (_tabOverview != null)
            {
                _tabOverview.clicked += () => SelectTab(Tab.Overview);
            }

            if (_tabBuildings != null)
            {
                _tabBuildings.clicked += () => SelectTab(Tab.Buildings);
            }

            if (_tabShipyard != null)
            {
                _tabShipyard.clicked += () => SelectTab(Tab.Shipyard);
            }

            if (_scanButton != null)
            {
                _scanButton.clicked += () => ScanRequested?.Invoke();
            }

            if (_abandonButton != null)
            {
                _abandonButton.clicked += () =>
                {
                    _confirmAbandon = true;
                    RefreshAbandon();
                };
            }

            if (_abandonConfirm != null)
            {
                _abandonConfirm.clicked += () =>
                {
                    _confirmAbandon = false;
                    RefreshAbandon();
                    ClickAbandon();
                };
            }

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
                if (_header != null)
                {
                    _header.style.display = DisplayStyle.None;
                }

                _baseStats.Hide();
                _production.Hide();
                _commitment.Hide();
                _buildings.Hide();
                SetBodyVisible(null);
                return;
            }

            if (_boundPlanetId != model.Header.PlanetId)
            {
                _boundPlanetId = model.Header.PlanetId;
                _tab = Tab.Overview;
                _confirmAbandon = false;
            }

            _showShipyardTab = model.ShowShipyardTab;
            ApplyHeader(model.Header);
            if (!model.Economy.IsRevealed)
            {
                _baseStats.Hide();
                _production.Hide();
                if (_netLine != null)
                {
                    _netLine.style.display = DisplayStyle.None;
                }
            }
            else
            {
                _baseStats.Show(model.Economy.Base);
                _production.ShowRate(model.Economy.Production, model.Economy.Upkeep);
                ApplyNetLine(model.Economy);
            }

            _commitment.Show(model.Commitment);
            _buildings.Show(model.Buildings);
            if (_tab == Tab.Shipyard && !_showShipyardTab)
            {
                _tab = Tab.Overview;
            }

            ApplyTab();
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

        private void ApplyHeader(SettlementHeaderModel header)
        {
            if (_header != null)
            {
                _header.style.display = DisplayStyle.Flex;
            }

            if (_planetName != null)
            {
                _planetName.text = header.PlanetName;
            }

            if (_scanChip != null)
            {
                _scanChip.text = header.IsScanned ? "SCANNED" : "UNSCANNED";
                _scanChip.style.display = header.CanScan ? DisplayStyle.None : DisplayStyle.Flex;
            }

            if (_scanButton != null)
            {
                _scanButton.style.display = header.CanScan ? DisplayStyle.Flex : DisplayStyle.None;
                _scanButton.SetEnabled(header.CanScan);
            }

            if (_presence != null)
            {
                switch (header.Presence)
                {
                    case SettlementPresenceKind.Colony:
                        _presence.text = "Colony · Level " + header.Level;
                        break;
                    case SettlementPresenceKind.Outpost:
                        _presence.text = "Outpost";
                        break;
                    default:
                        _presence.text = "Unclaimed";
                        break;
                }
            }

            if (_footprint != null)
            {
                _footprint.text = "Footprint " + header.FootprintUsed + "/" + header.FootprintTotal;
            }

            if (_chips != null)
            {
                _chips.Clear();
                for (var i = 0; i < header.Chips.Count; i++)
                {
                    var chip = new Label(header.Chips[i]);
                    chip.AddToClassList("hud-chip");
                    _chips.Add(chip);
                }
            }

            if (_abandonButton != null)
            {
                _abandonButton.SetEnabled(header.CanAbandon);
                _abandonButton.style.display = header.CanAbandon ? DisplayStyle.Flex : DisplayStyle.None;
            }

            RefreshAbandon();
        }

        private void ApplyNetLine(PlanetEconomy economy)
        {
            if (_netLine == null)
            {
                return;
            }

            var cycle = EconomyTickSystem.kIntervalSeconds;
            _netLine.text =
                "OUTPUT / CYCLE  "
                + FormatNet(ResourceKind.Industry, economy, cycle)
                + "   "
                + FormatNet(ResourceKind.Energy, economy, cycle)
                + "   "
                + FormatNet(ResourceKind.Science, economy, cycle)
                + "   "
                + FormatNet(ResourceKind.Strategic, economy, cycle);
            _netLine.style.display = DisplayStyle.Flex;
        }

        private static string FormatNet(ResourceKind kind, PlanetEconomy economy, int cycle)
        {
            var perCycle = economy.Rate(kind) * cycle;
            var sign = perCycle > 0.0001f ? "+" : string.Empty;
            return sign + perCycle.ToString("0.#", CultureInfo.InvariantCulture) + " " + kind;
        }

        private void SelectTab(Tab tab)
        {
            _tab = tab;
            ApplyTab();
        }

        private void ApplyTab()
        {
            if (_tabOverview != null)
            {
                _tabOverview.EnableInClassList("settlement-tab--selected", _tab == Tab.Overview);
            }

            if (_tabBuildings != null)
            {
                _tabBuildings.EnableInClassList("settlement-tab--selected", _tab == Tab.Buildings);
            }

            if (_tabShipyard != null)
            {
                _tabShipyard.style.display = _showShipyardTab ? DisplayStyle.Flex : DisplayStyle.None;
                _tabShipyard.EnableInClassList("settlement-tab--selected", _tab == Tab.Shipyard);
            }

            SetBodyVisible(_tab);
        }

        private void SetBodyVisible(Tab? tab)
        {
            if (_overviewBody != null)
            {
                _overviewBody.style.display = tab == Tab.Overview ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (_buildingsBody != null)
            {
                _buildingsBody.style.display = tab == Tab.Buildings ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (_shipyardHost != null)
            {
                _shipyardHost.style.display = tab == Tab.Shipyard ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void RefreshAbandon()
        {
            if (_abandonConfirm != null)
            {
                _abandonConfirm.style.display = _confirmAbandon ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void Hide()
        {
            _baseStats.Hide();
            _production.Hide();
            _commitment.Hide();
            _buildings.Hide();
            _boundPlanetId = default;
            _confirmAbandon = false;
            if (_panel != null)
            {
                _panel.style.display = DisplayStyle.None;
            }
        }
    }
}
