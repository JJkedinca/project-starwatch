using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// Top-down perspective map camera: pan only (left-drag, keyboard, touch).
    /// Sleeps when idle. Does not pan while the pointer is over HUD panels.
    /// </summary>
    public sealed class MapCameraController : MonoBehaviour
    {
        public event Action Moved;

        private const float kDragThresholdPixels = 6f;
        private const float kKeyboardPanSpeed = 18f;
        private const float kCenterEaseSeconds = 0.35f;

        [SerializeField]
        private Camera _camera;

        [SerializeField]
        private UIDocument _uiDocument;

        [SerializeField]
        private float _fixedHeight = 20f;

        private MapCameraBounds _bounds = MapCameraBounds.Empty;
        private InputAction _panAction;
        private InputAction _pointAction;
        private bool _isDragging;
        private bool _dragPassedThreshold;
        private bool _dragBlockedByUi;
        private Vector2 _pressScreenPosition;
        private Vector2 _lastPointerScreen;
        private bool _isEasing;
        private Vector3 _easeTarget;
        private float _easeElapsed;
        private bool _ownsActions = true;

        internal Camera MapCamera => _camera != null ? _camera : Camera.main;

        internal bool SuppressClick => _isDragging && _dragPassedThreshold;

        private void Awake()
        {
            if (_camera == null)
            {
                _camera = GetComponent<Camera>();
            }

            BuildActions();
        }

        private void OnEnable()
        {
            _panAction?.Enable();
            _pointAction?.Enable();
        }

        private void OnDisable()
        {
            _panAction?.Disable();
            _pointAction?.Disable();
            _isDragging = false;
            _dragPassedThreshold = false;
            _dragBlockedByUi = false;
        }

        private void OnDestroy()
        {
            if (!_ownsActions)
            {
                return;
            }

            _panAction?.Dispose();
            _pointAction?.Dispose();
        }

        private void Update()
        {
            var camera = MapCamera;
            if (camera == null)
            {
                return;
            }

            var moved = false;
            moved |= TickKeyboard(camera);
            moved |= TickPointerDrag(camera);
            moved |= TickEase(camera);

            if (moved)
            {
                ClampAndRaise(camera);
            }
        }

        internal void SetBounds(MapCameraBounds bounds)
        {
            _bounds = bounds;
            var camera = MapCamera;
            if (camera != null)
            {
                camera.transform.position = _bounds.ClampCameraPosition(camera.transform.position);
            }
        }

        internal void Configure(UIDocument uiDocument)
        {
            if (uiDocument != null)
            {
                _uiDocument = uiDocument;
            }
        }

        internal void CenterOn(Vector3 worldPosition, bool instant = false)
        {
            var camera = MapCamera;
            if (camera == null)
            {
                return;
            }

            var target = new Vector3(worldPosition.x, _fixedHeight, worldPosition.z);
            target = _bounds.ClampCameraPosition(target);
            if (instant)
            {
                _isEasing = false;
                camera.transform.position = target;
                Moved?.Invoke();
                return;
            }

            _easeTarget = target;
            _easeElapsed = 0f;
            _isEasing = true;
        }

        private void BuildActions()
        {
            _panAction = new InputAction("MapPan", InputActionType.Value, expectedControlType: "Vector2");
            _panAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            _panAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");

            _pointAction = new InputAction("MapPoint", InputActionType.Value, "<Pointer>/position");
        }

        private bool TickKeyboard(Camera camera)
        {
            if (_panAction == null)
            {
                return false;
            }

            var pan = _panAction.ReadValue<Vector2>();
            if (pan.sqrMagnitude < 0.0001f)
            {
                return false;
            }

            var delta = MapPanMath.KeyboardPanDelta(
                camera,
                pan,
                _fixedHeight,
                kKeyboardPanSpeed,
                Time.unscaledDeltaTime);
            if (delta.sqrMagnitude < 0.0000001f)
            {
                return false;
            }

            camera.transform.position += delta;
            return true;
        }

        private bool TickPointerDrag(Camera camera)
        {
            if (_pointAction == null)
            {
                return false;
            }

            var screen = _pointAction.ReadValue<Vector2>();
            var pressed = IsPrimaryPanPressed();
            if (!pressed)
            {
                _isDragging = false;
                _dragPassedThreshold = false;
                _dragBlockedByUi = false;
                return false;
            }

            if (!_isDragging)
            {
                _isDragging = true;
                _dragPassedThreshold = false;
                _dragBlockedByUi = MapHudHitTest.IsOverBlockingPanel(_uiDocument, screen);
                _pressScreenPosition = screen;
                _lastPointerScreen = screen;
                return false;
            }

            if (_dragBlockedByUi)
            {
                _lastPointerScreen = screen;
                return false;
            }

            if (!_dragPassedThreshold)
            {
                if ((screen - _pressScreenPosition).sqrMagnitude
                    < kDragThresholdPixels * kDragThresholdPixels)
                {
                    _lastPointerScreen = screen;
                    return false;
                }

                _dragPassedThreshold = true;
            }

            var screenDelta = screen - _lastPointerScreen;
            _lastPointerScreen = screen;
            if (screenDelta.sqrMagnitude < 0.0001f)
            {
                return false;
            }

            var worldDelta = MapPanMath.ScreenDeltaToWorldDelta(camera, screenDelta, _fixedHeight);
            camera.transform.position += worldDelta;
            _isEasing = false;
            return true;
        }

        private bool TickEase(Camera camera)
        {
            if (!_isEasing)
            {
                return false;
            }

            _easeElapsed += Time.unscaledDeltaTime;
            var t = Mathf.Clamp01(_easeElapsed / kCenterEaseSeconds);
            var smoothed = t * t * (3f - (2f * t));
            camera.transform.position = Vector3.Lerp(
                camera.transform.position,
                _easeTarget,
                smoothed);
            if (t >= 1f)
            {
                camera.transform.position = _easeTarget;
                _isEasing = false;
            }

            return true;
        }

        private void ClampAndRaise(Camera camera)
        {
            var clamped = _bounds.ClampCameraPosition(
                new Vector3(camera.transform.position.x, _fixedHeight, camera.transform.position.z));
            camera.transform.position = clamped;
            Moved?.Invoke();
        }

        private static bool IsPrimaryPanPressed()
        {
            if (Mouse.current != null)
            {
                return Mouse.current.leftButton.isPressed;
            }

            if (Touchscreen.current == null)
            {
                return false;
            }

            for (var i = 0; i < Touchscreen.current.touches.Count; i++)
            {
                if (Touchscreen.current.touches[i].press.isPressed)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
