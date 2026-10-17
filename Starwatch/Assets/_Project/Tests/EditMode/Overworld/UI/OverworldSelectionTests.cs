using EndlessHeros.Core.Identifiers;
using EndlessHeros.Overworld.UI;
using NUnit.Framework;

namespace EndlessHeros.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// Selection rules shared by the star map, star system, and settlement presenters.
    /// </summary>
    public sealed class OverworldSelectionTests
    {
        [Test]
        public void SelectNode_ReturnsToTheMapAndClearsThePlanet()
        {
            var selection = new OverworldSelection();
            var home = ContentId.From("home-system");
            var prime = ContentId.From("home-prime");
            selection.EnterSystem(home);
            selection.SelectPlanet(prime);

            selection.SelectNode(home);

            Assert.That(selection.InStarSystem, Is.False);
            Assert.That(selection.SelectedNodeId, Is.EqualTo(home));
            Assert.That(selection.SelectedPlanetId.IsEmpty, Is.True);
        }

        [Test]
        public void EnterSystem_ClearsThePlanet()
        {
            var selection = new OverworldSelection();
            var home = ContentId.From("home-system");
            selection.EnterSystem(home);
            selection.SelectPlanet(ContentId.From("home-prime"));

            selection.EnterSystem(home);

            Assert.That(selection.InStarSystem, Is.True);
            Assert.That(selection.SelectedNodeId, Is.EqualTo(home));
            Assert.That(selection.SelectedPlanetId.IsEmpty, Is.True);
        }

        [Test]
        public void SelectPlanet_IsIgnoredOnTheStarMap()
        {
            var selection = new OverworldSelection();
            selection.SelectNode(ContentId.From("home-system"));

            selection.SelectPlanet(ContentId.From("home-prime"));

            Assert.That(selection.InStarSystem, Is.False);
            Assert.That(selection.SelectedPlanetId.IsEmpty, Is.True);
        }

        [Test]
        public void BackToMap_KeepsTheNodeAndClearsThePlanet()
        {
            var selection = new OverworldSelection();
            var home = ContentId.From("home-system");
            selection.EnterSystem(home);
            selection.SelectPlanet(ContentId.From("home-prime"));

            selection.BackToMap();

            Assert.That(selection.Mode, Is.EqualTo(OverworldViewMode.StarMap));
            Assert.That(selection.SelectedNodeId, Is.EqualTo(home));
            Assert.That(selection.SelectedPlanetId.IsEmpty, Is.True);
        }

        [Test]
        public void Clear_EmptiesTheSelection()
        {
            var selection = new OverworldSelection();
            selection.EnterSystem(ContentId.From("home-system"));
            selection.SelectPlanet(ContentId.From("home-prime"));

            selection.Clear();

            Assert.That(selection.InStarSystem, Is.False);
            Assert.That(selection.SelectedNodeId.IsEmpty, Is.True);
            Assert.That(selection.SelectedPlanetId.IsEmpty, Is.True);
        }

        [Test]
        public void UnchangedSelection_DoesNotRaiseChanged()
        {
            var selection = new OverworldSelection();
            var raises = 0;
            selection.Changed += () => raises++;
            var home = ContentId.From("home-system");

            selection.SelectNode(home);
            selection.SelectNode(home);

            Assert.That(raises, Is.EqualTo(1));
        }
    }
}
