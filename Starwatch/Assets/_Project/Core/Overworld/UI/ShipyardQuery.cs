using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Content;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Effects;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Timeline;
using ProjectStarwatch.Fleet;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Buildable classes and the queue for a colony that has a shipyard.
    /// The starting colony can build ships before its planet is scanned.
    /// </summary>
    internal sealed class ShipyardQuery
    {
        private readonly ObjectDatabaseSO _database;
        private readonly IEffectQuery _effectQuery;
        private readonly ShipProductionService _shipProductionService;

        public ShipyardQuery(ObjectDatabaseSO database, IEffectQuery effectQuery, ShipProductionService shipProductionService)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _effectQuery = effectQuery ?? throw new ArgumentNullException(nameof(effectQuery));
            _shipProductionService = shipProductionService ?? throw new ArgumentNullException(nameof(shipProductionService));
        }

        internal ShipyardModel Describe(RunState run, OverworldSelection selection)
        {
            if (run == null
                || selection == null
                || !selection.InStarSystem
                || selection.SelectedPlanetId.IsEmpty
                || !run.Timeline.TryGetColony(selection.SelectedPlanetId, out var colony)
                || colony.Presence != ColonyPresence.Colony
                || !_shipProductionService.EnablesShipConstruction(colony))
            {
                return ShipyardModel.Hidden;
            }

            return new ShipyardModel(true, Classes(), Queue(colony));
        }

        private List<ShipyardClassRow> Classes()
        {
            List<ShipyardClassRow> rows = new();
            var catalog = _database.ShipCatalog;
            if (catalog == null)
            {
                return rows;
            }

            var classes = catalog.Classes;
            for (var i = 0; i < classes.Count; i++)
            {
                var shipClass = classes[i];
                if (shipClass == null || shipClass.Id.IsEmpty)
                {
                    continue;
                }

                var capability = shipClass.RequiredCapabilityId;
                var locked = !string.IsNullOrWhiteSpace(capability) && !_effectQuery.HasCapability(capability);
                TryUnlock(capability, out var researchId, out var unlockName);
                rows.Add(new ShipyardClassRow(
                    shipClass.Id,
                    string.IsNullOrEmpty(shipClass.DisplayName) ? shipClass.Id.Value : shipClass.DisplayName,
                    shipClass.BuildSeconds,
                    Costs(shipClass.BuildCost),
                    locked,
                    researchId,
                    unlockName));
            }

            return rows;
        }

        private List<ShipyardQueueRow> Queue(ColonyState colony)
        {
            List<ShipyardQueueRow> rows = new();
            var queue = colony.BuildQueue;
            for (var i = 0; i < queue.Count; i++)
            {
                var order = queue[i];
                var buildSeconds = order.SecondsRemaining;
                var displayName = order.ClassId.Value;
                if (_database.ShipCatalog != null
                    && _database.ShipCatalog.TryGet(order.ClassId, out var shipClass)
                    && shipClass != null)
                {
                    if (shipClass.BuildSeconds > 0f)
                    {
                        buildSeconds = shipClass.BuildSeconds;
                    }

                    if (!string.IsNullOrEmpty(shipClass.DisplayName))
                    {
                        displayName = shipClass.DisplayName;
                    }
                }

                rows.Add(new ShipyardQueueRow(
                    i,
                    order.ClassId,
                    displayName,
                    order.SecondsRemaining,
                    buildSeconds,
                    i == 0));
            }

            return rows;
        }

        private void TryUnlock(string capabilityId, out ContentId researchId, out string unlockName)
        {
            researchId = default;
            unlockName = capabilityId ?? string.Empty;
            var catalog = _database.ResearchCatalog;
            if (catalog?.Researches == null || string.IsNullOrWhiteSpace(capabilityId))
            {
                return;
            }

            var researches = catalog.Researches;
            for (var i = 0; i < researches.Count; i++)
            {
                var research = researches[i];
                if (research == null || research.Effect.Kind != TechnologyEffectKind.Capability)
                {
                    continue;
                }

                if (!string.Equals(research.Effect.CapabilityId, capabilityId, StringComparison.Ordinal))
                {
                    continue;
                }

                researchId = research.Id;
                unlockName = string.IsNullOrEmpty(research.DisplayName) ? research.Id.Value : research.DisplayName;
                return;
            }
        }

        private static List<ResourceAmount> Costs(ResourceBundle bundle)
        {
            List<ResourceAmount> costs = new();
            var amounts = bundle.Amounts;
            if (amounts == null)
            {
                return costs;
            }

            for (var i = 0; i < amounts.Count; i++)
            {
                if (amounts[i].Amount <= 0f)
                {
                    continue;
                }

                costs.Add(amounts[i]);
            }

            return costs;
        }
    }
}
