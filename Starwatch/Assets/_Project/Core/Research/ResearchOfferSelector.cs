using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Simulation;

namespace ProjectStarwatch.Research
{
    /// <summary>
    /// Picks at most three offers. A pinned project takes a slot, then the rest are shuffled.
    /// </summary>
    internal static class ResearchOfferSelector
    {
        // TODO: This can change depending on modifiers - we should move away from const at some point  
        internal const int kSlotCount = 3;

        public static void Fill(
            List<ContentId> eligible,
            IReadOnlyList<ContentId> pinned,
            int seed,
            int refresh,
            List<ContentId> destination)
        {
            destination.Clear();
            if (pinned != null)
            {
                for (var i = 0; i < pinned.Count && destination.Count < kSlotCount; i++)
                {
                    var id = pinned[i];
                    if (id.IsEmpty || Contains(destination, id) || !Remove(eligible, id))
                    {
                        continue;
                    }

                    destination.Add(id);
                }
            }

            Shuffle(eligible, unchecked(seed + refresh * 104729));
            for (var i = 0; i < eligible.Count && destination.Count < kSlotCount; i++)
            {
                destination.Add(eligible[i]);
            }
        }

        private static void Shuffle(List<ContentId> ids, int seed)
        {
            var random = new DeterministicRandom(seed);
            for (var i = ids.Count - 1; i > 0; i--)
            {
                var swap = random.Next(0, i + 1);
                var held = ids[i];
                ids[i] = ids[swap];
                ids[swap] = held;
            }
        }

        private static bool Contains(List<ContentId> ids, ContentId id)
        {
            for (var i = 0; i < ids.Count; i++)
            {
                if (ids[i] == id)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Remove(List<ContentId> ids, ContentId id)
        {
            for (var i = 0; i < ids.Count; i++)
            {
                if (ids[i] != id)
                {
                    continue;
                }

                ids.RemoveAt(i);
                return true;
            }

            return false;
        }
    }
}
