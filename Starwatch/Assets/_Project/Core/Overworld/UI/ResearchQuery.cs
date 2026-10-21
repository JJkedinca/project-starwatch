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
        private readonly ObjectDatabaseSO _database;
        private readonly EconomyService _economyService;

        public ResearchQuery(ObjectDatabaseSO database, EconomyService economyService)
        {
            _database = database;
            _economyService = economyService;
        }

        public ResearchPanelModel Describe(RunState run)
        {
            if (!TryRead(run, out var catalog, out var progress, out var active, out var science))
            {
                return ResearchPanelModel.Empty;
            }

            Completed(run, catalog, out var completedNames, out var completedTooltip);
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
                Offers(catalog, progress, science),
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

        private bool TryRead(
            RunState run,
            out ResearchCatalogSO catalog,
            out ResearchProgress progress,
            out ResearchDefinitionSO active,
            out float science)
        {
            if (run == null || _database == null || _database.ResearchCatalog == null)
            {
                catalog = null;
                progress = null;
                active = null;
                science = 0f;
                return false;
            }

            catalog = _database.ResearchCatalog;
            progress = run.Timeline.Research;
            active = progress.HasActive && catalog.TryGet(progress.ActiveId, out var project) ? project : null;
            science = _economyService != null ? _economyService.ProducedScience : 0f;
            return true;
        }

        private static List<ResearchOfferModel> Offers(
            ResearchCatalogSO catalog,
            ResearchProgress progress,
            float sciencePerSecond)
        {
            var offers = new List<ResearchOfferModel>();
            var ids = progress.Offers;
            for (var i = 0; i < ids.Count; i++)
            {
                if (!catalog.TryGet(ids[i], out var project) || project == null)
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

        private static void Completed(
            RunState run,
            ResearchCatalogSO catalog,
            out List<string> names,
            out string tooltip)
        {
            names = new List<string>();
            tooltip = string.Empty;
            var researches = catalog.Researches;
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
