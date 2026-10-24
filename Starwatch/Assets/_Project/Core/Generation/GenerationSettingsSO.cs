using UnityEngine;

namespace ProjectStarwatch.Core.Generation
{
    /// <summary>
    /// Shared placement parameters for galaxy layout. Route and ring fields land with later stages.
    /// </summary>
    [CreateAssetMenu(fileName = "GenerationSettings", menuName = "Starwatch/Generation Settings")]
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
        public float MinSpacing {get; private set;} = 0.08f;

        [Tooltip("Candidate samples drawn per additional node during best-candidate placement.")]
        [field: SerializeField]
        [Range(1, 100)]
        public int CandidatesPerNode {get; private set;} = 12;
    }
}
