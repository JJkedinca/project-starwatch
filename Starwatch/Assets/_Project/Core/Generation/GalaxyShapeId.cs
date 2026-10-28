namespace ProjectStarwatch.Core.Generation
{
    /// <summary>
    /// Player-facing galaxy shape. Each value maps to an authored <see cref="GalaxyShapeSO"/> in the catalog.
    /// </summary>
    public enum GalaxyShapeId
    {
        Circle = 0,
    }

     /// <summary>
    /// Player-facing galaxy size. Each value maps to authored <see cref="GenerationSettingsSO"/> in the catalog.
    /// </summary>
    public enum GalaxySizeId
    {
        Small = 0,
        Medium = 1,
        Large = 2,
    }
}
