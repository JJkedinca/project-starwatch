using System.Collections.Generic;
using EndlessHeros.Core.Identifiers;
using UnityEngine;

namespace EndlessHeros.Core.Data
{
    /// <summary>
    /// One research project and the effect it grants when completed.
    /// </summary>
    [CreateAssetMenu(fileName = "Research", menuName = "Endless Heros/Research")]
    public sealed class ResearchDefinitionSO : ScriptableObject
    {
        [Tooltip("Stable content id used by commands and the archive. This is not the asset file name.")]
        [SerializeField]
        private string _id;

        [Tooltip("Name shown in the research panel.")]
        [SerializeField]
        private string _displayName;

        [SerializeField]
        private ResearchDomain _domain;

        [Tooltip("Science points required to complete this project.")]
        [SerializeField]
        private float _pointCost = 40f;

        [Tooltip("Research ids that must already be completed.")]
        [SerializeField]
        private List<string> _prerequisiteIds = new();

        [SerializeField]
        private TechnologyEffect _effect;

        public ContentId Id => ContentId.From(_id);

        public string DisplayName => _displayName ?? string.Empty;

        public ResearchDomain Domain => _domain;

        public float PointCost => _pointCost > 0f ? _pointCost : 0f;

        public TechnologyEffect Effect => _effect;

        private void OnValidate()
        {
            if (_pointCost > 0f)
            {
                return;
            }

            Debug.LogWarning(
                $"{name} needs a Science point cost greater than zero. A zero-cost project cannot complete.",
                this);
        }

        public bool HasPrerequisite(ContentId researchId)
        {
            if (researchId.IsEmpty || _prerequisiteIds == null)
            {
                return false;
            }

            for (var i = 0; i < _prerequisiteIds.Count; i++)
            {
                if (ContentId.From(_prerequisiteIds[i]) == researchId)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// True when every prerequisite id is in <paramref name="completed"/>.
        /// </summary>
        public bool PrerequisitesMet(IResearchKnowledgeQuery completed)
        {
            if (_prerequisiteIds == null || _prerequisiteIds.Count == 0)
            {
                return true;
            }

            for (var i = 0; i < _prerequisiteIds.Count; i++)
            {
                var id = ContentId.From(_prerequisiteIds[i]);
                if (id.IsEmpty || completed == null || !completed.Contains(id))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Fills a runtime project. The authored asset is the play-mode source.
        /// </summary>
        internal void SetContent(
            string id,
            string displayName,
            ResearchDomain domain,
            float pointCost,
            IReadOnlyList<string> prerequisiteIds,
            TechnologyEffect effect)
        {
            _id = id ?? string.Empty;
            _displayName = displayName ?? string.Empty;
            _domain = domain;
            _pointCost = pointCost;
            _prerequisiteIds = Copy(prerequisiteIds);
            _effect = effect;
        }

        private static List<string> Copy(IReadOnlyList<string> source)
        {
            List<string> copy = new();
            if (source == null)
            {
                return copy;
            }

            for (var i = 0; i < source.Count; i++)
            {
                copy.Add(source[i] ?? string.Empty);
            }

            return copy;
        }
    }
}
