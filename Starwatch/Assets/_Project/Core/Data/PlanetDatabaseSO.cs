using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace ProjectStarwatch.Core.Data
{
    /// <summary>
    /// Pool of planet templates the galaxy generator picks from.
    /// </summary>
    [CreateAssetMenu(fileName = "PlanetDatabase", menuName = "Starwatch/Planet Database")]
    public sealed class PlanetDatabaseSO : ScriptableObject
    {
        [SerializeField]
        private List<PlanetTemplate> _planets = new();

        public IReadOnlyList<PlanetTemplate> Planets => _planets;

        /// <summary>
        /// Asserts the pool has no null slots and at least one template suitable for a home colony (footprint ≥ 8).
        /// </summary>
        public void ValidateRequired()
        {
            Assert.IsTrue(
                _planets != null && CountNonNull() > 0,
                "PlanetDatabaseSO requires at least one planet template.");
            Assert.IsFalse(
                HasNullEntries(),
                "PlanetDatabaseSO planet list must not contain null entries.");
            Assert.IsTrue(
                HasHomeSuitableTemplate(),
                "PlanetDatabaseSO requires at least one planet template with footprint budget ≥ 8 for the homeworld.");
        }

        /// <summary>
        /// Fills a runtime pool. The authored asset is the play-mode source.
        /// </summary>
        internal void SetContent(IReadOnlyList<PlanetTemplate> planets)
        {
            _planets = new List<PlanetTemplate>();
            if (planets == null)
            {
                return;
            }

            for (var i = 0; i < planets.Count; i++)
            {
                if (planets[i] != null)
                {
                    _planets.Add(planets[i]);
                }
            }
        }

        /// <summary>
        /// Assigns list entries as-is (including nulls). EditMode tests use this to cover ValidateRequired.
        /// </summary>
        internal void AssignEntriesForTests(IReadOnlyList<PlanetTemplate> planets)
        {
            _planets = planets != null
                ? new List<PlanetTemplate>(planets)
                : new List<PlanetTemplate>();
        }

        internal PlanetTemplate Pick(System.Random random, bool preferHomeSuitable)
        {
            if (_planets == null || random == null)
            {
                return null;
            }

            if (preferHomeSuitable)
            {
                var suitable = SuitableHomeIndices();
                if (suitable.Count > 0)
                {
                    return _planets[suitable[random.Next(suitable.Count)]];
                }
            }

            var valid = NonNullIndices();
            if (valid.Count == 0)
            {
                return null;
            }

            return _planets[valid[random.Next(valid.Count)]];
        }

        private bool HasHomeSuitableTemplate()
        {
            return SuitableHomeIndices().Count > 0;
        }

        private bool HasNullEntries()
        {
            if (_planets == null)
            {
                return false;
            }

            for (var i = 0; i < _planets.Count; i++)
            {
                if (_planets[i] == null)
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
            if (_planets == null)
            {
                return indices;
            }

            for (var i = 0; i < _planets.Count; i++)
            {
                if (_planets[i] != null)
                {
                    indices.Add(i);
                }
            }

            return indices;
        }

        private List<int> SuitableHomeIndices()
        {
            var indices = new List<int>();
            if (_planets == null)
            {
                return indices;
            }

            for (var i = 0; i < _planets.Count; i++)
            {
                var template = _planets[i];
                if (template != null && template.FootprintBudget >= GalaxyHomeFootprint.Minimum)
                {
                    indices.Add(i);
                }
            }

            return indices;
        }
    }

    /// <summary>
    /// Shared home-colony footprint floor used by planet pools and generation.
    /// </summary>
    internal static class GalaxyHomeFootprint
    {
        /// <summary>
        /// Room for the four producing starter buildings plus a shipyard (2 footprint each).
        /// </summary>
        internal const int Minimum = 10;
    }
}
