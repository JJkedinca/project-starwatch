using System.Collections.Generic;
using EndlessHeros.Core.Events;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Run.Timeline;
using EndlessHeros.Core.Simulation;
using EndlessHeros.Overworld.Services;

namespace EndlessHeros.Overworld.Systems
{
    /// <summary>
    /// Completes due fleet arrivals and explores the destination. Stepped by SimulationLoop, not Unity Update.
    /// </summary>
    public sealed class TravelSystem : ISimulationSystem
    {
        private readonly RunManager _runManager;
        private readonly ExplorationService _exploration;
        private readonly EventJournal _eventJournal;
        private readonly List<ActiveTravel> _completedBuffer = new();

        public TravelSystem(RunManager runManager, ExplorationService exploration, EventJournal eventJournal)
        {
            _runManager = runManager;
            _exploration = exploration;
            _eventJournal = eventJournal;
        }

        /// <summary>
        /// Completes travels that have arrived at or before the current sim time.
        /// </summary>
        public void OnSimulationTick(GameTime time, float tickDeltaSeconds)
        {
            _ = tickDeltaSeconds;
            CompleteArrivals(time);
        }

        /// <summary>
        /// Completes any travels that have reached their arrival time.
        /// </summary>
        public int CompleteArrivals(GameTime time)
        {
            if (_runManager == null || !_runManager.HasActiveRun)
            {
                return 0;
            }

            _completedBuffer.Clear();
            var completed = _runManager.CurrentRun.Timeline.CompleteArrivals(time, _completedBuffer);

            for (var i = 0; i < _completedBuffer.Count; i++)
            {
                var travel = _completedBuffer[i];
                _exploration?.Explore(
                    _runManager.CurrentRun,
                    ObserverId.Player,
                    travel.ToNodeId);
                _eventJournal?.Record(
                    GameEventType.FleetArrived,
                    time,
                    travel.FleetId,
                    travel.ToNodeId);
            }

            return completed;
        }
    }
}
