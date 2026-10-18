using System;
using System.Collections.Generic;
using EndlessHeros.Core.Identifiers;

namespace EndlessHeros.Overworld.UI
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

    /// <summary>
    /// Comparable identity of a research panel. Built without the display lists.
    /// </summary>
    internal readonly struct ResearchPanelStamp : IEquatable<ResearchPanelStamp>
    {
        public static ResearchPanelStamp Empty { get; } = new(
            hasRun: false,
            hasActive: false,
            activeId: default,
            points: 0f,
            cost: 0f,
            sciencePerSecond: 0f,
            scienceDropped: false,
            offer0: default,
            offer1: default,
            offer2: default,
            completedCount: 0,
            completedHash: 0);

        private readonly bool _hasRun;
        private readonly bool _hasActive;
        private readonly ContentId _activeId;
        private readonly float _points;
        private readonly float _cost;
        private readonly float _sciencePerSecond;
        private readonly bool _scienceDropped;
        private readonly ContentId _offer0;
        private readonly ContentId _offer1;
        private readonly ContentId _offer2;
        private readonly int _completedCount;
        private readonly int _completedHash;

        public ResearchPanelStamp(
            bool hasRun,
            bool hasActive,
            ContentId activeId,
            float points,
            float cost,
            float sciencePerSecond,
            bool scienceDropped,
            ContentId offer0,
            ContentId offer1,
            ContentId offer2,
            int completedCount,
            int completedHash)
        {
            _hasRun = hasRun;
            _hasActive = hasActive;
            _activeId = activeId;
            _points = points;
            _cost = cost;
            _sciencePerSecond = sciencePerSecond;
            _scienceDropped = scienceDropped;
            _offer0 = offer0;
            _offer1 = offer1;
            _offer2 = offer2;
            _completedCount = completedCount;
            _completedHash = completedHash;
        }

        public bool Equals(ResearchPanelStamp other)
        {
            return _hasRun == other._hasRun
                && _hasActive == other._hasActive
                && _activeId.Equals(other._activeId)
                && _points.Equals(other._points)
                && _cost.Equals(other._cost)
                && _sciencePerSecond.Equals(other._sciencePerSecond)
                && _scienceDropped == other._scienceDropped
                && _offer0.Equals(other._offer0)
                && _offer1.Equals(other._offer1)
                && _offer2.Equals(other._offer2)
                && _completedCount == other._completedCount
                && _completedHash == other._completedHash;
        }

        public override bool Equals(object obj)
        {
            return obj is ResearchPanelStamp other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_hasRun, _hasActive, _activeId, _points, _cost, _sciencePerSecond, _scienceDropped, _offer0);
        }
    }
}
