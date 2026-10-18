using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// One offered project.
    /// </summary>
    internal sealed class ResearchOfferModel
    {
        public ContentId Id { get; }

        public string Name { get; }

        public bool CanStart { get; }

        public string EffectText { get; }

        public ResearchOfferModel(ContentId id, string name, bool canStart, string effectText)
        {
            Id = id;
            Name = name ?? string.Empty;
            CanStart = canStart;
            EffectText = effectText ?? string.Empty;
        }
    }

    /// <summary>
    /// Research panel contents. The view does not derive these.
    /// </summary>
    internal sealed class ResearchPanelModel
    {
        public static ResearchPanelModel Empty { get; } = new(
            new List<ResearchOfferModel>(),
            hasActive: false,
            activeId: default,
            activeName: string.Empty,
            progress: 0f,
            cost: 0f,
            sciencePerSecond: 0f,
            scienceDropped: false,
            new List<string>(),
            activeEffectText: string.Empty,
            completedTooltip: string.Empty);

        public IReadOnlyList<ResearchOfferModel> Offers { get; }

        public bool HasActive { get; }

        public ContentId ActiveId { get; }

        public string ActiveName { get; }

        public float Progress { get; }

        public float Cost { get; }

        public float SciencePerSecond { get; }

        public bool ScienceDropped { get; }

        public IReadOnlyList<string> CompletedNames { get; }

        public string ActiveEffectText { get; }

        public string CompletedTooltip { get; }

        public ResearchPanelModel(
            IReadOnlyList<ResearchOfferModel> offers,
            bool hasActive,
            ContentId activeId,
            string activeName,
            float progress,
            float cost,
            float sciencePerSecond,
            bool scienceDropped,
            IReadOnlyList<string> completedNames,
            string activeEffectText,
            string completedTooltip)
        {
            Offers = offers ?? new List<ResearchOfferModel>();
            HasActive = hasActive;
            ActiveId = activeId;
            ActiveName = activeName ?? string.Empty;
            Progress = progress;
            Cost = cost;
            SciencePerSecond = sciencePerSecond;
            ScienceDropped = scienceDropped;
            CompletedNames = completedNames ?? new List<string>();
            ActiveEffectText = activeEffectText ?? string.Empty;
            CompletedTooltip = completedTooltip ?? string.Empty;
        }
    }
}
