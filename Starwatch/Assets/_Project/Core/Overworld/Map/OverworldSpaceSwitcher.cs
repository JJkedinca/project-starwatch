using UnityEngine;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// Owns galaxy vs star-system world roots, pointer consumers, and galaxy camera save/restore.
    /// </summary>
    public sealed class OverworldSpaceSwitcher : MonoBehaviour
    {
        [SerializeField]
        private GameObject _galaxyRoot;

        [SerializeField]
        private GameObject _systemRoot;

        [SerializeField]
        private MapPointerInput _mapPointerInput;

        [SerializeField]
        private SystemPointerInput _systemPointerInput;

        [SerializeField]
        private MapCameraController _cameraController;

        private OverworldSpace _active = OverworldSpace.Galaxy;
        private Vector3 _savedGalaxyCameraPosition;
        private MapCameraBounds _savedGalaxyBounds = MapCameraBounds.Empty;
        private bool _hasSavedGalaxyCamera;

        internal OverworldSpace Active => _active;

        internal bool IsGalaxyActive => _active == OverworldSpace.Galaxy;

        internal bool IsSystemActive => _active == OverworldSpace.StarSystem;

        private void Awake()
        {
            ApplyActive(force: true);
        }

        internal void ShowGalaxy()
        {
            if (_active == OverworldSpace.Galaxy)
            {
                return;
            }

            _active = OverworldSpace.Galaxy;
            ApplyActive(force: false);
            if (!_hasSavedGalaxyCamera || _cameraController == null)
            {
                return;
            }

            _cameraController.SetBounds(_savedGalaxyBounds);
            var camera = _cameraController.MapCamera;
            if (camera != null)
            {
                camera.transform.position = _savedGalaxyCameraPosition;
            }
        }

        internal void ShowStarSystem()
        {
            if (_active == OverworldSpace.StarSystem)
            {
                return;
            }

            if (_cameraController != null)
            {
                _savedGalaxyBounds = _cameraController.Bounds;
                var camera = _cameraController.MapCamera;
                if (camera != null)
                {
                    _savedGalaxyCameraPosition = camera.transform.position;
                    _hasSavedGalaxyCamera = true;
                }
            }

            _active = OverworldSpace.StarSystem;
            ApplyActive(force: false);
        }

        private void ApplyActive(bool force)
        {
            var galaxyOn = _active == OverworldSpace.Galaxy;
            if (_galaxyRoot != null)
            {
                _galaxyRoot.SetActive(galaxyOn);
            }

            if (_systemRoot != null)
            {
                _systemRoot.SetActive(!galaxyOn);
            }

            if (_mapPointerInput != null)
            {
                _mapPointerInput.enabled = galaxyOn;
            }

            if (_systemPointerInput != null)
            {
                _systemPointerInput.enabled = !galaxyOn;
            }

            if (force)
            {
                return;
            }
        }
    }
}
