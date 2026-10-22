namespace ProjectStarwatch.Overworld
{
    /// <summary>
    /// Fog / explored / fleet-present state for a map graph node.
    /// </summary>
    public enum MapNodeVisibility
    {
        Hidden = 0,
        Explored = 1,
        // TODO: We may want to change present to "scanner visible" or something for a scanner mechanic
        Present = 2,
    }
}
