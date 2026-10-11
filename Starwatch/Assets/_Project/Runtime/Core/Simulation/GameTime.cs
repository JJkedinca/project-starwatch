namespace EndlessHeros.Core.Simulation
{
    /// <summary>
    /// Immutable simulation timestamp measured in seconds from run start.
    /// </summary>
    public readonly struct GameTime
    {
        public GameTime(double seconds)
        {
            Seconds = seconds;
        }

        public double Seconds { get; }

        public static GameTime Zero => new GameTime(0d);

        /// <summary>
        /// Returns a new GameTime advanced by the given delta.
        /// </summary>
        public GameTime Add(double deltaSeconds)
        {
            return new GameTime(Seconds + deltaSeconds);
        }
    }
}
