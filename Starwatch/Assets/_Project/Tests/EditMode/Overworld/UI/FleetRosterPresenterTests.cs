using System;
using System.Collections.Generic;
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
    /// Fleet roster presenter against a fake view.
    /// </summary>
    public sealed class FleetRosterPresenterTests
    {
        [Test]
        public void Start_ListsEveryFleetAndMarksTheSelection()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 91);
            var home = RunTestFixture.HomeSystem(run);
            Assert.That(
                run.Timeline.Fleets.TryAddFleet(ContentId.From("line-ship"), 2, home, out var second),
                Is.True);
            var selection = new OverworldSelection();
            selection.SelectFleet(second);
            var view = new FakeFleetRosterView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();

            Assert.That(view.Fleets, Has.Count.EqualTo(2));
            Assert.That(view.Fleets[1].FleetId, Is.EqualTo(second));
            Assert.That(view.Fleets[1].IsSelected, Is.True);
            Assert.That(view.Fleets[0].IsSelected, Is.False);
            presenter.Dispose();
        }

        [Test]
        public void Click_SelectsTheFleet()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 92);
            var fleetId = run.Timeline.Fleets[0].Id;
            var selection = new OverworldSelection();
            var view = new FakeFleetRosterView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();

            view.ClickFleet(fleetId);

            Assert.That(selection.SelectedFleetId, Is.EqualTo(fleetId));
            Assert.That(view.Fleets[0].IsSelected, Is.True);
            presenter.Dispose();
        }

        [Test]
        public void FleetCreated_AddsTheNewRow()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 93);
            var home = RunTestFixture.HomeSystem(run);
            Assert.That(
                run.Timeline.Fleets.TrySetFleetLocation(run.Timeline.Fleets[0].Id, RunTestFixture.FirstNeighbor(run)),
                Is.True);
            var selection = new OverworldSelection();
            var view = new FakeFleetRosterView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();

            Assert.That(
                stack.FleetService.AddBuiltShip(run, home, ContentId.From("starter-screen"), out var created),
                Is.True);

            Assert.That(view.Fleets, Has.Some.Matches<FleetRowModel>(fleet => fleet.FleetId == created));
            presenter.Dispose();
        }

        [Test]
        public void ResearchCompleted_RefreshesTravelSpeed()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 94);
            var selection = new OverworldSelection();
            selection.SelectFleet(run.Timeline.Fleets[0].Id);
            var view = new FakeFleetRosterView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();

            Assert.That(view.Fleets[0].TravelSpeed, Is.EqualTo(1f).Within(0.001f));

            Assert.That(run.Archive.Research.TryAdd(ContentId.From("basic-travel")), Is.True);
            stack.Effects.Rebuild(run, stack.ObjectDatabase.ResearchCatalog);
            stack.Events.Publish(new ResearchCompletedEvent(GameTime.Zero, ContentId.From("basic-travel")));

            Assert.That(view.Fleets[0].TravelSpeed, Is.EqualTo(1.25f).Within(0.001f));
            presenter.Dispose();
        }

        [Test]
        public void Merge_DropsTheRemovedFleet()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 95);
            var home = RunTestFixture.HomeSystem(run);
            var target = run.Timeline.Fleets[0].Id;
            Assert.That(
                run.Timeline.Fleets.TryAddFleet(ContentId.From("starter-screen"), 1, home, out var source),
                Is.True);
            var selection = new OverworldSelection();
            selection.SelectFleet(source);
            var view = new FakeFleetRosterView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();

            var merged = stack.FleetService.Merge(run, target, source);

            Assert.That(merged.Success, Is.True);
            Assert.That(view.Fleets, Has.Count.EqualTo(1));
            Assert.That(view.Fleets[0].FleetId, Is.EqualTo(target));
            presenter.Dispose();
        }

        [Test]
        public void NoActiveRun_HidesTheRoster()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var view = new FakeFleetRosterView();
            var presenter = CreatePresenter(view, new OverworldSelection(), stack);
            presenter.Start();

            Assert.That(view.IsVisible, Is.False);
            Assert.That(view.Fleets, Is.Null);
            presenter.Dispose();
        }

        [Test]
        public void TransitEta_AdvancesWithSimulationTime()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 97);
            var fleetId = run.Timeline.Fleets[0].Id;
            Assert.That(
                stack.TravelService.StartTravel(run, ObserverId.Player, fleetId, RunTestFixture.FirstNeighbor(run)).Success,
                Is.True);
            var selection = new OverworldSelection();
            var view = new FakeFleetRosterView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();

            var before = view.Fleets[0].Route.ArrivalSeconds;
            Assert.That(before, Is.GreaterThan(0f));

            stack.Clock.SetPaused(false);
            var ticks = (int)Math.Round(1d / SimulationLoop.kTickIntervalSeconds);
            for (var i = 0; i < ticks; i++)
            {
                stack.Loop.Pump(SimulationLoop.kTickIntervalSeconds);
            }

            Assert.That(view.Fleets[0].Route.IsTraveling, Is.True);
            Assert.That(view.Fleets[0].Route.ArrivalSeconds, Is.LessThan(before));
            presenter.Dispose();
        }

        private static FleetRosterPresenter CreatePresenter(
            FakeFleetRosterView view,
            OverworldSelection selection,
            OverworldTestContext stack)
        {
            var database = stack.ObjectDatabase;
            return new FleetRosterPresenter(
                view,
                selection,
                stack.Manager,
                new FleetListQuery(database, stack.Effects, stack.Clock),
                stack.Clock,
                stack.Events);
        }

        private sealed class FakeFleetRosterView : IFleetRosterView
        {
            public event Action<EntityId> FleetSelected;

            public IReadOnlyList<FleetRowModel> Fleets { get; private set; }

            public bool IsVisible { get; private set; }

            public void Apply(IReadOnlyList<FleetRowModel> fleets)
            {
                Fleets = fleets;
                IsVisible = fleets != null;
            }

            public void ClickFleet(EntityId fleetId)
            {
                FleetSelected?.Invoke(fleetId);
            }
        }
    }
}
