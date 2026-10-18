using System;
using System.Collections.Generic;

namespace ProjectStarwatch.Core.Data
{
    /// <summary>
    /// Resources of the base settlement loop. Values are contiguous from zero.
    /// </summary>
    public enum ResourceKind
    {
        Industry = 0,
        Energy = 1,
        Science = 2,
        Strategic = 3,
    }

    /// <summary>
    /// Every <see cref="ResourceKind"/>, in enum order.
    /// </summary>
    public static class ResourceKinds
    {
        private static readonly ResourceKind[] kAll = (ResourceKind[])Enum.GetValues(typeof(ResourceKind));

        public static IReadOnlyList<ResourceKind> All => kAll;

        public static int Count => kAll.Length;

        public static bool IsDefined(ResourceKind kind)
        {
            return (uint)kind < (uint)kAll.Length;
        }
    }
}
