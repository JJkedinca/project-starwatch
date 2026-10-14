using EndlessHeros.Overworld;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// Presentation snapshot for one planet icon in the system view.
    /// </summary>
    internal readonly struct PlanetPresentationState
    {
        public PlanetVisibility Visibility { get; }

        public bool IsSelected { get; }

        public string DisplayName { get; }

        public PlanetPresentationState(
            PlanetVisibility visibility,
            bool isSelected,
            string displayName)
        {
            Visibility = visibility;
            IsSelected = isSelected;
            DisplayName = displayName ?? string.Empty;
        }
    }
}
