using EndlessHeros.Core.Content;
using NUnit.Framework;
using UnityEngine;

namespace EndlessHeros.Tests.EditMode.Core
{
    public sealed class ObjectDatabaseSOTests
    {
        [Test]
        public void TryGetScenario_RebuildsAfterRuntimeScenarioIsDestroyed()
        {
            var database = RunTestFixture.CreateObjectDatabase();
            Assert.That(database.TryGetScenario("authored-vertical-slice", out var first), Is.True);

            Object.DestroyImmediate(first);

            Assert.That(database.TryGetScenario("authored-vertical-slice", out var rebuilt), Is.True);
            Assert.That(rebuilt.ScenarioId, Is.EqualTo("authored-vertical-slice"));

            Object.DestroyImmediate(rebuilt);
            Object.DestroyImmediate(database);
        }

        [Test]
        public void TryGetScenario_RebuildsWhenTheIndexWasCleared()
        {
            var database = RunTestFixture.CreateObjectDatabase();
            Assert.That(database.TryGetScenario("authored-vertical-slice", out _), Is.True);

            database.ResetBuiltIndex();

            Assert.That(database.TryGetScenario("authored-vertical-slice", out var rebuilt), Is.True);
            Assert.That(rebuilt.ScenarioId, Is.EqualTo("authored-vertical-slice"));
            Assert.That(database.Scenarios.ContainsKey("authored-vertical-slice"), Is.True);

            Object.DestroyImmediate(rebuilt);
            Object.DestroyImmediate(database);
        }
    }
}
