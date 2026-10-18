using System.Collections.Generic;
using EndlessHeros.Core.Content;
using EndlessHeros.Core.Data;
using EndlessHeros.Core.Run;
using EndlessHeros.Settlement;

namespace EndlessHeros.Overworld.UI
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
            if (run == null || _database == null || _database.ResearchCatalog == null)
            {
                return ResearchPanelModel.Empty;
            }

            var catalog = _database.ResearchCatalog;
            var progress = run.Timeline.Research;
            var active = progress.HasActive && catalog.TryGet(progress.ActiveId, out var project)
                ? project
                : null;
            var science = _economy != null ? _economy.ProducedScience : 0f;
            return new ResearchPanelModel(
                Offers(run, catalog, progress.HasActive),
                progress.HasActive,
                progress.ActiveId,
                active != null ? active.DisplayName : string.Empty,
                progress.Points,
                active != null ? active.PointCost : 0f,
                progress.HasActive ? science : 0f,
                !progress.HasActive && science > 0f,
                CompletedNames(run, catalog));
        }

        private List<ResearchOfferModel> Offers(RunState run, ResearchCatalogSO catalog, bool hasActive)
        {
            var offers = new List<ResearchOfferModel>();
            var ids = run.Timeline.Research.Offers;
            for (var i = 0; i < ids.Count; i++)
            {
                if (!catalog.TryGet(ids[i], out var project) || project == null)
                {
                    continue;
                }

                offers.Add(new ResearchOfferModel(project.Id, project.DisplayName, !hasActive));
            }

            return offers;
        }

        private static List<string> CompletedNames(RunState run, ResearchCatalogSO catalog)
        {
            var names = new List<string>();
            var researches = catalog.Researches;
            for (var i = 0; i < researches.Count; i++)
            {
                var research = researches[i];
                if (research == null || !run.Archive.Research.Contains(research.Id))
                {
                    continue;
                }

                names.Add(research.DisplayName);
            }

            return names;
        }
    }
}
