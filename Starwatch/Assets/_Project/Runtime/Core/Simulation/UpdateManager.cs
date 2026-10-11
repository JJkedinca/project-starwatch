using System.Collections.Generic;
using UnityEngine;

namespace EndlessHeros.Core.Simulation
{
    /// <summary>
    /// Single Unity Update owner that advances the simulation clock and
    /// dispatches managed ticks to registered ITickable systems.
    /// </summary>
    public sealed class UpdateManager : MonoBehaviour
    {
        private readonly List<ITickable> _tickables = new List<ITickable>();
        private SimulationClock _clock;

        /// <summary>
        /// Injects the project simulation clock used for frame advances.
        /// </summary>
        public void Configure(SimulationClock clock)
        {
            _clock = clock;
        }

        /// <summary>
        /// Registers a tickable if it is not already present.
        /// </summary>
        public void Register(ITickable tickable)
        {
            if (tickable == null)
            {
                Debug.LogWarning("UpdateManager.Register ignored a null tickable.");
                return;
            }

            if (!_tickables.Contains(tickable))
            {
                _tickables.Add(tickable);
            }
        }

        /// <summary>
        /// Removes a previously registered tickable.
        /// </summary>
        public void Unregister(ITickable tickable)
        {
            if (tickable == null)
            {
                return;
            }

            _tickables.Remove(tickable);
        }

        private void Update()
        {
            if (_clock == null)
            {
                return;
            }

            var delta = _clock.Advance(Time.deltaTime);
            if (delta <= 0f)
            {
                return;
            }

            for (var i = 0; i < _tickables.Count; i++)
            {
                _tickables[i].ManagedUpdate(delta);
            }
        }
    }
}
