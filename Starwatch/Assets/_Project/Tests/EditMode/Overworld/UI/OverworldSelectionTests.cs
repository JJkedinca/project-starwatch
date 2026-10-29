using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Overworld.UI;
using NUnit.Framework;

namespace ProjectStarwatch.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// Selection rules shared by the star map, star system, and settlement presenters.
    /// </summary>
    public sealed class OverworldSelectionTests
    {
        private static readonly ContentId Home = ContentId.From("selection-home");
        private static readonly ContentId Prime = ContentId.From("selection-planet");

        [Test]
        public void SelectNode_ReturnsToTheMapAndClearsThePlanet()
        {
            var selection = new OverworldSelection();
            selection.EnterSystem(Home);
            selection.SelectPlanet(Prime);

            selection.SelectNode(Home);

            Assert.That(selection.InStarSystem, Is.False);
            Assert.That(selection.SelectedNodeId, Is.EqualTo(Home));
            Assert.That(selection.SelectedPlanetId.IsEmpty, Is.True);
        }

        [Test]
        public void EnterSystem_ClearsThePlanet()
        {
            var selection = new OverworldSelection();
            selection.EnterSystem(Home);
            selection.SelectPlanet(Prime);

            selection.EnterSystem(Home);

            Assert.That(selection.InStarSystem, Is.True);
            Assert.That(selection.SelectedNodeId, Is.EqualTo(Home));
            Assert.That(selection.SelectedPlanetId.IsEmpty, Is.True);
        }

        [Test]
        public void SelectPlanet_IsIgnoredOnTheStarMap()
        {
            var selection = new OverworldSelection();
            selection.SelectNode(Home);

            selection.SelectPlanet(Prime);

            Assert.That(selection.InStarSystem, Is.False);
            Assert.That(selection.SelectedPlanetId.IsEmpty, Is.True);
        }

        [Test]
        public void BackToMap_KeepsTheNodeAndClearsThePlanet()
        {
            var selection = new OverworldSelection();
            selection.EnterSystem(Home);
            selection.SelectPlanet(Prime);

            selection.BackToMap();

            Assert.That(selection.Mode, Is.EqualTo(OverworldViewMode.StarMap));
            Assert.That(selection.SelectedNodeId, Is.EqualTo(Home));
            Assert.That(selection.SelectedPlanetId.IsEmpty, Is.True);
        }

        [Test]
        public void Clear_EmptiesTheSelection()
        {
            var selection = new OverworldSelection();
            selection.EnterSystem(Home);
            selection.SelectPlanet(Prime);

            selection.SelectFleet(new EntityId(4));
            selection.Clear();

            Assert.That(selection.InStarSystem, Is.False);
            Assert.That(selection.SelectedNodeId.IsEmpty, Is.True);
            Assert.That(selection.SelectedPlanetId.IsEmpty, Is.True);
            Assert.That(selection.SelectedFleetId.IsNone, Is.True);
        }

        [Test]
        public void SelectFleet_StaysSelectedAcrossTheMapAndTheSystem()
        {
            var selection = new OverworldSelection();
            var fleetId = new EntityId(7);
            var raises = 0;
            selection.Changed += () => raises++;

            selection.SelectFleet(fleetId);
            selection.EnterSystem(Home);
            selection.SelectPlanet(Prime);
            selection.BackToMap();
            selection.SelectNode(Home);
            selection.SelectFleet(fleetId);

            Assert.That(selection.SelectedFleetId, Is.EqualTo(fleetId));
            Assert.That(selection.InStarSystem, Is.False);
            Assert.That(raises, Is.EqualTo(4));
        }

        [Test]
        public void UnchangedSelection_DoesNotRaiseChanged()
        {
            var selection = new OverworldSelection();
            var raises = 0;
            selection.Changed += () => raises++;

            selection.SelectNode(Home);
            selection.SelectNode(Home);

            Assert.That(raises, Is.EqualTo(1));
        }

        [Test]
        public void OpenFleetDetails_RequiresASelectedFleet()
        {
            var selection = new OverworldSelection();
            var raises = 0;
            selection.Changed += () => raises++;

            selection.OpenFleetDetails();

            Assert.That(selection.RightColumnFocus, Is.EqualTo(RightColumnFocus.Target));
            Assert.That(raises, Is.EqualTo(0));

            selection.SelectFleet(new EntityId(3));
            selection.OpenFleetDetails();

            Assert.That(selection.RightColumnFocus, Is.EqualTo(RightColumnFocus.FleetDetails));
            Assert.That(raises, Is.EqualTo(2));

            selection.OpenFleetDetails();
            Assert.That(raises, Is.EqualTo(2));
        }

        [Test]
        public void SelectNode_ClosesFleetDetails()
        {
            var selection = new OverworldSelection();
            selection.SelectFleet(new EntityId(5));
            selection.OpenFleetDetails();

            selection.SelectNode(Home);

            Assert.That(selection.RightColumnFocus, Is.EqualTo(RightColumnFocus.Target));
            Assert.That(selection.SelectedFleetId, Is.EqualTo(new EntityId(5)));
        }

        [Test]
        public void ClearFleet_ClosesFleetDetails()
        {
            var selection = new OverworldSelection();
            selection.SelectFleet(new EntityId(8));
            selection.OpenFleetDetails();

            selection.SelectFleet(EntityId.None);

            Assert.That(selection.SelectedFleetId.IsNone, Is.True);
            Assert.That(selection.RightColumnFocus, Is.EqualTo(RightColumnFocus.Target));
        }

        [Test]
        public void OpenResearch_ReplacesFleetDetails()
        {
            var selection = new OverworldSelection();
            selection.SelectFleet(new EntityId(2));
            selection.OpenFleetDetails();

            selection.OpenResearch();

            Assert.That(selection.RightColumnFocus, Is.EqualTo(RightColumnFocus.Research));

            selection.OpenFleetDetails();

            Assert.That(selection.RightColumnFocus, Is.EqualTo(RightColumnFocus.FleetDetails));

            selection.CloseFleetDetails();

            Assert.That(selection.RightColumnFocus, Is.EqualTo(RightColumnFocus.Target));
        }
    }
}
