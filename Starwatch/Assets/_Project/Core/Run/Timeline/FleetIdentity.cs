using System;
using ProjectStarwatch.Core.Identifiers;

namespace ProjectStarwatch.Core.Run.Timeline
{
    /// <summary>
    /// Identity of one fleet in a run. 
    /// TODO: Ships that make up a fleet are defined later.
    /// </summary>
    public sealed class FleetIdentity
    {
        public EntityId Id { get; }

        public FleetIdentity(EntityId id)
        {
            if (id.IsNone)
            {
                throw new ArgumentException("Fleet id is required.", nameof(id));
            }

            Id = id;
        }
    }
}
