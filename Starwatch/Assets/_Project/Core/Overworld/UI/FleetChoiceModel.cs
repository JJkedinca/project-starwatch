using ProjectStarwatch.Core.Identifiers;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// One fleet the player can select on the star map or in a star system.
    /// </summary>
    internal readonly struct FleetChoiceModel
    {
        public EntityId FleetId { get; }

        public ContentId LocationId { get; }

        public int ShipCount { get; }

        public bool IsTraveling { get; }

        public bool IsSelected { get; }

        public float TravelSpeed { get; }

        public string Label { get; }

        public FleetChoiceModel(
            EntityId fleetId,
            ContentId locationId,
            int shipCount,
            bool isTraveling,
            bool isSelected,
            float travelSpeed,
            string label)
        {
            FleetId = fleetId;
            LocationId = locationId;
            ShipCount = shipCount;
            IsTraveling = isTraveling;
            IsSelected = isSelected;
            TravelSpeed = travelSpeed;
            Label = label ?? string.Empty;
        }
    }
}
