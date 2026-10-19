using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using UnityEngine;

namespace ProjectStarwatch.Core.Data
{
    /// <summary>
    /// Authored building. Footprint is per building. Level scales every output and upkeep entry.
    /// </summary>
    [CreateAssetMenu(fileName = "Building", menuName = "Starwatch/Building")]
    public sealed class BuildingDefinitionSO : ScriptableObject
    {
        private static readonly IReadOnlyList<ResourceAmount> EmptyRates = Array.Empty<ResourceAmount>();

        [Tooltip("Stable content id used by commands and colony state. This is not the asset file name.")]
        [SerializeField]
        private string _id;

        [Tooltip("Name shown when the player inspects this building.")]
        [SerializeField]
        private string _displayName;

        [Tooltip("Space this building occupies on a planet. Level does not change the footprint.")]
        [SerializeField]
        private int _footprint = 1;

        [Tooltip("Output granted per level before planet modifiers. A building may produce more than one resource.")]
        [SerializeField]
        private List<ResourceAmount> _output = new();

        [Tooltip("Upkeep paid per level. Planet modifiers do not change upkeep. A building may pay more than one resource.")]
        [SerializeField]
        private List<ResourceAmount> _upkeep = new();

        [Tooltip("Highest level. The building is placed at level 1.")]
        [SerializeField]
        private int _maxLevel = 1;

        [Tooltip("Price to place this building at level 1.")]
        [field: SerializeField]
        public ResourceBundle BuildCost { get; private set; }

        [Tooltip("One price per upgrade. The first entry raises the building from level 1 to 2.")]
        [SerializeField]
        private List<ResourceBundle> _upgradeCosts = new();

        [Tooltip("A colony with this building can queue ships. Placing it does not require research.")]
        [SerializeField]
        private bool _enablesShipConstruction;

        public ContentId Id => ContentId.From(_id);

        public string DisplayName => _displayName ?? string.Empty;

        public int Footprint => _footprint < 0 ? 0 : _footprint;

        public IReadOnlyList<ResourceAmount> Output => _output ?? EmptyRates;

        public IReadOnlyList<ResourceAmount> Upkeep => _upkeep ?? EmptyRates;

        public int MaxLevel => _maxLevel < 1 ? 1 : _maxLevel;

        public bool EnablesShipConstruction => _enablesShipConstruction;

        public IReadOnlyList<ResourceBundle> UpgradeCosts => _upgradeCosts;

        private void OnValidate()
        {
            WarnIfSciencePriced(_upkeep, "upkeep");
            WarnIfSciencePriced(BuildCost.Amounts, "build cost");
            if (_upgradeCosts == null)
            {
                return;
            }

            for (var i = 0; i < _upgradeCosts.Count; i++)
            {
                WarnIfSciencePriced(_upgradeCosts[i].Amounts, "upgrade cost");
            }
        }

        private void WarnIfSciencePriced(IReadOnlyList<ResourceAmount> amounts, string slot)
        {
            if (ResourceAmount.Total(amounts, ResourceKind.Science) <= 0f)
            {
                return;
            }

            Debug.LogWarning(
                $"{name} prices {slot} in Science, which is not a stockpile and cannot be paid.",
                this);
        }

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

        /// <summary>
        /// Fills a runtime building. The authored asset is the play-mode source.
        /// </summary>
        internal void SetContent(string id, IReadOnlyList<ResourceAmount> output, IReadOnlyList<ResourceAmount> upkeep)
        {
            _id = id ?? string.Empty;
            _output = Copy(output);
            _upkeep = Copy(upkeep);
        }

        private static List<ResourceAmount> Copy(IReadOnlyList<ResourceAmount> source)
        {
            List<ResourceAmount> copy = new();
            if (source == null)
            {
                return copy;
            }

            for (var i = 0; i < source.Count; i++)
            {
                copy.Add(source[i]);
            }

            return copy;
        }
    }
}
