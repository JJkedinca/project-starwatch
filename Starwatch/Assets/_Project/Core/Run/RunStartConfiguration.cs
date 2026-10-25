using ProjectStarwatch.Core.Generation;

namespace ProjectStarwatch.Core.Run
{
    /// <summary>
    /// Input data for starting a run from the main menu or development tools.
    /// </summary>
    public sealed class RunStartConfiguration
    {
        public const string DefaultScenarioId = GalaxyGenerator.ScenarioId;

        public string ScenarioId { get; }

        public int? Seed { get; }

        /// <summary>Galaxy size. Used when generating.</summary>
        public GalaxySizeId GalaxySizeId { get; }

        /// <summary>Galaxy shape. Used when generating.</summary>
        public GalaxyShapeId GalaxyShapeId { get; }

        public RunStartConfiguration(
            string scenarioId,
            int? seed,
            GalaxySizeId galaxySizeId = GalaxyCatalogSO.DefaultSizeId,
            GalaxyShapeId galaxyShapeId = GalaxyCatalogSO.DefaultShapeId)
        {
            ScenarioId = string.IsNullOrWhiteSpace(scenarioId) ? DefaultScenarioId : scenarioId;
            Seed = seed;
            GalaxySizeId = galaxySizeId;
            GalaxyShapeId = galaxyShapeId;
        }
    }
}
