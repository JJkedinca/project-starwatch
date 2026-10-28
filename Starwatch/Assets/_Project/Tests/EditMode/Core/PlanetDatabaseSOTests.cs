using System;
using NUnit.Framework;
using ProjectStarwatch.Core.Data;
using UnityEngine;

namespace ProjectStarwatch.Tests.EditMode.Core
{
    public sealed class PlanetDatabaseSOTests
    {
        [Test]
        public void Pick_PreferHomeSuitable_SkipsTightFootprintTemplates()
        {
            var database = ScriptableObject.CreateInstance<PlanetDatabaseSO>();
            database.SetContent(new[]
            {
                new PlanetTemplate("Tiny", string.Empty, footprintBudget: 4),
                new PlanetTemplate("Home", string.Empty, footprintBudget: 10),
            });

            for (var i = 0; i < 20; i++)
            {
                var pick = database.Pick(new Random(i), preferHomeSuitable: true);
                Assert.That(pick, Is.Not.Null);
                Assert.That(pick.DisplayName, Is.EqualTo("Home"));
                Assert.That(pick.FootprintBudget, Is.GreaterThanOrEqualTo(GalaxyHomeFootprint.Minimum));
            }

            Object.DestroyImmediate(database);
        }

        [Test]
        public void ValidateRequired_AcceptsPoolWithHomeSuitableTemplate()
        {
            var database = ScriptableObject.CreateInstance<PlanetDatabaseSO>();
            database.SetContent(new[]
            {
                new PlanetTemplate("Home", string.Empty, footprintBudget: 10),
            });

            Assert.DoesNotThrow(database.ValidateRequired);
            Object.DestroyImmediate(database);
        }

        [Test]
        public void ValidateRequired_RejectsNullEntries()
        {
            var database = ScriptableObject.CreateInstance<PlanetDatabaseSO>();
            database.AssignEntriesForTests(new[]
            {
                new PlanetTemplate("Home", string.Empty, footprintBudget: 10),
                null,
            });

            Assert.Throws<UnityEngine.Assertions.AssertionException>(database.ValidateRequired);
            Object.DestroyImmediate(database);
        }

        [Test]
        public void Pick_SkipsNullEntries()
        {
            var database = ScriptableObject.CreateInstance<PlanetDatabaseSO>();
            database.AssignEntriesForTests(new[]
            {
                null,
                new PlanetTemplate("Home", string.Empty, footprintBudget: 10),
                null,
            });

            for (var i = 0; i < 20; i++)
            {
                var pick = database.Pick(new Random(i), preferHomeSuitable: false);
                Assert.That(pick, Is.Not.Null);
                Assert.That(pick.DisplayName, Is.EqualTo("Home"));
            }

            Object.DestroyImmediate(database);
        }
    }
}
