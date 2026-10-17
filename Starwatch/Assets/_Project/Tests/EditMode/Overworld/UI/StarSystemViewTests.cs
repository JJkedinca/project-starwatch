using EndlessHeros.Core.Identifiers;
using EndlessHeros.Overworld;
using EndlessHeros.Overworld.UI;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace EndlessHeros.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// EditMode coverage for StarSystemView planet-strip lifecycle.
    /// </summary>
    public sealed class StarSystemViewTests
    {
        [Test]
        public void Show_CreatesOneIconPerPlanet()
        {
            var root = BuildStarSystemViewRoot();
            var view = new StarSystemView();
            view.Bind(root);

            var homePrime = ContentId.From("home-prime");
            var homeMoon = ContentId.From("home-moon");
            view.Show("home-system", new[] { Planet(homePrime), Planet(homeMoon) });

            Assert.That(view.IsVisible, Is.True);
            Assert.That(view.PlanetCount, Is.EqualTo(2));
            Assert.That(view.HasPlanet(homePrime), Is.True);
            Assert.That(view.HasPlanet(homeMoon), Is.True);
        }

        [Test]
        public void Show_KeepsPlanetIconsWhenTheSetIsUnchanged()
        {
            var root = BuildStarSystemViewRoot();
            var view = new StarSystemView();
            view.Bind(root);
            var planets = new[]
            {
                Planet(ContentId.From("home-prime")),
                Planet(ContentId.From("home-moon")),
            };
            view.Show("home-system", planets);
            var prime = root.Q<VisualElement>("planet-home-prime");

            view.Show("home-system", planets);

            Assert.That(root.Q<VisualElement>("planet-home-prime"), Is.SameAs(prime));
            Assert.That(view.PlanetCount, Is.EqualTo(2));
        }

        [Test]
        public void Hide_ClearsPlanetsAndHidesRoot()
        {
            var root = BuildStarSystemViewRoot();
            var view = new StarSystemView();
            view.Bind(root);
            view.Show("home-system", new[] { Planet(ContentId.From("home-prime")) });

            view.Hide();

            Assert.That(view.IsVisible, Is.False);
            Assert.That(view.PlanetCount, Is.EqualTo(0));
        }

        [Test]
        public void SetPlanetState_AppliesSelectedClass()
        {
            var root = BuildStarSystemViewRoot();
            var view = new StarSystemView();
            view.Bind(root);
            var planetId = ContentId.From("home-prime");
            view.Show("home-system", new[] { Planet(planetId) });

            view.SetPlanetState(
                planetId,
                new PlanetPresentationState(PlanetVisibility.SystemExplored, isSelected: true, "Home Prime"));

            var icon = root.Q<VisualElement>("planet-home-prime");
            Assert.That(icon, Is.Not.Null);
            Assert.That(icon.ClassListContains("planet-icon--selected"), Is.True);
        }

        private static StarSystemPlanetModel Planet(ContentId planetId)
        {
            return new StarSystemPlanetModel(planetId, default);
        }

        private static VisualElement BuildStarSystemViewRoot()
        {
            var root = new VisualElement { name = "star-system" };
            root.Add(new Button { name = "star-system-back" });
            root.Add(new Label { name = "star-system-title" });
            root.Add(new VisualElement { name = "planet-strip" });
            return root;
        }
    }
}
