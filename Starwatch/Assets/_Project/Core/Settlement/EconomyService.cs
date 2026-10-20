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

            var quote = EmpireProductionQuote.Build(run, _objectDatabase, _productionService);
            var output = new ResourceInventory();
            output.Add(quote.Output);
            var upkeep = quote.Upkeep;
            var science = output.Get(ResourceKind.Science);
            output.Remove(ResourceKind.Science);
            if (!output.HasAny() && !upkeep.HasAny() && science <= 0f)
            {
                return false;
            }

            var ledger = run.Timeline.Resources;
            if (!ledger.TrySpend(upkeep))
            {
                return false;
            }

            ProducedScience = science;
            if (!output.HasAny() && !upkeep.HasAny())
            {
                return false;
            }

            _outputRemainder.Add(output);
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
