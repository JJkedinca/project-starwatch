using System;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Header status line. Any presenter can post a message without knowing who paints it.
    /// </summary>
    internal sealed class OverworldStatus
    {
        internal const string kNoRun = "No active run.";

        internal const string kMapHint = "Travel a known route to enter a star system, then scan its planets.";

        internal const string kSelectFleet = "Select a fleet.";

        public event Action<string> Changed;

        public string Text { get; private set; } = string.Empty;

        /// <summary>
        /// Replaces the status line and notifies the screen.
        /// </summary>
        public void Post(string message)
        {
            Text = message ?? string.Empty;
            Changed?.Invoke(Text);
        }
    }
}
