using System.Collections.Generic;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Run;

namespace ProjectStarwatch.Core.Effects
{
    /// <summary>
    /// Active buffs rebuilt from completed research. This registry is not saved.
    /// </summary>
    internal sealed class EffectRegistry : IEffectQuery
    {
        private readonly HashSet<string> _capabilities = new();
        private readonly Dictionary<ResourceKind, float> _multipliers = new();
        private float _travelSpeed = 1f;

        public bool HasCapability(string capabilityId)
        {
            return !string.IsNullOrWhiteSpace(capabilityId) && _capabilities.Contains(capabilityId);
        }

        public float OutputMultiplier(ResourceKind kind)
        {
            return _multipliers.TryGetValue(kind, out var value) && value > 0f ? value : 1f;
        }

        public float TravelSpeedMultiplier()
        {
            return _travelSpeed > 0f ? _travelSpeed : 1f;
        }

        /// <summary>
        /// Replaces the registry contents from the run's completed research.
        /// </summary>
        public void Rebuild(RunState run, ResearchCatalogSO catalog)
        {
            _capabilities.Clear();
            _multipliers.Clear();
            _travelSpeed = 1f;
            if (run == null || catalog == null)
            {
                return;
            }

            foreach (var researchId in run.Archive.Research.CompletedIds)
            {
                if (!catalog.TryGet(researchId, out var project) || project == null)
                {
                    continue;
                }

                Apply(project.Effect);
            }
        }

        private void Apply(TechnologyEffect effect)
        {
            switch (effect.Kind)
            {
                case TechnologyEffectKind.Capability:
                    if (!string.IsNullOrWhiteSpace(effect.CapabilityId))
                    {
                        _capabilities.Add(effect.CapabilityId);
                    }

                    break;
                case TechnologyEffectKind.OutputMultiplier:
                    if (!ResourceKinds.IsDefined(effect.Resource) || effect.Multiplier <= 0f)
                    {
                        break;
                    }

                    _multipliers[effect.Resource] = OutputMultiplier(effect.Resource) * effect.Multiplier;
                    break;
                case TechnologyEffectKind.TravelSpeed:
                    if (effect.Multiplier <= 0f)
                    {
                        break;
                    }

                    _travelSpeed *= effect.Multiplier;
                    break;
            }
        }
    }
}
