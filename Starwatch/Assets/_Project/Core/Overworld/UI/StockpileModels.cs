using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Settlement;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// How a stockpiled resource is trending for the top bar.
    /// </summary>
    internal enum StockpileState
    {
        Gaining = 0,
        Flat = 1,
        Draining = 2,
        RunningOut = 3,
        Stalled = 4,
    }

    /// <summary>
    /// One stockpiled resource: amount, rates per second, and warning state.
    /// </summary>
    internal readonly struct StockpileEntry
    {
        public ResourceKind Kind { get; }

        public int Amount { get; }

        public float GrossPerSecond { get; }

        public float UpkeepPerSecond { get; }

        public float NetPerSecond { get; }

        public float SecondsUntilEmpty { get; }

        public StockpileState State { get; }

        public StockpileEntry(
            ResourceKind kind,
            int amount,
            float grossPerSecond,
            float upkeepPerSecond,
            float netPerSecond,
            float secondsUntilEmpty,
            StockpileState state)
        {
            Kind = kind;
            Amount = amount;
            GrossPerSecond = grossPerSecond;
            UpkeepPerSecond = upkeepPerSecond;
            NetPerSecond = netPerSecond;
            SecondsUntilEmpty = secondsUntilEmpty;
            State = state;
        }
    }

    /// <summary>
    /// Science rate and active research progress for the top-bar research segment.
    /// </summary>
    internal readonly struct StockpileResearch
    {
        public static StockpileResearch Empty { get; } = new(
            sciencePerSecond: 0f,
            hasActive: false,
            activeName: string.Empty,
            points: 0f,
            cost: 0f,
            scienceDropped: false,
            estimatedSecondsLeft: 0f);

        public float SciencePerSecond { get; }

        public bool HasActive { get; }

        public string ActiveName { get; }

        public float Points { get; }

        public float Cost { get; }

        public bool ScienceDropped { get; }

        public float EstimatedSecondsLeft { get; }

        public float ProgressFraction
        {
            get
            {
                if (Cost <= 0f)
                {
                    return 0f;
                }

                var fraction = Points / Cost;
                if (fraction < 0f)
                {
                    return 0f;
                }

                return fraction > 1f ? 1f : fraction;
            }
        }

        public StockpileResearch(
            float sciencePerSecond,
            bool hasActive,
            string activeName,
            float points,
            float cost,
            bool scienceDropped = false,
            float estimatedSecondsLeft = 0f)
        {
            SciencePerSecond = sciencePerSecond;
            HasActive = hasActive;
            ActiveName = activeName ?? string.Empty;
            Points = points;
            Cost = cost;
            ScienceDropped = scienceDropped;
            EstimatedSecondsLeft = estimatedSecondsLeft < 0f ? 0f : estimatedSecondsLeft;
        }
    }

    /// <summary>
    /// Top-bar stockpile: stocked resources and the research segment.
    /// </summary>
    internal sealed class StockpileModel
    {
        public static StockpileModel Empty { get; } = new(
            Array.Empty<StockpileEntry>(),
            StockpileResearch.Empty,
            cycleSeconds: EconomyTickSystem.kIntervalSeconds,
            Array.Empty<ColonyProductionLine>(),
            isEmpireStalled: false);

        public IReadOnlyList<StockpileEntry> Entries { get; }

        public StockpileResearch Research { get; }

        public int CycleSeconds { get; }

        public IReadOnlyList<ColonyProductionLine> ColonyLines { get; }

        public bool IsEmpireStalled { get; }

        public StockpileModel(
            IReadOnlyList<StockpileEntry> entries,
            StockpileResearch research,
            int cycleSeconds,
            IReadOnlyList<ColonyProductionLine> colonyLines,
            bool isEmpireStalled)
        {
            Entries = entries ?? Array.Empty<StockpileEntry>();
            Research = research;
            CycleSeconds = cycleSeconds > 0 ? cycleSeconds : EconomyTickSystem.kIntervalSeconds;
            ColonyLines = colonyLines ?? Array.Empty<ColonyProductionLine>();
            IsEmpireStalled = isEmpireStalled;
        }

        public StockpileEntry Get(ResourceKind kind)
        {
            for (var i = 0; i < Entries.Count; i++)
            {
                if (Entries[i].Kind == kind)
                {
                    return Entries[i];
                }
            }

            return new StockpileEntry(kind, 0, 0f, 0f, 0f, float.PositiveInfinity, StockpileState.Flat);
        }
    }
}
