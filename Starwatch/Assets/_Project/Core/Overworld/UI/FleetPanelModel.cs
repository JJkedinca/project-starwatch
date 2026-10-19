using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// One ship class inside the open fleet panel.
    /// </summary>
    internal readonly struct FleetStackRow
    {
        public ContentId ClassId { get; }

        public int Count { get; }

        public float SpeedFactor { get; }

        public string DisplayName { get; }

        public bool HasClass { get; }

        public FleetStackRow(
            ContentId classId,
            int count,
            float speedFactor,
            string displayName,
            bool hasClass)
        {
            ClassId = classId;
            Count = count;
            SpeedFactor = speedFactor;
            DisplayName = displayName ?? string.Empty;
            HasClass = hasClass;
        }
    }

    /// <summary>
    /// An idle fleet at the same node that can be merged into the open fleet.
    /// The open fleet is the merge target. This entry is the source.
    /// </summary>
    internal readonly struct FleetMergeCandidate
    {
        public EntityId FleetId { get; }

        public int ShipCount { get; }

        public FleetMergeCandidate(EntityId fleetId, int shipCount)
        {
            FleetId = fleetId;
            ShipCount = shipCount;
        }
    }

    /// <summary>
    /// Inspection of the selected fleet. Closed when nothing is selected.
    /// </summary>
    internal sealed class FleetPanelModel
    {
        public static FleetPanelModel Closed { get; } = new(
            false,
            EntityId.None,
            default,
            false,
            1f,
            false,
            Array.Empty<FleetStackRow>(),
            Array.Empty<FleetMergeCandidate>());

        public bool IsOpen { get; }

        public EntityId FleetId { get; }

        public ContentId LocationId { get; }

        public bool IsTraveling { get; }

        public float TravelSpeed { get; }

        public bool CanSplit { get; }

        public IReadOnlyList<FleetStackRow> Stacks { get; }

        public IReadOnlyList<FleetMergeCandidate> MergeCandidates { get; }

        public FleetPanelModel(
            bool isOpen,
            EntityId fleetId,
            ContentId locationId,
            bool isTraveling,
            float travelSpeed,
            bool canSplit,
            IReadOnlyList<FleetStackRow> stacks,
            IReadOnlyList<FleetMergeCandidate> mergeCandidates)
        {
            IsOpen = isOpen;
            FleetId = fleetId;
            LocationId = locationId;
            IsTraveling = isTraveling;
            TravelSpeed = travelSpeed;
            CanSplit = canSplit;
            Stacks = stacks ?? Array.Empty<FleetStackRow>();
            MergeCandidates = mergeCandidates ?? Array.Empty<FleetMergeCandidate>();
        }
    }
}
