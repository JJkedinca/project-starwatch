using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectStarwatch.Core.Data
{
    /// <summary>
    /// A list of resource amounts. The owner decides whether the list can change.
    /// </summary>
    [Serializable]
    public struct ResourceBundle
    {
        [Tooltip("Resources in this bundle. Leave a resource out to count it as zero.")]
        [field: SerializeField]
        public List<ResourceAmount> Amounts { get; set; }
    }
}
