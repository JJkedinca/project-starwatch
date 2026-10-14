using System.Collections.Generic;
using EndlessHeros.Core.Identifiers;
using UnityEngine;

namespace EndlessHeros.Core.Data
{
    /// <summary>
    /// Builds the deterministic authored-vertical-slice scenario for Phase 1.
    /// TODO: Prefer loading a saved ScenarioDefinition asset for production once
    /// content tooling exists; keep this factory for EditMode defaults/tests.
    /// </summary>
    public static class VerticalSliceScenarioFactory
    {
        public const string ScenarioId = "authored-vertical-slice";

        /// <summary>
        /// Creates a configured ScenarioDefinition with 5 systems, 2 non-system
        /// nodes, connected routes, and a chokepoint corridor.
        /// </summary>
        public static ScenarioDefinition Create()
        {
            var home = ContentId.From("home-system");
            var forge = ContentId.From("forge-system");
            var choke = ContentId.From("choke-system");
            var frontier = ContentId.From("frontier-system");
            var relic = ContentId.From("relic-system");
            var belt = ContentId.From("asteroid-belt");
            var anomaly = ContentId.From("anomaly-rift");

            var homePlanet = ContentId.From("home-prime");
            var homeMoon = ContentId.From("home-moon");
            var forgePlanet = ContentId.From("forge-world");
            var forgeBelt = ContentId.From("forge-inner-belt");
            var frontierPlanet = ContentId.From("frontier-world");
            var relicPlanet = ContentId.From("relic-world");
            var relicOutpost = ContentId.From("relic-outpost");

            List<MapNodeDefinition> nodes = new()
            {
                new MapNodeDefinition(home, MapNodeKind.System, new Vector2(0f, 0f), new[] { homePlanet, homeMoon }),
                new MapNodeDefinition(forge, MapNodeKind.System, new Vector2(2f, 1f), new[] { forgePlanet, forgeBelt }),
                new MapNodeDefinition(choke, MapNodeKind.System, new Vector2(4f, 0f), System.Array.Empty<ContentId>()),
                new MapNodeDefinition(frontier, MapNodeKind.System, new Vector2(6f, 0f), new[] { frontierPlanet }),
                new MapNodeDefinition(relic, MapNodeKind.System, new Vector2(4f, 2f), new[] { relicPlanet, relicOutpost }),
                new MapNodeDefinition(belt, MapNodeKind.NonSystem, new Vector2(2f, -1f)),
                new MapNodeDefinition(anomaly, MapNodeKind.NonSystem, new Vector2(5f, 1f)),
            };

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

            List<PlanetDefinition> planets = new()
            {
                new PlanetDefinition(homePlanet, home, "Home Prime", "Temperate starter world."),
                new PlanetDefinition(homeMoon, home, "Home Moon", "Barren moon with sensor coverage."),
                new PlanetDefinition(forgePlanet, forge, "Forge World", "Industrial crust with rich metals."),
                new PlanetDefinition(forgeBelt, forge, "Inner Belt", "Dense ore field near the forge star."),
                new PlanetDefinition(frontierPlanet, frontier, "Frontier World", "Sparse but expandable rim colony site."),
                new PlanetDefinition(relicPlanet, relic, "Relic World", "Ruined surface with scan-worthy anomalies."),
                new PlanetDefinition(relicOutpost, relic, "Relic Outpost", "Derelict station in high orbit."),
            };

            var startingEmpire = new StartingEmpireDefinition(
                home,
                ContentId.From("starter-screen"),
                starterShipCount: 3);

            var definition = ScriptableObject.CreateInstance<ScenarioDefinition>();
            definition.Configure(ScenarioId, startingEmpire, nodes, routes, planets);
            return definition;
        }
    }
}
