using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Effects;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Timeline;
using ProjectStarwatch.Overworld;
using ProjectStarwatch.Overworld.UI;
using ProjectStarwatch.Tests.EditMode.Core;
using NUnit.Framework;

namespace ProjectStarwatch.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// Fleet panel read model: the selected fleet's stacks and travel speed.
    /// </summary>
    public sealed class FleetPanelQueryTests
    {
        [Test]
        public void Describe_IsClosedUntilAFleetIsSelected()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 70);
            var query = new FleetPanelQuery(stack.ObjectDatabase, stack.Effects, stack.Clock);
            var selection = new OverworldSelection();

            Assert.That(query.Describe(run, selection).HasFleet, Is.False);
            Assert.That(query.Describe(run, selection).IsOpen, Is.False);

            selection.EnterSystem(RunTestFixture.HomeSystem(run));
            selection.SelectFleet(new EntityId(99));

            Assert.That(query.Describe(run, selection).HasFleet, Is.False);
            Assert.That(query.Describe(run, selection).IsOpen, Is.False);
        }

        [Test]
        public void Describe_ShowsTheBarWithoutOpeningDetails()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 69);
            var query = new FleetPanelQuery(stack.ObjectDatabase, stack.Effects, stack.Clock);
            var selection = new OverworldSelection();
            selection.SelectFleet(run.Timeline.Fleets[0].Id);

            var model = query.Describe(run, selection);

            Assert.That(model.HasFleet, Is.True);
            Assert.That(model.IsOpen, Is.False);
        }

        [Test]
        public void Describe_ListsTheSelectedFleetsStacksWhenDetailsAreOpen()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 71);
            var database = stack.ObjectDatabase;
            var query = new FleetPanelQuery(database, stack.Effects, stack.Clock);
            var home = RunTestFixture.HomeSystem(run);
            var fleet = run.Timeline.Fleets[0];
            var selection = new OverworldSelection();
            selection.EnterSystem(home);
            selection.SelectFleet(fleet.Id);
            selection.OpenFleetDetails();

            var model = query.Describe(run, selection);

            Assert.That(model.HasFleet, Is.True);
            Assert.That(model.IsOpen, Is.True);
            Assert.That(model.FleetId, Is.EqualTo(fleet.Id));
            Assert.That(model.Route.LocationId, Is.EqualTo(home));
            Assert.That(model.Route.IsTraveling, Is.False);
            Assert.That(model.Stacks, Has.Count.EqualTo(fleet.Stacks.Count));
            Assert.That(model.Stacks[0].ClassId, Is.EqualTo(fleet.Stacks[0].ClassId));
            Assert.That(model.Stacks[0].Count, Is.EqualTo(fleet.Stacks[0].Count));
            Assert.That(database.ShipCatalog.TryGet(fleet.Stacks[0].ClassId, out var shipClass), Is.True);
            Assert.That(model.TravelSpeed, Is.EqualTo(shipClass.SpeedFactor));
        }

        [Test]
        public void Describe_UsesTheSlowestClassAndTheResearchMultiplier()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = RunTestFixture.CreateStandardRun(stack.Manager, 72);
            var database = stack.ObjectDatabase;
            var screen = ContentId.From("starter-screen");
            var line = ContentId.From("line-ship");
            Assert.That(database.ShipCatalog.TryGet(screen, out var screenClass), Is.True);
            Assert.That(database.ShipCatalog.TryGet(line, out var lineClass), Is.True);
            Assert.That(
                run.Timeline.Fleets.TryAddFleet(
                    new[] { new ShipStack(screen, 2), new ShipStack(line, 1) },
                    RunTestFixture.HomeSystem(run),
                    out var fleetId),
                Is.True);
            var effects = new FixedTravelSpeed(1.25f);
            var query = new FleetPanelQuery(database, effects, stack.Clock);
            var selection = new OverworldSelection();
            selection.EnterSystem(RunTestFixture.HomeSystem(run));
            selection.SelectFleet(fleetId);
            selection.OpenFleetDetails();

            var model = query.Describe(run, selection);

            Assert.That(model.Stacks, Has.Count.EqualTo(2));
            Assert.That(model.Stacks[0].ClassId, Is.EqualTo(screen));
            Assert.That(model.Stacks[0].DisplayName, Is.EqualTo(screenClass.DisplayName));
            Assert.That(model.Stacks[1].ClassId, Is.EqualTo(line));
            Assert.That(model.Stacks[1].DisplayName, Is.EqualTo(lineClass.DisplayName));
            Assert.That(model.Stacks[1].Count, Is.EqualTo(1));
            Assert.That(model.TravelSpeed, Is.EqualTo(lineClass.SpeedFactor * 1.25f).Within(0.001f));
            Assert.That(model.TravelSpeed, Is.LessThan(screenClass.SpeedFactor * 1.25f));
        }

        private sealed class FixedTravelSpeed : IEffectQuery
        {
            private readonly float _travelSpeed;

            public FixedTravelSpeed(float travelSpeed)
            {
                _travelSpeed = travelSpeed;
            }

            public bool HasCapability(string capabilityId)
            {
                return false;
            }

            public float OutputMultiplier(ResourceKind kind)
            {
                return 1f;
            }

            public float TravelSpeedMultiplier()
            {
                return _travelSpeed;
            }
        }
    }
}
