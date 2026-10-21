using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Content;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Timeline;

namespace ProjectStarwatch.Settlement
{
    /// <summary>
    /// One colony's contribution to the empire quote for a single second.
    /// </summary>
    internal readonly struct ColonyProductionLine
    {
        public ContentId PlanetId { get; }

        public string PlanetName { get; }

        public ResourceInventory Output { get; }

        public ResourceInventory Upkeep { get; }

        public ColonyProductionLine(
            ContentId planetId,
            string planetName,
            ResourceInventory output,
            ResourceInventory upkeep)
        {
            PlanetId = planetId;
            PlanetName = planetName ?? string.Empty;
            Output = output ?? new ResourceInventory();
            Upkeep = upkeep ?? new ResourceInventory();
        }
    }

    /// <summary>
    /// Sum of colony output and upkeep for one second. Shared by the economy tick and the stockpile bar.
    /// </summary>
    internal sealed class EmpireProductionQuote
    {
        public ResourceInventory Output { get; }

        public ResourceInventory Upkeep { get; }

        public IReadOnlyList<ColonyProductionLine> Lines { get; }

        private EmpireProductionQuote(
            ResourceInventory output,
            ResourceInventory upkeep,
            IReadOnlyList<ColonyProductionLine> lines)
        {
            Output = output ?? new ResourceInventory();
            Upkeep = upkeep ?? new ResourceInventory();
            Lines = lines ?? Array.Empty<ColonyProductionLine>();
        }

        /// <summary>
        /// Quotes every colony. Lines are filled only when <paramref name="includeLines"/> is true.
        /// </summary>
        public static EmpireProductionQuote Build(
            RunState run,
            ObjectDatabaseSO objectDatabase,
            ProductionService productionService,
            bool includeLines = false)
        {
            var output = new ResourceInventory();
            var upkeep = new ResourceInventory();
            if (run == null || objectDatabase == null || productionService == null)
            {
                return new EmpireProductionQuote(output, upkeep, Array.Empty<ColonyProductionLine>());
            }

            List<ColonyProductionLine> lines = includeLines ? new() : null;
            var colonies = run.Timeline.Colonies;
            for (var i = 0; i < colonies.Count; i++)
            {
                var colony = colonies[i];
                objectDatabase.TryGetPlanet(colony.PlanetId.Value, out var planet);
                var quote = productionService.Quote(colony, planet);
                output.Add(quote.Output);
                upkeep.Add(quote.Upkeep);
                if (lines == null)
                {
                    continue;
                }

                var name = planet != null && !string.IsNullOrEmpty(planet.DisplayName)
                    ? planet.DisplayName
                    : colony.PlanetId.Value;
                lines.Add(new ColonyProductionLine(colony.PlanetId, name, quote.Output, quote.Upkeep));
            }

            return new EmpireProductionQuote(
                output,
                upkeep,
                lines != null ? lines : Array.Empty<ColonyProductionLine>());
        }
    }
}
