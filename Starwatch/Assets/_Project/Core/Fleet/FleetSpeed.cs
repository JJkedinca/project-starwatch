using ProjectStarwatch.Core.Content;
using ProjectStarwatch.Core.Effects;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run.Timeline;
using UnityEngine;

namespace ProjectStarwatch.Fleet
{
    /// <summary>
    /// Fleet travel speed: the slowest class, multiplied by completed travel research.
    /// A class with a non-positive speed is treated as 1, and that lookup logs a warning.
    /// </summary>
    internal static class FleetSpeed
    {
        internal static float Effective(FleetComposition fleet, ObjectDatabaseSO database, IEffectQuery effectQuery)
        {
            return Slowest(fleet, database) * Multiplier(effectQuery);
        }

        internal static float ClassFactor(ObjectDatabaseSO database, ContentId classId)
        {
            if (database == null
                || database.ShipCatalog == null
                || !database.ShipCatalog.TryGet(classId, out var shipClass)
                || shipClass == null)
            {
                return 1f;
            }

            if (shipClass.SpeedFactor <= 0f)
            {
                Debug.LogWarning($"FleetSpeed treats '{classId.Value}' speed {shipClass.SpeedFactor} as 1.");
                return 1f;
            }

            return shipClass.SpeedFactor;
        }

        private static float Slowest(FleetComposition fleet, ObjectDatabaseSO database)
        {
            if (fleet == null)
            {
                return 1f;
            }

            var slowest = float.MaxValue;
            for (var i = 0; i < fleet.Stacks.Count; i++)
            {
                var factor = ClassFactor(database, fleet.Stacks[i].ClassId);
                if (factor < slowest)
                {
                    slowest = factor;
                }
            }

            return slowest == float.MaxValue ? 1f : slowest;
        }

        private static float Multiplier(IEffectQuery effectQuery)
        {
            var multiplier = effectQuery != null ? effectQuery.TravelSpeedMultiplier() : 1f;
            return multiplier > 0f ? multiplier : 1f;
        }
    }
}
