using ProjectStarwatch.Core.WorldInput;
using ProjectStarwatch.Overworld.Map;
using UnityEngine;
using UnityEngine.InputSystem;
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

        internal FleetRosterView FleetRoster { get; } = new();

        internal FleetBarView FleetBar { get; } = new();

        internal FleetPanelView FleetPanel { get; } = new();

        internal ShipyardSectionView Shipyard { get; } = new();

        [SerializeField]
        private UIDocument _document;

        [SerializeField]
        private StarMapWorldPresenter _starMapWorldPresenter;

        [SerializeField]
        private MapCameraController _mapCameraController;

        [SerializeField]
        private MapPointerInput _mapPointerInput;

        [SerializeField]
        private WorldPointerInput _worldPointerInput;

        [SerializeField]
        private Camera _mapCamera;

        [SerializeField]
        private InputActionAsset _starwatchInput;

        private OverworldStatus _status;
        private readonly ToastStackView _toasts = new();
        private bool _isBound;

        internal ToastStackView Toasts => _toasts;

        /// <summary>
        /// Receives the shared toast bus. The screen paints it and does not interpret it.
        /// </summary>
        [Inject]
        internal void Construct(OverworldStatus status)
        {
            _status = status;
        }

        private void OnDestroy()
        {
            _toasts.Unbind();
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

            var title = root.Q<Label>("title");
            var mapSurface = root.Q<VisualElement>("map-surface");
            var starSystem = root.Q<VisualElement>("star-system");
            var starSystemInspection = root.Q<VisualElement>("star-system-inspection");
            var settlement = root.Q<VisualElement>("settlement-panel");
            var stockpile = root.Q<VisualElement>("stockpile-bar");
            var research = root.Q<VisualElement>("research-panel");
            var researchEntry = stockpile != null ? stockpile.Q<Button>("stockpile-research") : null;
            var fleetRoster = root.Q<VisualElement>("fleet-roster-host");
            var fleetBar = root.Q<VisualElement>("fleet-bar");
            var fleetPanel = root.Q<VisualElement>("fleet-panel");
            var shipyard = root.Q<VisualElement>("shipyard-section");
            var toastStack = root.Q<VisualElement>("toast-stack");
            if (mapSurface == null
                || starSystem == null
                || starSystemInspection == null
                || settlement == null
                || stockpile == null
                || research == null
                || researchEntry == null
                || fleetRoster == null
                || fleetBar == null
                || fleetPanel == null
                || shipyard == null
                || toastStack == null)
            {
                Debug.LogError("OverworldScreenView could not find the map, star system, fleet, settlement, research, stockpile, or toast roots.");
                return;
            }

            if (_worldPointerInput != null)
            {
                _worldPointerInput.Configure(_document, _starwatchInput);
            }

            if (_mapCameraController != null)
            {
                _mapCameraController.Configure(_worldPointerInput, _starwatchInput);
            }

            if (_mapPointerInput != null)
            {
                _mapPointerInput.Configure(_mapCamera, _worldPointerInput);
            }

            StarMap.Bind(
                mapSurface,
                starSystemInspection,
                _starMapWorldPresenter,
                _mapCameraController,
                _mapPointerInput,
                _mapCamera);
            StarSystem.Bind(starSystem, mapSurface, title);
            Settlement.Bind(settlement);
            Stockpile.Bind(stockpile);
            Research.Bind(research, researchEntry);
            FleetRoster.Bind(fleetRoster);
            FleetBar.Bind(fleetBar);
            FleetPanel.Bind(fleetPanel);
            Shipyard.Bind(shipyard);
            if (_status != null)
            {
                _toasts.Bind(toastStack, _status);
            }

            _isBound = true;
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
        private readonly FleetRosterPresenter _fleetRosterPresenter;
        private readonly FleetPanelPresenter _fleetPanelPresenter;
        private readonly ShipyardPresenter _shipyardPresenter;
        private readonly OverworldToastPresenter _toastPresenter;

        public OverworldScreenBinder(
            OverworldScreenView screen,
            StarMapPresenter starMapPresenter,
            StarSystemPresenter starSystemPresenter,
            SettlementPresenter settlementPresenter,
            StockpilePresenter stockpilePresenter,
            ResearchPresenter researchPresenter,
            FleetRosterPresenter fleetRosterPresenter,
            FleetPanelPresenter fleetPanelPresenter,
            ShipyardPresenter shipyardPresenter,
            OverworldToastPresenter toastPresenter)
        {
            _screen = screen;
            _starMapPresenter = starMapPresenter;
            _starSystemPresenter = starSystemPresenter;
            _settlementPresenter = settlementPresenter;
            _stockpilePresenter = stockpilePresenter;
            _researchPresenter = researchPresenter;
            _fleetRosterPresenter = fleetRosterPresenter;
            _fleetPanelPresenter = fleetPanelPresenter;
            _shipyardPresenter = shipyardPresenter;
            _toastPresenter = toastPresenter;
        }

        public void Start()
        {
            _screen.Bind();
            _toastPresenter.Start();
            _starMapPresenter.Start();
            _starSystemPresenter.Start();
            _settlementPresenter.Start();
            _stockpilePresenter.Start();
            _researchPresenter.Start();
            _fleetRosterPresenter.Start();
            _fleetPanelPresenter.Start();
            _shipyardPresenter.Start();
        }
    }
}
