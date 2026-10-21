using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// One planet icon in the open star system.
    /// </summary>
    internal readonly struct StarSystemPlanetModel
    {
        public ContentId PlanetId { get; }

        public PlanetPresentationState State { get; }

        public StarSystemPlanetModel(ContentId planetId, PlanetPresentationState state)
        {
            PlanetId = planetId;
            State = state;
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
            string.Empty);

        public bool IsOpen { get; }

        public string Title { get; }

        public IReadOnlyList<StarSystemPlanetModel> Planets { get; }

        public IReadOnlyList<FleetRowModel> Fleets { get; }

        public string Status { get; }

        public StarSystemModel(
            bool isOpen,
            string title,
            IReadOnlyList<StarSystemPlanetModel> planets,
            IReadOnlyList<FleetRowModel> fleets,
            string status)
        {
            IsOpen = isOpen;
            Title = title ?? string.Empty;
            Planets = planets ?? Array.Empty<StarSystemPlanetModel>();
            Fleets = fleets ?? Array.Empty<FleetRowModel>();
            Status = status ?? string.Empty;
        }
    }
}
