using System.Collections.Generic;
using EndlessHeros.Core.Identifiers;

namespace EndlessHeros.Core.Data
{
    /// <summary>
    /// One building already placed on a starting settlement. Level is at least 1.
    /// </summary>
    public readonly struct StartingBuilding
    {
        public ContentId BuildingId { get; }

        public int Level { get; }

        public StartingBuilding(ContentId buildingId, int level)
        {
            BuildingId = buildingId;
            Level = level < 1 ? 1 : level;
        }
    }

    /// <summary>
    /// A settlement the empire already holds when a timeline begins.
    /// </summary>
    public sealed class StartingSettlement
    {
        public ContentId PlanetId { get; }

        public bool IsColony { get; }

        public IReadOnlyList<StartingBuilding> Buildings { get; }

        public StartingSettlement(ContentId planetId, bool isColony, IReadOnlyList<StartingBuilding> buildings)
        {
            PlanetId = planetId;
            IsColony = isColony;
            Buildings = Copy(buildings);
        }

        private static List<StartingBuilding> Copy(IReadOnlyList<StartingBuilding> source)
        {
            var copy = new List<StartingBuilding>();
            if (source == null)
            {
                return copy;
            }

            for (var i = 0; i < source.Count; i++)
            {
                copy.Add(source[i]);
            }

            return copy;
        }
    }
}
