using System;
using EndlessHeros.Core.Content;
using EndlessHeros.Core.Events;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Run.Timeline;
using EndlessHeros.Core.Simulation;

namespace EndlessHeros.Settlement
{
    /// <summary>
    /// Applies one empire-wide production and upkeep step to the timeline ledger.
    /// </summary>
    internal sealed class EconomyService
    {
        private readonly ObjectDatabaseSO _objectDatabase;
        private readonly ProductionService _production;
        private readonly DomainEventBus _events;
        private readonly SimulationClock _clock;
        private readonly ResourceInventory _output = new();
        private readonly ResourceInventory _upkeep = new();
        private readonly ResourceInventory _outputRemainder = new();

        public EconomyService(
            ObjectDatabaseSO objectDatabase,
            ProductionService production,
            DomainEventBus events,
            SimulationClock clock)
        {
            _objectDatabase = objectDatabase ?? throw new ArgumentNullException(nameof(objectDatabase));
            _production = production ?? throw new ArgumentNullException(nameof(production));
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
        /// Pays upkeep and grants whole output when the ledger can cover every colony. A fraction of output stays for the next second. Unpaid upkeep leaves that fraction unchanged.
        /// </summary>
        public void ApplyTick(RunState run)
        {
            if (run == null)
            {
                return;
            }

            _output.Clear();
            _upkeep.Clear();
            var colonies = run.Timeline.Colonies;
            for (var i = 0; i < colonies.Count; i++)
            {
                var colony = colonies[i];
                _objectDatabase.TryGetPlanet(colony.PlanetId.Value, out var planet);
                var quote = _production.Quote(colony, planet);
                _output.Add(quote.Output);
                _upkeep.Add(quote.Upkeep);
            }

            if (!_output.HasAny() && !_upkeep.HasAny())
            {
                return;
            }

            var ledger = run.Timeline.Resources;
            if (!ledger.TrySpend(_upkeep))
            {
                return;
            }

            _outputRemainder.Add(_output);
            _outputRemainder.DepositFlooredInto(ledger);
            _outputRemainder.RetainFraction();
            _events.Publish(new ResourceChangedEvent(_clock.CurrentTime));
        }
    }
}
