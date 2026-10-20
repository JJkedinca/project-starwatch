using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Binds the overworld screen and gives each group the elements it owns.
    /// </summary>
    public sealed class OverworldScreenView : MonoBehaviour
    {
        internal StarMapScreen StarMap { get; } = new();

        internal StarSystemScreen StarSystem { get; } = new();

        internal SettlementPanelView Settlement { get; } = new();

        internal StockpileBarView Stockpile { get; } = new();

        internal ResearchPanelView Research { get; } = new();

        internal FleetPanelView FleetPanel { get; } = new();

        internal ShipyardSectionView Shipyard { get; } = new();

        [SerializeField]
        private UIDocument _document;

        private OverworldStatus _status;
        private Label _statusLabel;
        private bool _isBound;

        /// <summary>
        /// Receives the shared status line. The screen paints it and does not interpret it.
        /// </summary>
        [Inject]
        internal void Construct(OverworldStatus status)
        {
            _status = status;
        }

        private void OnDestroy()
        {
            if (_status != null)
            {
                _status.Changed -= OnStatus;
            }
        }

        /// <summary>
        /// Queries the UIDocument visual tree and binds each group.
        /// </summary>
        internal void Bind()
        {
            if (_isBound)
            {
                return;
            }

            if (_document == null || _document.rootVisualElement == null)
            {
                Debug.LogError("OverworldScreenView requires a UIDocument with a visual tree.");
                return;
            }

            Bind(_document.rootVisualElement);
        }

        /// <summary>
        /// Binds an existing visual tree.
        /// </summary>
        internal void Bind(VisualElement root)
        {
            if (_isBound)
            {
                return;
            }

            if (root == null)
            {
                Debug.LogError("OverworldScreenView requires a visual tree.");
                return;
            }

            _statusLabel = root.Q<Label>("status-label");
            var title = root.Q<Label>("title");
            var mapSurface = root.Q<VisualElement>("map-surface");
            var starSystem = root.Q<VisualElement>("star-system");
            var starSystemInspection = root.Q<VisualElement>("star-system-inspection");
            var planetInspection = root.Q<VisualElement>("planet-detail");
            var settlement = root.Q<VisualElement>("settlement-panel");
            var stockpile = root.Q<VisualElement>("stockpile-bar");
            var research = root.Q<VisualElement>("research-panel");
            var researchEntry = stockpile != null ? stockpile.Q<Button>("stockpile-research") : null;
            var fleetPanel = root.Q<VisualElement>("fleet-panel");
            var shipyard = root.Q<VisualElement>("shipyard-section");
            if (mapSurface == null
                || starSystem == null
                || starSystemInspection == null
                || planetInspection == null
                || settlement == null
                || stockpile == null
                || research == null
                || researchEntry == null
                || fleetPanel == null
                || shipyard == null)
            {
                Debug.LogError("OverworldScreenView could not find the map, star system, fleet, settlement, research, or stockpile roots.");
                return;
            }

            StarMap.Bind(mapSurface, starSystemInspection);
            StarSystem.Bind(starSystem, planetInspection, mapSurface, title);
            Settlement.Bind(settlement);
            Stockpile.Bind(stockpile);
            Research.Bind(research, researchEntry);
            FleetPanel.Bind(fleetPanel);
            Shipyard.Bind(shipyard);
            if (_status != null)
            {
                _status.Changed += OnStatus;
                OnStatus(_status.Text);
            }

            _isBound = true;
        }

        private void OnStatus(string message)
        {
            if (_statusLabel != null)
            {
                _statusLabel.text = message ?? string.Empty;
            }
        }
    }

    /// <summary>
    /// Binds the screen, then starts the presenters. Their first Apply needs the bound views.
    /// </summary>
    internal sealed class OverworldScreenBinder : IStartable
    {
        private readonly OverworldScreenView _screen;
        private readonly StarMapPresenter _starMapPresenter;
        private readonly StarSystemPresenter _starSystemPresenter;
        private readonly SettlementPresenter _settlementPresenter;
        private readonly StockpilePresenter _stockpilePresenter;
        private readonly ResearchPresenter _researchPresenter;
        private readonly FleetPanelPresenter _fleetPanelPresenter;
        private readonly ShipyardPresenter _shipyardPresenter;

        public OverworldScreenBinder(
            OverworldScreenView screen,
            StarMapPresenter starMapPresenter,
            StarSystemPresenter starSystemPresenter,
            SettlementPresenter settlementPresenter,
            StockpilePresenter stockpilePresenter,
            ResearchPresenter researchPresenter,
            FleetPanelPresenter fleetPanelPresenter,
            ShipyardPresenter shipyardPresenter)
        {
            _screen = screen;
            _starMapPresenter = starMapPresenter;
            _starSystemPresenter = starSystemPresenter;
            _settlementPresenter = settlementPresenter;
            _stockpilePresenter = stockpilePresenter;
            _researchPresenter = researchPresenter;
            _fleetPanelPresenter = fleetPanelPresenter;
            _shipyardPresenter = shipyardPresenter;
        }

        public void Start()
        {
            _screen.Bind();
            _starMapPresenter.Start();
            _starSystemPresenter.Start();
            _settlementPresenter.Start();
            _stockpilePresenter.Start();
            _researchPresenter.Start();
            _fleetPanelPresenter.Start();
            _shipyardPresenter.Start();
        }
    }
}
