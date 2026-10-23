using System.Collections.Generic;
using NUnit.Framework;
using ProjectStarwatch.Core.WorldInput;
using UnityEngine;

namespace ProjectStarwatch.Tests.EditMode.Core.WorldInput
{
    public sealed class PointerGestureTrackerTests
    {
        [Test]
        public void PressReleaseWithinThreshold_IsClick_PrimaryAndSecondary()
        {
            var listener = new FakeListener();
            var tracker = new PointerGestureTracker(listener, PointerGestureSettings.Default);

            tracker.ButtonPressed(PointerButton.Primary, Vector2.zero, overUi: false, time: 0f);
            tracker.ButtonReleased(PointerButton.Primary, new Vector2(2f, 0f));

            tracker.ButtonPressed(PointerButton.Secondary, Vector2.zero, overUi: false, time: 1f);
            tracker.ButtonReleased(PointerButton.Secondary, new Vector2(1f, 1f));

            Assert.That(listener.Clicks.Count, Is.EqualTo(2));
            Assert.That(listener.Clicks[0].Button, Is.EqualTo(PointerButton.Primary));
            Assert.That(listener.Clicks[1].Button, Is.EqualTo(PointerButton.Secondary));
            Assert.That(listener.DragStarts, Is.EqualTo(0));
        }

        [Test]
        public void PrimaryPressPastThreshold_DragsAndDoesNotClick()
        {
            var listener = new FakeListener();
            var tracker = new PointerGestureTracker(listener, new PointerGestureSettings(6f, 0.5f));

            tracker.ButtonPressed(PointerButton.Primary, Vector2.zero, overUi: false, time: 0f);
            tracker.PointerMoved(new Vector2(10f, 0f), overUi: false);
            tracker.PointerMoved(new Vector2(15f, 0f), overUi: false);
            tracker.ButtonReleased(PointerButton.Primary, new Vector2(15f, 0f));

            Assert.That(listener.DragStarts, Is.EqualTo(1));
            Assert.That(listener.DragDeltas.Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(listener.DragEnds, Is.EqualTo(1));
            Assert.That(listener.Clicks, Is.Empty);
        }

        [Test]
        public void SecondaryPressPastThreshold_DoesNotClick()
        {
            var listener = new FakeListener();
            var tracker = new PointerGestureTracker(listener, PointerGestureSettings.Default);

            tracker.ButtonPressed(PointerButton.Secondary, Vector2.zero, overUi: false, time: 0f);
            tracker.PointerMoved(new Vector2(20f, 0f), overUi: false);
            tracker.ButtonReleased(PointerButton.Secondary, new Vector2(20f, 0f));

            Assert.That(listener.Clicks, Is.Empty);
            Assert.That(listener.DragStarts, Is.EqualTo(0));
            Assert.That(listener.DragEnds, Is.EqualTo(0));
        }

        [Test]
        public void PressStartingOverUi_ProducesNothing()
        {
            var listener = new FakeListener();
            var tracker = new PointerGestureTracker(listener, PointerGestureSettings.Default);

            tracker.ButtonPressed(PointerButton.Primary, Vector2.zero, overUi: true, time: 0f);
            tracker.PointerMoved(new Vector2(20f, 0f), overUi: false);
            tracker.ButtonReleased(PointerButton.Primary, new Vector2(20f, 0f));

            Assert.That(listener.Clicks, Is.Empty);
            Assert.That(listener.DragStarts, Is.EqualTo(0));
            Assert.That(listener.LongPresses, Is.EqualTo(0));
        }

        [Test]
        public void LongPress_FiresOnceAndSuppressesClick()
        {
            var listener = new FakeListener();
            var tracker = new PointerGestureTracker(listener, new PointerGestureSettings(6f, 0.5f));

            tracker.ButtonPressed(PointerButton.Primary, Vector2.zero, overUi: false, time: 0f);
            tracker.Tick(0.6f);
            tracker.Tick(1.2f);
            tracker.ButtonReleased(PointerButton.Primary, Vector2.zero);

            Assert.That(listener.LongPresses, Is.EqualTo(1));
            Assert.That(listener.Clicks, Is.Empty);
        }

        [Test]
        public void LongPress_ThenMove_DoesNotStartDrag()
        {
            var listener = new FakeListener();
            var tracker = new PointerGestureTracker(listener, new PointerGestureSettings(6f, 0.5f));

            tracker.ButtonPressed(PointerButton.Primary, Vector2.zero, overUi: false, time: 0f);
            tracker.Tick(0.6f);
            tracker.PointerMoved(new Vector2(20f, 0f), overUi: false);
            tracker.ButtonReleased(PointerButton.Primary, new Vector2(20f, 0f));

            Assert.That(listener.LongPresses, Is.EqualTo(1));
            Assert.That(listener.DragStarts, Is.EqualTo(0));
            Assert.That(listener.DragEnds, Is.EqualTo(0));
            Assert.That(listener.Clicks, Is.Empty);
        }

        [Test]
        public void Hover_ReportsWhileIdle_ClearsOverUi_NotWhilePressed()
        {
            var listener = new FakeListener();
            var tracker = new PointerGestureTracker(listener, PointerGestureSettings.Default);

            tracker.PointerMoved(new Vector2(1f, 1f), overUi: false);
            Assert.That(listener.HoverCount, Is.EqualTo(1));

            tracker.PointerMoved(new Vector2(2f, 2f), overUi: true);
            Assert.That(listener.HoverClears, Is.EqualTo(1));

            tracker.ButtonPressed(PointerButton.Primary, Vector2.zero, overUi: false, time: 0f);
            var hoversBefore = listener.HoverCount;
            tracker.PointerMoved(new Vector2(3f, 3f), overUi: false);
            Assert.That(listener.HoverCount, Is.EqualTo(hoversBefore));
        }

        private sealed class FakeListener : IPointerGestureListener
        {
            public List<PointerClick> Clicks { get; } = new();

            public List<Vector2> DragDeltas { get; } = new();

            public int DragStarts { get; private set; }

            public int DragEnds { get; private set; }

            public int LongPresses { get; private set; }

            public int HoverCount { get; private set; }

            public int HoverClears { get; private set; }

            public void OnClick(PointerClick click) => Clicks.Add(click);

            public void OnLongPress(Vector2 screenPosition) => LongPresses++;

            public void OnDragStarted(Vector2 screenPosition) => DragStarts++;

            public void OnDragged(Vector2 screenDelta) => DragDeltas.Add(screenDelta);

            public void OnDragEnded() => DragEnds++;

            public void OnHover(Vector2 screenPosition) => HoverCount++;

            public void OnHoverCleared() => HoverClears++;
        }
    }
}
