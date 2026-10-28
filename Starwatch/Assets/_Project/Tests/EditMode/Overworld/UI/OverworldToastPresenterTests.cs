using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Simulation;
using ProjectStarwatch.Overworld.UI;
using ProjectStarwatch.Tests.EditMode.Core;
using NUnit.Framework;

namespace ProjectStarwatch.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// Domain event → event toast mapping.
    /// </summary>
    public sealed class OverworldToastPresenterTests
    {
        [Test]
        public void FleetArrival_PostsEventToastWithDisplayName()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 42);
            var database = stack.ObjectDatabase;
            var status = new OverworldStatus();
            var presenter = new OverworldToastPresenter(status, stack.Events, database);
            presenter.Start();
            var forge = RunTestFixture.FirstNeighbor(run);
            var home = RunTestFixture.HomeSystem(run);

            stack.Events.Publish(new FleetTravelEvent(
                GameTime.Zero,
                new EntityId(1),
                home,
                forge,
                FleetTravelKind.Arrived));

            Assert.That(status.Toasts, Has.Count.EqualTo(1));
            Assert.That(status.Toasts[0].Kind, Is.EqualTo(StatusKind.Event));
            Assert.That(status.Text, Does.Contain(OverworldNames.StarSystem(database, forge)));

            presenter.Dispose();
        }

        [Test]
        public void FleetDepart_DoesNotPost()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 43);
            var status = new OverworldStatus();
            var presenter = new OverworldToastPresenter(
                status,
                stack.Events,
                stack.ObjectDatabase);
            presenter.Start();

            stack.Events.Publish(new FleetTravelEvent(
                GameTime.Zero,
                new EntityId(1),
                RunTestFixture.HomeSystem(run),
                RunTestFixture.FirstNeighbor(run),
                FleetTravelKind.Started));

            Assert.That(status.Toasts, Is.Empty);
            presenter.Dispose();
        }

        [Test]
        public void SettlementFounded_PostsEventToastWithPlanetDisplayName()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 44);
            var database = stack.ObjectDatabase;
            var status = new OverworldStatus();
            var presenter = new OverworldToastPresenter(status, stack.Events, database);
            presenter.Start();
            var prime = RunTestFixture.HomePlanet(run);
            Assert.That(database.TryGetPlanet(prime.Value, out var planet), Is.True);

            stack.Events.Publish(new SettlementFoundedEvent(
                GameTime.Zero,
                prime,
                SettlementKind.Colony));

            Assert.That(status.Toasts[0].Kind, Is.EqualTo(StatusKind.Event));
            Assert.That(status.Text, Does.Contain("Colony"));
            Assert.That(status.Text, Does.Contain(planet.DisplayName));
            Assert.That(status.Text, Does.Not.Contain(prime.Value));
            presenter.Dispose();
        }

        [Test]
        public void ResearchCompleted_PostsEventToastWithDisplayName()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var database = stack.ObjectDatabase;
            var status = new OverworldStatus();
            var presenter = new OverworldToastPresenter(status, stack.Events, database);
            presenter.Start();
            var researchId = ContentId.From("basic-travel");
            Assert.That(database.ResearchCatalog.TryGet(researchId, out var project), Is.True);

            stack.Events.Publish(new ResearchCompletedEvent(GameTime.Zero, researchId));

            Assert.That(status.Text, Does.Contain(project.DisplayName));
            Assert.That(status.Text, Does.Not.Contain(researchId.Value));
            presenter.Dispose();
        }
    }
}
