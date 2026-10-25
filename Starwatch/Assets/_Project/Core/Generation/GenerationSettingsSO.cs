using UnityEngine;

namespace ProjectStarwatch.Core.Generation
{
    /// <summary>
    /// Shared parameters for galaxy placement and the first route/timing pass.
    /// </summary>
    [CreateAssetMenu(fileName = "GenerationSettings", menuName = "Starwatch/Map/Generation/Settings")]
    public sealed class GenerationSettingsSO : ScriptableObject
    {
        [Tooltip("Nominal node count including home. Actual count rolls within the variance percentage.")]
        [field: SerializeField]
        [Range(2, 100)]
        public int TargetNodeCount { get; private set; } = 30;

        [Tooltip("Allowed percent swing around TargetNodeCount when rolling the layout size.")]
        [field: SerializeField]
        [Range(0f, 100f)]
        public float NodeCountVariancePercentage { get; private set; } = 10f;

        [Tooltip("Minimum distance between any two nodes in unit-box space. Layouts that violate this are rejected.")]
        [field: SerializeField]
        [Range(0.001f, 1f)]
        public float MinSpacing { get; private set; } = 0.08f;

        [Tooltip("Candidate samples drawn per additional node during best-candidate placement.")]
        [field: SerializeField]
        [Range(1, 100)]
        public int CandidatesPerNode { get; private set; } = 12;

        [Tooltip("How many times to retry placement before failing scenario generation.")]
        [field: SerializeField]
        [Range(1, 256)]
        public int MaxPlacementAttempts { get; private set; } = 32;

        [Tooltip("Multiplies unit-box positions into map coordinates used by the overworld.")]
        [field: SerializeField]
        [Range(1f, 100f)]
        public float MapScale { get; private set; } = 20f;

        [Tooltip("Travel duration of the median kept route after length scaling.")]
        [field: SerializeField]
        [Range(1f, 120f)]
        public float TargetMedianTravelSeconds { get; private set; } = 40f;
    }
}
