namespace EndlessHeros.Core.Run
{
    /// <summary>
    /// Deterministic seed used for scenario generation, offers, and combat rolls.
    /// </summary>
    public readonly struct RunSeed
    {
        public RunSeed(int value)
        {
            Value = value;
        }

        public int Value { get; }

        /// <summary>
        /// Convenience factory for constructing a seed from an integer value.
        /// </summary>
        public static RunSeed FromValue(int value)
        {
            return new RunSeed(value);
        }
    }
}
