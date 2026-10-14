using EndlessHeros.Core.Identifiers;
using EndlessHeros.Overworld;
using EndlessHeros.Overworld.UI;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace EndlessHeros.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// EditMode coverage for SystemView planet-strip lifecycle.
    /// </summary>
    public sealed class SystemViewTests
    {
        [Test]
        public void Show_CreatesOneIconPerPlanet()
        {
            var root = BuildSystemViewRoot();
            var view = new SystemView();
            view.Bind(root);

            var homePrime = ContentId.From("home-prime");
            var homeMoon = ContentId.From("home-moon");
            view.Show("home-system", new[] { homePrime, homeMoon });

            Assert.That(view.IsVisible, Is.True);
            Assert.That(view.PlanetCount, Is.EqualTo(2));
            Assert.That(view.HasPlanet(homePrime), Is.True);
            Assert.That(view.HasPlanet(homeMoon), Is.True);
        }

        [Test]
        public void Hide_ClearsPlanetsAndHidesRoot()
        {
            var root = BuildSystemViewRoot();
            var view = new SystemView();
            view.Bind(root);
            view.Show("home-system", new[] { ContentId.From("home-prime") });

            view.Hide();

            Assert.That(view.IsVisible, Is.False);
            Assert.That(view.PlanetCount, Is.EqualTo(0));
        }

        [Test]
        public void SetPlanetState_AppliesSelectedClass()
        {
            var root = BuildSystemViewRoot();
            var view = new SystemView();
            view.Bind(root);
            var planetId = ContentId.From("home-prime");
            view.Show("home-system", new[] { planetId });

            view.SetPlanetState(
                planetId,
                new PlanetPresentationState(PlanetVisibility.SystemExplored, isSelected: true, "Home Prime"));

            var icon = root.Q<VisualElement>("planet-home-prime");
            Assert.That(icon, Is.Not.Null);
            Assert.That(icon.ClassListContains("planet-icon--selected"), Is.True);
        }

        private static VisualElement BuildSystemViewRoot()
        {
            var root = new VisualElement { name = "system-view" };
            root.Add(new Button { name = "system-back-button" });
            root.Add(new Label { name = "system-view-title" });
            root.Add(new VisualElement { name = "planet-strip" });
            return root;
        }
    }
}
