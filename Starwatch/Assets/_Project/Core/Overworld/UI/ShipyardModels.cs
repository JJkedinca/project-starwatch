using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Identifiers;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// One ship class the selected colony can queue.
    /// </summary>
    internal readonly struct ShipyardClassRow
    {
        public ContentId ClassId { get; }

        public string DisplayName { get; }

        public float BuildSeconds { get; }

        public IReadOnlyList<ResourceAmount> Costs { get; }

        public bool IsLocked { get; }

        public ContentId UnlockResearchId { get; }

        public string UnlockName { get; }

        public ShipyardClassRow(
            ContentId classId,
            string displayName,
            float buildSeconds,
            IReadOnlyList<ResourceAmount> costs,
            bool isLocked,
            ContentId unlockResearchId,
            string unlockName)
        {
            ClassId = classId;
            DisplayName = displayName ?? string.Empty;
            BuildSeconds = buildSeconds;
            Costs = costs ?? Array.Empty<ResourceAmount>();
            IsLocked = isLocked;
            UnlockResearchId = unlockResearchId;
            UnlockName = unlockName ?? string.Empty;
        }
    }

    /// <summary>
    /// One order in the colony build queue. The head order is the one being built.
    /// </summary>
    internal readonly struct ShipyardQueueRow
    {
        public int Index { get; }

        public ContentId ClassId { get; }

        public string DisplayName { get; }

        public float SecondsRemaining { get; }

        public float BuildSeconds { get; }

        public bool IsHead { get; }

        public ShipyardQueueRow(
            int index,
            ContentId classId,
            string displayName,
            float secondsRemaining,
            float buildSeconds,
            bool isHead)
        {
            Index = index;
            ClassId = classId;
            DisplayName = displayName ?? string.Empty;
            SecondsRemaining = secondsRemaining;
            BuildSeconds = buildSeconds;
            IsHead = isHead;
        }
    }

    /// <summary>
    /// Shipyard section for the selected colony. Hidden when that colony cannot build ships.
    /// </summary>
    internal sealed class ShipyardModel
    {
        public static ShipyardModel Hidden { get; } = new(
            false,
            Array.Empty<ShipyardClassRow>(),
            Array.Empty<ShipyardQueueRow>());

        public bool IsShown { get; }

        public IReadOnlyList<ShipyardClassRow> Classes { get; }

        public IReadOnlyList<ShipyardQueueRow> Queue { get; }

        public ShipyardModel(
            bool isShown,
            IReadOnlyList<ShipyardClassRow> classes,
            IReadOnlyList<ShipyardQueueRow> queue)
        {
            IsShown = isShown;
            Classes = classes ?? Array.Empty<ShipyardClassRow>();
            Queue = queue ?? Array.Empty<ShipyardQueueRow>();
        }
    }
}
