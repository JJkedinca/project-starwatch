using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// Turns pointer input into node select, travel, and hover. Ignores pointers over HUD panels.
    /// </summary>
    public sealed class MapPointerInput : MonoBehaviour
    {
        public event Action<ContentId> NodeClicked;

        public event Action<ContentId> NodeRightClicked;

        public event Action<ContentId> NodeHovered;

        [SerializeField]
        private Camera _camera;

        [SerializeField]
        private UIDocument _uiDocument;

        [SerializeField]
        private float _pickRadius = 5f;

        private MapCameraController _cameraController;
        private readonly List<MapPicker.NodePickPoint> _pickPoints = new();
        private ContentId _hoveredNodeId;
        private InputAction _pointAction;
        private Vector2 _pressScreen;
        private bool _primaryPressArmed;
        private bool _pressWasOverUi;
        private bool _suppressClickForGesture;

        internal void Configure(MapCameraController cameraController, Camera camera, UIDocument uiDocument)
        {
            _cameraController = cameraController;
            if (camera != null)
            {
                _camera = camera;
            }

            if (uiDocument != null)
            {
                _uiDocument = uiDocument;
            }
        }

        internal void SetPickPoints(IReadOnlyList<MapPicker.NodePickPoint> points)
        {
            _pickPoints.Clear();
            if (points == null)
            {
                ClearHover();
                return;
            }

            for (var i = 0; i < points.Count; i++)
            {
                _pickPoints.Add(points[i]);
            }
        }

        private void Awake()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            _pointAction = new InputAction("MapPickPoint", InputActionType.Value, "<Pointer>/position");
        }

        private void OnEnable()
        {
            _pointAction?.Enable();
        }

        private void OnDisable()
        {
            _pointAction?.Disable();
            ClearHover();
        }

        private void OnDestroy()
        {
            _pointAction?.Dispose();
        }

        private void Update()
        {
            if (_camera == null || _pointAction == null)
            {
                return;
            }

            var screen = _pointAction.ReadValue<Vector2>();
            var overUi = IsPointerOverBlockingUi(screen);

            if (!overUi)
            {
                UpdateHover(screen);
            }
            else
            {
                ClearHover();
            }

            TickRightClick(screen, overUi);
            TickPrimaryClick(screen, overUi);
        }

        private void TickRightClick(Vector2 screen, bool overUi)
        {
            if (Mouse.current == null || !Mouse.current.rightButton.wasReleasedThisFrame)
            {
                return;
            }

            if (overUi)
            {
                return;
            }

            if (!TryPick(screen, out var nodeId))
            {
                return;
            }

            NodeRightClicked?.Invoke(nodeId);
        }

        private void TickPrimaryClick(Vector2 screen, bool overUi)
        {
            var pressed = IsPrimaryPressed();
            if (pressed && !_primaryPressArmed)
            {
                _primaryPressArmed = true;
                _pressScreen = screen;
                _pressWasOverUi = overUi;
                _suppressClickForGesture = false;
                return;
            }

            if (_primaryPressArmed && pressed)
            {
                if ((screen - _pressScreen).sqrMagnitude > 36f)
                {
                    _suppressClickForGesture = true;
                }

                if (_cameraController != null && _cameraController.SuppressClick)
                {
                    _suppressClickForGesture = true;
                }

                return;
            }

            if (!_primaryPressArmed || pressed)
            {
                return;
            }

            _primaryPressArmed = false;
            var suppress = _suppressClickForGesture
                || (_cameraController != null && _cameraController.SuppressClick);
            _suppressClickForGesture = false;
            if (_pressWasOverUi || overUi || suppress)
            {
                return;
            }

            if (!TryPick(screen, out var nodeId))
            {
                return;
            }

            NodeClicked?.Invoke(nodeId);
        }

        private static bool IsPrimaryPressed()
        {
            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                return true;
            }

            if (Touchscreen.current != null)
            {
                for (var i = 0; i < Touchscreen.current.touches.Count; i++)
                {
                    var touch = Touchscreen.current.touches[i];
                    if (touch.press.isPressed)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void UpdateHover(Vector2 screen)
        {
            if (!TryPick(screen, out var nodeId))
            {
                ClearHover();
                return;
            }

            if (_hoveredNodeId == nodeId)
            {
                return;
            }

            _hoveredNodeId = nodeId;
            NodeHovered?.Invoke(nodeId);
        }

        private void ClearHover()
        {
            if (_hoveredNodeId.IsEmpty)
            {
                return;
            }

            _hoveredNodeId = default;
            NodeHovered?.Invoke(default);
        }

        private bool TryPick(Vector2 screen, out ContentId nodeId)
        {
            nodeId = default;
            var ray = _camera.ScreenPointToRay(screen);
            return MapPicker.TryPickNearest(ray, _pickPoints, _pickRadius, out nodeId);
        }

        private bool IsPointerOverBlockingUi(Vector2 screen)
        {
            return MapHudHitTest.IsOverBlockingPanel(_uiDocument, screen);
        }
    }
}
