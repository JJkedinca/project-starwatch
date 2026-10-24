using System;
using UnityEngine;

namespace ProjectStarwatch.Core.WorldInput
{
    /// <summary>
    /// Thresholds for classifying pointer presses as click, drag, or long press.
    /// </summary>
    [Serializable]
    public struct PointerGestureSettings
    {
        public static PointerGestureSettings Default => new(6f, 0.5f);

        [SerializeField]
        private float _dragThresholdPixels;

        [SerializeField]
        private float _longPressSeconds;

        public float DragThresholdPixels => _dragThresholdPixels > 0f ? _dragThresholdPixels : 6f;

        public float LongPressSeconds => _longPressSeconds > 0f ? _longPressSeconds : 0.5f;

        public PointerGestureSettings(float dragThresholdPixels, float longPressSeconds)
        {
            _dragThresholdPixels = dragThresholdPixels;
            _longPressSeconds = longPressSeconds;
        }
    }
}
