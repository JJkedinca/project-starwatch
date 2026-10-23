using System.Collections.Generic;
using UnityEngine;

namespace ProjectStarwatch.Core.Simulation
{
    /// <summary>
    /// Single Unity Update owner. Ticks opt-in frame consumers, then pumps simulation.
    /// </summary>
    public sealed class UpdateManager : MonoBehaviour, IUpdateManager
    {
        private readonly List<IFrameTickable> _frameTickables = new();
        private readonly List<IFrameTickable> _tickSnapshot = new();
        private ISimulationUpdateManager _simulation;

        /// <summary>
        /// Injects the simulation update manager driven each frame after frame tickables.
        /// </summary>
        internal void Configure(ISimulationUpdateManager simulation)
        {
            _simulation = simulation;
        }

        public void Register(IFrameTickable tickable)
        {
            if (tickable == null || _frameTickables.Contains(tickable))
            {
                return;
            }

            _frameTickables.Add(tickable);
        }

        public void Unregister(IFrameTickable tickable)
        {
            if (tickable == null)
            {
                return;
            }

            _frameTickables.Remove(tickable);
        }

        private void Update()
        {
            Tick(Time.unscaledDeltaTime, Time.deltaTime);
        }

        /// <summary>
        /// Runs one frame of presentation ticks then simulation pump. Used by Unity Update and tests.
        /// Snapshots the list so register/unregister during a callback cannot double-tick or skip.
        /// </summary>
        internal void Tick(float unscaledDeltaTime, float scaledDeltaTime)
        {
            _tickSnapshot.Clear();
            _tickSnapshot.AddRange(_frameTickables);
            for (var i = 0; i < _tickSnapshot.Count; i++)
            {
                var tickable = _tickSnapshot[i];
                if (!_frameTickables.Contains(tickable))
                {
                    continue;
                }

                tickable.ManagedUpdate(unscaledDeltaTime);
            }

            _simulation?.Pump(scaledDeltaTime);
        }
    }
}
