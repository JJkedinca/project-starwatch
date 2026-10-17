using System;
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
    internal sealed class TravelSystem : ISimulationSystem
    {
        private readonly ExplorationService _exploration;
        private readonly DomainEventBus _events;
        private readonly List<ActiveTravel> _completedBuffer = new();
        private RunState _run;

        public TravelSystem(ExplorationService exploration, DomainEventBus events)
        {
            _exploration = exploration ?? throw new ArgumentNullException(nameof(exploration));
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public void OnRunChanged(RunState run)
        {
            _run = run;
        }

        public void OnTimelineReset()
        {
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
            if (_run == null)
            {
                return 0;
            }

            _completedBuffer.Clear();
            var completed = _run.Timeline.CompleteArrivals(time, _completedBuffer);

            for (var i = 0; i < _completedBuffer.Count; i++)
            {
                var travel = _completedBuffer[i];
                _events.Publish(new FleetTravelEvent(
                    time,
                    travel.FleetId,
                    travel.FromNodeId,
                    travel.ToNodeId,
                    FleetTravelKind.Arrived));
                _exploration.Explore(_run, ObserverId.Player, travel.ToNodeId);
            }

            return completed;
        }
    }
}
