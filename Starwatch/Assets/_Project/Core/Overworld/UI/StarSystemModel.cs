using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// One planet in the open star system, including its authored orbit slot.
    /// </summary>
    internal readonly struct StarSystemPlanetModel
    {
        public ContentId PlanetId { get; }

        public PlanetPresentationState State { get; }

        /// <summary>
        /// Authored slot in the system (index in the system's planet list), not the visible-list index.
        /// </summary>
        public int OrbitIndex { get; }

        public StarSystemPlanetModel(ContentId planetId, PlanetPresentationState state, int orbitIndex)
        {
            PlanetId = planetId;
            State = state;
            OrbitIndex = orbitIndex < 0 ? 0 : orbitIndex;
        }
    }

    /// <summary>
    /// Open star system snapshot: title, planet icons, fleets, and status text.
    /// Planet / settlement detail lives on the settlement panel.
    /// </summary>
    internal sealed class StarSystemModel
    {
        public static StarSystemModel Closed { get; } = new(
            false,
            string.Empty,
            Array.Empty<StarSystemPlanetModel>(),
            Array.Empty<FleetRowModel>(),
            string.Empty,
            orbitSlotCount: 0);

        public bool IsOpen { get; }

        public string Title { get; }

        public IReadOnlyList<StarSystemPlanetModel> Planets { get; }

        public IReadOnlyList<FleetRowModel> Fleets { get; }

        public string Status { get; }

        /// <summary>
        /// Total planet slots in the system (including hidden). Used to space orbit radii.
        /// </summary>
        public int OrbitSlotCount { get; }

        public StarSystemModel(
            bool isOpen,
            string title,
            IReadOnlyList<StarSystemPlanetModel> planets,
            IReadOnlyList<FleetRowModel> fleets,
            string status,
            int orbitSlotCount)
        {
            IsOpen = isOpen;
            Title = title ?? string.Empty;
            Planets = planets ?? Array.Empty<StarSystemPlanetModel>();
            Fleets = fleets ?? Array.Empty<FleetRowModel>();
            Status = status ?? string.Empty;
            OrbitSlotCount = orbitSlotCount < 0 ? 0 : orbitSlotCount;
        }
    }
}
