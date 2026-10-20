using ProjectStarwatch.Core.Data
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Overworld.UI;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// Shipyard rows stay in place while only build progress changes.
    /// </summary>
    public sealed class ShipyardSectionViewTests
    {
        [Test]
        public void Apply_KeepsButtonsWhenOnlyProgressChanges()
        {
            var classes = new VisualElement { name = "shipyard-classes" };
            var queue = new VisualElement { name = "shipyard-queue" };
            var view = Bind(classes, queue);
            var screen = Class("starter-screen");
            view.Apply(Shown(new[] { screen }, new[] { Order("starter-screen", 10f) }));
            var build = classes.Q<Button>();
            var cancel = queue.Q<Button>();
            var fill = queue.Q<VisualElement>("shipyard-progress-fill");

            view.Apply(Shown(new[] { screen }, new[] { Order("starter-screen", 5f) }));

            Assert.That(classes.Q<Button>(), Is.SameAs(build));
            Assert.That(queue.Q<Button>(), Is.SameAs(cancel));
            Assert.That(fill.style.width.value.value, Is.EqualTo(50f).Within(0.01f));
        }

        [Test]
        public void Apply_RebuildsTheQueueWhenAnOrderChanges()
        {
            var classes = new VisualElement { name = "shipyard-classes" };
            var queue = new VisualElement { name = "shipyard-queue" };
            var view = Bind(classes, queue);
            var screen = Class("starter-screen");
            view.Apply(Shown(new[] { screen }, new[] { Order("starter-screen", 10f) }));
            var build = classes.Q<Button>();
            var cancel = queue.Q<Button>();

            view.Apply(Shown(new[] { screen }, new[] { Order("line-ship", 10f) }));

            Assert.That(classes.Q<Button>(), Is.SameAs(build));
            Assert.That(queue.Q<Button>(), Is.Not.SameAs(cancel));
        }

        private static ShipyardSectionView Bind(VisualElement classes, VisualElement queue)
        {
            var root = new VisualElement { name = "shipyard-section" };
            root.Add(classes);
            root.Add(queue);
            var view = new ShipyardSectionView();
            view.Bind(root);
            return view;
        }

        private static ShipyardModel Shown(ShipyardClassRow[] classes, ShipyardQueueRow[] queue)
        {
            return new ShipyardModel(true, classes, queue);
        }

        private static ShipyardClassRow Class(string id)
        {
            return new ShipyardClassRow(
                ContentId.From(id),
                id,
                10f,
                new[] { new ResourceAmount(ResourceKind.Industry, 10f) },
                false,
                default,
                string.Empty);
        }

        private static ShipyardQueueRow Order(string id, float secondsRemaining)
        {
            return new ShipyardQueueRow(0, ContentId.From(id), id, secondsRemaining, 10f, true);
        }
    }
}
