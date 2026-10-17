using EndlessHeros.Core.Data;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Run.Timeline;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// Empire stockpile for the bar on the star map and in a star system.
    /// </summary>
    internal sealed class StockpileQuery
    {
        /// <summary>
        /// Whole resources on hand. Empty when there is no run.
        /// </summary>
        internal ResourceInventory Describe(RunState run)
        {
            var inventory = new ResourceInventory();
            if (run == null)
            {
                return inventory;
            }

            var ledger = run.Timeline.Resources;
            var kinds = ResourceKinds.All;
            for (var i = 0; i < kinds.Count; i++)
            {
                inventory.Add(kinds[i], ledger.Get(kinds[i]));
            }

            return inventory;
        }
    }
}
