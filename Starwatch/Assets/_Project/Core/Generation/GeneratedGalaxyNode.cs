using UnityEngine;

namespace ProjectStarwatch.Core.Generation
{
    /// <summary>
    /// One generated node in unit-box space after shape placement.
    /// </summary>
    public readonly struct GeneratedGalaxyNode
    {
        public Vector2 Position { get; }

        public NodeShapeTags Tags { get; }

        public GeneratedGalaxyNode(Vector2 position, NodeShapeTags tags)
        {
            Position = position;
            Tags = tags;
        }
    }
}
