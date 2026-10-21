using System.Collections.Generic;
using ProjectStarwatch.Core.Data;
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

        public ResearchDomain Domain { get; }

        public bool CanStart { get; }

        public string EffectText { get; }

        public float Cost { get; }

        public float EstimatedSeconds { get; }

        public string UnavailableReason { get; }

        public ResearchOfferModel(
            ContentId id,
            string name,
            ResearchDomain domain,
            bool canStart,
            string effectText,
            float cost,
            float estimatedSeconds,
            string unavailableReason)
        {
            Id = id;
            Name = name ?? string.Empty;
            Domain = domain;
            CanStart = canStart;
            EffectText = effectText ?? string.Empty;
            Cost = cost < 0f ? 0f : cost;
            EstimatedSeconds = estimatedSeconds < 0f ? 0f : estimatedSeconds;
            UnavailableReason = unavailableReason ?? string.Empty;
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
            estimatedSecondsLeft: 0f,
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

        public float EstimatedSecondsLeft { get; }

        public IReadOnlyList<string> CompletedNames { get; }

        public string ActiveEffectText { get; }

        public string CompletedTooltip { get; }

        public float ProgressFraction
        {
            get
            {
                if (Cost <= 0f)
                {
                    return 0f;
                }

                var fraction = Progress / Cost;
                if (fraction < 0f)
                {
                    return 0f;
                }

                return fraction > 1f ? 1f : fraction;
            }
        }

        public ResearchPanelModel(
            IReadOnlyList<ResearchOfferModel> offers,
            bool hasActive,
            ContentId activeId,
            string activeName,
            float progress,
            float cost,
            float sciencePerSecond,
            bool scienceDropped,
            float estimatedSecondsLeft,
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
            EstimatedSecondsLeft = estimatedSecondsLeft < 0f ? 0f : estimatedSecondsLeft;
            CompletedNames = completedNames ?? new List<string>();
            ActiveEffectText = activeEffectText ?? string.Empty;
            CompletedTooltip = completedTooltip ?? string.Empty;
        }
    }
}
