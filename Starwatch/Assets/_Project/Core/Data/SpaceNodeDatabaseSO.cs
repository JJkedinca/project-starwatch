using System.Collections.Generic;
using UnityEngine;

namespace EndlessHeros.Core.Data
{
    /// <summary>
    /// Authored map nodes. Assigned on ObjectDatabaseSO.
    /// </summary>
    [CreateAssetMenu(fileName = "SpaceNodeDatabaseSO", menuName = "Endless Heros/Space Node Database")]
    public sealed class SpaceNodeDatabaseSO : ScriptableObject
    {
        [SerializeField]
        private List<SpaceNodeDefinition> _nodes = new();

        public IReadOnlyList<SpaceNodeDefinition> Nodes => _nodes;
    }
}
