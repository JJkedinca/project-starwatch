using System.Collections.Generic;
using EndlessHeros.Core.Content;
using EndlessHeros.Core.Data;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Run.Timeline;
using EndlessHeros.Research;
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

        /// <summary>
        /// Identity of the panel contents without allocating the display lists.
        /// </summary>
        public ResearchPanelStamp Stamp(RunState run)
        {
            if (!TryRead(run, out var catalog, out var progress, out var active, out var science))
            {
                return ResearchPanelStamp.Empty;
            }

            var hasActive = progress.HasActive;
            OfferIds(catalog, progress, out var offer0, out var offer1, out var offer2);
            CompletedStamp(run, catalog, out var completedCount, out var completedHash);
            return new ResearchPanelStamp(
                hasRun: true,
                hasActive,
                progress.ActiveId,
                progress.Points,
                active != null ? active.PointCost : 0f,
                hasActive ? science : 0f,
                !hasActive && science > 0f,
                offer0,
                offer1,
                offer2,
                completedCount,
                completedHash);
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

        private static void OfferIds(
            ResearchCatalogSO catalog,
            ResearchProgress progress,
            out ContentId offer0,
            out ContentId offer1,
            out ContentId offer2)
        {
            offer0 = default;
            offer1 = default;
            offer2 = default;
            var slot = 0;
            var ids = progress.Offers;
            for (var i = 0; i < ids.Count && slot < ResearchOfferSelector.kSlotCount; i++)
            {
                if (!catalog.TryGet(ids[i], out var project) || project == null)
                {
                    continue;
                }

                if (slot == 0)
                {
                    offer0 = project.Id;
                }
                else if (slot == 1)
                {
                    offer1 = project.Id;
                }
                else
                {
                    offer2 = project.Id;
                }

                slot++;
            }
        }

        private static void CompletedStamp(RunState run, ResearchCatalogSO catalog, out int count, out int hash)
        {
            count = 0;
            hash = 0;
            var researches = catalog.Researches;
            for (var i = 0; i < researches.Count; i++)
            {
                var research = researches[i];
                if (research == null || !run.Archive.Research.Contains(research.Id))
                {
                    continue;
                }

                count++;
                hash = unchecked(hash * 31 + research.Id.GetHashCode());
            }
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
