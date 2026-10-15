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
    }
}
