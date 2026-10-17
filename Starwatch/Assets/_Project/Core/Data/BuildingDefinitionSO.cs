using System.Collections.Generic;
using EndlessHeros.Core.Identifiers;
using UnityEngine;

namespace EndlessHeros.Core.Data
{
    /// <summary>
    /// Authored building. Footprint is per building. Level scales output and upkeep.
    /// </summary>
    [CreateAssetMenu(fileName = "Building", menuName = "Endless Heros/Building")]
    public sealed class BuildingDefinitionSO : ScriptableObject
    {
        [Tooltip("Stable content id used by commands and colony state. This is not the asset file name.")]
        [SerializeField]
        private string _id;

        [Tooltip("Name shown when the player inspects this building.")]
        [SerializeField]
        private string _displayName;

        [Tooltip("Space this building occupies on a planet. Level does not change the footprint.")]
        [SerializeField]
        private int _footprint = 1;

        [Tooltip("Resource this building produces each second of simulation time.")]
        [field: SerializeField]
        public ResourceKind OutputKind { get; private set; }

        [Tooltip("Output granted per level before planet modifiers.")]
        [SerializeField]
        private int _outputPerLevel;

        [Tooltip("Resource this building consumes each second of simulation time.")]
        [field: SerializeField]
        public ResourceKind UpkeepKind { get; private set; }

        [Tooltip("Upkeep paid per level. Planet modifiers do not change upkeep.")]
        [SerializeField]
        private int _upkeepPerLevel;

        [Tooltip("Highest level. The building is placed at level 1.")]
        [SerializeField]
        private int _maxLevel = 1;

        [Tooltip("Price to place this building at level 1.")]
        [field: SerializeField]
        public ResourceBundle BuildCost { get; private set; }

        [Tooltip("One price per upgrade. The first entry raises the building from level 1 to 2.")]
        [SerializeField]
        private List<ResourceBundle> _upgradeCosts = new();

        public ContentId Id => ContentId.From(_id);

        public string DisplayName => _displayName ?? string.Empty;

        public int Footprint => _footprint < 0 ? 0 : _footprint;

        public int OutputPerLevel => _outputPerLevel < 0 ? 0 : _outputPerLevel;

        public int UpkeepPerLevel => _upkeepPerLevel < 0 ? 0 : _upkeepPerLevel;

        public int MaxLevel => _maxLevel < 1 ? 1 : _maxLevel;

        public IReadOnlyList<ResourceBundle> UpgradeCosts => _upgradeCosts;

        /// <summary>
        /// Cost to raise a building from <paramref name="currentLevel"/> to the next level.
        /// Index 0 is the upgrade from level 1 to 2.
        /// </summary>
        public bool TryGetUpgradeCost(int currentLevel, out ResourceBundle cost)
        {
            var index = currentLevel - 1;
            if (_upgradeCosts == null || index < 0 || index >= _upgradeCosts.Count)
            {
                cost = default;
                return false;
            }

            cost = _upgradeCosts[index];
            return true;
        }
    }
}
