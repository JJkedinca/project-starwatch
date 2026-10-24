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

            internal SystemOrbitRingView Orbit { get; }

            internal Vector3 World { get; }

            internal float OrbitRadius { get; }

            internal PlacedPlanetEntry(
                SystemPlanetView view,
                SystemOrbitRingView orbit,
                Vector3 world,
                float orbitRadius)
            {
                View = view;
                Orbit = orbit;
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
        private ObjectPool<SystemPlanetView> _planetPool;
        private ObjectPool<SystemOrbitRingView> _orbitPool;

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
                ReleaseEntry(pair.Value);
            }

            _planets.Clear();
            PlanetsRebuilt?.Invoke();
        }

        internal void Rebuild(IReadOnlyList<StarSystemPlanetModel> planets, int orbitSlotCount)
        {
            if (_starView != null)
            {
                _starView.transform.position = kStarWorld;
                _starView.Apply(_presentation.StarColor);
            }

            HashSet<ContentId> keep = new();
            var count = planets?.Count ?? 0;
            var slots = orbitSlotCount > 0 ? orbitSlotCount : count;
            for (var i = 0; i < count; i++)
            {
                var model = planets[i];
                if (model.PlanetId.IsEmpty)
                {
                    continue;
                }

                keep.Add(model.PlanetId);
                var radius = SystemOrbitLayout.OrbitRadius(
                    model.OrbitIndex,
                    slots,
                    _presentation.MinOrbitRadius,
                    _presentation.MaxOrbitRadius);
                var angle = SystemOrbitLayout.OrbitAngle(model.OrbitIndex, model.PlanetId);
                var world = SystemOrbitLayout.PlanetWorld(kStarWorld, radius, angle);

                SystemPlanetView visual;
                SystemOrbitRingView orbit;
                if (_planets.TryGetValue(model.PlanetId, out var existing))
                {
                    visual = existing.View;
                    orbit = existing.Orbit;
                }
                else
                {
                    visual = RentPlanet();
                    orbit = RentOrbit();
                    if (visual == null || orbit == null)
                    {
                        ReleasePlanet(visual);
                        ReleaseOrbit(orbit);
                        continue;
                    }

                    visual.name = "planet-" + model.PlanetId.Value;
                }

                visual.Bind(model.PlanetId);
                visual.transform.position = world;
                ApplyPlanetVisual(visual, model.State);
                ApplyOrbitVisual(orbit, radius, model.State.IsSelected);
                _planets[model.PlanetId] = new PlacedPlanetEntry(visual, orbit, world, radius);
            }

            PooledMapSync.ReleaseAbsent(_planets, keep, (_, entry) => ReleaseEntry(entry));
            PlanetsRebuilt?.Invoke();
        }

        internal void SetPlanetState(ContentId planetId, PlanetPresentationState state)
        {
            if (!_planets.TryGetValue(planetId, out var entry))
            {
                return;
            }

            ApplyPlanetVisual(entry.View, state);
            ApplyOrbitVisual(entry.Orbit, entry.OrbitRadius, state.IsSelected);
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

        private void ApplyPlanetVisual(SystemPlanetView visual, PlanetPresentationState state)
        {
            if (visual == null)
            {
                return;
            }

            var scale = SystemOrbitLayout.PlanetScale(
                visual.PlanetId,
                _presentation.MinPlanetScale,
                _presentation.MaxPlanetScale);
            visual.ApplyState(state, _presentation.ResolvePlanetColor(state), scale);
        }

        private void ApplyOrbitVisual(SystemOrbitRingView orbit, float radius, bool selected)
        {
            if (orbit == null)
            {
                return;
            }

            orbit.Apply(
                kStarWorld,
                radius,
                _presentation.ResolveOrbitColor(selected),
                _presentation.ResolveOrbitWidth(selected));
        }

        private void ReleaseEntry(PlacedPlanetEntry entry)
        {
            ReleasePlanet(entry.View);
            ReleaseOrbit(entry.Orbit);
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
