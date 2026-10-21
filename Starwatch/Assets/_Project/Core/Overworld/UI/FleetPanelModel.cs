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

        public string DisplayName { get; }

        public FleetStackRow(ContentId classId, int count, string displayName)
        {
            ClassId = classId;
            Count = count;
            DisplayName = displayName ?? string.Empty;
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
    /// Selected fleet for the bottom bar and optional details in the right column.
    /// </summary>
    internal sealed class FleetPanelModel
    {
        public static FleetPanelModel Closed { get; } = new(
            hasFleet: false,
            isOpen: false,
            EntityId.None,
            default,
            1f,
            false,
            Array.Empty<FleetStackRow>(),
            Array.Empty<FleetMergeCandidate>());

        /// <summary>
        /// A fleet is selected. The bottom bar shows.
        /// </summary>
        public bool HasFleet { get; }

        /// <summary>
        /// Fleet details are open in the right column.
        /// </summary>
        public bool IsOpen { get; }

        public EntityId FleetId { get; }

        public FleetRouteModel Route { get; }

        public float TravelSpeed { get; }

        public bool CanSplit { get; }

        public IReadOnlyList<FleetStackRow> Stacks { get; }

        public IReadOnlyList<FleetMergeCandidate> MergeCandidates { get; }

        public int ShipCount
        {
            get
            {
                var total = 0;
                for (var i = 0; i < Stacks.Count; i++)
                {
                    total += Stacks[i].Count;
                }

                return total;
            }
        }

        public FleetPanelModel(
            bool hasFleet,
            bool isOpen,
            EntityId fleetId,
            FleetRouteModel route,
            float travelSpeed,
            bool canSplit,
            IReadOnlyList<FleetStackRow> stacks,
            IReadOnlyList<FleetMergeCandidate> mergeCandidates)
        {
            HasFleet = hasFleet;
            IsOpen = isOpen;
            FleetId = fleetId;
            Route = route;
            TravelSpeed = travelSpeed;
            CanSplit = canSplit;
            Stacks = stacks ?? Array.Empty<FleetStackRow>();
            MergeCandidates = mergeCandidates ?? Array.Empty<FleetMergeCandidate>();
        }
    }
}
