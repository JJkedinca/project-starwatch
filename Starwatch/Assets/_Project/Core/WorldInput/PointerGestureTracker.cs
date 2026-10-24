using UnityEngine;

namespace ProjectStarwatch.Core.WorldInput
{
    /// <summary>
    /// Pure gesture classifier: click vs drag vs long press, plus idle hover.
    /// </summary>
    public sealed class PointerGestureTracker
    {
        private readonly IPointerGestureListener _listener;
        private PointerGestureSettings _settings;
        private bool _isPressed;
        private bool _blockedByUi;
        private bool _isDragging;
        private bool _longPressFired;
        private PointerButton _button;
        private Vector2 _pressScreen;
        private Vector2 _lastScreen;
        private float _pressTime;
        private bool _hoverActive;

        public PointerGestureTracker(IPointerGestureListener listener, PointerGestureSettings settings)
        {
            _listener = listener ?? throw new System.ArgumentNullException(nameof(listener));
            _settings = settings;
        }

        public void SetSettings(PointerGestureSettings settings)
        {
            _settings = settings;
        }

        public void PointerMoved(Vector2 screen, bool overUi)
        {
            _lastScreen = screen;
            if (_isPressed)
            {
                ClearHover();
                if (_blockedByUi)
                {
                    return;
                }

                if (_isDragging)
                {
                    if (_button == PointerButton.Primary)
                    {
                        EmitDragDelta(screen);
                    }
                    else
                    {
                        _lastScreen = screen;
                    }

                    return;
                }

                // Long press already committed this press; do not morph it into a drag.
                if (_longPressFired)
                {
                    return;
                }

                var thresholdSq = _settings.DragThresholdPixels * _settings.DragThresholdPixels;
                if ((screen - _pressScreen).sqrMagnitude < thresholdSq)
                {
                    return;
                }

                if (_button == PointerButton.Primary)
                {
                    _isDragging = true;
                    _listener.OnDragStarted(_pressScreen);
                    EmitDragDelta(screen);
                }
                else
                {
                    // Secondary: past threshold cancels the would-be click.
                    _isDragging = true;
                }

                return;
            }

            if (overUi)
            {
                ClearHover();
                return;
            }

            if (!_hoverActive)
            {
                _hoverActive = true;
            }

            _listener.OnHover(screen);
        }

        public void ButtonPressed(PointerButton button, Vector2 screen, bool overUi, float time)
        {
            Cancel();
            _isPressed = true;
            _button = button;
            _pressScreen = screen;
            _lastScreen = screen;
            _pressTime = time;
            _blockedByUi = overUi;
            _isDragging = false;
            _longPressFired = false;
            ClearHover();
        }

        public void ButtonReleased(PointerButton button, Vector2 screen)
        {
            if (!_isPressed || _button != button)
            {
                return;
            }

            var wasBlocked = _blockedByUi;
            var wasDragging = _isDragging;
            var longPressFired = _longPressFired;
            var pressScreen = _pressScreen;
            ResetPress();

            if (wasBlocked)
            {
                return;
            }

            if (wasDragging)
            {
                if (button == PointerButton.Primary)
                {
                    _listener.OnDragEnded();
                }

                return;
            }

            if (longPressFired)
            {
                return;
            }

            var thresholdSq = _settings.DragThresholdPixels * _settings.DragThresholdPixels;
            if ((screen - pressScreen).sqrMagnitude > thresholdSq)
            {
                return;
            }

            _listener.OnClick(new PointerClick(screen, button));
        }

        /// <summary>
        /// Advances long-press timing. Call from a timer or a frame tick with the same clock used at press.
        /// </summary>
        public void Tick(float time)
        {
            if (!_isPressed || _blockedByUi || _isDragging || _longPressFired)
            {
                return;
            }

            if (time - _pressTime < _settings.LongPressSeconds)
            {
                return;
            }

            _longPressFired = true;
            _listener.OnLongPress(_lastScreen);
        }

        public void Cancel()
        {
            if (_isPressed && _isDragging && _button == PointerButton.Primary && !_blockedByUi)
            {
                _listener.OnDragEnded();
            }

            ResetPress();
            ClearHover();
        }

        public void ClearHoverState()
        {
            ClearHover();
        }

        private void EmitDragDelta(Vector2 screen)
        {
            var delta = screen - _lastScreen;
            _lastScreen = screen;
            if (delta.sqrMagnitude < 0.0001f)
            {
                return;
            }

            _listener.OnDragged(delta);
        }

        private void ResetPress()
        {
            _isPressed = false;
            _blockedByUi = false;
            _isDragging = false;
            _longPressFired = false;
            _pressScreen = default;
            _pressTime = 0f;
        }

        private void ClearHover()
        {
            if (!_hoverActive)
            {
                return;
            }

            _hoverActive = false;
            _listener.OnHoverCleared();
        }
    }
}
