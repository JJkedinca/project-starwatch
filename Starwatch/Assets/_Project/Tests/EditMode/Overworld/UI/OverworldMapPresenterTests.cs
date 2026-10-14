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
                    stack.Visibility,
                    stack.Exploration,
                    stack.Sensor,
                    RunTestFixture.CreateObjectDatabase());
                presenter.Start();

                Assert.That(bridge.SystemView.IsVisible, Is.False);
                Assert.That(bridge.StarMap.RouteCount, Is.GreaterThan(0));
                Assert.That(DisplayOf("map-surface", root), Is.EqualTo(DisplayStyle.Flex));

                bridge.NotifyNodeSelected(ContentId.From("home-system"));

                Assert.That(bridge.SystemView.IsVisible, Is.True);
                Assert.That(bridge.SystemView.HasPlanet(ContentId.From("home-prime")), Is.True);
                Assert.That(bridge.SystemView.HasPlanet(ContentId.From("home-moon")), Is.True);
                Assert.That(DisplayOf("map-surface", root), Is.EqualTo(DisplayStyle.None));
                Assert.That(DisplayOf("planet-detail", root), Is.EqualTo(DisplayStyle.None));

                bridge.NotifySystemBackRequested();

                Assert.That(bridge.SystemView.IsVisible, Is.False);
                Assert.That(DisplayOf("map-surface", root), Is.EqualTo(DisplayStyle.Flex));

                var forge = ContentId.From("forge-system");
                bridge.NotifyNodeSelected(forge);

                Assert.That(bridge.SystemView.IsVisible, Is.False);
                Assert.That(root.Q<Button>("explore-button").enabledSelf, Is.True);

                bridge.NotifyExploreRequested();

                var forgeWorld = ContentId.From("forge-world");
                Assert.That(bridge.SystemView.IsVisible, Is.True);
                Assert.That(bridge.SystemView.HasPlanet(forgeWorld), Is.True);
                Assert.That(bridge.SystemView.HasPlanet(ContentId.From("forge-inner-belt")), Is.True);
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
                Assert.That(bridge.SystemView.IsVisible, Is.True);
                Assert.That(
                    root.Q<VisualElement>("planet-forge-world").Q<Button>().ClassListContains("planet-icon__glyph--scanned"),
                    Is.True);

                bridge.NotifySystemBackRequested();

                Assert.That(bridge.SystemView.IsVisible, Is.False);
                Assert.That(DisplayOf("map-surface", root), Is.EqualTo(DisplayStyle.Flex));
                Assert.That(DisplayOf("planet-detail", root), Is.EqualTo(DisplayStyle.None));
            }
            finally
            {
                Object.DestroyImmediate(host);
            }
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
            var systemView = new VisualElement { name = "system-view" };
            systemView.Add(new Button { name = "system-back-button", text = "Back to map" });
            systemView.Add(new Label { name = "system-view-title" });
            systemView.Add(new VisualElement { name = "planet-strip" });
            root.Add(mapSurface);
            root.Add(systemView);

            var inspection = new VisualElement { name = "inspection-panel" };
            inspection.Add(new Label { name = "inspection-title", text = "Select a node" });
            inspection.Add(new Label { name = "inspection-summary" });
            inspection.Add(new Button { name = "explore-button", text = "Explore" });
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
