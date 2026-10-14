using EndlessHeros.Core.Commands;
using EndlessHeros.Core.Events;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Run.Archive;
using EndlessHeros.Core.Simulation;

namespace EndlessHeros.Overworld.Services
{
    /// <summary>
    /// Reveals map nodes into archive knowledge when adjacency / presence rules allow.
    /// </summary>
    public sealed class ExplorationService
    {
        private readonly ArchiveService _archiveService;
        private readonly RouteService _routeService;
        private readonly EventJournal _eventJournal;
        private readonly SimulationClock _clock;

        public ExplorationService(
            ArchiveService archiveService,
            RouteService routeService,
            EventJournal eventJournal,
            SimulationClock clock)
        {
            _archiveService = archiveService;
            _routeService = routeService;
            _eventJournal = eventJournal;
            _clock = clock;
        }

        /// <summary>
        /// Explores a node for the observer when it exists and is reachable from known space.
        /// </summary>
        public CommandResult Explore(RunState run, ObserverId observerId, ContentId nodeId)
        {
            if (run == null)
            {
                return CommandResult.Fail(CommandErrorCode.InvalidArgument, "Run is required.");
            }

            if (nodeId.IsEmpty)
            {
                return CommandResult.Fail(CommandErrorCode.InvalidArgument, "Node id is required.");
            }

            var graph = run.Universe.MapGraph;
            if (!graph.TryGetNode(nodeId, out _))
            {
                return CommandResult.Fail(CommandErrorCode.InvalidArgument, "Unknown map node.");
            }

            if (!run.Archive.TryGetMapKnowledge(observerId, out var knowledge))
            {
                return CommandResult.Fail(CommandErrorCode.NotAllowed, "Observer has no map knowledge.");
            }

            if (knowledge.HasExploredNode(nodeId))
            {
                return CommandResult.Ok();
            }

            if (!CanExplore(run, observerId, nodeId))
            {
                return CommandResult.Fail(
                    CommandErrorCode.PrerequisiteNotMet,
                    "Node is not adjacent to explored space or a present fleet.");
            }

            _archiveService.TryRecordExploredNode(run.Archive, observerId, nodeId);
            RevealRoutesToExploredNeighbors(run, observerId, knowledge, nodeId);

            _eventJournal?.Record(
                GameEventType.NodeExplored,
                _clock != null ? _clock.CurrentTime : GameTime.Zero,
                EntityId.None,
                nodeId);

            return CommandResult.Ok();
        }

        /// <summary>
        /// Returns whether the observer may explore the node from current knowledge/presence.
        /// </summary>
        public bool CanExplore(RunState run, ObserverId observerId, ContentId nodeId)
        {
            if (run == null
                || nodeId.IsEmpty
                || !run.Universe.MapGraph.TryGetNode(nodeId, out _)
                || !run.Archive.TryGetMapKnowledge(observerId, out var knowledge)
                || knowledge.HasExploredNode(nodeId))
            {
                return false;
            }

            for (var i = 0; i < run.Timeline.Fleets.Count; i++)
            {
                if (run.Timeline.Fleets[i].LocationSystemId == nodeId)
                {
                    return true;
                }
            }

            var neighbors = _routeService.GetNeighbors(run.Universe.MapGraph, nodeId);
            for (var i = 0; i < neighbors.Count; i++)
            {
                if (knowledge.HasExploredNode(neighbors[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private void RevealRoutesToExploredNeighbors(
            RunState run,
            ObserverId observerId,
            MapKnowledge knowledge,
            ContentId nodeId)
        {
            var neighbors = _routeService.GetNeighbors(run.Universe.MapGraph, nodeId);
            for (var i = 0; i < neighbors.Count; i++)
            {
                var neighbor = neighbors[i];
                if (!knowledge.HasExploredNode(neighbor))
                {
                    continue;
                }

                var key = RouteService.MakeRouteKey(nodeId, neighbor);
                _archiveService.TryRecordKnownRoute(run.Archive, observerId, key);
            }
        }
    }
}
