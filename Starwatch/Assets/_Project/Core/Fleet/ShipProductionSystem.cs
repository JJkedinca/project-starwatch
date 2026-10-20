using System;
using ProjectStarwatch.Core.Content;
using ProjectStarwatch.Core.Events;
using EntityId = ProjectStarwatch.Core.Identifiers.EntityId;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Timeline;
using ProjectStarwatch.Core.Simulation;
using UnityEngine;

namespace ProjectStarwatch.Fleet
{
    /// <summary>
    /// Advances each colony's head build order on the simulation clock.
    /// A finished ship joins a fleet at that planet's system.
    /// </summary>
    internal sealed class ShipProductionSystem : ISimulationSystem
    {
        private readonly ObjectDatabaseSO _database;
        private readonly FleetService _fleetService;
        private readonly ShipProductionService _shipProductionService;
        private readonly DomainEventBus _events;
        private RunState _run;

        public ShipProductionSystem(
            ObjectDatabaseSO database,
            FleetService fleetService,
            ShipProductionService shipProductionService,
            DomainEventBus events)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _fleetService = fleetService ?? throw new ArgumentNullException(nameof(fleetService));
            _shipProductionService = shipProductionService ?? throw new ArgumentNullException(nameof(shipProductionService));
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public void OnRunChanged(RunState run)
        {
            _run = run;
        }

        public void OnTimelineReset()
        {
        }

        public void OnSimulationTick(GameTime time, float tickDeltaSeconds)
        {
            if (_run == null || tickDeltaSeconds <= 0f)
            {
                return;
            }

            var colonies = _run.Timeline.Colonies;
            for (var i = 0; i < colonies.Count; i++)
            {
                Advance(colonies[i], time, tickDeltaSeconds);
            }
        }

        private void Advance(ColonyState colony, GameTime time, float seconds)
        {
            while (seconds > 0f && colony.BuildQueue.Count > 0)
            {
                var head = colony.BuildQueue[0];
                var leftover = head.Consume(seconds);
                if (head.SecondsRemaining > 0f)
                {
                    return;
                }

                if (!Finish(colony, head, out var fleetId))
                {
                    if (!Drop(colony, head))
                    {
                        return;
                    }

                    seconds = leftover;
                    continue;
                }

                if (!colony.TryTakeBuild(0, out _))
                {
                    return;
                }

                // The order is gone before subscribers refresh, so the queue does not keep a finished 0s head.
                _events.Publish(new ShipConstructionCompletedEvent(time, colony.PlanetId, head.ClassId, fleetId));
                seconds = leftover;
            }
        }

        private bool Finish(ColonyState colony, ShipBuildOrder order, out EntityId fleetId)
        {
            fleetId = default;
            if (!_database.TryGetPlanet(colony.PlanetId.Value, out var planet) || planet == null || planet.SystemId.IsEmpty)
            {
                return false;
            }

            return _fleetService.AddBuiltShip(_run, planet.SystemId, order.ClassId, out fleetId);
        }

        private bool Drop(ColonyState colony, ShipBuildOrder order)
        {
            Debug.LogWarning(
                $"ShipProductionSystem dropped a finished {order.ClassId.Value} on '{colony.PlanetId.Value}' and refunded it because it could not be placed.");
            return _shipProductionService.Cancel(_run, colony.PlanetId, 0).Success;
        }
    }
}
