namespace EndlessHeros.Core.Run
{
    /// <summary>
    /// High-level result kinds for a timeline or complete run.
    /// </summary>
    public enum RunOutcomeKind
    {
        Continue = 0,
        TimelineDefeat = 1,
        RunVictory = 2,
        RunDefeat = 3,
    }

    /// <summary>
    /// Explicit outcome produced by shared evaluation rules, not by individual
    /// battle or invasion systems ending the game on their own.
    /// </summary>
    public readonly struct RunOutcome
    {
        public RunOutcomeKind Kind { get; }

        public string Reason { get; }

        public RunOutcome(RunOutcomeKind kind, string reason)
        {
            Kind = kind;
            Reason = reason ?? string.Empty;
        }

        /// <summary>
        /// Returns a non-terminal continue outcome.
        /// </summary>
        public static RunOutcome Continue()
        {
            return new RunOutcome(RunOutcomeKind.Continue, string.Empty);
        }
    }
}
