using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;

namespace ProjectStarwatch.Core.Run.Timeline
{
    /// <summary>
    /// Mutable empire state for the current timeline attempt. Resets on rewind.
    /// Owns its collections; must not alias universe or archive collections.
    /// Home system lives on Universe.StartingEmpire — not duplicated here.
    /// </summary>
    public sealed class TimelineState
    {
        public EntityIdFactory EntityIds { get; }

        public FleetLedger Fleets { get; }

        public IReadOnlyList<ColonyState> Colonies => _colonies;

        public ResourceLedger Resources { get; } = new();

        public ResearchProgress Research { get; } = new();

        private readonly List<ColonyState> _colonies = new();

        public TimelineState(EntityIdFactory entityIds)
        {
            EntityIds = entityIds ?? new EntityIdFactory();
            Fleets = new FleetLedger(EntityIds);
        }

        /// <summary>
        /// Colonies only. Outposts do not raise the settlement curve.
        /// </summary>
        public int ColonyCount()
        {
            var count = 0;
            for (var i = 0; i < _colonies.Count; i++)
            {
                if (_colonies[i].Presence == ColonyPresence.Colony)
                {
                    count++;
                }
            }

            return count;
        }

        public bool TryGetColony(ContentId planetId, out ColonyState colony)
        {
            for (var i = 0; i < _colonies.Count; i++)
            {
                if (_colonies[i].PlanetId == planetId)
                {
                    colony = _colonies[i];
                    return true;
                }
            }

            colony = null;
            return false;
        }

        public bool TryAddColony(ColonyState colony)
        {
            if (colony == null || colony.PlanetId.IsEmpty || TryGetColony(colony.PlanetId, out _))
            {
                return false;
            }

            _colonies.Add(colony);
            return true;
        }

        /// <summary>
        /// Removes an outpost or colony. Buildings on that claim go with it.
        /// </summary>
        public bool TryRemoveColony(ContentId planetId)
        {
            for (var i = 0; i < _colonies.Count; i++)
            {
                if (_colonies[i].PlanetId != planetId)
                {
                    continue;
                }

                _colonies.RemoveAt(i);
                return true;
            }

            return false;
        }
    }
}
