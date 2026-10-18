using System.Collections.Generic;
using EndlessHeros.Core.Identifiers;

namespace EndlessHeros.Core.Run.Timeline
{
    /// <summary>
    /// In-progress research for one timeline. A new timeline starts this over.
    /// </summary>
    public sealed class ResearchProgress
    {
        public ContentId ActiveId { get; private set; }

        public float Points { get; private set; }

        public int RefreshCount { get; private set; }

        public IReadOnlyList<ContentId> Offers => _offers;

        public bool HasActive => !ActiveId.IsEmpty;

        private readonly List<ContentId> _offers = new();

        public bool IsOffered(ContentId researchId)
        {
            for (var i = 0; i < _offers.Count; i++)
            {
                if (_offers[i] == researchId)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the current refresh index, then advances it.
        /// </summary>
        public int NextRefresh()
        {
            var current = RefreshCount;
            RefreshCount++;
            return current;
        }

        public void SetOffers(IReadOnlyList<ContentId> offers)
        {
            _offers.Clear();
            if (offers == null)
            {
                return;
            }

            for (var i = 0; i < offers.Count; i++)
            {
                if (!offers[i].IsEmpty)
                {
                    _offers.Add(offers[i]);
                }
            }
        }

        public void Start(ContentId researchId)
        {
            ActiveId = researchId;
            Points = 0f;
        }

        public void AddPoints(float amount)
        {
            if (amount > 0f)
            {
                Points += amount;
            }
        }

        public void ClearActive()
        {
            ActiveId = default;
            Points = 0f;
        }
    }
}
