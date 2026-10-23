using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Core.WorldInput
{
    /// <summary>
    /// Scene pointer source: feeds the gesture tracker and re-raises classified events.
    /// </summary>
    public sealed class WorldPointerInput : MonoBehaviour, IPointerGestureListener
    {
        public event Action<PointerClick> Clicked;

        public event Action<Vector2> LongPressed;

        public event Action<Vector2> DragStarted;

        public event Action<Vector2> Dragged;

        public event Action DragEnded;

        public event Action<Vector2> Hovered;

        public event Action HoverCleared;

        [SerializeField]
        private UIDocument _uiDocument;

        [SerializeField]
        private InputActionAsset _actions;

        [SerializeField]
        private PointerGestureSettings _gestureSettings = PointerGestureSettings.Default;

        private PointerGestureTracker _tracker;
        private InputAction _pointAction;
        private InputAction _primaryAction;
        private InputAction _secondaryAction;
        private InputActionMap _worldPointerMap;
        private CancellationTokenSource _longPressCts;
        private Vector2 _lastScreen;

        internal void Configure(UIDocument uiDocument, InputActionAsset actions)
        {
            if (uiDocument != null)
            {
                _uiDocument = uiDocument;
            }

            if (actions == null)
            {
                return;
            }

            if (_actions == actions && _pointAction != null)
            {
                return;
            }

            RebindActions(actions);
        }

        private void Awake()
        {
            _tracker = new PointerGestureTracker(this, _gestureSettings);
            if (_actions == null)
            {
                Debug.LogError("WorldPointerInput requires an assigned StarwatchInput asset.", this);
                return;
            }

            BindActions();
        }

        private void OnEnable()
        {
            _worldPointerMap?.Enable();
        }

        private void OnDisable()
        {
            CancelLongPress();
            _tracker?.Cancel();
            _worldPointerMap?.Disable();
        }

        private void OnDestroy()
        {
            UnbindActionCallbacks();
        }

        private void RebindActions(InputActionAsset actions)
        {
            var shouldEnable = isActiveAndEnabled;
            UnbindActionCallbacks();
            _worldPointerMap?.Disable();

            _actions = actions;
            BindActions();

            if (shouldEnable)
            {
                _worldPointerMap?.Enable();
            }
        }

        private void BindActions()
        {
            if (_actions == null)
            {
                Debug.LogError("WorldPointerInput requires an assigned StarwatchInput asset.", this);
                return;
            }

            _worldPointerMap = _actions.FindActionMap("WorldPointer", throwIfNotFound: false);
            if (_worldPointerMap == null)
            {
                Debug.LogError("StarwatchInput is missing the WorldPointer action map.", this);
                return;
            }

            _pointAction = _worldPointerMap.FindAction("Point", throwIfNotFound: false);
            _primaryAction = _worldPointerMap.FindAction("Primary", throwIfNotFound: false);
            _secondaryAction = _worldPointerMap.FindAction("Secondary", throwIfNotFound: false);

            if (_pointAction != null)
            {
                _pointAction.performed += OnPointPerformed;
            }

            if (_primaryAction != null)
            {
                _primaryAction.started += OnPrimaryStarted;
                _primaryAction.canceled += OnPrimaryCanceled;
            }

            if (_secondaryAction != null)
            {
                _secondaryAction.started += OnSecondaryStarted;
                _secondaryAction.canceled += OnSecondaryCanceled;
            }
        }

        private void UnbindActionCallbacks()
        {
            if (_pointAction != null)
            {
                _pointAction.performed -= OnPointPerformed;
            }

            if (_primaryAction != null)
            {
                _primaryAction.started -= OnPrimaryStarted;
                _primaryAction.canceled -= OnPrimaryCanceled;
            }

            if (_secondaryAction != null)
            {
                _secondaryAction.started -= OnSecondaryStarted;
                _secondaryAction.canceled -= OnSecondaryCanceled;
            }

            _pointAction = null;
            _primaryAction = null;
            _secondaryAction = null;
            _worldPointerMap = null;
        }

        private void OnPointPerformed(InputAction.CallbackContext context)
        {
            _lastScreen = context.ReadValue<Vector2>();
            var overUi = IsOverUi(_lastScreen);
            _tracker.PointerMoved(_lastScreen, overUi);
        }

        private void OnPrimaryStarted(InputAction.CallbackContext context)
        {
            BeginPress(PointerButton.Primary);
        }

        private void OnPrimaryCanceled(InputAction.CallbackContext context)
        {
            EndPress(PointerButton.Primary);
        }

        private void OnSecondaryStarted(InputAction.CallbackContext context)
        {
            BeginPress(PointerButton.Secondary);
        }

        private void OnSecondaryCanceled(InputAction.CallbackContext context)
        {
            EndPress(PointerButton.Secondary);
        }

        private void BeginPress(PointerButton button)
        {
            if (_pointAction != null)
            {
                _lastScreen = _pointAction.ReadValue<Vector2>();
            }

            var overUi = IsOverUi(_lastScreen);
            var time = Time.unscaledTime;
            _tracker.ButtonPressed(button, _lastScreen, overUi, time);
            StartLongPressWatch(time);
        }

        private void EndPress(PointerButton button)
        {
            CancelLongPress();
            if (_pointAction != null)
            {
                _lastScreen = _pointAction.ReadValue<Vector2>();
            }

            _tracker.ButtonReleased(button, _lastScreen);

            // Touch leaves no ongoing hover sample; clear so hover does not stick.
            if (Touchscreen.current != null && Mouse.current == null)
            {
                _tracker.ClearHoverState();
            }
        }

        private void StartLongPressWatch(float pressTime)
        {
            CancelLongPress();
            _longPressCts = new CancellationTokenSource();
            WatchLongPressAsync(pressTime, _longPressCts.Token).Forget();
        }

        private async UniTaskVoid WatchLongPressAsync(float pressTime, CancellationToken token)
        {
            try
            {
                var delayMs = Mathf.CeilToInt(_gestureSettings.LongPressSeconds * 1000f);
                await UniTask.Delay(delayMs, DelayType.UnscaledDeltaTime, cancellationToken: token);
                _tracker.Tick(pressTime + _gestureSettings.LongPressSeconds + 0.001f);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void CancelLongPress()
        {
            if (_longPressCts == null)
            {
                return;
            }

            _longPressCts.Cancel();
            _longPressCts.Dispose();
            _longPressCts = null;
        }

        private bool IsOverUi(Vector2 screen)
        {
            return UiPointerBlocker.IsOverUi(_uiDocument, screen);
        }

        void IPointerGestureListener.OnClick(PointerClick click)
        {
            Clicked?.Invoke(click);
        }

        void IPointerGestureListener.OnLongPress(Vector2 screenPosition)
        {
            CancelLongPress();
            LongPressed?.Invoke(screenPosition);
        }

        void IPointerGestureListener.OnDragStarted(Vector2 screenPosition)
        {
            CancelLongPress();
            DragStarted?.Invoke(screenPosition);
        }

        void IPointerGestureListener.OnDragged(Vector2 screenDelta)
        {
            Dragged?.Invoke(screenDelta);
        }

        void IPointerGestureListener.OnDragEnded()
        {
            DragEnded?.Invoke();
        }

        void IPointerGestureListener.OnHover(Vector2 screenPosition)
        {
            Hovered?.Invoke(screenPosition);
        }

        void IPointerGestureListener.OnHoverCleared()
        {
            HoverCleared?.Invoke();
        }
    }
}
