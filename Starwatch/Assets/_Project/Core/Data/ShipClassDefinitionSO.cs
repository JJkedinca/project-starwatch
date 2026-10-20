using ProjectStarwatch.Core.Identifiers;
using UnityEngine;

namespace ProjectStarwatch.Core.Data
{
    /// <summary>
    /// Combat role of a ship class. This phase uses it for identity, not for battle.
    /// </summary>
    public enum ShipRole
    {
        Screen = 0,
        Line = 1,
        Carrier = 2,
    }

    /// <summary>
    /// Authored ship class. Cost, build time, and travel speed live here. Combat stats do not.
    /// </summary>
    [CreateAssetMenu(fileName = "ShipClass", menuName = "Starwatch/Ship Class")]
    public sealed class ShipClassDefinitionSO : ScriptableObject
    {
        [Tooltip("Stable content id used by fleets and build orders. This is not the asset file name.")]
        [SerializeField]
        private string _id;

        [Tooltip("Name shown when the player queues or inspects this class.")]
        [SerializeField]
        private string _displayName;

        [SerializeField]
        private ShipRole _role;

        [Tooltip("Paid in full when the ship is queued.")]
        [SerializeField]
        private ResourceBundle _buildCost;

        [Tooltip("Seconds of simulation time to build one ship.")]
        [SerializeField]
        private float _buildSeconds = 10f;

        [Tooltip("Travel time is divided by this. 1 is baseline. Higher is faster.")]
        [SerializeField]
        private float _speedFactor = 1f;

        [Tooltip("Empty means any colony with a shipyard can build it. Otherwise the research capability must be completed.")]
        [SerializeField]
        private string _requiredCapabilityId;

        public ContentId Id => ContentId.From(_id);

        public string DisplayName => _displayName ?? string.Empty;

        public ShipRole Role => _role;

        public ResourceBundle BuildCost => _buildCost;

        public float BuildSeconds => _buildSeconds;

        public float SpeedFactor => _speedFactor;

        public string RequiredCapabilityId => _requiredCapabilityId ?? string.Empty;

        private void OnValidate()
        {
            if (_buildSeconds <= 0f)
            {
                Debug.LogWarning($"{name} has no build time, so it cannot be queued.", this);
            }

            if (_speedFactor <= 0f)
            {
                Debug.LogWarning($"{name} has a non-positive speed factor. Travel treats that as 1.", this);
            }
        }
    }
}
