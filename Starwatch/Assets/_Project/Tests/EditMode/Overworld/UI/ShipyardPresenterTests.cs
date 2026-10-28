using System;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Simulation;
using ProjectStarwatch.Overworld.UI;
using ProjectStarwatch.Tests.EditMode.Core;
using NUnit.Framework;

namespace ProjectStarwatch.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// Shipyard presenter against a fake view: locked classes, queue, cancel, and progress.
    /// </summary>
    public sealed class ShipyardPresenterTests
    {
        [Test]
        public void WithoutAShipyard_TheSectionStaysHidden()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 80);
            var selection = new OverworldSelection();
            selection.EnterSystem(RunTestFixture.HomeSystem(run));
            selection.SelectPlanet(RunTestFixture.HomePlanet(run));
            var view = new FakeShipyardView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();

            Assert.That(view.Model.IsShown, Is.False);
            presenter.Dispose();
        }

        [Test]
        public void LockedClass_NamesTheResearchThatUnlocksIt()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 81);
            var planet = RunTestFixture.HomePlanet(run);
            Assert.That(stack.BuildingService.Place(run, planet, ContentId.From("shipyard")).Success, Is.True);
            var selection = new OverworldSelection();
            selection.EnterSystem(RunTestFixture.HomeSystem(run));
            selection.SelectPlanet(planet);
            var view = new FakeShipyardView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();

            var screen = FindClass(view.Model, ContentId.From("starter-screen"));
            Assert.That(run.Archive.PlayerMapKnowledge.HasScannedPlanet(planet), Is.False);
            Assert.That(view.Model.IsShown, Is.True);
            Assert.That(screen.IsLocked, Is.True);
            Assert.That(screen.UnlockResearchId, Is.EqualTo(ContentId.From("basic-fleet")));
            Assert.That(screen.Costs, Has.Count.EqualTo(1));
            Assert.That(screen.Costs[0].Kind, Is.EqualTo(ResourceKind.Industry));
            Assert.That(screen.Costs[0].Amount, Is.EqualTo(10f));
            Assert.That(screen.BuildSeconds, Is.EqualTo(10f));

            Unlock(stack, run);
            stack.Events.Publish(new ResearchCompletedEvent(GameTime.Zero, ContentId.From("basic-fleet")));

            Assert.That(FindClass(view.Model, ContentId.From("starter-screen")).IsLocked, Is.False);
            presenter.Dispose();
        }

        [Test]
        public void QueueAndCancel_UpdateTheQueue()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 82);
            var planet = RunTestFixture.HomePlanet(run);
            Unlock(stack, run);
            Assert.That(stack.BuildingService.Place(run, planet, ContentId.From("shipyard")).Success, Is.True);
            var selection = new OverworldSelection();
            selection.EnterSystem(RunTestFixture.HomeSystem(run));
            selection.SelectPlanet(planet);
            var view = new FakeShipyardView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();
            var industry = run.Timeline.Resources.Get(ResourceKind.Industry);

            view.ClickQueue(ContentId.From("starter-screen"));

            Assert.That(view.Model.Queue, Has.Count.EqualTo(1));
            Assert.That(view.Model.Queue[0].IsHead, Is.True);
            Assert.That(view.Model.Queue[0].SecondsRemaining, Is.EqualTo(10f));
            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(industry - 10f));

            view.ClickCancel(0);

            Assert.That(view.Model.Queue, Is.Empty);
            Assert.That(run.Timeline.Resources.Get(ResourceKind.Industry), Is.EqualTo(industry));
            presenter.Dispose();
        }

        [Test]
        public void BuildProgress_AdvancesWithSimulationTime()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 83);
            var planet = RunTestFixture.HomePlanet(run);
            Unlock(stack, run);
            Assert.That(stack.BuildingService.Place(run, planet, ContentId.From("shipyard")).Success, Is.True);
            var selection = new OverworldSelection();
            selection.EnterSystem(RunTestFixture.HomeSystem(run));
            selection.SelectPlanet(planet);
            var view = new FakeShipyardView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();
            view.ClickQueue(ContentId.From("starter-screen"));

            stack.Clock.SetPaused(false);
            var ticks = (int)Math.Round(1d / SimulationLoop.kTickIntervalSeconds);
            for (var i = 0; i < ticks; i++)
            {
                stack.Loop.Pump(SimulationLoop.kTickIntervalSeconds);
            }

            Assert.That(view.Model.Queue, Has.Count.EqualTo(1));
            Assert.That(view.Model.Queue[0].SecondsRemaining, Is.LessThan(view.Model.Queue[0].BuildSeconds));
            presenter.Dispose();
        }

        private static ShipyardClassRow FindClass(ShipyardModel model, ContentId classId)
        {
            for (var i = 0; i < model.Classes.Count; i++)
            {
                if (model.Classes[i].ClassId == classId)
                {
                    return model.Classes[i];
                }
            }

            Assert.Fail("Missing ship class.");
            return default;
        }

        private static void Unlock(OverworldTestContext stack, RunState run)
        {
            Assert.That(run.Archive.Research.TryAdd(ContentId.From("basic-fleet")), Is.True);
            stack.Effects.Rebuild(run, stack.ObjectDatabase.ResearchCatalog);
        }

        private static ShipyardPresenter CreatePresenter(
            FakeShipyardView view,
            OverworldSelection selection,
            OverworldTestContext stack)
        {
            return new ShipyardPresenter(
                view,
                selection,
                new OverworldStatus(),
                stack.Manager,
                stack.ShipProductionService,
                new ShipyardQuery(stack.ObjectDatabase, stack.Effects, stack.ShipProductionService),
                stack.Clock,
                stack.Events);
        }

        private sealed class FakeShipyardView : IShipyardView
        {
            public event Action<ContentId> QueueRequested;

            public event Action<int> CancelRequested;

            public ShipyardModel Model { get; private set; } = ShipyardModel.Hidden;

            public void Apply(ShipyardModel model)
            {
                Model = model;
            }

            public void ClickQueue(ContentId classId)
            {
                QueueRequested?.Invoke(classId);
            }

            public void ClickCancel(int index)
            {
                CancelRequested?.Invoke(index);
            }
        }
    }
}
