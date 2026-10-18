using System;
using System.Collections.Generic;
using EndlessHeros.Core.Commands;
using EndlessHeros.Core.Content;
using EndlessHeros.Core.Data;
using EndlessHeros.Core.Effects;
using EndlessHeros.Core.Events;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run;
using EndlessHeros.Core.Simulation;
using UnityEngine;

namespace EndlessHeros.Research
{
    /// <summary>
    /// Offers, the one active project, and completion into research knowledge.
    /// </summary>
    internal sealed class ResearchService
    {
        private readonly ObjectDatabaseSO _database;
        private readonly EffectRegistry _effects;
        private readonly DomainEventBus _events;
        private readonly SimulationClock _clock;
        private readonly List<ContentId> _eligible = new();
        private readonly List<ContentId> _offers = new();

        /// <summary>
        /// Science was applied or dropped this second. Not a domain event, so it is not journaled.
        /// </summary>
        internal event Action ScienceApplied;

        public ResearchService(
            ObjectDatabaseSO database,
            EffectRegistry effects,
            DomainEventBus events,
            SimulationClock clock)
        {
            _database = database;
            _effects = effects;
            _events = events;
            _clock = clock;
        }

        /// <summary>
        /// Rebuilds buffs and fills offers for a new run or a replaced timeline.
        /// </summary>
        public void OnRunStarted(RunState run)
        {
            if (run != null)
            {
                EnsureProjects(run);
            }

            _effects?.Rebuild(run, Catalog);
            if (run == null)
            {
                return;
            }

            RefreshOffers(run, null);
        }

        public CommandResult Start(RunState run, ContentId researchId)
        {
            if (run == null)
            {
                return CommandResult.Fail(CommandErrorCode.InvalidArgument, "Run is required.");
            }

            if (researchId.IsEmpty || Catalog == null || !Catalog.TryGet(researchId, out var project))
            {
                return CommandResult.Fail(CommandErrorCode.InvalidArgument, "Unknown research project.");
            }

            if (project.PointCost <= 0f)
            {
                return CommandResult.Fail(CommandErrorCode.InvalidArgument, "That project has no Science cost.");
            }

            var progress = run.Timeline.Research;
            if (progress.HasActive)
            {
                return CommandResult.Fail(CommandErrorCode.NotAllowed, "A project is already in progress.");
            }

            if (!progress.IsOffered(researchId))
            {
                return CommandResult.Fail(CommandErrorCode.NotAllowed, "That project is not one of the current offers.");
            }

            if (!IsEligible(run, researchId))
            {
                return CommandResult.Fail(CommandErrorCode.PrerequisiteNotMet, "That project's prerequisites are not met.");
            }

            progress.Start(researchId);
            RefreshOffers(run, null);
            Publish(new ResearchStartedEvent(Now, researchId));
            return CommandResult.Ok();
        }

        /// <summary>
        /// Drops the active project's progress and starts another offered project.
        /// </summary>
        public CommandResult Switch(RunState run, ContentId researchId)
        {
            if (run == null)
            {
                return CommandResult.Fail(CommandErrorCode.InvalidArgument, "Run is required.");
            }

            var progress = run.Timeline.Research;
            if (progress.HasActive && progress.ActiveId == researchId)
            {
                return CommandResult.Fail(CommandErrorCode.NotAllowed, "That project is already in progress.");
            }

            if (researchId.IsEmpty || Catalog == null || !Catalog.TryGet(researchId, out _))
            {
                return CommandResult.Fail(CommandErrorCode.InvalidArgument, "Unknown research project.");
            }

            if (!progress.IsOffered(researchId))
            {
                return CommandResult.Fail(CommandErrorCode.NotAllowed, "That project is not one of the current offers.");
            }

            progress.ClearActive();
            return Start(run, researchId);
        }

        /// <summary>
        /// Adds this second's Science to the active project, or records that it was dropped.
        /// </summary>
        public void ApplyScience(RunState run, float science)
        {
            if (run == null)
            {
                return;
            }

            if (science <= 0f)
            {
                ScienceApplied?.Invoke();
                return;
            }

            var progress = run.Timeline.Research;
            if (!progress.HasActive)
            {
                ScienceApplied?.Invoke();
                return;
            }

            if (Catalog == null || !Catalog.TryGet(progress.ActiveId, out var project) || project == null)
            {
                Debug.LogWarning(
                    $"ResearchService ignored Science for active project '{progress.ActiveId.Value}' because it is not in the catalog.");
                ScienceApplied?.Invoke();
                return;
            }

            progress.AddPoints(science);
            if (project.PointCost <= 0f || progress.Points < project.PointCost)
            {
                ScienceApplied?.Invoke();
                return;
            }

            Complete(run, progress.ActiveId);
        }

        private void Complete(RunState run, ContentId researchId)
        {
            run.Timeline.Research.ClearActive();
            run.Archive.Research.TryAdd(researchId);
            _effects?.Rebuild(run, Catalog);
            RefreshOffers(run, null);
            Publish(new ResearchCompletedEvent(Now, researchId));
        }

        private void RefreshOffers(RunState run, IReadOnlyList<ContentId> pinned)
        {
            var progress = run.Timeline.Research;
            var refresh = progress.NextRefresh();
            CollectEligible(run, _eligible);
            ResearchOfferSelector.Fill(_eligible, pinned, run.Seed.Value, refresh, _offers);
            progress.SetOffers(_offers);
        }

        private void CollectEligible(RunState run, List<ContentId> destination)
        {
            destination.Clear();
            if (Catalog == null)
            {
                return;
            }

            var projects = run.Archive.Research.Projects;
            for (var i = 0; i < projects.Count; i++)
            {
                if (Catalog.TryGet(projects[i], out var project) && IsEligible(run, project))
                {
                    destination.Add(project.Id);
                }
            }
        }

        /// <summary>
        /// The first time a run starts, every catalog research is available. Discoveries add to this list later.
        /// </summary>
        private void EnsureProjects(RunState run)
        {
            var knowledge = run.Archive.Research;
            if (knowledge.ProjectsSeeded || Catalog == null)
            {
                return;
            }

            var researches = Catalog.Researches;
            for (var i = 0; i < researches.Count; i++)
            {
                var research = researches[i];
                if (research != null)
                {
                    knowledge.TryAddProject(research.Id);
                }
            }

            knowledge.MarkProjectsSeeded();
        }

        internal bool IsEligible(RunState run, ContentId researchId)
        {
            return Catalog != null
                && Catalog.TryGet(researchId, out var project)
                && IsEligible(run, project);
        }

        private bool IsEligible(RunState run, ResearchDefinitionSO project)
        {
            if (project == null
                || !run.Archive.Research.IsAvailable(project.Id)
                || run.Archive.Research.Contains(project.Id)
                || run.Timeline.Research.ActiveId == project.Id)
            {
                return false;
            }

            return project.PrerequisitesMet(run.Archive.Research);
        }

        private ResearchCatalogSO Catalog => _database != null ? _database.ResearchCatalog : null;

        private GameTime Now => _clock != null ? _clock.CurrentTime : GameTime.Zero;

        private void Publish<T>(T domainEvent)
            where T : struct, IDomainEvent
        {
            _events?.Publish(domainEvent);
        }
    }
}
