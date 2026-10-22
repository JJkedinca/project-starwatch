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
    }
}
