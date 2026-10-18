using ProjectStarwatch.Core.Data;

namespace ProjectStarwatch.Core.Effects
{
    /// <summary>
    /// Read-only capabilities and output multipliers from completed research.
    /// </summary>
    internal interface IEffectQuery
    {
        bool HasCapability(string capabilityId);

        /// <summary>
        /// Combined research multiplier for this resource. Missing effects return 1.
        /// </summary>
        float OutputMultiplier(ResourceKind kind);
    }
}
