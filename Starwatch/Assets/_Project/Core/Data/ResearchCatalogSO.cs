using System.Collections.Generic;
using EndlessHeros.Core.Identifiers;
using UnityEngine;

namespace EndlessHeros.Core.Data
{
    /// <summary>
    /// Every research definition in the game. A run keeps its own list of currently available projects.
    /// </summary>
    [CreateAssetMenu(fileName = "ResearchCatalog", menuName = "Endless Heros/Research Catalog")]
    public sealed class ResearchCatalogSO : ScriptableObject
    {
        [Tooltip("Every research in the game, in offer-draw order before the shuffle.")]
        [SerializeField]
        private List<ResearchDefinitionSO> _researches = new();

        private Dictionary<ContentId, ResearchDefinitionSO> _byId = new();
        private bool _indexed;

        public IReadOnlyList<ResearchDefinitionSO> Researches => _researches;

        private void OnEnable()
        {
            _indexed = false;
        }

        /// <summary>
        /// Fills a runtime catalog. The authored asset is the play-mode source.
        /// </summary>
        internal void SetContent(IReadOnlyList<ResearchDefinitionSO> researches)
        {
            _researches = Copy(researches);
            _indexed = false;
        }

        public bool TryGet(ContentId researchId, out ResearchDefinitionSO project)
        {
            EnsureIndexed();
            if (researchId.IsEmpty)
            {
                project = null;
                return false;
            }

            return _byId.TryGetValue(researchId, out project) && project != null;
        }

        private void EnsureIndexed()
        {
            _researches ??= new List<ResearchDefinitionSO>();
            _byId ??= new Dictionary<ContentId, ResearchDefinitionSO>();
            if (_indexed)
            {
                return;
            }

            _byId.Clear();
            for (var i = 0; i < _researches.Count; i++)
            {
                var project = _researches[i];
                if (project == null || project.Id.IsEmpty)
                {
                    continue;
                }

                if (_byId.ContainsKey(project.Id))
                {
                    Debug.LogWarning($"ResearchCatalogSO ignores duplicate research id '{project.Id.Value}'.", this);
                    continue;
                }

                _byId.Add(project.Id, project);
            }

            _indexed = true;
        }

        private static List<ResearchDefinitionSO> Copy(IReadOnlyList<ResearchDefinitionSO> source)
        {
            List<ResearchDefinitionSO> copy = new();
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
