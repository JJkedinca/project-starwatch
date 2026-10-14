using EndlessHeros.Core.Data;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run.Universe;
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
                new MapNode(ContentId.From("home-system"), MapNodeKind.System, Vector2.zero, null),
                new MapNode(ContentId.From("forge-system"), MapNodeKind.System, Vector2.one, null),
            });

            Assert.That(map.NodeCount, Is.EqualTo(2));
            Assert.That(map.HasNode(ContentId.From("home-system")), Is.True);
            // Route layer + two node elements.
            Assert.That(surface.childCount, Is.EqualTo(3));
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
                new MapNode(nodeId, MapNodeKind.System, Vector2.one, null),
            });

            map.SetNodeState(
                nodeId,
                new MapNodePresentationState(
                    MapNodeVisibility.Hidden,
                    isExplorable: false,
                    isSelected: false,
                    hasFleet: false,
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
                new MapNode(nodeId, MapNodeKind.System, Vector2.one, null),
            });

            map.SetNodeState(
                nodeId,
                new MapNodePresentationState(
                    MapNodeVisibility.Hidden,
                    isExplorable: true,
                    isSelected: false,
                    hasFleet: false,
                    revealedLabel: string.Empty));

            var button = surface.Q<Button>("node-forge-system");
            Assert.That(button, Is.Not.Null);
            Assert.That(button.ClassListContains("map-node--hidden"), Is.True);
            Assert.That(button.ClassListContains("map-node--explorable"), Is.True);
        }
    }
}
