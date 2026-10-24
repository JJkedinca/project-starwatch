using System.Collections.Generic;
using ProjectStarwatch.Overworld.Map;
using NUnit.Framework;

namespace ProjectStarwatch.Tests.EditMode.Overworld.Map
{
    public sealed class PooledMapSyncTests
    {
        [Test]
        public void ReleaseAbsent_ReleasesOnlyKeysNotInKeep()
        {
            var map = new Dictionary<string, int>
            {
                ["keep"] = 1,
                ["drop"] = 2,
            };
            var released = new List<string>();

            PooledMapSync.ReleaseAbsent(
                map,
                new HashSet<string> { "keep" },
                (key, _) => released.Add(key));

            Assert.That(map.Keys, Is.EquivalentTo(new[] { "keep" }));
            Assert.That(released, Is.EquivalentTo(new[] { "drop" }));
        }

        [Test]
        public void ReleaseAbsent_EmptyKeep_ReleasesAll()
        {
            var map = new Dictionary<int, string>
            {
                [1] = "a",
                [2] = "b",
            };

            PooledMapSync.ReleaseAbsent(map, new HashSet<int>(), (_, __) => { });

            Assert.That(map, Is.Empty);
        }
    }
}
