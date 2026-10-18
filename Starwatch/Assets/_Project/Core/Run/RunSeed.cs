namespace ProjectStarwatch.Core.Run
{
    /// <summary>
    /// Deterministic seed used for scenario generation, offers, and combat rolls.
    /// </summary>
    public readonly struct RunSeed
    {
        public int Value { get; }

        public RunSeed(int value)
        {
            Value = value;
        }

        /// <summary>
        /// Convenience factory for constructing a seed from an integer value.
        /// </summary>
        public static RunSeed FromValue(int value)
        {
            return new RunSeed(value);
        }
    }
}
