using System;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Overworld.UI;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// Split counts stay put when a fleet refresh does not change the class list.
    /// </summary>
    public sealed class FleetPanelViewTests
    {
        [Test]
        public void Apply_KeepsASplitCountWhenTheClassListIsUnchanged()
        {
            var stacks = new VisualElement { name = "fleet-panel-stacks" };
            var view = Bind(stacks);
            view.Apply(Open(speed: 1f, count: 3));
            var field = stacks.Q<IntegerField>();
            field.value = 2;

            view.Apply(Open(speed: 1.25f, count: 4));

            Assert.That(stacks.Q<IntegerField>(), Is.SameAs(field));
            Assert.That(field.value, Is.EqualTo(2));
        }

        [Test]
        public void Apply_ResetsTheSplitCountWhenTheClassChanges()
        {
            var stacks = new VisualElement { name = "fleet-panel-stacks" };
            var view = Bind(stacks);
            view.Apply(Open(speed: 1f, count: 3));
            var field = stacks.Q<IntegerField>();
            field.value = 2;

            view.Apply(Open(speed: 1f, count: 3, classId: "line-ship"));

            Assert.That(stacks.Q<IntegerField>(), Is.Not.SameAs(field));
            Assert.That(stacks.Q<IntegerField>().value, Is.EqualTo(1));
        }

        private static FleetPanelView Bind(VisualElement stacks)
        {
            var root = new VisualElement { name = "fleet-panel" };
            root.Add(new Label { name = "fleet-panel-title" });
            root.Add(new Label { name = "fleet-panel-status" });
            root.Add(new Label { name = "fleet-panel-speed" });
            root.Add(stacks);
            root.Add(new VisualElement { name = "fleet-panel-merges" });
            root.Add(new Button { name = "fleet-panel-close" });
            var view = new FleetPanelView();
            view.Bind(root);
            return view;
        }

        private static FleetPanelModel Open(float speed, int count, string classId = "starter-screen")
        {
            return new FleetPanelModel(
                hasFleet: true,
                isOpen: true,
                new EntityId(1),
                new FleetRouteModel(ContentId.From("home-system"), "Home", default, string.Empty, 0f, false),
                speed,
                true,
                new[] { new FleetStackRow(ContentId.From(classId), count, "Screen") },
                Array.Empty<FleetMergeCandidate>());
        }
    }
}
