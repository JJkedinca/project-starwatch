using System;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.WorldInput;
using UnityEngine;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// Map pick consumer: turns WorldPointerInput gestures into node select / travel / hover.
    /// </summary>
    public sealed class MapPointerInput : MonoBehaviour
    {
        public event Action<ContentId> NodeClicked;

        public event Action<ContentId> NodeRightClicked;

        public event Action<ContentId> NodeHovered;

        [SerializeField]
        private Camera _camera;

        [SerializeField]
        private WorldPointerInput _pointerInput;

        [SerializeField]
        private LayerMask _pickMask;

        [SerializeField]
        private float _pickTolerancePixels = 24f;

        [SerializeField]
        private float _pickMaxDistance = 500f;

        private ContentId _hoveredNodeId;
        private bool _pointerSubscribed;

        internal void Configure(Camera camera, WorldPointerInput pointerInput)
        {
            if (camera != null)
            {
                _camera = camera;
            }

            if (pointerInput == null || pointerInput == _pointerInput)
            {
                return;
            }

            UnsubscribePointer();
            _pointerInput = pointerInput;
            if (isActiveAndEnabled)
            {
                SubscribePointer();
            }
        }

        private void Awake()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            if (_pickMask.value == 0)
            {
                _pickMask = StarwatchLayers.MapNode.ToMask();
            }
        }

        private void OnEnable()
        {
            SubscribePointer();
        }

        private void OnDisable()
        {
            UnsubscribePointer();
            ClearHover();
        }

        private void SubscribePointer()
        {
            if (_pointerSubscribed || _pointerInput == null)
            {
                return;
            }

            _pointerInput.Clicked += OnClicked;
            _pointerInput.LongPressed += OnLongPressed;
            _pointerInput.Hovered += OnHovered;
            _pointerInput.HoverCleared += OnHoverCleared;
            _pointerSubscribed = true;
        }

        private void UnsubscribePointer()
        {
            if (!_pointerSubscribed || _pointerInput == null)
            {
                _pointerSubscribed = false;
                return;
            }

            _pointerInput.Clicked -= OnClicked;
            _pointerInput.LongPressed -= OnLongPressed;
            _pointerInput.Hovered -= OnHovered;
            _pointerInput.HoverCleared -= OnHoverCleared;
            _pointerSubscribed = false;
        }

        private void OnClicked(PointerClick click)
        {
            if (!TryPick(click.ScreenPosition, out var nodeId))
            {
                return;
            }

            if (click.Button == PointerButton.Secondary)
            {
                NodeRightClicked?.Invoke(nodeId);
                return;
            }

            NodeClicked?.Invoke(nodeId);
        }

        private void OnLongPressed(Vector2 screen)
        {
            if (!TryPick(screen, out var nodeId))
            {
                return;
            }

            NodeRightClicked?.Invoke(nodeId);
        }

        private void OnHovered(Vector2 screen)
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

        private void OnHoverCleared()
        {
            ClearHover();
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
            if (_camera == null)
            {
                return false;
            }

            var query = new WorldPickQuery(_pickMask, _pickTolerancePixels, _pickMaxDistance);
            if (!WorldPicker.TryPick(_camera, screen, query, out var hit))
            {
                return false;
            }

            var view = hit.collider.GetComponentInParent<MapNodeView>();
            if (view == null || view.NodeId.IsEmpty)
            {
                return false;
            }

            nodeId = view.NodeId;
            return true;
        }
    }
}
