using System;
using UnityEngine;

namespace EndlessHeros.Core.Data
{
    /// <summary>
    /// Output multipliers for one planet. 1 is baseline. 0 counts as unset.
    /// </summary>
    [Serializable]
    public struct PlanetModifiers
    {
        [Tooltip("Multiplier on Industry output. 1 is baseline.")]
        [SerializeField]
        private float _industryMultiplier;

        [Tooltip("Multiplier on Energy output. 1 is baseline.")]
        [SerializeField]
        private float _energyMultiplier;

        [Tooltip("Multiplier on Science output. 1 is baseline.")]
        [SerializeField]
        private float _scienceMultiplier;

        [Tooltip("Multiplier on Strategic output. 1 is baseline.")]
        [SerializeField]
        private float _strategicMultiplier;

        public PlanetModifiers(float industry, float energy, float science, float strategic)
        {
            _industryMultiplier = industry;
            _energyMultiplier = energy;
            _scienceMultiplier = science;
            _strategicMultiplier = strategic;
        }

        /// <summary>
        /// Multiplier applied to output of <paramref name="kind"/>. Unset values count as 1.
        /// </summary>
        public float Multiplier(ResourceKind kind)
        {
            var value = kind switch
            {
                ResourceKind.Industry => _industryMultiplier,
                ResourceKind.Energy => _energyMultiplier,
                ResourceKind.Science => _scienceMultiplier,
                ResourceKind.Strategic => _strategicMultiplier,
                _ => 1f,
            };
            return value <= 0f ? 1f : value;
        }
    }
}
