using System.Collections.Generic;
using ProjectStarwatch.Core.Content;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run.Timeline;
using ProjectStarwatch.Core.Simulation;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Shared display labels for overworld UI (ships, places, research, travel ETA).
    /// </summary>
    internal static class OverworldNames
    {
        /// <summary>
        /// Composition title matching the roster, e.g. "4 Line Ship, 2 Screen".
        /// </summary>
        internal static string FleetTitle(ObjectDatabaseSO database, FleetComposition fleet)
        {
            if (fleet == null || fleet.Stacks.Count == 0)
            {
                return "Fleet";
            }

            var title = StackText(database, fleet.Stacks[0]);
            for (var i = 1; i < fleet.Stacks.Count; i++)
            {
                title += ", " + StackText(database, fleet.Stacks[i]);
            }

            return title;
        }

        /// <summary>
        /// Composition title from panel stack rows (same format as <see cref="FleetTitle"/>).
        /// </summary>
        internal static string FleetTitle(IReadOnlyList<FleetStackRow> stacks)
        {
            if (stacks == null || stacks.Count == 0)
            {
                return "Fleet";
            }

            var title = StackText(stacks[0]);
            for (var i = 1; i < stacks.Count; i++)
            {
                title += ", " + StackText(stacks[i]);
            }

            return title;
        }

        internal static string ShipClass(ObjectDatabaseSO database, ContentId classId)
        {
            if (classId.IsEmpty)
            {
                return "ship";
            }

            if (database != null
                && database.ShipCatalog != null
                && database.ShipCatalog.TryGet(classId, out var shipClass)
                && shipClass != null
                && !string.IsNullOrEmpty(shipClass.DisplayName))
            {
                return shipClass.DisplayName;
            }

            return classId.Value;
        }

        internal static string StarSystem(ObjectDatabaseSO database, ContentId locationId)
        {
            if (locationId.IsEmpty)
            {
                return "unknown";
            }

            if (database != null
                && database.TryGetStarSystemDisplayName(locationId.Value, out var displayName)
                && !string.IsNullOrEmpty(displayName))
            {
                return displayName;
            }

            return locationId.Value;
        }

        internal static string Planet(ObjectDatabaseSO database, ContentId planetId)
        {
            if (planetId.IsEmpty)
            {
                return "unknown";
            }

            if (database != null
                && database.TryGetPlanet(planetId.Value, out var planet)
                && !string.IsNullOrEmpty(planet.DisplayName))
            {
                return planet.DisplayName;
            }

            return planetId.Value;
        }

        internal static string Research(ObjectDatabaseSO database, ContentId researchId)
        {
            if (researchId.IsEmpty)
            {
                return "project";
            }

            if (database != null
                && database.ResearchCatalog != null
                && database.ResearchCatalog.TryGet(researchId, out var project)
                && project != null
                && !string.IsNullOrEmpty(project.DisplayName))
            {
                return project.DisplayName;
            }

            return researchId.Value;
        }

        internal static float RemainingSeconds(SimulationClock clock, GameTime arriveTime)
        {
            if (clock == null)
            {
                return 0f;
            }

            var remaining = arriveTime.Seconds - clock.CurrentTime.Seconds;
            return remaining > 0d ? (float)remaining : 0f;
        }

        private static string StackText(ObjectDatabaseSO database, ShipStack stack)
        {
            return stack.Count + " " + ShipClass(database, stack.ClassId);
        }

        private static string StackText(FleetStackRow stack)
        {
            var name = string.IsNullOrEmpty(stack.DisplayName) ? stack.ClassId.Value : stack.DisplayName;
            return stack.Count + " " + name;
        }
    }
}
