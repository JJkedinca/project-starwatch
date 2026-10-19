using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using UnityEngine;

namespace ProjectStarwatch.Core.Data
{
    /// <summary>
    /// Every ship class in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "ShipCatalog", menuName = "Starwatch/Ship Catalog")]
    public sealed class ShipCatalogSO : ScriptableObject
    {
        [Tooltip("Every ship class, in the order the shipyard lists them.")]
        [SerializeField]
        private List<ShipClassDefinitionSO> _classes = new();

        private Dictionary<ContentId, ShipClassDefinitionSO> _byId = new();
        private bool _indexed;

        public IReadOnlyList<ShipClassDefinitionSO> Classes => _classes;

        private void OnEnable()
        {
            _indexed = false;
        }

        /// <summary>
        /// Fills a runtime catalog. The authored asset is the play-mode source.
        /// </summary>
        internal void SetContent(IReadOnlyList<ShipClassDefinitionSO> classes)
        {
            _classes = Copy(classes);
            _indexed = false;
        }

        public bool TryGet(ContentId classId, out ShipClassDefinitionSO shipClass)
        {
            EnsureIndexed();
            if (classId.IsEmpty)
            {
                shipClass = null;
                return false;
            }

            return _byId.TryGetValue(classId, out shipClass) && shipClass != null;
        }

        private void EnsureIndexed()
        {
            _classes ??= new List<ShipClassDefinitionSO>();
            _byId ??= new Dictionary<ContentId, ShipClassDefinitionSO>();
            if (_indexed)
            {
                return;
            }

            _byId.Clear();
            for (var i = 0; i < _classes.Count; i++)
            {
                var shipClass = _classes[i];
                if (shipClass == null || shipClass.Id.IsEmpty)
                {
                    continue;
                }

                if (_byId.ContainsKey(shipClass.Id))
                {
                    Debug.LogWarning($"ShipCatalogSO ignores duplicate ship class id '{shipClass.Id.Value}'.", this);
                    continue;
                }

                _byId.Add(shipClass.Id, shipClass);
            }

            _indexed = true;
        }

        private static List<ShipClassDefinitionSO> Copy(IReadOnlyList<ShipClassDefinitionSO> source)
        {
            var copy = new List<ShipClassDefinitionSO>();
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
