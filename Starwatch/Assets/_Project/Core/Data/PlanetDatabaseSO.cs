using System.Collections.Generic;
using UnityEngine;

namespace EndlessHeros.Core.Data
{
    /// <summary>
    /// Authored set of planet definitions. Assigned on ObjectDatabaseSO.
    /// </summary>
    [CreateAssetMenu(fileName = "PlanetDatabaseSO", menuName = "Endless Heros/Planet Database")]
    public sealed class PlanetDatabaseSO : ScriptableObject
    {
        [SerializeField]
        private List<PlanetDefinition> _planets = new();

        public IReadOnlyList<PlanetDefinition> Planets => _planets;
    }
}
