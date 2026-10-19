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
    /// Fleet panel presenter against a fake view: open, refresh, and close.
    /// </summary>
    public sealed class FleetPanelPresenterTests
    {
        [Test]
        public void SelectFleet_OpensThePanelAndCloseClearsIt()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(73));
            var fleetId = run.Timeline.Fleets[0].Id;
            var selection = new OverworldSelection();
            selection.EnterSystem(ContentId.From("home-system"));
            var view = new FakeFleetPanelView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();

            selection.SelectFleet(fleetId);

            Assert.That(view.Model.IsOpen, Is.True);
            Assert.That(view.Model.FleetId, Is.EqualTo(fleetId));
            Assert.That(view.Model.Stacks[0].Count, Is.EqualTo(run.Timeline.Fleets[0].ShipCount));

            view.ClickClose();

            Assert.That(selection.SelectedFleetId.IsNone, Is.True);
            Assert.That(view.Model.IsOpen, Is.False);
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
            var view = new FakeFleetPanelView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();

            var started = stack.Travel.StartTravel(
                run,
                ObserverId.Player,
                fleetId,
                ContentId.From("forge-system"));

            Assert.That(started.Success, Is.True);
            Assert.That(view.Model.IsTraveling, Is.True);
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
            var view = new FakeFleetPanelView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();

            Assert.That(run.Timeline.TryAddShips(fleet.Id, fleet.Stacks[0].ClassId, 2), Is.True);
            stack.Events.Publish(new ShipConstructionCompletedEvent(
                GameTime.Zero,
                ContentId.From("home-prime"),
                fleet.Stacks[0].ClassId,
                fleet.Id));

            Assert.That(view.Model.Stacks[0].Count, Is.EqualTo(before + 2));
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
            var view = new FakeFleetPanelView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();

            view.ClickSplit(fleet.Stacks[0].ClassId, 1);

            Assert.That(run.Timeline.Fleets, Has.Count.EqualTo(2));
            Assert.That(view.Model.FleetId, Is.EqualTo(fleet.Id));
            Assert.That(view.Model.Stacks[0].Count, Is.EqualTo(fleet.ShipCount - 1));
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
                run.Timeline.TryAddFleet(ContentId.From("starter-screen"), 1, home, out var source),
                Is.True);
            var selection = new OverworldSelection();
            selection.EnterSystem(ContentId.From("home-system"));
            selection.SelectFleet(target.Id);
            var view = new FakeFleetPanelView();
            var presenter = CreatePresenter(view, selection, stack);
            presenter.Start();

            Assert.That(view.Model.MergeCandidates, Has.Count.EqualTo(1));
            Assert.That(view.Model.MergeCandidates[0].FleetId, Is.EqualTo(source));

            var before = target.ShipCount;
            view.ClickMerge(source);

            Assert.That(run.Timeline.Fleets, Has.Count.EqualTo(1));
            Assert.That(run.Timeline.TryGetFleet(target.Id, out var merged), Is.True);
            Assert.That(merged.ShipCount, Is.EqualTo(before + 1));
            Assert.That(view.Model.Stacks[0].Count, Is.EqualTo(merged.ShipCount));
            Assert.That(selection.SelectedFleetId, Is.EqualTo(target.Id));
            presenter.Dispose();
        }

        private static FleetPanelPresenter CreatePresenter(
            FakeFleetPanelView view,
            OverworldSelection selection,
            OverworldTestContext stack)
        {
            return new FleetPanelPresenter(
                view,
                selection,
                new OverworldStatus(),
                stack.Manager,
                stack.Fleets,
                new FleetPanelQuery(RunTestFixture.CreateObjectDatabase(), stack.Effects),
                stack.Events);
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
