using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Presentation;
using ProjectStarwatch.Overworld.UI;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// World star-system view: pools and places star, orbit rings, and planet views from models.
    /// </summary>
    public sealed class StarSystemWorldView : MonoBehaviour
    {

        /// <summary>
        /// One planet currently shown in the system: pooled view plus layout used for orbits and labels.
        /// </summary>
        private readonly struct PlacedPlanetEntry
        {
            internal SystemPlanetView View { get; }

            internal Vector3 World { get; }

            internal float OrbitRadius { get; }

            internal PlacedPlanetEntry(SystemPlanetView view, Vector3 world, float orbitRadius)
            {
                View = view;
                World = world;
                OrbitRadius = orbitRadius;
            }
        }

        private static readonly Vector3 kStarWorld = Vector3.zero;

        [SerializeField]
        private Transform _planetsRoot;

        [SerializeField]
        private Transform _orbitsRoot;

        [SerializeField]
        private SystemStarView _starView;

        // TODO: Replace with SO containing planet and orbit prefabs
        [SerializeField]
        private SystemPlanetView _planetPrefab;

        [SerializeField]
        private SystemOrbitRingView _orbitPrefab;

        private StarSystemPresentationSO _presentation;
        private readonly Dictionary<ContentId, PlacedPlanetEntry> _planets = new();
        private readonly List<SystemOrbitRingView> _orbits = new();
        private ObjectPool<SystemPlanetView> _planetPool;
        private ObjectPool<SystemOrbitRingView> _orbitPool;
        private ContentId _selectedPlanetId;

        internal event Action PlanetsRebuilt;

        [Inject]
        internal void Construct(StarwatchPresentationDatabaseSO presentationDatabase)
        {
            if (presentationDatabase == null)
            {
                throw new ArgumentNullException(nameof(presentationDatabase));
            }

            _presentation = presentationDatabase.StarSystem
                ?? throw new ArgumentException(
                    "StarwatchPresentationDatabaseSO.StarSystem is required.",
                    nameof(presentationDatabase));
        }

        private void Awake()
        {
            _planetPool = new ObjectPool<SystemPlanetView>(
                createFunc: () => Instantiate(_planetPrefab, _planetsRoot),
                actionOnGet: visual => visual.gameObject.SetActive(true),
                actionOnRelease: visual =>
                {
                    visual.Bind(default);
                    visual.gameObject.SetActive(false);
                });
            _orbitPool = new ObjectPool<SystemOrbitRingView>(
                createFunc: () => Instantiate(_orbitPrefab, _orbitsRoot),
                actionOnGet: visual => visual.gameObject.SetActive(true),
                actionOnRelease: visual => visual.gameObject.SetActive(false));
        }

        private void OnDestroy()
        {
            _planetPool?.Dispose();
            _orbitPool?.Dispose();
        }

        internal void Clear()
        {
            foreach (var pair in _planets)
            {
                ReleasePlanet(pair.Value.View);
            }

            _planets.Clear();
            ClearOrbits();
            _selectedPlanetId = default;
            PlanetsRebuilt?.Invoke();
        }

        internal void Rebuild(IReadOnlyList<StarSystemPlanetModel> planets)
        {
            var minRadius = _presentation.MinOrbitRadius;
            var maxRadius = _presentation.MaxOrbitRadius;
            var minScale = _presentation.MinPlanetScale;
            var maxScale = _presentation.MaxPlanetScale;
            var hueCount = _presentation.HueVariantCount;

            if (_starView != null)
            {
                _starView.transform.position = kStarWorld;
                _starView.Apply(_presentation.StarColor);
            }

            HashSet<ContentId> keep = new();
            var count = planets?.Count ?? 0;
            ClearOrbits();
            for (var i = 0; i < count; i++)
            {
                var model = planets[i];
                if (model.PlanetId.IsEmpty)
                {
                    continue;
                }

                keep.Add(model.PlanetId);
                var radius = SystemOrbitLayout.OrbitRadius(i, count, minRadius, maxRadius);
                var angle = SystemOrbitLayout.OrbitAngle(i, model.PlanetId);
                var world = SystemOrbitLayout.PlanetWorld(kStarWorld, radius, angle);

                SystemPlanetView visual;
                if (_planets.TryGetValue(model.PlanetId, out var existing))
                {
                    visual = existing.View;
                }
                else
                {
                    visual = RentPlanet();
                    if (visual == null)
                    {
                        continue;
                    }

                    visual.name = "planet-" + model.PlanetId.Value;
                }

                visual.Bind(model.PlanetId);
                visual.transform.position = world;
                ApplyPlanetVisual(visual, model.State, minScale, maxScale, hueCount);
                _planets[model.PlanetId] = new PlacedPlanetEntry(visual, world, radius);

                var orbit = RentOrbit();
                if (orbit != null)
                {
                    var selected = model.State.IsSelected;
                    if (selected)
                    {
                        _selectedPlanetId = model.PlanetId;
                    }

                    orbit.Apply(
                        kStarWorld,
                        radius,
                        _presentation.ResolveOrbitColor(selected),
                        _presentation.ResolveOrbitWidth(selected));
                    _orbits.Add(orbit);
                }
            }

            List<ContentId> remove = null;
            foreach (var pair in _planets)
            {
                if (keep.Contains(pair.Key))
                {
                    continue;
                }

                remove ??= new List<ContentId>();
                remove.Add(pair.Key);
            }

            if (remove != null)
            {
                for (var i = 0; i < remove.Count; i++)
                {
                    var id = remove[i];
                    ReleasePlanet(_planets[id].View);
                    _planets.Remove(id);
                }
            }

            PlanetsRebuilt?.Invoke();
        }

        internal void SetPlanetState(ContentId planetId, PlanetPresentationState state)
        {
            if (!_planets.TryGetValue(planetId, out var entry))
            {
                return;
            }

            ApplyPlanetVisual(
                entry.View,
                state,
                _presentation.MinPlanetScale,
                _presentation.MaxPlanetScale,
                _presentation.HueVariantCount);

            if (state.IsSelected)
            {
                _selectedPlanetId = planetId;
            }
            else if (_selectedPlanetId == planetId)
            {
                _selectedPlanetId = default;
            }

            RefreshOrbitHighlights();
        }

        internal MapCameraBounds BuildBounds()
        {
            List<Vector3> points = new(_planets.Count + 1) { kStarWorld };
            foreach (var pair in _planets)
            {
                points.Add(pair.Value.World);
            }

            return MapCameraBounds.FromWorldPoints(points, _presentation.CameraBoundsMargin);
        }

        internal Vector3 StarWorld => kStarWorld;

        internal float RightPanelInsetNormalized => _presentation.RightPanelInsetNormalized;

        internal bool TryGetPlanetWorld(ContentId planetId, out Vector3 worldPosition)
        {
            if (!_planets.TryGetValue(planetId, out var entry))
            {
                worldPosition = default;
                return false;
            }

            worldPosition = entry.World;
            return true;
        }

        private void RefreshOrbitHighlights()
        {
            ClearOrbits();
            foreach (var pair in _planets)
            {
                var selected = pair.Key == _selectedPlanetId;
                var orbit = RentOrbit();
                if (orbit == null)
                {
                    continue;
                }

                orbit.Apply(
                    kStarWorld,
                    pair.Value.OrbitRadius,
                    _presentation.ResolveOrbitColor(selected),
                    _presentation.ResolveOrbitWidth(selected));
                _orbits.Add(orbit);
            }
        }

        private void ApplyPlanetVisual(
            SystemPlanetView visual,
            PlanetPresentationState state,
            float minScale,
            float maxScale,
            int hueCount)
        {
            if (visual == null)
            {
                return;
            }

            // Hue variant reserved for future archetype tinting; scale uses id hash.
            _ = SystemOrbitLayout.HueVariant(visual.PlanetId, hueCount);
            var scale = SystemOrbitLayout.PlanetScale(visual.PlanetId, minScale, maxScale);
            visual.ApplyState(state, _presentation.ResolvePlanetColor(state), scale);
        }

        private void ClearOrbits()
        {
            for (var i = 0; i < _orbits.Count; i++)
            {
                ReleaseOrbit(_orbits[i]);
            }

            _orbits.Clear();
        }

        private SystemPlanetView RentPlanet()
        {
            if (_planetPrefab == null)
            {
                Debug.LogError("StarSystemWorldView requires a planet prefab.", this);
                return null;
            }

            return _planetPool.Get();
        }

        private void ReleasePlanet(SystemPlanetView visual)
        {
            if (visual == null)
            {
                return;
            }

            _planetPool.Release(visual);
        }

        private SystemOrbitRingView RentOrbit()
        {
            if (_orbitPrefab == null)
            {
                Debug.LogError("StarSystemWorldView requires an orbit prefab.", this);
                return null;
            }

            return _orbitPool.Get();
        }

        private void ReleaseOrbit(SystemOrbitRingView visual)
        {
            if (visual == null)
            {
                return;
            }

            _orbitPool.Release(visual);
        }
    }
}
