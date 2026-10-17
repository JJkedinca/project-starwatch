using System;
using System.Collections.Generic;
using EndlessHeros.Core.Identifiers;

namespace EndlessHeros.Overworld.UI
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
    /// Open star system snapshot: title, planet icons, selected planet, and status text.
    /// </summary>
    internal sealed class StarSystemModel
    {
        public static StarSystemModel Closed { get; } = new(
            false,
            string.Empty,
            Array.Empty<StarSystemPlanetModel>(),
            false,
            default,
            string.Empty);

        public bool IsOpen { get; }

        public string Title { get; }

        public IReadOnlyList<StarSystemPlanetModel> Planets { get; }

        public bool ShowPlanetDetail { get; }

        public PlanetDetailModel PlanetDetail { get; }

        public string Status { get; }

        public StarSystemModel(
            bool isOpen,
            string title,
            IReadOnlyList<StarSystemPlanetModel> planets,
            bool showPlanetDetail,
            PlanetDetailModel planetDetail,
            string status)
        {
            IsOpen = isOpen;
            Title = title ?? string.Empty;
            Planets = planets ?? Array.Empty<StarSystemPlanetModel>();
            ShowPlanetDetail = showPlanetDetail;
            PlanetDetail = planetDetail;
            Status = status ?? string.Empty;
        }
    }
}
