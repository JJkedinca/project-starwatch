using ProjectStarwatch.Core.Data;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Player-facing description of one research effect.
    /// </summary>
    internal static class ResearchEffectText
    {
        public static string Describe(TechnologyEffect effect)
        {
            switch (effect.Kind)
            {
                case TechnologyEffectKind.OutputMultiplier:
                    return Output(effect);
                case TechnologyEffectKind.Capability:
                    return Capability(effect);
                default:
                    return "No effect.";
            }
        }

        private static string Output(TechnologyEffect effect)
        {
            if (effect.Multiplier <= 0f)
            {
                return "No effect.";
            }

            var percent = (effect.Multiplier - 1f) * 100f;
            var sign = percent > 0f ? "+" : string.Empty;
            return $"{effect.Resource} output {sign}{percent:0.#}%";
        }

        private static string Capability(TechnologyEffect effect)
        {
            if (string.IsNullOrWhiteSpace(effect.CapabilityId))
            {
                return "No effect.";
            }

            return $"Unlocks {effect.CapabilityId}";
        }
    }
}
