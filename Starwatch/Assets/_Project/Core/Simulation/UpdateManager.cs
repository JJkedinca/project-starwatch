using UnityEngine;

namespace ProjectStarwatch.Core.Simulation
{
    /// <summary>
    /// Single Unity Update owner. Feeds real-time delta into SimulationLoop;
    /// does not call feature systems directly.
    /// </summary>
    public sealed class UpdateManager : MonoBehaviour
    {
        private SimulationLoop _simulationLoop;

        /// <summary>
        /// Injects the fixed-rate simulation loop driven each frame.
        /// </summary>
        internal void Configure(SimulationLoop simulationLoop)
        {
            _simulationLoop = simulationLoop;
        }

        private void Update()
        {
            if (_simulationLoop == null)
            {
                return;
            }

            _simulationLoop.Pump(Time.deltaTime);
        }
    }
}
