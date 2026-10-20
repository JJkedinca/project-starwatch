using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Content;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Timeline;
using ProjectStarwatch.Settlement;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Empire stockpile rates and warning states for the top bar.
    /// </summary>
    internal sealed class StockpileQuery
    {
        private static readonly ResourceKind[] kStockpileKinds =
        {
            ResourceKind.Industry,
            ResourceKind.Energy,
            ResourceKind.Strategic,
        };

        private const float kRunningOutSeconds = 60f;

        private readonly ObjectDatabaseSO _objectDatabase;
        private readonly ProductionService _productionService;

        public StockpileQuery(ObjectDatabaseSO objectDatabase, ProductionService productionService)
        {
            _objectDatabase = objectDatabase ?? throw new ArgumentNullException(nameof(objectDatabase));
            _productionService = productionService ?? throw new ArgumentNullException(nameof(productionService));
        }

        /// <summary>
        /// Stock, net rates, research segment, and colony lines for the tooltip.
        /// </summary>
        internal StockpileModel Describe(RunState run)
        {
            if (run == null)
            {
                return StockpileModel.Empty;
            }

            var quote = EmpireProductionQuote.Build(run, _objectDatabase, _productionService, includeLines: true);
            var ledger = run.Timeline.Resources;
            var empireStalled = quote.Upkeep.HasAny() && !CanCover(ledger, quote.Upkeep);
            List<StockpileEntry> entries = new(kStockpileKinds.Length);
            for (var i = 0; i < kStockpileKinds.Length; i++)
            {
                entries.Add(BuildEntry(kStockpileKinds[i], ledger, quote.Output, quote.Upkeep, empireStalled));
            }

            return new StockpileModel(
                entries,
                BuildResearch(run, quote.Output.Get(ResourceKind.Science)),
                EconomyTickSystem.kIntervalSeconds,
                quote.Lines,
                empireStalled);
        }

        private StockpileResearch BuildResearch(RunState run, float sciencePerSecond)
        {
            var progress = run.Timeline.Research;
            if (!progress.HasActive)
            {
                return new StockpileResearch(
                    sciencePerSecond,
                    hasActive: false,
                    activeName: string.Empty,
                    points: 0f,
                    cost: 0f);
            }

            var catalog = _objectDatabase.ResearchCatalog;
            if (catalog != null && catalog.TryGet(progress.ActiveId, out var project) && project != null)
            {
                return new StockpileResearch(
                    sciencePerSecond,
                    hasActive: true,
                    project.DisplayName,
                    progress.Points,
                    project.PointCost);
            }

            return new StockpileResearch(
                sciencePerSecond,
                hasActive: true,
                progress.ActiveId.Value,
                progress.Points,
                cost: 0f);
        }

        private static StockpileEntry BuildEntry(
            ResourceKind kind,
            ResourceLedger ledger,
            ResourceInventory output,
            ResourceInventory upkeep,
            bool empireStalled)
        {
            var amount = ledger.Get(kind);
            var gross = output.Get(kind);
            var upkeepRate = upkeep.Get(kind);
            var net = gross - upkeepRate;
            var upkeepWhole = Whole(upkeepRate);
            var shortOnThis = upkeepWhole > amount;
            var state = ResolveState(amount, net, shortOnThis || (empireStalled && upkeepWhole > 0));
            var secondsUntilEmpty = net < 0f && amount > 0
                ? amount / -net
                : float.PositiveInfinity;
            if (net < 0f && amount <= 0)
            {
                secondsUntilEmpty = 0f;
            }

            return new StockpileEntry(kind, amount, gross, upkeepRate, net, secondsUntilEmpty, state);
        }

        private static StockpileState ResolveState(int amount, float net, bool stalled)
        {
            if (stalled)
            {
                return StockpileState.Stalled;
            }

            if (net > 0f)
            {
                return StockpileState.Gaining;
            }

            if (net >= 0f)
            {
                return StockpileState.Flat;
            }

            var seconds = amount <= 0 ? 0f : amount / -net;
            return seconds <= kRunningOutSeconds ? StockpileState.RunningOut : StockpileState.Draining;
        }

        private static bool CanCover(ResourceLedger ledger, ResourceInventory upkeep)
        {
            var probe = new ResourceInventory();
            var kinds = ResourceKinds.All;
            for (var i = 0; i < kinds.Count; i++)
            {
                probe.Add(kinds[i], ledger.Get(kinds[i]));
            }

            return probe.Covers(upkeep);
        }

        private static int Whole(float amount)
        {
            if (amount <= 0f)
            {
                return 0;
            }

            return (int)MathF.Floor(amount);
        }
    }
}
