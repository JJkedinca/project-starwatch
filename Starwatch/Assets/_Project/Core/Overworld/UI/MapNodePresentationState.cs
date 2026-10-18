using ProjectStarwatch.Overworld;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Presentation snapshot for one map node element.
    /// </summary>
    internal readonly struct MapNodePresentationState
    {
        public MapNodeVisibility Visibility { get; }

        public bool IsExplorable { get; }

        public bool IsSelected { get; }

        public bool HasFleet { get; }

        public bool IsFleetTraveling { get; }

        public string RevealedLabel { get; }

        public MapNodePresentationState(
            MapNodeVisibility visibility,
            bool isExplorable,
            bool isSelected,
            bool hasFleet,
            bool isFleetTraveling,
            string revealedLabel)
        {
            Visibility = visibility;
            IsExplorable = isExplorable;
            IsSelected = isSelected;
            HasFleet = hasFleet;
            IsFleetTraveling = isFleetTraveling;
            RevealedLabel = revealedLabel ?? string.Empty;
        }
    }
}
