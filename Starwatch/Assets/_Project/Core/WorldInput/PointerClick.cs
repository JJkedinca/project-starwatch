using UnityEngine;

namespace ProjectStarwatch.Core.WorldInput
{
    /// <summary>
    /// A completed click that stayed within the drag threshold.
    /// </summary>
    public readonly struct PointerClick
    {
        public Vector2 ScreenPosition { get; }

        public PointerButton Button { get; }

        public PointerClick(Vector2 screenPosition, PointerButton button)
        {
            ScreenPosition = screenPosition;
            Button = button;
        }
    }
}
