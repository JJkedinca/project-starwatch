using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Simulation;

namespace ProjectStarwatch.Core.Run.Timeline
{
    /// <summary>
    /// Timeline-owned in-progress fleet travel leg.
    /// </summary>
    public sealed class ActiveTravel
    {
        public EntityId FleetId { get; }

        public ContentId FromNodeId { get; }

        public ContentId ToNodeId { get; }

        public GameTime DepartTime { get; }

        public GameTime ArriveTime { get; }

        public ActiveTravel(
            EntityId fleetId,
            ContentId fromNodeId,
            ContentId toNodeId,
            GameTime departTime,
            GameTime arriveTime)
        {
            FleetId = fleetId;
            FromNodeId = fromNodeId;
            ToNodeId = toNodeId;
            DepartTime = departTime;
            ArriveTime = arriveTime;
        }
    }
}
