using System;

namespace EndlessHeros.Core.Simulation
{
    /// <summary>
    /// Seeded System.Random wrapper so scenario and combat rolls are reproducible.
    /// </summary>
    public sealed class DeterministicRandom : ISimulationRandom
    {
        private readonly Random _random;

        public DeterministicRandom(int seed)
        {
            _random = new Random(seed);
        }

        /// <summary>
        /// Returns a value in [minInclusive, maxExclusive).
        /// </summary>
        public int Next(int minInclusive, int maxExclusive)
        {
            return _random.Next(minInclusive, maxExclusive);
        }

        /// <summary>
        /// Returns a float in [0, 1).
        /// </summary>
        public float NextFloat()
        {
            return (float)_random.NextDouble();
        }
    }
}
