using ProjectStarwatch.Core.Commands;
using ProjectStarwatch.Core.Content;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Simulation;

namespace ProjectStarwatch.Overworld.Services
{
    /// <summary>
    /// Records planet scan details into archive map knowledge.
    /// </summary>
    internal sealed class SensorService
    {
        private readonly ArchiveService _archiveService;
        private readonly ObjectDatabaseSO _objectDatabase;
        private readonly DomainEventBus _events;
        private readonly SimulationClock _clock;

        public SensorService(
            ArchiveService archiveService,
            ObjectDatabaseSO objectDatabase,
            DomainEventBus events,
            SimulationClock clock)
        {
            _archiveService = archiveService;
            _objectDatabase = objectDatabase;
            _events = events ?? throw new System.ArgumentNullException(nameof(events));
            _clock = clock;
        }

        /// <summary>
        /// Scans a planet when its host system is already explored for the observer.
        /// </summary>
        public CommandResult ScanPlanet(RunState run, ObserverId observerId, ContentId planetId)
        {
            if (run == null)
            {
                return CommandResult.Fail(CommandErrorCode.InvalidArgument, "Run is required.");
            }

            if (planetId.IsEmpty)
            {
                return CommandResult.Fail(CommandErrorCode.InvalidArgument, "Planet id is required.");
            }

            if (!run.Universe.MapGraph.TryGetNodeForPlanet(planetId, out var hostNode))
            {
                return CommandResult.Fail(CommandErrorCode.InvalidArgument, "Planet is not part of this universe.");
            }

            if (_objectDatabase == null || !_objectDatabase.TryGetPlanet(planetId.Value, out var planet))
            {
                return CommandResult.Fail(CommandErrorCode.InvalidArgument, "Unknown planet.");
            }

            if (planet.SystemId != hostNode.Id)
            {
                return CommandResult.Fail(CommandErrorCode.InvalidArgument, "Planet host does not match universe map.");
            }

            if (!run.Archive.TryGetMapKnowledge(observerId, out var knowledge))
            {
                return CommandResult.Fail(CommandErrorCode.NotAllowed, "Observer has no map knowledge.");
            }

            if (!knowledge.HasExploredNode(hostNode.Id))
            {
                return CommandResult.Fail(
                    CommandErrorCode.PrerequisiteNotMet,
                    "Host system must be explored before scanning.");
            }

            if (knowledge.HasScannedPlanet(planetId))
            {
                return CommandResult.Ok();
            }

            _archiveService.TryRecordScannedPlanet(run.Archive, observerId, planetId);
            _events.Publish(new MapKnowledgeChangedEvent(
                _clock != null ? _clock.CurrentTime : GameTime.Zero,
                observerId,
                planetId,
                MapKnowledgeKind.PlanetScanned));

            return CommandResult.Ok();
        }
    }
}
