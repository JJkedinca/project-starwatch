using System;
using System.Collections.Generic;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// Releases dictionary entries whose keys were not seen in the current rebuild pass.
    /// </summary>
    internal static class PooledMapSync
    {
        internal static void ReleaseAbsent<TKey, TValue>(
            Dictionary<TKey, TValue> map,
            HashSet<TKey> keep,
            Action<TKey, TValue> release)
        {
            if (map == null || map.Count == 0)
            {
                return;
            }

            List<TKey> remove = null;
            foreach (var pair in map)
            {
                if (keep != null && keep.Contains(pair.Key))
                {
                    continue;
                }

                remove ??= new List<TKey>();
                remove.Add(pair.Key);
            }

            if (remove == null)
            {
                return;
            }

            for (var i = 0; i < remove.Count; i++)
            {
                var key = remove[i];
                if (!map.TryGetValue(key, out var value))
                {
                    continue;
                }

                release?.Invoke(key, value);
                map.Remove(key);
            }
        }
    }
}
