namespace ProjectStarwatch.Core.Generation
{
    /// <summary>
    /// Shape-local labels for a placed node: sector and optional arm/lane progress.
    /// </summary>
    public readonly struct NodeShapeTags
    {
        /// <summary>
        /// Content region index from the shape (circle wedge, spiral arm, or linear lane).
        /// Used later for sector bias; not a map-node id.
        /// </summary>
        public int SectorIndex { get; }

        /// <summary>
        /// Geometric path the node sits on: arm index (spiral), lane index (linear),
        /// or the same as <see cref="SectorIndex"/> for circle wedges.
        /// </summary>
        public int PathIndex { get; }

        /// <summary>
        /// How far along that path in [0, 1]: rim distance for circle, arm <c>t</c> for spiral,
        /// or band <c>x</c> for linear.
        /// </summary>
        public float Progress { get; }

        /// <summary>
        /// True when the node is in a shape core (spiral hub). False for circle and linear.
        /// </summary>
        public bool IsCore { get; }

        public NodeShapeTags(int sectorIndex, int pathIndex, float progress, bool isCore)
        {
            SectorIndex = sectorIndex;
            PathIndex = pathIndex;
            Progress = progress;
            IsCore = isCore;
        }
    }
}
