using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace ProjectStarwatch.Core.Data
{
    /// <summary>
    /// Pool of star-system templates the galaxy generator picks from.
    /// </summary>
    [CreateAssetMenu(fileName = "StarSystemDatabase", menuName = "Starwatch/Star System Database")]
    public sealed class StarSystemDatabaseSO : ScriptableObject
    {
        [SerializeField]
        private List<StarSystemTemplate> _starSystems = new();

        public IReadOnlyList<StarSystemTemplate> StarSystems => _starSystems;

        /// <summary>
        /// Asserts the pool has no null slots and at least one template.
        /// </summary>
        public void ValidateRequired()
        {
            Assert.IsTrue(
                _starSystems != null && CountNonNull() > 0,
                "StarSystemDatabaseSO requires at least one star-system template.");
            Assert.IsFalse(
                HasNullEntries(),
                "StarSystemDatabaseSO star-system list must not contain null entries.");
        }

        /// <summary>
        /// Fills a runtime pool. The authored asset is the play-mode source.
        /// </summary>
        internal void SetContent(IReadOnlyList<StarSystemTemplate> systems)
        {
            _starSystems = new List<StarSystemTemplate>();
            if (systems == null)
            {
                return;
            }

            for (var i = 0; i < systems.Count; i++)
            {
                if (systems[i] != null)
                {
                    _starSystems.Add(systems[i]);
                }
            }
        }

        /// <summary>
        /// Assigns list entries as-is (including nulls). EditMode tests use this to cover ValidateRequired.
        /// </summary>
        internal void AssignEntriesForTests(IReadOnlyList<StarSystemTemplate> systems)
        {
            _starSystems = systems != null
                ? new List<StarSystemTemplate>(systems)
                : new List<StarSystemTemplate>();
        }

        internal StarSystemTemplate Pick(System.Random random)
        {
            if (_starSystems == null || random == null)
            {
                return null;
            }

            var valid = NonNullIndices();
            if (valid.Count == 0)
            {
                return null;
            }

            return _starSystems[valid[random.Next(valid.Count)]];
        }

        private bool HasNullEntries()
        {
            if (_starSystems == null)
            {
                return false;
            }

            for (var i = 0; i < _starSystems.Count; i++)
            {
                if (_starSystems[i] == null)
                {
                    return true;
                }
            }

            return false;
        }

        private int CountNonNull()
        {
            return NonNullIndices().Count;
        }

        private List<int> NonNullIndices()
        {
            var indices = new List<int>();
            if (_starSystems == null)
            {
                return indices;
            }

            for (var i = 0; i < _starSystems.Count; i++)
            {
                if (_starSystems[i] != null)
                {
                    indices.Add(i);
                }
            }

            return indices;
        }
    }
}
