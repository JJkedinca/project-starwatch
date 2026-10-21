using ProjectStarwatch.Core.Simulation;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Once-a-second refresh while fleets are in transit. Shared by roster and fleet panel.
    /// </summary>
    internal sealed class TransitCountdown
    {
        private const float kRefreshSeconds = 1f;

        private bool _active;
        private double _nextRefresh;

        /// <summary>
        /// True when enough simulation time has passed to redraw ETAs.
        /// </summary>
        internal bool ShouldRefresh(GameTime time, float deltaSeconds)
        {
            if (!_active || deltaSeconds <= 0f || time.Seconds + 0.0001d < _nextRefresh)
            {
                return false;
            }

            _nextRefresh = time.Seconds + kRefreshSeconds;
            return true;
        }

        /// <summary>
        /// Arms the next tick when travel begins; clears when nothing is traveling.
        /// </summary>
        internal void SetTraveling(bool traveling, SimulationClock clock)
        {
            if (traveling && !_active && clock != null)
            {
                _nextRefresh = clock.CurrentTime.Seconds + kRefreshSeconds;
            }

            _active = traveling;
        }

        internal void Clear()
        {
            _active = false;
        }
    }
}
