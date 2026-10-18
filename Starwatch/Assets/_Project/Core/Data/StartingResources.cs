using System;
using UnityEngine;

namespace ProjectStarwatch.Core.Data
{
    /// <summary>
    /// Opening stockpile for one playable faction. Each faction carries its own.
    /// </summary>
    [Serializable]
    public struct StartingResources
    {
        [SerializeField]
        private int _industry;

        [SerializeField]
        private int _energy;

        [SerializeField]
        private int _science;

        [SerializeField]
        private int _strategic;

        public int Industry => _industry < 0 ? 0 : _industry;

        public int Energy => _energy < 0 ? 0 : _energy;

        public int Science => _science < 0 ? 0 : _science;

        public int Strategic => _strategic < 0 ? 0 : _strategic;

        public StartingResources(int industry = 0, int energy = 0, int science = 0, int strategic = 0)
        {
            _industry = industry;
            _energy = energy;
            _science = science;
            _strategic = strategic;
        }
    }
}
