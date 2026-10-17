using System.Collections.Generic;
using EndlessHeros.Core.Identifiers;
using UnityEngine;

namespace EndlessHeros.Core.Data
{
    /// <summary>
    /// Builds the deterministic authored-vertical-slice scenario for Phase 1.
    /// Star systems come from StarSystemDatabaseSO and planets from PlanetDatabaseSO.
    /// Routes and this faction's opening stockpile stay here until the scenario itself is an asset.
    /// </summary>
    public static class VerticalSliceScenarioFactory
    {
        public const string ScenarioId = "authored-vertical-slice";

        /// <summary>
        /// Creates a configured ScenarioDefinition with the authored map, connected
        /// routes, and a chokepoint corridor.
        /// </summary>
        public static ScenarioDefinition Create(
            StarSystemDatabaseSO starSystemDatabase,
            PlanetDatabaseSO planetDatabase)
        {
            var home = ContentId.From("home-system");
            var forge = ContentId.From("forge-system");
            var choke = ContentId.From("choke-system");
            var frontier = ContentId.From("frontier-system");
            var relic = ContentId.From("relic-system");
            var belt = ContentId.From("asteroid-belt");
            var anomaly = ContentId.From("anomaly-rift");

            List<MapRouteDefinition> routes = new()
            {
                new MapRouteDefinition(home, forge, 3f),
                new MapRouteDefinition(home, belt, 2f),
                new MapRouteDefinition(forge, choke, 4f),
                new MapRouteDefinition(belt, choke, 3f),
                new MapRouteDefinition(choke, frontier, 5f),
                new MapRouteDefinition(choke, anomaly, 2f),
                new MapRouteDefinition(anomaly, relic, 3f),
            };

            var startingEmpire = new StartingEmpireDefinition(
                home,
                ContentId.From("starter-screen"),
                starterShipCount: 3,
                new StartingResources(industry: 80, energy: 40));

            var planets = planetDatabase != null
                ? planetDatabase.Planets
                : System.Array.Empty<PlanetDefinition>();

            var definition = ScriptableObject.CreateInstance<ScenarioDefinition>();
            definition.Configure(
                ScenarioId,
                startingEmpire,
                BuildNodes(starSystemDatabase, planetDatabase),
                routes,
                planets);
            return definition;
        }

        private static List<MapNodeDefinition> BuildNodes(
            StarSystemDatabaseSO starSystemDatabase,
            PlanetDatabaseSO planetDatabase)
        {
            List<MapNodeDefinition> nodes = new();
            if (starSystemDatabase == null || starSystemDatabase.StarSystems == null)
            {
                return nodes;
            }

            var starSystems = starSystemDatabase.StarSystems;
            for (var i = 0; i < starSystems.Count; i++)
            {
                var starSystem = starSystems[i];
                if (starSystem == null || starSystem.Id.IsEmpty)
                {
                    continue;
                }

                var planetIds = starSystem.Kind == MapNodeKind.System
                    ? PlanetsInSystem(planetDatabase, starSystem.Id)
                    : System.Array.Empty<ContentId>();
                nodes.Add(new MapNodeDefinition(starSystem.Id, starSystem.Kind, starSystem.MapPosition, planetIds));
            }

            return nodes;
        }

        private static ContentId[] PlanetsInSystem(PlanetDatabaseSO planetDatabase, ContentId systemId)
        {
            if (planetDatabase == null || planetDatabase.Planets == null)
            {
                return System.Array.Empty<ContentId>();
            }

            List<ContentId> ids = new();
            var planets = planetDatabase.Planets;
            for (var i = 0; i < planets.Count; i++)
            {
                var planet = planets[i];
                if (planet != null && planet.SystemId == systemId)
                {
                    ids.Add(planet.Id);
                }
            }

            return ids.ToArray();
        }
    }
}
