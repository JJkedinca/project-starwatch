using ProjectStarwatch.Core.Identifiers;
using UnityEngine;
using EntityId = ProjectStarwatch.Core.Identifiers.EntityId;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Parked beside a node, or animating along a travel leg.
    /// </summary>
    internal enum FleetMarkerKind
    {
        Parked = 0,
        Transit = 1,
    }

    /// <summary>
    /// One fleet arrow on the star map. Parked markers aggregate fleets at a node;
    /// transit markers are one per traveling fleet.
    /// </summary>
    internal readonly struct FleetMarkerModel
    {
        public FleetMarkerKind Kind { get; }

        public EntityId FleetId { get; }

        public ContentId NodeId { get; }

        public ContentId DestinationNodeId { get; }

        public Vector2 MapPosition { get; }

        public Vector2 DestinationMapPosition { get; }

        public int Count { get; }

        public bool IsSelected { get; }

        /// <summary>
        /// Seconds left until arrival. Used as the USS transition duration.
        /// </summary>
        public float RemainingSeconds { get; }

        /// <summary>
        /// Full trip length in seconds. Used to place a mid-flight marker.
        /// </summary>
        public float TotalSeconds { get; }

        public bool IsTransit => Kind == FleetMarkerKind.Transit;

        public FleetMarkerModel(
            FleetMarkerKind kind,
            EntityId fleetId,
            ContentId nodeId,
            ContentId destinationNodeId,
            Vector2 mapPosition,
            Vector2 destinationMapPosition,
            int count,
            bool isSelected,
            float remainingSeconds,
            float totalSeconds)
        {
            Kind = kind;
            FleetId = fleetId;
            NodeId = nodeId;
            DestinationNodeId = destinationNodeId;
            MapPosition = mapPosition;
            DestinationMapPosition = destinationMapPosition;
            Count = count < 1 ? 1 : count;
            IsSelected = isSelected;
            RemainingSeconds = remainingSeconds < 0f ? 0f : remainingSeconds;
            TotalSeconds = totalSeconds < 0f ? 0f : totalSeconds;
        }

        /// <summary>
        /// Stable key for view element reuse.
        /// </summary>
        internal string Key
        {
            get
            {
                if (Kind == FleetMarkerKind.Transit)
                {
                    return "transit:" + FleetId.Value;
                }

                return "parked:" + (NodeId.IsEmpty ? string.Empty : NodeId.Value);
            }
        }
    }
}
