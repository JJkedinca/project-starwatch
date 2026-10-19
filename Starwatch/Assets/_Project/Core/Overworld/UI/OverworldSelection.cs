using System;
using ProjectStarwatch.Core.Identifiers;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Which overworld surface is open, and which node, planet, and fleet are selected.
    /// Presenters read this and change it. They do not call each other.
    /// The fleet stays selected across the map and the open system.
    /// </summary>
    internal sealed class OverworldSelection
    {
        public event Action Changed;

        public OverworldViewMode Mode { get; private set; } = OverworldViewMode.StarMap;

        public ContentId SelectedNodeId { get; private set; }

        public ContentId SelectedPlanetId { get; private set; }

        public EntityId SelectedFleetId { get; private set; }

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
        /// Selects a fleet on the map or in a system. The node and planet stay as they are.
        /// </summary>
        public void SelectFleet(EntityId fleetId)
        {
            if (SelectedFleetId == fleetId)
            {
                return;
            }

            SelectedFleetId = fleetId;
            Changed?.Invoke();
        }

        /// <summary>
        /// Closes the star system and clears the planet. The node and fleet stay selected.
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
            var clearFleet = !SelectedFleetId.IsNone;
            SelectedFleetId = EntityId.None;
            if (Mode == OverworldViewMode.StarMap && SelectedNodeId.IsEmpty && SelectedPlanetId.IsEmpty)
            {
                if (clearFleet)
                {
                    Changed?.Invoke();
                }

                return;
            }

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
