using System;
using ProjectStarwatch.Core.Identifiers;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Which overworld surface is open, and which node and planet are selected.
    /// Presenters read this and change it. They do not call each other.
    /// </summary>
    internal sealed class OverworldSelection
    {
        public event Action Changed;

        public OverworldViewMode Mode { get; private set; } = OverworldViewMode.StarMap;

        public ContentId SelectedNodeId { get; private set; }

        public ContentId SelectedPlanetId { get; private set; }

        public bool InStarSystem => Mode == OverworldViewMode.StarSystem;

        /// <summary>
        /// Selects a map node and returns to the star map. The planet selection is cleared.
        /// </summary>
        public void SelectNode(ContentId nodeId)
        {
            Set(OverworldViewMode.StarMap, nodeId, default);
        }

        /// <summary>
        /// Opens a star system and clears the planet selection.
        /// </summary>
        public void EnterSystem(ContentId nodeId)
        {
            Set(OverworldViewMode.StarSystem, nodeId, default);
        }

        /// <summary>
        /// Selects a planet while a star system is open.
        /// </summary>
        public void SelectPlanet(ContentId planetId)
        {
            if (!InStarSystem)
            {
                return;
            }

            Set(Mode, SelectedNodeId, planetId);
        }

        /// <summary>
        /// Closes the star system and clears the planet. The node stays selected.
        /// </summary>
        public void BackToMap()
        {
            Set(OverworldViewMode.StarMap, SelectedNodeId, default);
        }

        /// <summary>
        /// Returns to an empty star map. Used when there is no active run.
        /// </summary>
        public void Clear()
        {
            Set(OverworldViewMode.StarMap, default, default);
        }

        private void Set(OverworldViewMode mode, ContentId nodeId, ContentId planetId)
        {
            if (Mode == mode && SelectedNodeId == nodeId && SelectedPlanetId == planetId)
            {
                return;
            }

            Mode = mode;
            SelectedNodeId = nodeId;
            SelectedPlanetId = planetId;
            Changed?.Invoke();
        }
    }

    /// <summary>
    /// Whether the overworld is showing the star map or an open star system.
    /// </summary>
    internal enum OverworldViewMode
    {
        StarMap = 0,
        StarSystem = 1,
    }
}
