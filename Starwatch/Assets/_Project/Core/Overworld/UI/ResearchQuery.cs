using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Content;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Timeline;
using ProjectStarwatch.Settlement;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Builds the research panel from the stored offers, the catalog, and the last Science hand-off.
    /// </summary>
    internal sealed class ResearchQuery
    {
        private readonly ResearchCatalogSO _catalog;
        private readonly EconomyService _economyService;

        public ResearchQuery(ObjectDatabaseSO database, EconomyService economyService)
        {
            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            _catalog = database.ResearchCatalog
                ?? throw new ArgumentException(
                    "ObjectDatabaseSO.ResearchCatalog is required.",
                    nameof(database));
            _economyService = economyService ?? throw new ArgumentNullException(nameof(economyService));
        }

        public ResearchPanelModel Describe(RunState run)
        {
            if (run == null)
            {
                return ResearchPanelModel.Empty;
            }

            var progress = run.Timeline.Research;
            var active = progress.HasActive && _catalog.TryGet(progress.ActiveId, out var project)
                ? project
                : null;
            var science = _economyService.ProducedScience;

            Completed(run, out var completedNames, out var completedTooltip);
            var remaining = 0f;
            if (active != null && science > 0.0001f)
            {
                remaining = (active.PointCost - progress.Points) / science;
                if (remaining < 0f)
                {
                    remaining = 0f;
                }
            }

            return new ResearchPanelModel(
                Offers(progress, science),
                progress.HasActive,
                progress.ActiveId,
                active != null ? active.DisplayName : string.Empty,
                progress.Points,
                active != null ? active.PointCost : 0f,
                progress.HasActive ? science : 0f,
                !progress.HasActive && science > 0f,
                remaining,
                completedNames,
                active != null ? ResearchEffectText.Describe(active.Effect) : string.Empty,
                completedTooltip);
        }

        private List<ResearchOfferModel> Offers(
            ResearchProgress progress,
            float sciencePerSecond)
        {
            var offers = new List<ResearchOfferModel>();
            var ids = progress.Offers;
            for (var i = 0; i < ids.Count; i++)
            {
                if (!_catalog.TryGet(ids[i], out var project) || project == null)
                {
                    continue;
                }

                var eta = 0f;
                if (sciencePerSecond > 0.0001f)
                {
                    eta = project.PointCost / sciencePerSecond;
                }

                // Always clickable: the presenter prompts a switch when a project is already active.
                offers.Add(new ResearchOfferModel(
                    project.Id,
                    project.DisplayName,
                    project.Domain,
                    canStart: true,
                    ResearchEffectText.Describe(project.Effect),
                    project.PointCost,
                    eta,
                    unavailableReason: string.Empty));
            }

            return offers;
        }

        private void Completed(
            RunState run,
            out List<string> names,
            out string tooltip)
        {
            names = new List<string>();
            tooltip = string.Empty;
            var researches = _catalog.Researches;
            for (var i = 0; i < researches.Count; i++)
            {
                var research = researches[i];
                if (research == null || !run.Archive.Research.Contains(research.Id))
                {
                    continue;
                }

                var effect = ResearchEffectText.Describe(research.Effect);
                names.Add(research.DisplayName);
                var line = string.IsNullOrEmpty(research.DisplayName) ? effect : research.DisplayName + ": " + effect;
                tooltip = tooltip.Length == 0 ? line : tooltip + "\n" + line;
            }
        }
    }
}
