namespace EndlessHeros.Core.Run
{
    /// <summary>
    /// Mutable empire state for the current timeline attempt. Resets on rewind.
    /// </summary>
    public sealed class TimelineState
    {
        public TimelineState(int rewindIndex)
        {
            RewindIndex = rewindIndex;
        }

        public int RewindIndex { get; }
    }
}
