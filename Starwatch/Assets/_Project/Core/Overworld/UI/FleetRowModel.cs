using ProjectStarwatch.Core.Identifiers;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Location and optional in-transit destination for a fleet row.
    /// </summary>
    internal readonly struct FleetRouteModel
    {
        public ContentId LocationId { get; }

        public string LocationName { get; }

        public ContentId DestinationId { get; }

        public string DestinationName { get; }

        public float ArrivalSeconds { get; }

        public bool IsTraveling { get; }

        public FleetRouteModel(
            ContentId locationId,
            string locationName,
            ContentId destinationId,
            string destinationName,
            float arrivalSeconds,
            bool isTraveling)
        {
            LocationId = locationId;
            LocationName = locationName ?? string.Empty;
            DestinationId = destinationId;
            DestinationName = destinationName ?? string.Empty;
            ArrivalSeconds = arrivalSeconds < 0f ? 0f : arrivalSeconds;
            IsTraveling = isTraveling;
        }
    }

    /// <summary>
    /// Ship makeup shown on a fleet row.
    /// </summary>
    internal readonly struct FleetCompositionModel
    {
        public int ShipCount { get; }

        public string Title { get; }

        public FleetCompositionModel(int shipCount, string title)
        {
            ShipCount = shipCount < 0 ? 0 : shipCount;
            Title = title ?? string.Empty;
        }
    }

    /// <summary>
    /// One fleet row on the roster, star map, or in a star system.
    /// </summary>
    internal readonly struct FleetRowModel
    {
        public EntityId FleetId { get; }

        public FleetRouteModel Route { get; }

        public FleetCompositionModel Composition { get; }

        public bool IsSelected { get; }

        public float TravelSpeed { get; }

        public string Label { get; }

        public FleetRowModel(
            EntityId fleetId,
            FleetRouteModel route,
            FleetCompositionModel composition,
            bool isSelected,
            float travelSpeed,
            string label)
        {
            FleetId = fleetId;
            Route = route;
            Composition = composition;
            IsSelected = isSelected;
            TravelSpeed = travelSpeed;
            Label = label ?? string.Empty;
        }
    }
}
