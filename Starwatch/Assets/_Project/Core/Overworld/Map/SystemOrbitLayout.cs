using ProjectStarwatch.Core.Identifiers;
using UnityEngine;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// Places planets on circular orbits in world XZ around the system star.
    /// Pure math; radii come from presentation (fixed world units).
    /// </summary>
    internal static class SystemOrbitLayout
    {
        /// <summary>
        /// Angle of the first planet, in radians.
        /// </summary>
        private const float kStartAngle = 0.35f;

        /// <summary>
        /// Golden angle spreads any number of planets without bunching on one side.
        /// </summary>
        private const float kGoldenAngle = 2.39996f;

        private const float kAngleJitter = 0.2f;

        /// <summary>
        /// Evenly spaced orbit radius for the planet at <paramref name="orbitIndex"/>, inner to outer.
        /// </summary>
        internal static float OrbitRadius(int orbitIndex, int orbitCount, float minRadius, float maxRadius)
        {
            if (orbitCount <= 1)
            {
                return Mathf.Lerp(minRadius, maxRadius, 0.5f);
            }

            var t = Mathf.Clamp01(orbitIndex / (float)(orbitCount - 1));
            return Mathf.Lerp(minRadius, maxRadius, t);
        }

        /// <summary>
        /// Stable orbit angle in radians. The order spreads planets; the id adds a small offset.
        /// </summary>
        internal static float OrbitAngle(int orbitIndex, ContentId planetId)
        {
            var jitter = (Unit(planetId, 0x9e3779b9u) - 0.5f) * 2f * kAngleJitter;
            var angle = kStartAngle + (orbitIndex * kGoldenAngle) + jitter;
            return Mathf.Repeat(angle, Mathf.PI * 2f);
        }

        /// <summary>
        /// Planet world position on the XZ plane (y = 0).
        /// </summary>
        internal static Vector3 PlanetWorld(Vector3 starWorld, float radius, float angle)
        {
            return new Vector3(
                starWorld.x + (Mathf.Cos(angle) * radius),
                starWorld.y,
                starWorld.z + (Mathf.Sin(angle) * radius));
        }

        /// <summary>
        /// Placeholder planet scale until planets carry an authored size class.
        /// </summary>
        internal static float PlanetScale(ContentId planetId, float minScale, float maxScale)
        {
            return Mathf.Lerp(minScale, maxScale, Unit(planetId, 0x85ebca6bu));
        }

        private static float Unit(ContentId planetId, uint salt)
        {
            return (Hash(planetId, salt) & 0xffffu) / 65535f;
        }

        /// <summary>
        /// FNV-1a. string.GetHashCode is not stable across runtimes.
        /// </summary>
        private static uint Hash(ContentId planetId, uint salt)
        {
            var text = planetId.IsEmpty ? string.Empty : planetId.Value;
            var hash = 2166136261u ^ salt;
            for (var i = 0; i < text.Length; i++)
            {
                hash ^= text[i];
                hash *= 16777619u;
            }

            hash ^= hash >> 15;
            hash *= 0x2c1b3c6du;
            hash ^= hash >> 12;
            return hash;
        }
    }
}
