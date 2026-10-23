using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectStarwatch.Core.WorldInput;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// Top-down perspective map camera: pan only (drag via WorldPointerInput, keyboard via MapCamera/Pan).
    /// Runs a frame loop only while panning or easing.
    /// </summary>
    public sealed class MapCameraController : MonoBehaviour
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

        private MapCameraBounds _bounds = MapCameraBounds.Empty;
        private InputAction _panAction;
        private InputActionMap _mapCameraMap;
        private bool _isPointerDragging;
        private bool _isEasing;
        private Vector3 _easeTarget;
        private float _easeElapsed;
        private bool _loopRunning;
        private CancellationTokenSource _loopCts;
        private bool _pointerSubscribed;
        private bool _panSubscribed;

        internal Camera MapCamera => _camera != null ? _camera : Camera.main;

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
        }

        private void OnDisable()
        {
            StopLoop();
            _isPointerDragging = false;
            UnsubscribePointer();
            UnsubscribePan();
            _mapCameraMap?.Disable();
        }

        private void OnDestroy()
        {
            StopLoop();
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
            EnsureLoop();
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
            EnsureLoop();
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
            EnsureLoop();
        }

        private void OnPanCanceled(InputAction.CallbackContext context)
        {
        }

        private void EnsureLoop()
        {
            if (_loopRunning)
            {
                return;
            }

            _loopCts = new CancellationTokenSource();
            RunLoopAsync(_loopCts.Token).Forget();
        }

        private async UniTaskVoid RunLoopAsync(CancellationToken token)
        {
            _loopRunning = true;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var camera = MapCamera;
                    var moved = false;
                    if (camera != null)
                    {
                        moved |= TickKeyboard(camera);
                        moved |= TickEase(camera);
                        if (moved)
                        {
                            ClampAndRaise(camera);
                        }
                    }

                    var panHeld = _panAction != null && _panAction.ReadValue<Vector2>().sqrMagnitude > 0.0001f;
                    if (!_isPointerDragging && !_isEasing && !panHeld)
                    {
                        break;
                    }

                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _loopRunning = false;
            }
        }

        private void StopLoop()
        {
            if (_loopCts == null)
            {
                return;
            }

            _loopCts.Cancel();
            _loopCts.Dispose();
            _loopCts = null;
            _loopRunning = false;
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
    }
}
