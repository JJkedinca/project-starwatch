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

        public RightColumnFocus RightColumnFocus { get; private set; } = RightColumnFocus.Target;

        public bool InStarSystem => Mode == OverworldViewMode.StarSystem;

        /// <summary>
        /// Selects a map node and returns to the star map. The planet selection is cleared.
        /// Fleet details close so the target card shows again.
        /// </summary>
        public void SelectNode(ContentId nodeId)
        {
            Set(OverworldViewMode.StarMap, nodeId, default, RightColumnFocus.Target);
        }

        /// <summary>
        /// Opens a star system and clears the planet selection.
        /// </summary>
        public void EnterSystem(ContentId nodeId)
        {
            Set(OverworldViewMode.StarSystem, nodeId, default, RightColumnFocus.Target);
        }

        /// <summary>
        /// Selects a planet while a star system is open. Fleet details close.
        /// </summary>
        public void SelectPlanet(ContentId planetId)
        {
            if (!InStarSystem)
            {
                return;
            }

            Set(Mode, SelectedNodeId, planetId, RightColumnFocus.Target);
        }

        /// <summary>
        /// Selects a fleet on the map or in a system. The node and planet stay as they are.
        /// Clearing the fleet also closes fleet details.
        /// </summary>
        public void SelectFleet(EntityId fleetId)
        {
            if (SelectedFleetId == fleetId)
            {
                return;
            }

            SelectedFleetId = fleetId;
            if (fleetId.IsNone)
            {
                RightColumnFocus = RightColumnFocus.Target;
            }

            Changed?.Invoke();
        }

        /// <summary>
        /// Opens fleet details in the right column for the selected fleet.
        /// </summary>
        public void OpenFleetDetails()
        {
            if (SelectedFleetId.IsNone || RightColumnFocus == RightColumnFocus.FleetDetails)
            {
                return;
            }

            RightColumnFocus = RightColumnFocus.FleetDetails;
            Changed?.Invoke();
        }

        /// <summary>
        /// Returns the right column to the target / planet panels.
        /// </summary>
        public void CloseFleetDetails()
        {
            if (RightColumnFocus == RightColumnFocus.Target)
            {
                return;
            }

            RightColumnFocus = RightColumnFocus.Target;
            Changed?.Invoke();
        }

        /// <summary>
        /// Closes the star system and clears the planet. The node and fleet stay selected.
        /// </summary>
        public void BackToMap()
        {
            Set(OverworldViewMode.StarMap, SelectedNodeId, default, RightColumnFocus);
        }

        /// <summary>
        /// Returns to an empty star map. Used when there is no active run.
        /// </summary>
        public void Clear()
        {
            var clearFleet = !SelectedFleetId.IsNone;
            var clearDetails = RightColumnFocus != RightColumnFocus.Target;
            SelectedFleetId = EntityId.None;
            RightColumnFocus = RightColumnFocus.Target;
            if (Mode == OverworldViewMode.StarMap && SelectedNodeId.IsEmpty && SelectedPlanetId.IsEmpty)
            {
                if (clearFleet || clearDetails)
                {
                    Changed?.Invoke();
                }

                return;
            }

            Set(OverworldViewMode.StarMap, default, default, RightColumnFocus.Target);
        }

        private void Set(
            OverworldViewMode mode,
            ContentId nodeId,
            ContentId planetId,
            RightColumnFocus rightColumnFocus)
        {
            if (Mode == mode
                && SelectedNodeId == nodeId
                && SelectedPlanetId == planetId
                && RightColumnFocus == rightColumnFocus)
            {
                return;
            }

            Mode = mode;
            SelectedNodeId = nodeId;
            SelectedPlanetId = planetId;
            RightColumnFocus = rightColumnFocus;
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

    /// <summary>
    /// What the right column shows: the target/planet panels, or fleet details.
    /// </summary>
    internal enum RightColumnFocus
    {
        Target = 0,
        FleetDetails = 1,
    }
}
