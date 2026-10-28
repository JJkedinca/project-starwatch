using System;
using NUnit.Framework;
using ProjectStarwatch.Core.Data;
using UnityEngine;

namespace ProjectStarwatch.Tests.EditMode.Core
{
    public sealed class StarSystemDatabaseSOTests
    {
        [Test]
        public void Pick_ReturnsAuthoredDisplayName()
        {
            var database = ScriptableObject.CreateInstance<StarSystemDatabaseSO>();
            database.SetContent(new[]
            {
                new StarSystemTemplate("Helios Reach"),
            });

            var pick = database.Pick(new Random(1));

            Assert.That(pick, Is.Not.Null);
            Assert.That(pick.DisplayName, Is.EqualTo("Helios Reach"));
            Assert.DoesNotThrow(database.ValidateRequired);
            Object.DestroyImmediate(database);
        }

        [Test]
        public void ValidateRequired_RejectsNullEntries()
        {
            var database = ScriptableObject.CreateInstance<StarSystemDatabaseSO>();
            database.AssignEntriesForTests(new[]
            {
                new StarSystemTemplate("Helios Reach"),
                null,
            });

            Assert.Throws<UnityEngine.Assertions.AssertionException>(database.ValidateRequired);
            Object.DestroyImmediate(database);
        }

        [Test]
        public void Pick_SkipsNullEntries()
        {
            var database = ScriptableObject.CreateInstance<StarSystemDatabaseSO>();
            database.AssignEntriesForTests(new[]
            {
                null,
                new StarSystemTemplate("Helios Reach"),
                null,
            });

            for (var i = 0; i < 20; i++)
            {
                var pick = database.Pick(new Random(i));
                Assert.That(pick, Is.Not.Null);
                Assert.That(pick.DisplayName, Is.EqualTo("Helios Reach"));
            }

            Object.DestroyImmediate(database);
        }
    }
}
