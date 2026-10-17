using EndlessHeros.Core.Data;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Overworld;
using EndlessHeros.Overworld.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace EndlessHeros.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// EditMode coverage for StarMapView node lifecycle and fog state.
    /// </summary>
    public sealed class StarMapViewTests
    {
        [Test]
        public void Rebuild_CreatesOneElementPerNode()
        {
            var surface = new VisualElement();
            var map = new StarMapView();
            map.Bind(surface);

            map.Rebuild(new[]
            {
                new StarMapNodeModel(ContentId.From("home-system"), MapNodeKind.System, Vector2.zero, default),
                new StarMapNodeModel(ContentId.From("forge-system"), MapNodeKind.System, Vector2.one, default),
            });

            Assert.That(map.NodeCount, Is.EqualTo(2));
            Assert.That(map.HasNode(ContentId.From("home-system")), Is.True);
            // Route layer + two node elements.
            Assert.That(surface.childCount, Is.EqualTo(3));
        }

        [Test]
        public void Rebuild_KeepsNodeElementsWhenTheSetIsUnchanged()
        {
            var surface = new VisualElement();
            var map = new StarMapView();
            map.Bind(surface);
            var nodes = new[]
            {
                new StarMapNodeModel(ContentId.From("home-system"), MapNodeKind.System, Vector2.zero, default),
                new StarMapNodeModel(ContentId.From("forge-system"), MapNodeKind.System, Vector2.one, default),
            };
            map.Rebuild(nodes);
            var home = surface.Q<Button>("node-home-system");

            map.Rebuild(nodes);

            Assert.That(surface.Q<Button>("node-home-system"), Is.SameAs(home));
            Assert.That(map.NodeCount, Is.EqualTo(2));
        }

        [Test]
        public void SetNodeState_MarksHiddenWhenFogged()
        {
            var surface = new VisualElement();
            var map = new StarMapView();
            map.Bind(surface);
            var nodeId = ContentId.From("forge-system");
            map.Rebuild(new[]
            {
                new StarMapNodeModel(nodeId, MapNodeKind.System, Vector2.one, default),
            });

            map.SetNodeState(
                nodeId,
                new MapNodePresentationState(
                    MapNodeVisibility.Hidden,
                    isExplorable: false,
                    isSelected: false,
                    hasFleet: false,
                    isFleetTraveling: false,
                    revealedLabel: string.Empty));

            var button = surface.Q<Button>("node-forge-system");
            Assert.That(button, Is.Not.Null);
            Assert.That(button.ClassListContains("map-node--hidden"), Is.True);
            Assert.That(button.ClassListContains("map-node--explorable"), Is.False);
        }

        [Test]
        public void SetNodeState_MarksExplorableWhenHiddenAndReachable()
        {
            var surface = new VisualElement();
            var map = new StarMapView();
            map.Bind(surface);
            var nodeId = ContentId.From("forge-system");
            map.Rebuild(new[]
            {
                new StarMapNodeModel(nodeId, MapNodeKind.System, Vector2.one, default),
            });

            map.SetNodeState(
                nodeId,
                new MapNodePresentationState(
                    MapNodeVisibility.Hidden,
                    isExplorable: true,
                    isSelected: false,
                    hasFleet: false,
                    isFleetTraveling: false,
                    revealedLabel: string.Empty));

            var button = surface.Q<Button>("node-forge-system");
            Assert.That(button, Is.Not.Null);
            Assert.That(button.ClassListContains("map-node--hidden"), Is.True);
            Assert.That(button.ClassListContains("map-node--explorable"), Is.True);
        }

        [Test]
        public void SetNodeState_ShowsOrbitWhenFleetIsInSystem()
        {
            var surface = new VisualElement();
            var map = new StarMapView();
            map.Bind(surface);
            var nodeId = ContentId.From("home-system");
            map.Rebuild(new[]
            {
                new StarMapNodeModel(nodeId, MapNodeKind.System, Vector2.zero, default),
            });

            var orbit = surface.Q<Button>("node-home-system").Q<VisualElement>("fleet-orbit");
            Assert.That(orbit.ClassListContains("map-node__orbit--visible"), Is.False);

            map.SetNodeState(
                nodeId,
                new MapNodePresentationState(
                    MapNodeVisibility.Present,
                    isExplorable: false,
                    isSelected: false,
                    hasFleet: true,
                    isFleetTraveling: false,
                    revealedLabel: "home-system"));

            Assert.That(orbit.ClassListContains("map-node__orbit--visible"), Is.True);
            Assert.That(orbit.ClassListContains("map-node__orbit--traveling"), Is.False);
            Assert.That(
                orbit.Q<VisualElement>("fleet-marker").ClassListContains("map-node__fleet--visible"),
                Is.True);
        }

        [Test]
        public void SetNodeState_MarksOrbitTravelingWhileFleetDeparts()
        {
            var surface = new VisualElement();
            var map = new StarMapView();
            map.Bind(surface);
            var nodeId = ContentId.From("home-system");
            map.Rebuild(new[]
            {
                new StarMapNodeModel(nodeId, MapNodeKind.System, Vector2.zero, default),
            });

            map.SetNodeState(
                nodeId,
                new MapNodePresentationState(
                    MapNodeVisibility.Present,
                    isExplorable: false,
                    isSelected: false,
                    hasFleet: true,
                    isFleetTraveling: true,
                    revealedLabel: "home-system"));

            var orbit = surface.Q<Button>("node-home-system").Q<VisualElement>("fleet-orbit");
            Assert.That(orbit.ClassListContains("map-node__orbit--visible"), Is.True);
            Assert.That(orbit.ClassListContains("map-node__orbit--traveling"), Is.True);
        }
    }
}
