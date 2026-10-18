using System.Collections.Generic;
using ProjectStarwatch.Core.Content;
using ProjectStarwatch.Core.Data;
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
        private readonly EconomyService _economy;

        public ResearchQuery(ObjectDatabaseSO database, EconomyService economy)
        {
            _database = database;
            _economy = economy;
        }

        public ResearchPanelModel Describe(RunState run)
        {
            if (!TryRead(run, out var catalog, out var progress, out var active, out var science))
            {
                return ResearchPanelModel.Empty;
            }

            Completed(run, catalog, out var completedNames, out var completedTooltip);
            return new ResearchPanelModel(
                Offers(catalog, progress, progress.HasActive),
                progress.HasActive,
                progress.ActiveId,
                active != null ? active.DisplayName : string.Empty,
                progress.Points,
                active != null ? active.PointCost : 0f,
                progress.HasActive ? science : 0f,
                !progress.HasActive && science > 0f,
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
            science = _economy != null ? _economy.ProducedScience : 0f;
            return true;
        }

        private static List<ResearchOfferModel> Offers(ResearchCatalogSO catalog, ResearchProgress progress, bool hasActive)
        {
            var offers = new List<ResearchOfferModel>();
            var ids = progress.Offers;
            for (var i = 0; i < ids.Count; i++)
            {
                if (!catalog.TryGet(ids[i], out var project) || project == null)
                {
                    continue;
                }

                offers.Add(new ResearchOfferModel(
                    project.Id,
                    project.DisplayName,
                    !hasActive,
                    ResearchEffectText.Describe(project.Effect)));
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
