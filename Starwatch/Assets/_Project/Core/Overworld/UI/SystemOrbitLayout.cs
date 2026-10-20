using ProjectStarwatch.Core.Identifiers;
using UnityEngine;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Places planets on tilted elliptical orbits around the star in the open star system.
    /// Pure math so the scene stays stable across refreshes and is testable without a panel.
    /// </summary>
    internal static class SystemOrbitLayout
    {
        /// <summary>
        /// Vertical squash of each orbit. Gives the system a 2.5D look from above.
        /// </summary>
        internal const float kTilt = 0.42f;

        /// <summary>
        /// Innermost orbit radius. Keeps planets clear of the star corona.
        /// </summary>
        internal const float kMinOrbitRadius = 120f;

        /// <summary>
        /// Gap between the outermost orbit and the scene edge, leaving room for name plates.
        /// </summary>
        internal const float kEdgePadding = 72f;

        internal const float kMinPlanetSize = 36f;

        internal const float kMaxPlanetSize = 60f;

        internal const int kHueVariantCount = 5;

        internal const float kOrbitLineWidth = 1.5f;

        internal static readonly Color kOrbitColor = new Color(0.47f, 0.6f, 0.78f, 0.28f);

        internal static readonly Color kSelectedOrbitColor = new Color(0.94f, 0.78f, 0.35f, 0.65f);

        /// <summary>
        /// Angle of the first planet, in radians. Slightly in front of the star on the right.
        /// </summary>
        private const float kStartAngle = 0.35f;

        /// <summary>
        /// Golden angle spreads any number of planets without bunching on one side.
        /// </summary>
        private const float kGoldenAngle = 2.39996f;

        private const float kAngleJitter = 0.2f;

        /// <summary>
        /// Largest horizontal orbit radius that keeps the tilted ellipse inside the scene.
        /// </summary>
        internal static float MaxOrbitRadius(Vector2 sceneSize)
        {
            var horizontal = (sceneSize.x * 0.5f) - kEdgePadding;
            var vertical = ((sceneSize.y * 0.5f) - kEdgePadding) / kTilt;
            return Mathf.Max(kMinOrbitRadius, Mathf.Min(horizontal, vertical));
        }

        /// <summary>
        /// Evenly spaced orbit radius for the planet at <paramref name="orbitIndex"/>, inner to outer.
        /// </summary>
        internal static float OrbitRadius(int orbitIndex, int orbitCount, float maxRadius)
        {
            if (orbitCount <= 1)
            {
                return Mathf.Lerp(kMinOrbitRadius, maxRadius, 0.5f);
            }

            var t = Mathf.Clamp01(orbitIndex / (float)(orbitCount - 1));
            return Mathf.Lerp(kMinOrbitRadius, maxRadius, t);
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
        /// Planet center on the tilted orbit, in scene-local pixels (+Y down).
        /// </summary>
        internal static Vector2 PlanetCenter(Vector2 starCenter, float radius, float angle)
        {
            return new Vector2(
                starCenter.x + (Mathf.Cos(angle) * radius),
                starCenter.y + (Mathf.Sin(angle) * radius * kTilt));
        }

        /// <summary>
        /// Unit vector pointing from the star toward the planet on screen. The planet's shadow falls this way.
        /// </summary>
        internal static Vector2 AwayFromStar(float angle)
        {
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * kTilt).normalized;
        }

        /// <summary>
        /// Placeholder glyph diameter until planets carry an authored size class.
        /// </summary>
        internal static float PlanetSize(ContentId planetId)
        {
            return Mathf.Lerp(kMinPlanetSize, kMaxPlanetSize, Unit(planetId, 0x85ebca6bu));
        }

        /// <summary>
        /// Placeholder hue variant until planets carry an authored archetype.
        /// </summary>
        internal static int HueVariant(ContentId planetId)
        {
            return (int)(Hash(planetId, 0xc2b2ae35u) % kHueVariantCount);
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
