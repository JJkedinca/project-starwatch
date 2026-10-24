using System;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.WorldInput;
using UnityEngine;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// System pick consumer: turns WorldPointerInput gestures into planet select / hover.
    /// Reuses the MapNode layer while the system root is the only active pick target.
    /// </summary>
    public sealed class SystemPointerInput : MonoBehaviour
    {
        public event Action<ContentId> PlanetClicked;

        public event Action<ContentId> PlanetHovered;

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

        private ContentId _hoveredPlanetId;
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
            _pointerInput.Hovered -= OnHovered;
            _pointerInput.HoverCleared -= OnHoverCleared;
            _pointerSubscribed = false;
        }

        private void OnClicked(PointerClick click)
        {
            if (click.Button != PointerButton.Primary)
            {
                return;
            }

            if (!TryPick(click.ScreenPosition, out var planetId))
            {
                return;
            }

            PlanetClicked?.Invoke(planetId);
        }

        private void OnHovered(Vector2 screen)
        {
            if (!TryPick(screen, out var planetId))
            {
                ClearHover();
                return;
            }

            if (planetId == _hoveredPlanetId)
            {
                return;
            }

            _hoveredPlanetId = planetId;
            PlanetHovered?.Invoke(planetId);
        }

        private void OnHoverCleared()
        {
            ClearHover();
        }

        private void ClearHover()
        {
            if (_hoveredPlanetId.IsEmpty)
            {
                return;
            }

            _hoveredPlanetId = default;
            PlanetHovered?.Invoke(default);
        }

        private bool TryPick(Vector2 screen, out ContentId planetId)
        {
            planetId = default;
            if (_camera == null)
            {
                return false;
            }

            var query = new WorldPickQuery(_pickMask, _pickTolerancePixels, _pickMaxDistance);
            if (!WorldPicker.TryPick(_camera, screen, query, out var hit))
            {
                return false;
            }

            var view = hit.collider.GetComponentInParent<SystemPlanetView>();
            if (view == null || view.PlanetId.IsEmpty)
            {
                return false;
            }

            planetId = view.PlanetId;
            return true;
        }
    }
}
