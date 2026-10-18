using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

namespace EndlessHeros.Overworld.UI
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
            if (mapSurface == null
                || starSystem == null
                || starSystemInspection == null
                || planetInspection == null
                || settlement == null
                || stockpile == null
                || research == null
                || researchEntry == null)
            {
                Debug.LogError("OverworldScreenView could not find the map, star system, settlement, research, or stockpile roots.");
                return;
            }

            StarMap.Bind(mapSurface, starSystemInspection);
            StarSystem.Bind(starSystem, planetInspection, mapSurface, title);
            Settlement.Bind(settlement);
            Stockpile.Bind(stockpile);
            Research.Bind(research, researchEntry);
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
        private readonly StarMapPresenter _map;
        private readonly StarSystemPresenter _system;
        private readonly SettlementPresenter _settlement;
        private readonly StockpilePresenter _stockpile;

        public OverworldScreenBinder(
            OverworldScreenView screen,
            StarMapPresenter map,
            StarSystemPresenter system,
            SettlementPresenter settlement,
            StockpilePresenter stockpile)
        {
            _screen = screen;
            _map = map;
            _system = system;
            _settlement = settlement;
            _stockpile = stockpile;
        }

        public void Start()
        {
            _screen.Bind();
            _map.Start();
            _system.Start();
            _settlement.Start();
            _stockpile.Start();
        }
    }
}
