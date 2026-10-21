using ProjectStarwatch.Overworld.UI;
using NUnit.Framework;

namespace ProjectStarwatch.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// Kinded toast queue rules for the overworld status bus.
    /// </summary>
    public sealed class OverworldStatusTests
    {
        [Test]
        public void Post_KeepsNewestFirstAndCapsAtThree()
        {
            var status = new OverworldStatus();
            var raises = 0;
            status.Changed += () => raises++;

            status.Post("one", StatusKind.Result);
            status.Post("two", StatusKind.Error);
            status.Post("three", StatusKind.Event);
            status.Post("four", StatusKind.Result);

            Assert.That(raises, Is.EqualTo(4));
            Assert.That(status.Toasts, Has.Count.EqualTo(3));
            Assert.That(status.Toasts[0].Message, Is.EqualTo("four"));
            Assert.That(status.Toasts[0].Kind, Is.EqualTo(StatusKind.Result));
            Assert.That(status.Toasts[1].Message, Is.EqualTo("three"));
            Assert.That(status.Toasts[2].Message, Is.EqualTo("two"));
            Assert.That(status.Text, Is.EqualTo("four"));
        }

        [Test]
        public void Dismiss_RemovesMatchingToast()
        {
            var status = new OverworldStatus();
            status.Post("keep", StatusKind.Result);
            status.Post("drop", StatusKind.Error);
            var dropId = status.Toasts[0].Id;

            status.Dismiss(dropId);

            Assert.That(status.Toasts, Has.Count.EqualTo(1));
            Assert.That(status.Text, Is.EqualTo("keep"));
        }

        [Test]
        public void PostCommand_UsesErrorKindOnFailure()
        {
            var status = new OverworldStatus();

            status.PostCommand(false, "ok", "fail");

            Assert.That(status.Toasts[0].Kind, Is.EqualTo(StatusKind.Error));
            Assert.That(status.Text, Is.EqualTo("fail"));
        }

        [Test]
        public void Post_StampsFixedExpiry_LaterPostsDoNotExtendOlderToasts()
        {
            var now = 10d;
            var status = new OverworldStatus();
            status.SetClockForTests(() => now);

            status.Post("first", StatusKind.Result);
            var firstExpire = status.Toasts[0].ExpireAtUnscaledSeconds;
            Assert.That(firstExpire, Is.EqualTo(10d + OverworldStatus.kResultSeconds).Within(0.001d));

            now = 12d;
            status.Post("second", StatusKind.Result);

            Assert.That(status.Toasts, Has.Count.EqualTo(2));
            Assert.That(status.Toasts[1].Message, Is.EqualTo("first"));
            Assert.That(status.Toasts[1].ExpireAtUnscaledSeconds, Is.EqualTo(firstExpire).Within(0.001d));
            Assert.That(status.Toasts[0].ExpireAtUnscaledSeconds, Is.EqualTo(12d + OverworldStatus.kResultSeconds).Within(0.001d));
        }

        [Test]
        public void DismissExpired_RemovesOnlyDueToasts()
        {
            var now = 0d;
            var status = new OverworldStatus();
            status.SetClockForTests(() => now);

            status.Post("early", StatusKind.Result);
            now = 1d;
            status.Post("late", StatusKind.Event);

            now = OverworldStatus.kResultSeconds + 0.1d;
            var removed = status.DismissExpired();

            Assert.That(removed, Is.EqualTo(1));
            Assert.That(status.Toasts, Has.Count.EqualTo(1));
            Assert.That(status.Text, Is.EqualTo("late"));
            Assert.That(status.Toasts[0].RemainingSeconds(now), Is.GreaterThan(0f));
        }
    }
}
