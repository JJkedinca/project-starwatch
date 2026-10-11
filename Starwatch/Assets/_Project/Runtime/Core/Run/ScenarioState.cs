namespace EndlessHeros.Core.Run
{
    /// <summary>
    /// Stable run-level content generated once at run start and preserved across rewinds.
    /// </summary>
    public sealed class ScenarioState
    {
        public ScenarioState(string scenarioId)
        {
            ScenarioId = scenarioId ?? string.Empty;
        }

        public string ScenarioId { get; }
    }
}
