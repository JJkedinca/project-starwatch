using System;
using ProjectStarwatch.Core.Content;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Timeline;
using ProjectStarwatch.Core.Simulation;

namespace ProjectStarwatch.Settlement
{
    /// <summary>
    /// Applies one empire-wide production and upkeep step to the timeline ledger.
    /// </summary>
    internal sealed class EconomyService
    {
        private readonly ObjectDatabaseSO _objectDatabase;
        private readonly ProductionService _productionService;
        private readonly DomainEventBus _events;
        private readonly SimulationClock _clock;
        private readonly ResourceInventory _output = new();
        private readonly ResourceInventory _upkeep = new();
        private readonly ResourceInventory _outputRemainder = new();

        /// <summary>
        /// Science produced by the last <see cref="ApplyTick"/> attempt. Zero when that second granted nothing.
        /// </summary>
        public float ProducedScience { get; private set; }

        public EconomyService(
            ObjectDatabaseSO objectDatabase,
            ProductionService productionService,
            DomainEventBus events,
            SimulationClock clock)
        {
            _objectDatabase = objectDatabase ?? throw new ArgumentNullException(nameof(objectDatabase));
            _productionService = productionService ?? throw new ArgumentNullException(nameof(productionService));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        /// <summary>
        /// Drops fractional output carried between seconds.
        /// </summary>
        internal void ClearOutputRemainder()
        {
            _outputRemainder.Clear();
        }

        /// <summary>
        /// Pays one second of upkeep and grants that second's whole output when the ledger can cover every colony.
        /// A fraction of output stays for the next second. Unpaid upkeep leaves that fraction unchanged.
        /// </summary>
        public void ApplyTick(RunState run)
        {
            if (SettleSecond(run))
            {
                PublishChanged();
            }
        }

        /// <summary>
        /// Settles one second without notifying listeners. Returns true when the ledger changed.
        /// </summary>
        internal bool SettleSecond(RunState run)
        {
            ProducedScience = 0f;
            if (run == null)
            {
                return false;
            }

            _output.Clear();
            _upkeep.Clear();
            var colonies = run.Timeline.Colonies;
            for (var i = 0; i < colonies.Count; i++)
            {
                var colony = colonies[i];
                _objectDatabase.TryGetPlanet(colony.PlanetId.Value, out var planet);
                var quote = _productionService.Quote(colony, planet);
                _output.Add(quote.Output);
                _upkeep.Add(quote.Upkeep);
            }

            var science = _output.Get(ResourceKind.Science);
            _output.Remove(ResourceKind.Science);
            if (!_output.HasAny() && !_upkeep.HasAny() && science <= 0f)
            {
                return false;
            }

            var ledger = run.Timeline.Resources;
            if (!ledger.TrySpend(_upkeep))
            {
                return false;
            }

            ProducedScience = science;
            if (!_output.HasAny() && !_upkeep.HasAny())
            {
                return false;
            }

            _outputRemainder.Add(_output);
            _outputRemainder.DepositFlooredInto(ledger);
            _outputRemainder.RetainFraction();
            return true;
        }

        internal void PublishChanged()
        {
            _events?.Publish(new ResourceChangedEvent(_clock != null ? _clock.CurrentTime : GameTime.Zero));
        }
    }
}
