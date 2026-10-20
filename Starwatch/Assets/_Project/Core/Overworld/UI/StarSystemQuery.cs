using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Content;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Overworld;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Universe;
using ProjectStarwatch.Overworld.Services;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Read model for the open star system and the selected planet.
    /// </summary>
    internal sealed class StarSystemQuery
    {
        private readonly VisibilityService _visibilityService;
        private readonly ObjectDatabaseSO _objectDatabase;
        private readonly FleetListQuery _fleetListQuery;

        public StarSystemQuery(
            VisibilityService visibilityService,
            ObjectDatabaseSO objectDatabase,
            FleetListQuery fleetListQuery)
        {
            _visibilityService = visibilityService ?? throw new ArgumentNullException(nameof(visibilityService));
            _objectDatabase = objectDatabase ?? throw new ArgumentNullException(nameof(objectDatabase));
            _fleetListQuery = fleetListQuery ?? throw new ArgumentNullException(nameof(fleetListQuery));
        }

        /// <summary>
        /// Title, visible planets, selected planet detail, and the status line for an open system.
        /// </summary>
        internal StarSystemModel Describe(RunState run, OverworldSelection selection)
        {
            if (run == null || selection == null || !selection.InStarSystem)
            {
                return StarSystemModel.Closed;
            }

            if (!run.Universe.MapGraph.TryGetNode(selection.SelectedNodeId, out var node))
            {
                return StarSystemModel.Closed;
            }

            var planetIds = VisiblePlanets(run, node);
            List<StarSystemPlanetModel> planets = new();
            for (var i = 0; i < planetIds.Count; i++)
            {
                var planetId = planetIds[i];
                planets.Add(new StarSystemPlanetModel(
                    planetId,
                    PlanetState(run, node, planetId, selection.SelectedPlanetId)));
            }

            var showDetail = TryPlanetDetail(run, node, selection.SelectedPlanetId, out var detail);
            return new StarSystemModel(
                true,
                NodeLabel(node),
                planets,
                _fleetListQuery.AtLocation(run, node.Id, selection.SelectedFleetId),
                showDetail,
                detail,
                StarSystemStatus(planets.Count));
        }

        /// <summary>
        /// Planets in the location that are no longer hidden.
        /// </summary>
        internal List<ContentId> VisiblePlanets(RunState run, MapNode node)
        {
            List<ContentId> planets = new();
            if (run == null || node == null)
            {
                return planets;
            }

            for (var i = 0; i < node.PlanetIds.Count; i++)
            {
                var planetId = node.PlanetIds[i];
                var visibility = _visibilityService.GetPlanetVisibility(run.Archive, ObserverId.Player, node.Id, planetId);
                if (visibility == PlanetVisibility.Hidden)
                {
                    continue;
                }

                planets.Add(planetId);
            }

            return planets;
        }

        /// <summary>
        /// Status line for an open star system, including one with no planets.
        /// </summary>
        internal string StarSystemStatus(int visiblePlanetCount)
        {
            return visiblePlanetCount > 0
                ? "Select a planet to inspect or scan."
                : "No planets in this system.";
        }

        internal PlanetPresentationState PlanetState(
            RunState run,
            MapNode node,
            ContentId planetId,
            ContentId selectedPlanetId)
        {
            var visibility = _visibilityService.GetPlanetVisibility(run.Archive, ObserverId.Player, node.Id, planetId);
            return new PlanetPresentationState(
                visibility,
                isSelected: planetId == selectedPlanetId,
                DisplayName(planetId));
        }

        private bool TryPlanetDetail(
            RunState run,
            MapNode node,
            ContentId selectedPlanetId,
            out PlanetDetailModel detail)
        {
            detail = default;
            if (selectedPlanetId.IsEmpty)
            {
                return false;
            }

            var planetVisibility = _visibilityService.GetPlanetVisibility(
                run.Archive,
                ObserverId.Player,
                node.Id,
                selectedPlanetId);
            if (planetVisibility == PlanetVisibility.Hidden)
            {
                return false;
            }

            var displayName = DisplayName(selectedPlanetId);
            var text = "Ready to scan.";
            var canScan = planetVisibility == PlanetVisibility.SystemExplored;
            if (_objectDatabase.TryGetPlanet(selectedPlanetId.Value, out var definition))
            {
                if (planetVisibility == PlanetVisibility.Scanned)
                {
                    text = definition.Summary;
                }
            }
            else if (planetVisibility == PlanetVisibility.Scanned)
            {
                text = "Scanned.";
            }

            detail = new PlanetDetailModel(selectedPlanetId, displayName, text, canScan);
            return true;
        }

        private string DisplayName(ContentId planetId)
        {
            if (_objectDatabase.TryGetPlanet(planetId.Value, out var definition))
            {
                return definition.DisplayName;
            }

            return planetId.Value;
        }

        private static string NodeLabel(MapNode node)
        {
            return node == null || node.Id.IsEmpty ? string.Empty : node.Id.Value;
        }
    }
}
