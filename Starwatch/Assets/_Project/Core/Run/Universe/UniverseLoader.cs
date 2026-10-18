using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run.Archive;
using ProjectStarwatch.Core.Run.Timeline;
using UnityEngine;

namespace ProjectStarwatch.Core.Run.Universe
{
    /// <summary>
    /// Loads a ScenarioDefinition into UniverseState and seeds starter timeline/archive.
    /// </summary>
    internal sealed class UniverseLoader
    {
        private readonly TimelineFactory _timelineFactory;

        public UniverseLoader(TimelineFactory timelineFactory)
        {
            _timelineFactory = timelineFactory ?? throw new ArgumentNullException(nameof(timelineFactory));
        }

        /// <summary>
        /// Builds universe, archive, and timeline for a new run from authored content.
        /// </summary>
        public bool TryLoad(
            ScenarioDefinition definition,
            EntityIdFactory entityIds,
            out UniverseState universe,
            out RunArchive archive,
            out TimelineState timeline,
            StartingEmpireDefinition startingEmpireOverride = null)
        {
            universe = null;
            archive = null;
            timeline = null;

            if (definition == null || entityIds == null)
            {
                Debug.LogError("UniverseLoader.TryLoad requires a scenario definition and entity id factory.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(definition.ScenarioId))
            {
                Debug.LogError("UniverseLoader.TryLoad requires a scenario id.");
                return false;
            }

            if (!TryBuildGraph(definition, out var graph))
            {
                return false;
            }

            if (graph.NodeCount == 0 || !graph.IsFullyConnected())
            {
                Debug.LogError("UniverseLoader.TryLoad requires a connected non-empty map graph.");
                return false;
            }

            var startingEmpire = startingEmpireOverride
                ?? definition.StartingEmpire
                ?? StartingEmpireDefinition.CreateDefault();
            if (!graph.TryGetNode(startingEmpire.HomeSystemId, out var homeNode)
                || homeNode.Kind != MapNodeKind.System)
            {
                Debug.LogError(
                    "UniverseLoader.TryLoad starting home must be a MapNodeKind.System node on the map.");
                return false;
            }

            universe = new UniverseState(definition.ScenarioId, startingEmpire, graph);
            archive = new RunArchive();
            archive.PlayerMapKnowledge.TryAddExploredNode(startingEmpire.HomeSystemId);
            SeedKnownRoutes(graph, archive.PlayerMapKnowledge, startingEmpire.HomeSystemId);
            timeline = _timelineFactory.Create(universe, archive, entityIds);
            return true;
        }

        private static bool TryBuildGraph(ScenarioDefinition definition, out MapGraph graph)
        {
            graph = null;
            List<MapNode> nodes = new();
            if (definition.Nodes != null)
            {
                for (var i = 0; i < definition.Nodes.Count; i++)
                {
                    var def = definition.Nodes[i];
                    if (def == null)
                    {
                        Debug.LogError($"UniverseLoader.TryLoad map node at index {i} is null.");
                        return false;
                    }

                    if (def.Id.IsEmpty)
                    {
                        Debug.LogError($"UniverseLoader.TryLoad map node at index {i} has an empty id.");
                        return false;
                    }

                    nodes.Add(new MapNode(def.Id, def.Kind, def.MapPosition, def.PlanetIds));
                }
            }

            List<MapRoute> routes = new();
            if (definition.Routes != null)
            {
                for (var i = 0; i < definition.Routes.Count; i++)
                {
                    var def = definition.Routes[i];
                    if (def == null)
                    {
                        Debug.LogError($"UniverseLoader.TryLoad map route at index {i} is null.");
                        return false;
                    }

                    if (def.NodeAId.IsEmpty || def.NodeBId.IsEmpty)
                    {
                        Debug.LogError($"UniverseLoader.TryLoad map route at index {i} has an empty endpoint.");
                        return false;
                    }

                    if (def.TravelDurationSeconds < 0f || float.IsNaN(def.TravelDurationSeconds))
                    {
                        Debug.LogError(
                            $"UniverseLoader.TryLoad map route at index {i} has a negative or invalid duration.");
                        return false;
                    }

                    routes.Add(new MapRoute(def.NodeAId, def.NodeBId, def.TravelDurationSeconds));
                }
            }

            if (!MapGraph.TryCreate(nodes, routes, out graph, out var error))
            {
                Debug.LogError($"UniverseLoader.TryLoad rejected map graph: {error}");
                return false;
            }

            return true;
        }

        private static void SeedKnownRoutes(MapGraph graph, MapKnowledge knowledge, ContentId exploredNodeId)
        {
            var neighbors = graph.GetNeighbors(exploredNodeId);
            for (var i = 0; i < neighbors.Count; i++)
            {
                knowledge.TryAddKnownRoute(MapRoute.MakeKnowledgeKey(exploredNodeId, neighbors[i]));
            }
        }
    }
}
