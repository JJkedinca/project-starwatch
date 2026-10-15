using EndlessHeros.Core.Content;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run;
using EndlessHeros.Overworld.UI;
using EndlessHeros.Tests.EditMode.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace EndlessHeros.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// EditMode coverage for overworld map navigation: map, system, planet, scan, back.
    /// </summary>
    public sealed class OverworldMapPresenterTests
    {
        [Test]
        public void Navigation_MapToSystemToPlanetScanAndBack()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(31));
            var root = BuildTree();
            var host = new GameObject("overworld-map-presenter-test");
            try
            {
                var bridge = host.AddComponent<OverworldViewBridge>();
                bridge.Bind(root);
                var presenter = new OverworldMapPresenter(
                    bridge,
                    stack.Manager,
                    stack.Sensor,
                    stack.Travel,
                    new OverworldMapQuery(
                        stack.Visibility,
                        stack.Travel,
                        RunTestFixture.CreateObjectDatabase()),
                    stack.Events);
                presenter.Start();

                Assert.That(bridge.StarSystemView.IsVisible, Is.False);
                Assert.That(bridge.StarMap.RouteCount, Is.GreaterThan(0));
                Assert.That(DisplayOf("map-surface", root), Is.EqualTo(DisplayStyle.Flex));

                bridge.NotifyNodeSelected(ContentId.From("home-system"));

                Assert.That(bridge.StarSystemView.IsVisible, Is.True);
                Assert.That(bridge.StarSystemView.HasPlanet(ContentId.From("home-prime")), Is.True);
                Assert.That(bridge.StarSystemView.HasPlanet(ContentId.From("home-moon")), Is.True);
                Assert.That(DisplayOf("map-surface", root), Is.EqualTo(DisplayStyle.None));
                Assert.That(DisplayOf("planet-detail", root), Is.EqualTo(DisplayStyle.None));

                bridge.NotifyStarSystemBackRequested();

                Assert.That(bridge.StarSystemView.IsVisible, Is.False);
                Assert.That(DisplayOf("map-surface", root), Is.EqualTo(DisplayStyle.Flex));

                var forge = ContentId.From("forge-system");
                bridge.NotifyNodeSelected(forge);

                Assert.That(bridge.StarSystemView.IsVisible, Is.False);
                Assert.That(root.Q<Button>("explore-button"), Is.Null);
                Assert.That(root.Q<Button>("travel-button").enabledSelf, Is.True);

                bridge.NotifyTravelRequested();
                stack.Clock.SetPaused(false);
                var ticks = 0;
                while (run.Timeline.IsFleetTraveling(run.Timeline.Fleets[0].Id) && ticks < 200)
                {
                    stack.Loop.Pump(1f / 30f);
                    ticks++;
                }

                var forgeWorld = ContentId.From("forge-world");
                Assert.That(run.Archive.PlayerMapKnowledge.HasExploredNode(forge), Is.True);
                Assert.That(run.Archive.PlayerMapKnowledge.HasScannedPlanet(forgeWorld), Is.False);

                bridge.NotifyNodeSelected(forge);

                Assert.That(bridge.StarSystemView.IsVisible, Is.True);
                Assert.That(bridge.StarSystemView.HasPlanet(forgeWorld), Is.True);
                Assert.That(bridge.StarSystemView.HasPlanet(ContentId.From("forge-inner-belt")), Is.True);
                Assert.That(run.Archive.PlayerMapKnowledge.HasExploredNode(forge), Is.True);

                bridge.NotifyPlanetSelected(forgeWorld);

                Assert.That(root.Q<Button>("scan-button").enabledSelf, Is.True);
                Assert.That(DisplayOf("planet-detail", root), Is.EqualTo(DisplayStyle.Flex));
                Assert.That(
                    root.Q<VisualElement>("planet-forge-world").ClassListContains("planet-icon--selected"),
                    Is.True);

                bridge.NotifyScanPlanetRequested(forgeWorld);

                Assert.That(run.Archive.PlayerMapKnowledge.HasScannedPlanet(forgeWorld), Is.True);
                Assert.That(root.Q<Button>("scan-button").enabledSelf, Is.False);
                Assert.That(bridge.StarSystemView.IsVisible, Is.True);
                Assert.That(
                    root.Q<VisualElement>("planet-forge-world").Q<Button>().ClassListContains("planet-icon__glyph--scanned"),
                    Is.True);

                bridge.NotifyStarSystemBackRequested();

                Assert.That(bridge.StarSystemView.IsVisible, Is.False);
                Assert.That(DisplayOf("map-surface", root), Is.EqualTo(DisplayStyle.Flex));
                Assert.That(DisplayOf("planet-detail", root), Is.EqualTo(DisplayStyle.None));
            }
            finally
            {
                Object.DestroyImmediate(host);
            }
        }

        [Test]
        public void Navigation_EntersExploredLocationWithNoPlanets()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(33));
            var root = BuildTree();
            var host = new GameObject("overworld-empty-system-test");
            try
            {
                var bridge = host.AddComponent<OverworldViewBridge>();
                bridge.Bind(root);
                var presenter = new OverworldMapPresenter(
                    bridge,
                    stack.Manager,
                    stack.Sensor,
                    stack.Travel,
                    new OverworldMapQuery(
                        stack.Visibility,
                        stack.Travel,
                        RunTestFixture.CreateObjectDatabase()),
                    stack.Events);
                presenter.Start();

                var belt = ContentId.From("asteroid-belt");
                bridge.NotifyNodeSelected(belt);
                Assert.That(bridge.StarSystemView.IsVisible, Is.False);

                run.Timeline.TrySetFleetLocation(run.Timeline.Fleets[0].Id, belt);
                stack.Exploration.Explore(run, ObserverId.Player, belt);
                bridge.NotifyNodeSelected(belt);

                Assert.That(bridge.StarSystemView.IsVisible, Is.True);
                Assert.That(bridge.StarSystemView.PlanetCount, Is.EqualTo(0));
                Assert.That(root.Q<Label>("status-label").text, Is.EqualTo("No planets in this system."));

                presenter.Dispose();
                presenter.Start();

                Assert.That(root.Q<Label>("status-label").text, Is.EqualTo("No planets in this system."));
            }
            finally
            {
                Object.DestroyImmediate(host);
            }
        }

        [Test]
        public void Travel_MovesFleetMarkerToExploredNeighbor()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(32));
            var root = BuildTree();
            var host = new GameObject("overworld-fleet-travel-test");
            try
            {
                var bridge = host.AddComponent<OverworldViewBridge>();
                bridge.Bind(root);
                var presenter = new OverworldMapPresenter(
                    bridge,
                    stack.Manager,
                    stack.Sensor,
                    stack.Travel,
                    new OverworldMapQuery(
                        stack.Visibility,
                        stack.Travel,
                        RunTestFixture.CreateObjectDatabase()),
                    stack.Events);
                presenter.Start();

                var home = ContentId.From("home-system");
                var forge = ContentId.From("forge-system");
                Assert.That(FleetMarkerVisible(root, home), Is.True);
                Assert.That(FleetMarkerTraveling(root, home), Is.False);
                Assert.That(FleetMarkerVisible(root, forge), Is.False);
                var routesBeforeTravel = bridge.StarMap.RouteCount;

                bridge.NotifyNodeSelected(forge);
                Assert.That(root.Q<Button>("travel-button").enabledSelf, Is.True);
                Assert.That(root.Q<Button>("explore-button"), Is.Null);
                Assert.That(
                    root.Q<Button>("node-forge-system").ClassListContains("map-node--explorable"),
                    Is.True);
                Assert.That(run.Archive.PlayerMapKnowledge.HasExploredNode(forge), Is.False);

                presenter.Dispose();
                presenter.Start();

                bridge.NotifyTravelRequested();

                Assert.That(run.Timeline.IsFleetTraveling(run.Timeline.Fleets[0].Id), Is.True);
                Assert.That(root.Q<Label>("status-label").text, Is.EqualTo("Fleet is traveling."));
                Assert.That(run.Timeline.Fleets[0].LocationSystemId, Is.EqualTo(home));
                Assert.That(FleetMarkerVisible(root, home), Is.True);
                Assert.That(FleetMarkerTraveling(root, home), Is.True);
                Assert.That(FleetMarkerVisible(root, forge), Is.False);
                Assert.That(root.Q<Button>("travel-button").enabledSelf, Is.False);
                Assert.That(bridge.StarMap.RouteCount, Is.EqualTo(routesBeforeTravel));
                Assert.That(
                    root.Q<Button>("node-forge-system").ClassListContains("map-node--explorable"),
                    Is.True);
                Assert.That(root.Q<Button>("node-forge-system").text, Is.EqualTo("?"));
                Assert.That(
                    root.Q<Button>("node-asteroid-belt").ClassListContains("map-node--explorable"),
                    Is.True);
                Assert.That(root.Q<Button>("node-asteroid-belt").text, Is.EqualTo("?"));

                stack.Clock.SetPaused(false);
                var ticks = 0;
                while (run.Timeline.IsFleetTraveling(run.Timeline.Fleets[0].Id) && ticks < 200)
                {
                    stack.Loop.Pump(1f / 30f);
                    ticks++;
                }

                Assert.That(run.Timeline.Fleets[0].LocationSystemId, Is.EqualTo(forge));
                Assert.That(root.Q<Label>("status-label").text, Is.EqualTo("Fleet arrived."));
                Assert.That(FleetMarkerVisible(root, home), Is.False);
                Assert.That(FleetMarkerTraveling(root, home), Is.False);
                Assert.That(FleetMarkerVisible(root, forge), Is.True);
                Assert.That(FleetMarkerTraveling(root, forge), Is.False);
                Assert.That(bridge.StarMap.RouteCount, Is.EqualTo(routesBeforeTravel + 1));
                Assert.That(
                    root.Q<Button>("node-forge-system").ClassListContains("map-node--explorable"),
                    Is.False);
                Assert.That(run.Archive.PlayerMapKnowledge.HasExploredNode(forge), Is.True);
                Assert.That(
                    run.Archive.PlayerMapKnowledge.HasScannedPlanet(ContentId.From("forge-world")),
                    Is.False);
            }
            finally
            {
                Object.DestroyImmediate(host);
            }
        }

        private static bool FleetMarkerVisible(VisualElement root, ContentId nodeId)
        {
            return FleetMarker(root, nodeId).ClassListContains("map-node__fleet--visible");
        }

        private static bool FleetMarkerTraveling(VisualElement root, ContentId nodeId)
        {
            return FleetMarker(root, nodeId).ClassListContains("map-node__fleet--traveling");
        }

        private static VisualElement FleetMarker(VisualElement root, ContentId nodeId)
        {
            return root.Q<Button>("node-" + nodeId.Value).Q<VisualElement>("fleet-marker");
        }

        private static DisplayStyle DisplayOf(string name, VisualElement root)
        {
            return root.Q<VisualElement>(name).style.display.value;
        }

        private static VisualElement BuildTree()
        {
            var root = new VisualElement { name = "root" };
            root.Add(new Label { name = "title", text = "Star Map" });
            root.Add(new Label { name = "status-label" });

            var mapSurface = new VisualElement { name = "map-surface" };
            var starSystem = new VisualElement { name = "star-system" };
            starSystem.Add(new Button { name = "star-system-back", text = "Back to map" });
            starSystem.Add(new Label { name = "star-system-title" });
            starSystem.Add(new VisualElement { name = "planet-strip" });
            root.Add(mapSurface);
            root.Add(starSystem);

            var inspection = new VisualElement { name = "inspection-panel" };
            var starSystemInspection = new VisualElement { name = "star-system-inspection" };
            starSystemInspection.Add(new Label { name = "inspection-title", text = "Select a star system" });
            starSystemInspection.Add(new Label { name = "inspection-summary" });
            starSystemInspection.Add(new Button { name = "travel-button", text = "Travel" });
            inspection.Add(starSystemInspection);
            var detail = new VisualElement { name = "planet-detail" };
            detail.Add(new Label { name = "planet-detail-name" });
            detail.Add(new Label { name = "planet-detail-summary" });
            detail.Add(new Button { name = "scan-button", text = "Scan" });
            inspection.Add(detail);
            root.Add(inspection);
            return root;
        }
    }
}
