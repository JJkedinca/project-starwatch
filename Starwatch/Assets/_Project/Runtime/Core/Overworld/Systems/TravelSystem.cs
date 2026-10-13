using System.Collections.Generic;
using EndlessHeros.Core.Events;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Run.Timeline;
using EndlessHeros.Core.Simulation;

namespace EndlessHeros.Overworld.Systems
{
    /// <summary>
    /// Completes due fleet arrivals. Stepped by SimulationLoop, not Unity Update.
    /// </summary>
    public sealed class TravelSystem : ISimulationSystem
    {
        private readonly RunManager _runManager;
        private readonly EventJournal _eventJournal;
        private readonly List<ActiveTravel> _completedBuffer = new();

        public TravelSystem(RunManager runManager, EventJournal eventJournal)
        {
            _runManager = runManager;
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

            if (_eventJournal != null)
            {
                for (var i = 0; i < _completedBuffer.Count; i++)
                {
                    var travel = _completedBuffer[i];
                    _eventJournal.Record(
                        GameEventType.FleetArrived,
                        time,
                        travel.FleetId,
                        travel.ToNodeId);
                }
            }

            return completed;
        }
    }
}
