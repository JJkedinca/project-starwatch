using System.Collections.Generic;
using UnityEngine;

namespace ProjectStarwatch.Core.Data
{
    /// <summary>
    /// Authored star systems. Assigned on ObjectDatabaseSO.
    /// </summary>
    [CreateAssetMenu(fileName = "StarSystemDatabaseSO", menuName = "Starwatch/Star System Database")]
    public sealed class StarSystemDatabaseSO : ScriptableObject
    {
        [SerializeField]
        private List<StarSystemDefinition> _starSystems = new();

        public IReadOnlyList<StarSystemDefinition> StarSystems => _starSystems;
    }
}
