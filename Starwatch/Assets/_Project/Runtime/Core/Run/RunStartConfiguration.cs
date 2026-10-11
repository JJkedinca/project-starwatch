namespace EndlessHeros.Core.Run
{
    /// <summary>
    /// Input data for starting a run from the main menu or development tools.
    /// </summary>
    public sealed class RunStartConfiguration
    {
        public const string DefaultScenarioId = "authored-vertical-slice";

        public RunStartConfiguration(string scenarioId, int? seed)
        {
            ScenarioId = string.IsNullOrWhiteSpace(scenarioId) ? DefaultScenarioId : scenarioId;
            Seed = seed;
        }

        public string ScenarioId { get; }

        public int? Seed { get; }
    }
}
