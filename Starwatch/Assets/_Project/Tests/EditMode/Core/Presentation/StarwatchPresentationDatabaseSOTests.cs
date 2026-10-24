using System.Reflection;
using NUnit.Framework;
using ProjectStarwatch.Core.Presentation;
using ProjectStarwatch.Overworld.Map;
using UnityEngine;

namespace ProjectStarwatch.Tests.EditMode.Core.Presentation
{
    public sealed class StarwatchPresentationDatabaseSOTests
    {
        [Test]
        public void StarMap_ExposesAssignedPresentation()
        {
            var starMap = ScriptableObject.CreateInstance<StarMapPresentationSO>();
            var database = ScriptableObject.CreateInstance<StarwatchPresentationDatabaseSO>();
            var backingField = typeof(StarwatchPresentationDatabaseSO).GetField(
                "<StarMap>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(backingField, Is.Not.Null);
            backingField.SetValue(database, starMap);

            Assert.That(database.StarMap, Is.SameAs(starMap));

            Object.DestroyImmediate(database);
            Object.DestroyImmediate(starMap);
        }

        [Test]
        public void StarSystem_ExposesAssignedPresentation()
        {
            var starSystem = ScriptableObject.CreateInstance<StarSystemPresentationSO>();
            var database = ScriptableObject.CreateInstance<StarwatchPresentationDatabaseSO>();
            var backingField = typeof(StarwatchPresentationDatabaseSO).GetField(
                "<StarSystem>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(backingField, Is.Not.Null);
            backingField.SetValue(database, starSystem);

            Assert.That(database.StarSystem, Is.SameAs(starSystem));

            Object.DestroyImmediate(database);
            Object.DestroyImmediate(starSystem);
        }

        [Test]
        public void ValidateRequired_DoesNotThrowWhenChildrenAssigned()
        {
            var starMap = ScriptableObject.CreateInstance<StarMapPresentationSO>();
            var starSystem = ScriptableObject.CreateInstance<StarSystemPresentationSO>();
            var database = ScriptableObject.CreateInstance<StarwatchPresentationDatabaseSO>();
            SetBacking(database, "<StarMap>k__BackingField", starMap);
            SetBacking(database, "<StarSystem>k__BackingField", starSystem);

            Assert.DoesNotThrow(database.ValidateRequired);

            Object.DestroyImmediate(database);
            Object.DestroyImmediate(starMap);
            Object.DestroyImmediate(starSystem);
        }

        private static void SetBacking(StarwatchPresentationDatabaseSO database, string fieldName, Object value)
        {
            var backingField = typeof(StarwatchPresentationDatabaseSO).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(backingField, Is.Not.Null);
            backingField.SetValue(database, value);
        }
    }
}
