using System;
using System.Collections.Generic;

namespace ProjectStarwatch.Core.Generation
{
    /// <summary>
    /// Output of the shape placement stage: node positions and shape tags in unit-box space.
    /// Later stages add routes, rings, and content; this type stays geometry-only.
    /// </summary>
    public sealed class GeneratedGalaxyLayout
    {
        /// <summary>Placed nodes in placement order.</summary>
        public IReadOnlyList<GeneratedGalaxyNode> Nodes { get; }

        /// <summary>Index of the randomly chosen home node in <see cref="Nodes"/>.</summary>
        public int HomeIndex { get; }

        public GeneratedGalaxyLayout(IReadOnlyList<GeneratedGalaxyNode> nodes, int homeIndex)
        {
            Nodes = nodes ?? Array.Empty<GeneratedGalaxyNode>();
            HomeIndex = homeIndex;
        }
    }
}
