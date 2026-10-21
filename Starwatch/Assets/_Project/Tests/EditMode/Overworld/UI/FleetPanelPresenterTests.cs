using System;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Simulation;
using ProjectStarwatch.Overworld;
using ProjectStarwatch.Overworld.UI;
using ProjectStarwatch.Tests.EditMode.Core;
using NUnit.Framework;

namespace ProjectStarwatch.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// Fleet panel presenter against fake bar and details views.
    /// </summary>
    public sealed class FleetPanelPresenterTests
    {
        [Test]
        public void SelectFleet_ShowsTheBarAndDetailsOpensFromTheBar()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(73));
            var fleetId = run.Timeline.Fleets[0].Id;
            var selection = new OverworldSelection();
            selection.EnterSystem(ContentId.From("home-system"));
            var details = new FakeFleetPanelView();
            var bar = new FakeFleetBarView();
            var presenter = CreatePresenter(details, bar, selection, stack);
            presenter.Start();

            selection.SelectFleet(fleetId);

            Assert.That(bar.Model.HasFleet, Is.True);
            Assert.That(bar.Model.IsOpen, Is.False);
            Assert.That(details.Model.IsOpen, Is.False);
            Assert.That(bar.Model.FleetId, Is.EqualTo(fleetId));
            Assert.That(bar.Model.Stacks[0].Count, Is.EqualTo(run.Timeline.Fleets[0].ShipCount));

            bar.ClickDetails();

            Assert.That(selection.RightColumnFocus, Is.EqualTo(RightColumnFocus.FleetDetails));
            Assert.That(details.Model.IsOpen, Is.True);
            Assert.That(details.Model.FleetId, Is.EqualTo(fleetId));

            details.ClickClose();

            Assert.That(selection.SelectedFleetId, Is.EqualTo(fleetId));
            Assert.That(selection.RightColumnFocus, Is.EqualTo(RightColumnFocus.Target));
            Assert.That(details.Model.IsOpen, Is.False);
            Assert.That(bar.Model.HasFleet, Is.True);

            bar.ClickClose();

            Assert.That(selection.SelectedFleetId.IsNone, Is.True);
            Assert.That(bar.Model.HasFleet, Is.False);
            presenter.Dispose();
        }

        [Test]
        public void ResearchCompleted_RefreshesTravelSpeed()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(88));
            var selection = new OverworldSelection();
            selection.EnterSystem(ContentId.From("home-system"));
            selection.SelectFleet(run.Timeline.Fleets[0].Id);
            selection.OpenFleetDetails();
            var details = new FakeFleetPanelView();
            var bar = new FakeFleetBarView();
            var presenter = CreatePresenter(details, bar, selection, stack);
            presenter.Start();

            Assert.That(details.Model.TravelSpeed, Is.EqualTo(1f).Within(0.001f));

            Assert.That(run.Archive.Research.TryAdd(ContentId.From("basic-travel")), Is.True);
            stack.Effects.Rebuild(run, RunTestFixture.CreateObjectDatabase().ResearchCatalog);
            stack.Events.Publish(new ResearchCompletedEvent(GameTime.Zero, ContentId.From("basic-travel")));

            Assert.That(details.Model.TravelSpeed, Is.EqualTo(1.25f).Within(0.001f));
            Assert.That(bar.Model.TravelSpeed, Is.EqualTo(1.25f).Within(0.001f));
            presenter.Dispose();
        }

        [Test]
        public void Travel_MarksTheOpenPanelTraveling()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(74));
            var fleetId = run.Timeline.Fleets[0].Id;
            var selection = new OverworldSelection();
            selection.EnterSystem(ContentId.From("home-system"));
            selection.SelectFleet(fleetId);
            selection.OpenFleetDetails();
            var details = new FakeFleetPanelView();
            var bar = new FakeFleetBarView();
            var presenter = CreatePresenter(details, bar, selection, stack);
            presenter.Start();

            var started = stack.TravelService.StartTravel(
                run,
                ObserverId.Player,
                fleetId,
                ContentId.From("forge-system"));

            Assert.That(started.Success, Is.True);
            Assert.That(details.Model.Route.IsTraveling, Is.True);
            Assert.That(bar.Model.Route.IsTraveling, Is.True);
            presenter.Dispose();
        }

        [Test]
        public void CompletedShip_UpdatesTheStackCount()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(75));
            var fleet = run.Timeline.Fleets[0];
            var before = fleet.Stacks[0].Count;
            var selection = new OverworldSelection();
            selection.EnterSystem(ContentId.From("home-system"));
            selection.SelectFleet(fleet.Id);
            selection.OpenFleetDetails();
            var details = new FakeFleetPanelView();
            var bar = new FakeFleetBarView();
            var presenter = CreatePresenter(details, bar, selection, stack);
            presenter.Start();

            Assert.That(run.Timeline.Fleets.TryAddShips(fleet.Id, fleet.Stacks[0].ClassId, 2), Is.True);
            stack.Events.Publish(new ShipConstructionCompletedEvent(
                GameTime.Zero,
                ContentId.From("home-prime"),
                fleet.Stacks[0].ClassId,
                fleet.Id));

            Assert.That(details.Model.Stacks[0].Count, Is.EqualTo(before + 2));
            Assert.That(bar.Model.Stacks[0].Count, Is.EqualTo(before + 2));
            presenter.Dispose();
        }

        [Test]
        public void Split_MovesShipsIntoANewFleet()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(76));
            var fleet = run.Timeline.Fleets[0];
            var selection = new OverworldSelection();
            selection.EnterSystem(ContentId.From("home-system"));
            selection.SelectFleet(fleet.Id);
            selection.OpenFleetDetails();
            var details = new FakeFleetPanelView();
            var bar = new FakeFleetBarView();
            var presenter = CreatePresenter(details, bar, selection, stack);
            presenter.Start();

            details.ClickSplit(fleet.Stacks[0].ClassId, 1);

            Assert.That(run.Timeline.Fleets, Has.Count.EqualTo(2));
            Assert.That(details.Model.FleetId, Is.EqualTo(fleet.Id));
            Assert.That(details.Model.Stacks[0].Count, Is.EqualTo(fleet.ShipCount - 1));
            Assert.That(selection.SelectedFleetId, Is.EqualTo(fleet.Id));
            presenter.Dispose();
        }

        [Test]
        public void Merge_FoldsTheOtherFleetIntoTheSelectedOne()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(77));
            var home = ContentId.From("home-system");
            var target = run.Timeline.Fleets[0];
            Assert.That(
                run.Timeline.Fleets.TryAddFleet(ContentId.From("starter-screen"), 1, home, out var source),
                Is.True);
            var selection = new OverworldSelection();
            selection.EnterSystem(ContentId.From("home-system"));
            selection.SelectFleet(target.Id);
            selection.OpenFleetDetails();
            var details = new FakeFleetPanelView();
            var bar = new FakeFleetBarView();
            var presenter = CreatePresenter(details, bar, selection, stack);
            presenter.Start();

            Assert.That(details.Model.MergeCandidates, Has.Count.EqualTo(1));
            Assert.That(details.Model.MergeCandidates[0].FleetId, Is.EqualTo(source));

            var before = target.ShipCount;
            details.ClickMerge(source);

            Assert.That(run.Timeline.Fleets, Has.Count.EqualTo(1));
            Assert.That(run.Timeline.Fleets.TryGetFleet(target.Id, out var merged), Is.True);
            Assert.That(merged.ShipCount, Is.EqualTo(before + 1));
            Assert.That(details.Model.Stacks[0].Count, Is.EqualTo(merged.ShipCount));
            Assert.That(selection.SelectedFleetId, Is.EqualTo(target.Id));
            presenter.Dispose();
        }

        [Test]
        public void TransitEta_AdvancesWithSimulationTime()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(78));
            var fleetId = run.Timeline.Fleets[0].Id;
            var selection = new OverworldSelection();
            selection.SelectFleet(fleetId);
            selection.OpenFleetDetails();
            var details = new FakeFleetPanelView();
            var bar = new FakeFleetBarView();
            var presenter = CreatePresenter(details, bar, selection, stack);
            presenter.Start();

            Assert.That(
                stack.TravelService.StartTravel(run, ObserverId.Player, fleetId, ContentId.From("forge-system")).Success,
                Is.True);
            var before = bar.Model.Route.ArrivalSeconds;
            Assert.That(before, Is.GreaterThan(0f));
            Assert.That(details.Model.Route.ArrivalSeconds, Is.EqualTo(before).Within(0.001f));

            stack.Clock.SetPaused(false);
            var ticks = (int)Math.Round(1d / SimulationLoop.kTickIntervalSeconds);
            for (var i = 0; i < ticks; i++)
            {
                stack.Loop.Pump(SimulationLoop.kTickIntervalSeconds);
            }

            Assert.That(bar.Model.Route.IsTraveling, Is.True);
            Assert.That(bar.Model.Route.ArrivalSeconds, Is.LessThan(before));
            Assert.That(
                details.Model.Route.ArrivalSeconds,
                Is.EqualTo(bar.Model.Route.ArrivalSeconds).Within(0.001f));
            presenter.Dispose();
        }

        private static FleetPanelPresenter CreatePresenter(
            FakeFleetPanelView details,
            FakeFleetBarView bar,
            OverworldSelection selection,
            OverworldTestContext stack)
        {
            return new FleetPanelPresenter(
                details,
                bar,
                selection,
                new OverworldStatus(),
                stack.Manager,
                stack.FleetService,
                new FleetPanelQuery(RunTestFixture.CreateObjectDatabase(), stack.Effects, stack.Clock),
                stack.Clock,
                stack.Events);
        }

        private sealed class FakeFleetBarView : IFleetBarView
        {
            public event Action DetailsRequested;

            public event Action CloseRequested;

            public FleetPanelModel Model { get; private set; } = FleetPanelModel.Closed;

            public void Apply(FleetPanelModel model)
            {
                Model = model;
            }

            public void ClickDetails()
            {
                DetailsRequested?.Invoke();
            }

            public void ClickClose()
            {
                CloseRequested?.Invoke();
            }
        }

        private sealed class FakeFleetPanelView : IFleetPanelView
        {
            public event Action CloseRequested;

            public event Action<ContentId, int> SplitRequested;

            public event Action<EntityId> MergeRequested;

            public FleetPanelModel Model { get; private set; } = FleetPanelModel.Closed;

            public void Apply(FleetPanelModel model)
            {
                Model = model;
            }

            public void ClickClose()
            {
                CloseRequested?.Invoke();
            }

            public void ClickSplit(ContentId classId, int count)
            {
                SplitRequested?.Invoke(classId, count);
            }

            public void ClickMerge(EntityId fleetId)
            {
                MergeRequested?.Invoke(fleetId);
            }
        }
    }
}
