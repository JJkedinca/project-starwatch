using UnityEngine;

namespace ProjectStarwatch.Core.WorldInput
{
    /// <summary>
    /// Receives classified pointer gestures from <see cref="PointerGestureTracker"/>.
    /// </summary>
    public interface IPointerGestureListener
    {
        void OnClick(PointerClick click);

        void OnLongPress(Vector2 screenPosition);

        void OnDragStarted(Vector2 screenPosition);

        void OnDragged(Vector2 screenDelta);

        void OnDragEnded();

        void OnHover(Vector2 screenPosition);

        void OnHoverCleared();
    }
}
