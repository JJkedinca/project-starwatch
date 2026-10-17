using EndlessHeros.Core.Run.Timeline;

namespace EndlessHeros.Settlement
{
    /// <summary>
    /// One colony's output and upkeep for a single second. The quote owns both totals.
    /// </summary>
    internal readonly struct ProductionQuote
    {
        public ResourceInventory Output { get; }

        public ResourceInventory Upkeep { get; }

        public ProductionQuote(ResourceInventory output, ResourceInventory upkeep)
        {
            Output = output ?? new ResourceInventory();
            Upkeep = upkeep ?? new ResourceInventory();
        }
    }
}
