using System;
using ProjectStarwatch.Core.Simulation;
using ProjectStarwatch.Core.WorldInput;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// Top-down perspective map camera: pan only (drag via WorldPointerInput, keyboard via MapCamera/Pan).
    /// Registers for frame ticks only while panning or easing.
    /// </summary>
    public sealed class MapCameraController : MonoBehaviour, IFrameTickable
    {
        public event Action Moved;

        private const float kKeyboardPanSpeed = 18f;
        private const float kCenterEaseSeconds = 0.35f;

        [SerializeField]
        private Camera _camera;

        [SerializeField]
        private WorldPointerInput _pointerInput;

        [SerializeField]
        private InputActionAsset _actions;

        [SerializeField]
        private float _fixedHeight = 20f;

        private IUpdateManager _updateManager;
        private MapCameraBounds _bounds = MapCameraBounds.Empty;
        private InputAction _panAction;
        private InputActionMap _mapCameraMap;
        private bool _isPointerDragging;
        private bool _isEasing;
        private Vector3 _easeTarget;
        private float _easeElapsed;
        private bool _frameRegistered;
        private bool _pointerSubscribed;
        private bool _panSubscribed;

        internal Camera MapCamera => _camera != null ? _camera : Camera.main;

        [Inject]
        public void Construct(IUpdateManager updateManager)
        {
            _updateManager = updateManager;
        }

        internal void Configure(WorldPointerInput pointerInput, InputActionAsset actions)
        {
            if (pointerInput != null && pointerInput != _pointerInput)
            {
                UnsubscribePointer();
                _pointerInput = pointerInput;
                if (isActiveAndEnabled)
                {
                    SubscribePointer();
                }
            }

            if (actions == null)
            {
                return;
            }

            if (_actions == actions && _panAction != null)
            {
                return;
            }

            RebindPanAction(actions);
        }

        private void Awake()
        {
            if (_camera == null)
            {
                _camera = GetComponent<Camera>();
            }

            if (_actions == null)
            {
                Debug.LogError("MapCameraController requires an assigned StarwatchInput asset.", this);
                return;
            }

            BindPanAction();
        }

        private void OnEnable()
        {
            _mapCameraMap?.Enable();
            SubscribePointer();
            SubscribePan();
            if (NeedsFrameTicks())
            {
                EnsureFrameRegistered();
            }
        }

        public void ManagedUpdate(float unscaledDeltaTime)
        {
            var camera = MapCamera;
            if (camera != null)
            {
                var moved = TickKeyboard(camera, unscaledDeltaTime);
                moved |= TickEase(camera, unscaledDeltaTime);
                if (moved)
                {
                    ClampAndRaise(camera);
                }
            }

            if (!NeedsFrameTicks())
            {
                UnregisterFrame();
            }
        }

        private void OnDisable()
        {
            UnregisterFrame();
            _isPointerDragging = false;
            UnsubscribePointer();
            UnsubscribePan();
            _mapCameraMap?.Disable();
        }

        private void OnDestroy()
        {
            UnregisterFrame();
            UnsubscribePointer();
            UnsubscribePan();
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
            EnsureFrameRegistered();
        }

        private void RebindPanAction(InputActionAsset actions)
        {
            var shouldEnable = isActiveAndEnabled;
            UnsubscribePan();
            _mapCameraMap?.Disable();

            _actions = actions;
            BindPanAction();

            if (shouldEnable)
            {
                _mapCameraMap?.Enable();
                SubscribePan();
            }
        }

        private void BindPanAction()
        {
            if (_actions == null)
            {
                Debug.LogError("MapCameraController requires an assigned StarwatchInput asset.", this);
                return;
            }

            _mapCameraMap = _actions.FindActionMap("MapCamera", throwIfNotFound: false);
            _panAction = _mapCameraMap?.FindAction("Pan", throwIfNotFound: false);
            if (_panAction == null)
            {
                Debug.LogError("StarwatchInput is missing MapCamera/Pan.", this);
            }
        }

        private void SubscribePointer()
        {
            if (_pointerSubscribed || _pointerInput == null)
            {
                return;
            }

            _pointerInput.DragStarted += OnDragStarted;
            _pointerInput.Dragged += OnDragged;
            _pointerInput.DragEnded += OnDragEnded;
            _pointerSubscribed = true;
        }

        private void UnsubscribePointer()
        {
            if (!_pointerSubscribed || _pointerInput == null)
            {
                _pointerSubscribed = false;
                return;
            }

            _pointerInput.DragStarted -= OnDragStarted;
            _pointerInput.Dragged -= OnDragged;
            _pointerInput.DragEnded -= OnDragEnded;
            _pointerSubscribed = false;
        }

        private void SubscribePan()
        {
            if (_panSubscribed || _panAction == null)
            {
                return;
            }

            _panAction.performed += OnPanPerformed;
            _panAction.canceled += OnPanCanceled;
            _panSubscribed = true;
        }

        private void UnsubscribePan()
        {
            if (!_panSubscribed || _panAction == null)
            {
                _panSubscribed = false;
                return;
            }

            _panAction.performed -= OnPanPerformed;
            _panAction.canceled -= OnPanCanceled;
            _panSubscribed = false;
        }

        private void OnDragStarted(Vector2 screen)
        {
            _isPointerDragging = true;
            _isEasing = false;
            EnsureFrameRegistered();
        }

        private void OnDragged(Vector2 screenDelta)
        {
            var camera = MapCamera;
            if (camera == null || screenDelta.sqrMagnitude < 0.0001f)
            {
                return;
            }

            var worldDelta = MapPanMath.ScreenDeltaToWorldDelta(camera, screenDelta, _fixedHeight);
            camera.transform.position += worldDelta;
            ClampAndRaise(camera);
        }

        private void OnDragEnded()
        {
            _isPointerDragging = false;
        }

        private void OnPanPerformed(InputAction.CallbackContext context)
        {
            EnsureFrameRegistered();
        }

        private void OnPanCanceled(InputAction.CallbackContext context)
        {
        }

        private void EnsureFrameRegistered()
        {
            if (_frameRegistered || _updateManager == null)
            {
                return;
            }

            _updateManager.Register(this);
            _frameRegistered = true;
        }

        private void UnregisterFrame()
        {
            if (!_frameRegistered)
            {
                return;
            }

            _updateManager?.Unregister(this);
            _frameRegistered = false;
        }

        private bool NeedsFrameTicks()
        {
            if (_isPointerDragging || _isEasing)
            {
                return true;
            }

            return _panAction != null && _panAction.ReadValue<Vector2>().sqrMagnitude > 0.0001f;
        }

        private bool TickKeyboard(Camera camera, float unscaledDeltaTime)
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
                unscaledDeltaTime);
            if (delta.sqrMagnitude < 0.0000001f)
            {
                return false;
            }

            camera.transform.position += delta;
            _isEasing = false;
            return true;
        }

        private bool TickEase(Camera camera, float unscaledDeltaTime)
        {
            if (!_isEasing)
            {
                return false;
            }

            _easeElapsed += unscaledDeltaTime;
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
    }
}
