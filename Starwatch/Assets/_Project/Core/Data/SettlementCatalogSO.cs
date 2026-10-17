using System.Collections.Generic;
using EndlessHeros.Core.Identifiers;
using UnityEngine;

namespace EndlessHeros.Core.Data
{
    /// <summary>
    /// References the building assets available to colonies.
    /// </summary>
    [CreateAssetMenu(fileName = "SettlementCatalog", menuName = "Endless Heros/Settlement Catalog")]
    public sealed class SettlementCatalogSO : ScriptableObject
    {
        [Tooltip("Building assets included in this catalog.")]
        [SerializeField]
        private List<BuildingDefinitionSO> _buildings = new();

        /// <summary>
        /// Fills a runtime catalog. The authored asset is the play-mode source.
        /// </summary>
        internal void SetContent(IReadOnlyList<BuildingDefinitionSO> buildings)
        {
            _buildings = Copy(buildings);
        }

        public bool TryGetBuilding(ContentId buildingId, out BuildingDefinitionSO building)
        {
            for (var i = 0; i < _buildings.Count; i++)
            {
                var candidate = _buildings[i];
                if (candidate != null && candidate.Id == buildingId)
                {
                    building = candidate;
                    return true;
                }
            }

            building = null;
            return false;
        }

        private static List<T> Copy<T>(IReadOnlyList<T> source)
        {
            List<T> copy = new();
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
