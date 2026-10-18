using ProjectStarwatch.Core.Commands;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Simulation;

namespace ProjectStarwatch.Overworld.Services
{
    /// <summary>
    /// Reveals a map node when a fleet is there. Planets stay unscanned until a manual scan.
    /// </summary>
    internal sealed class ExplorationService
    {
        private readonly ArchiveService _archiveService;
        private readonly RouteService _routeService;
        private readonly DomainEventBus _events;
        private readonly SimulationClock _clock;

        public ExplorationService(
            ArchiveService archiveService,
            RouteService routeService,
            DomainEventBus events,
            SimulationClock clock)
        {
            _archiveService = archiveService;
            _routeService = routeService;
            _events = events ?? throw new System.ArgumentNullException(nameof(events));
            _clock = clock;
        }

        /// <summary>
        /// Explores a node the observer's fleet has entered.
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
                    "A fleet must be at the node to explore it.");
            }

            _archiveService.TryRecordExploredNode(run.Archive, observerId, nodeId);
            RevealIncidentRoutes(run, observerId, nodeId);

            _events.Publish(new MapKnowledgeChangedEvent(
                _clock != null ? _clock.CurrentTime : GameTime.Zero,
                observerId,
                nodeId,
                MapKnowledgeKind.NodeExplored));

            return CommandResult.Ok();
        }

        /// <summary>
        /// Returns whether a fleet is at this unexplored node.
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

            return run.Timeline.HasFleetAt(nodeId);
        }

        private void RevealIncidentRoutes(RunState run, ObserverId observerId, ContentId nodeId)
        {
            var neighbors = _routeService.GetNeighbors(run.Universe.MapGraph, nodeId);
            for (var i = 0; i < neighbors.Count; i++)
            {
                var key = RouteService.MakeRouteKey(nodeId, neighbors[i]);
                _archiveService.TryRecordKnownRoute(run.Archive, observerId, key);
            }
        }
    }
}
