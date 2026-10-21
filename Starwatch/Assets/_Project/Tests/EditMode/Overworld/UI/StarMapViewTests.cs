using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Overworld;
using ProjectStarwatch.Overworld.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using EntityId = ProjectStarwatch.Core.Identifiers.EntityId;

namespace ProjectStarwatch.Tests.EditMode.Overworld.UI
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
            // Route layer + fleet-marker layer + tooltip + two node elements.
            Assert.That(surface.childCount, Is.EqualTo(5));
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
        public void SetFleetMarkers_CreatesOneElementPerMarker()
        {
            var surface = new VisualElement();
            var map = new StarMapView();
            map.Bind(surface);
            map.Rebuild(new[]
            {
                new StarMapNodeModel(ContentId.From("home-system"), MapNodeKind.System, Vector2.zero, default),
            });

            map.SetFleetMarkers(new[]
            {
                new FleetMarkerModel(
                    FleetMarkerKind.Parked,
                    EntityId.None,
                    ContentId.From("home-system"),
                    default,
                    Vector2.zero,
                    Vector2.zero,
                    count: 2,
                    isSelected: true,
                    remainingSeconds: 0f,
                    totalSeconds: 0f),
            });

            Assert.That(map.FleetMarkerCount, Is.EqualTo(1));
        }
    }
}
