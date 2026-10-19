using System;
using UnityEngine;

namespace ProjectStarwatch.Core.Data
{
    /// <summary>
    /// What a completed project grants. One project carries one effect.
    /// </summary>
    public enum TechnologyEffectKind
    {
        None = 0,
        Capability = 1,
        OutputMultiplier = 2,
        TravelSpeed = 3,
    }

    /// <summary>
    /// Serializable effect payload stored on a research definition.
    /// </summary>
    [Serializable]
    public struct TechnologyEffect
    {
        [Tooltip("Capability id is stored until a system reads it. Output multiplier changes production. Travel speed changes route time.")]
        [field: SerializeField]
        public TechnologyEffectKind Kind { get; private set; }

        [Tooltip("Stable id queried by later phases. Used when the kind is Capability.")]
        [field: SerializeField]
        public string CapabilityId { get; private set; }

        [Tooltip("Resource whose output is multiplied. Used when the kind is Output Multiplier.")]
        [field: SerializeField]
        public ResourceKind Resource { get; private set; }

        [Tooltip("Output multiplier, or travel speed multiplier. 1.1 is a small bonus. Values at or below 0 are ignored.")]
        [field: SerializeField]
        public float Multiplier { get; private set; }

        public TechnologyEffect(TechnologyEffectKind kind, string capabilityId, ResourceKind resource, float multiplier)
        {
            Kind = kind;
            CapabilityId = capabilityId ?? string.Empty;
            Resource = resource;
            Multiplier = multiplier;
        }

        public static TechnologyEffect Capability(string capabilityId)
        {
            return new TechnologyEffect(TechnologyEffectKind.Capability, capabilityId, ResourceKind.Industry, 0f);
        }

        public static TechnologyEffect Output(ResourceKind resource, float multiplier)
        {
            return new TechnologyEffect(TechnologyEffectKind.OutputMultiplier, string.Empty, resource, multiplier);
        }

        public static TechnologyEffect TravelSpeed(float multiplier)
        {
            return new TechnologyEffect(TechnologyEffectKind.TravelSpeed, string.Empty, ResourceKind.Industry, multiplier);
        }
    }
}
