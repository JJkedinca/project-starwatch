using System;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Effects;
using ProjectStarwatch.Core.Events;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run;
using ProjectStarwatch.Core.Run.Timeline;
using ProjectStarwatch.Core.Simulation;
using ProjectStarwatch.Research;
using ProjectStarwatch.Settlement;
using ProjectStarwatch.Tests.EditMode.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ProjectStarwatch.Tests.EditMode.Research
{
    /// <summary>
    /// Offer rules, Science hand-off, and effect stacking.
    /// </summary>
    public sealed class ResearchServiceTests
    {
        [Test]
        public void CoreTechnologies_AreOfferedAndSpecializationsWaitOnPrerequisites()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(70));

            Assert.That(stack.Research.IsEligible(run, ContentId.From("basic-construction")), Is.True);
            Assert.That(stack.Research.IsEligible(run, ContentId.From("basic-energy")), Is.True);
            Assert.That(stack.Research.IsEligible(run, ContentId.From("basic-travel")), Is.True);
            Assert.That(stack.Research.IsEligible(run, ContentId.From("basic-fleet")), Is.True);
            Assert.That(stack.Research.IsEligible(run, ContentId.From("industrial-focus")), Is.False);
            Assert.That(stack.Research.IsEligible(run, ContentId.From("relic-foundry")), Is.False);
            Assert.That(run.Timeline.Research.Offers, Has.Count.EqualTo(3));
            Assert.That(run.Timeline.Research.IsOffered(ContentId.From("relic-foundry")), Is.False);
        }

        [Test]
        public void AvailableProjects_AreSeededFromTheCatalogOnce()
        {
            var harness = Harness(
                Project("basic-construction", 4f, TechnologyEffect.Capability("construction")),
                Project("basic-energy", 4f, TechnologyEffect.Capability("energy")));

            Assert.That(harness.Research.IsEligible(harness.Run, Id("basic-construction")), Is.False);

            harness.Research.OnRunStarted(harness.Run);
            harness.Research.OnRunStarted(harness.Run);

            Assert.That(
                harness.Run.Archive.Research.Projects,
                Is.EqualTo(new[] { Id("basic-construction"), Id("basic-energy") }));
            Assert.That(harness.Research.IsEligible(harness.Run, Id("basic-construction")), Is.True);
        }

        [Test]
        public void Prerequisite_ExcludesAProjectUntilTheParentCompletes()
        {
            var harness = Harness(
                Project("basic-construction", 4f, TechnologyEffect.Output(ResourceKind.Industry, 1.1f)),
                Project("industrial-focus", 4f, TechnologyEffect.Output(ResourceKind.Industry, 1.25f), "basic-construction"));
            harness.Research.OnRunStarted(harness.Run);

            Assert.That(harness.Research.IsEligible(harness.Run, Id("industrial-focus")), Is.False);

            Assert.That(harness.Research.Start(harness.Run, Id("basic-construction")).Success, Is.True);
            harness.Research.ApplyScience(harness.Run, 4f);

            Assert.That(harness.Run.Archive.Research.Contains(Id("basic-construction")), Is.True);
            Assert.That(harness.Research.IsEligible(harness.Run, Id("industrial-focus")), Is.True);
            Assert.That(harness.Run.Timeline.Research.IsOffered(Id("industrial-focus")), Is.True);
        }

        [Test]
        public void SecondStart_DoesNotReplaceTheActiveProject()
        {
            var harness = Harness(
                Project("basic-construction", 20f, TechnologyEffect.Capability("construction")),
                Project("basic-energy", 20f, TechnologyEffect.Capability("energy")));
            harness.Research.OnRunStarted(harness.Run);
            Assert.That(harness.Research.Start(harness.Run, Id("basic-construction")).Success, Is.True);

            var rejected = harness.Research.Start(harness.Run, Id("basic-energy"));

            Assert.That(rejected.Success, Is.False);
            Assert.That(rejected.Error.Code, Is.EqualTo(ProjectStarwatch.Core.Commands.CommandErrorCode.NotAllowed));
            Assert.That(harness.Run.Timeline.Research.ActiveId, Is.EqualTo(Id("basic-construction")));
        }

        [Test]
        public void Switch_ReplacesTheActiveProjectAndDropsItsProgress()
        {
            var harness = Harness(
                Project("basic-construction", 20f, TechnologyEffect.Capability("construction")),
                Project("basic-energy", 20f, TechnologyEffect.Capability("energy")));
            harness.Research.OnRunStarted(harness.Run);
            Assert.That(harness.Research.Start(harness.Run, Id("basic-construction")).Success, Is.True);
            harness.Run.Timeline.Research.AddPoints(5f);
            var next = harness.Run.Timeline.Research.Offers[0];

            var switched = harness.Research.Switch(harness.Run, next);

            Assert.That(switched.Success, Is.True);
            Assert.That(harness.Run.Timeline.Research.ActiveId, Is.EqualTo(next));
            Assert.That(harness.Run.Timeline.Research.Points, Is.EqualTo(0f));
            Assert.That(next, Is.Not.EqualTo(Id("basic-construction")));
        }

        [Test]
        public void Switch_KeepsTheActiveProjectWhenTheReplacementIsRejected()
        {
            var harness = Harness(
                Project("basic-construction", 20f, TechnologyEffect.Capability("construction")),
                Project("free", 0f, TechnologyEffect.Capability("free")));
            harness.Research.OnRunStarted(harness.Run);
            Assert.That(harness.Research.Start(harness.Run, Id("basic-construction")).Success, Is.True);
            harness.Run.Timeline.Research.AddPoints(5f);

            var rejected = harness.Research.Switch(harness.Run, Id("free"));

            Assert.That(rejected.Success, Is.False);
            Assert.That(harness.Run.Timeline.Research.ActiveId, Is.EqualTo(Id("basic-construction")));
            Assert.That(harness.Run.Timeline.Research.Points, Is.EqualTo(5f));
        }

        [Test]
        public void ActiveProject_AdvancesFromLabScienceAndReplayMatches()
        {
            var projects = new[]
            {
                Project("basic-construction", 20f, TechnologyEffect.Capability("construction")),
                Project("basic-energy", 20f, TechnologyEffect.Capability("energy")),
                Project("basic-travel", 20f, TechnologyEffect.Capability("travel")),
                Project("basic-fleet", 20f, TechnologyEffect.Capability("fleet")),
            };
            var first = LabHarness(71, projects: projects);
            var second = LabHarness(71, projects: projects);
            first.Research.OnRunStarted(first.Run);
            second.Research.OnRunStarted(second.Run);
            var offer = first.Run.Timeline.Research.Offers[0];
            Assert.That(second.Run.Timeline.Research.Offers[0], Is.EqualTo(offer));
            Assert.That(second.Research.Start(second.Run, offer).Success, Is.True);
            Assert.That(first.Research.Start(first.Run, offer).Success, Is.True);

            var firstTick = TicksUntilComplete(first);
            var secondTick = TicksUntilComplete(second);

            Assert.That(firstTick, Is.GreaterThan(1));
            Assert.That(secondTick, Is.EqualTo(firstTick));
            Assert.That(second.Run.Archive.Research.Contains(offer), Is.True);
            Assert.That(Ids(second.Run), Is.EqualTo(Ids(first.Run)));
            Assert.That(first.Run.Timeline.Resources.Get(ResourceKind.Science), Is.EqualTo(0));
        }

        [Test]
        public void IdleScience_DoesNotStockpileOrBankProgress()
        {
            var harness = LabHarness(seed: 72);
            harness.Research.OnRunStarted(harness.Run);

            harness.Economy.ApplyTick(harness.Run);
            harness.Research.ApplyScience(harness.Run, harness.Economy.ProducedScience);

            Assert.That(harness.Economy.ProducedScience, Is.EqualTo(2f));
            Assert.That(harness.Run.Timeline.Resources.Get(ResourceKind.Science), Is.EqualTo(0));
            Assert.That(harness.Run.Timeline.Research.Points, Is.EqualTo(0f));

            Assert.That(harness.Research.Start(harness.Run, Id("basic-construction")).Success, Is.True);
            Assert.That(harness.Run.Timeline.Research.Points, Is.EqualTo(0f));
        }

        [Test]
        public void ZeroPointCost_IsRejected()
        {
            var harness = Harness(Project("free", 0f, TechnologyEffect.Capability("free")));
            harness.Research.OnRunStarted(harness.Run);

            var rejected = harness.Research.Start(harness.Run, Id("free"));

            Assert.That(rejected.Success, Is.False);
            Assert.That(rejected.Error.Code, Is.EqualTo(ProjectStarwatch.Core.Commands.CommandErrorCode.InvalidArgument));
            Assert.That(harness.Run.Timeline.Research.HasActive, Is.False);
        }

        [Test]
        public void ZeroScience_NotifiesListenersWithoutAddingProgress()
        {
            var harness = Harness(Project("basic-construction", 20f, TechnologyEffect.Capability("construction")));
            harness.Research.OnRunStarted(harness.Run);
            Assert.That(harness.Research.Start(harness.Run, Id("basic-construction")).Success, Is.True);
            var notifications = 0;
            harness.Research.ScienceApplied += () => notifications++;

            harness.Research.ApplyScience(harness.Run, 0f);

            Assert.That(notifications, Is.EqualTo(1));
            Assert.That(harness.Run.Timeline.Research.Points, Is.EqualTo(0f));
            Assert.That(harness.Run.Timeline.Research.ActiveId, Is.EqualTo(Id("basic-construction")));
        }

        [Test]
        public void UnpaidUpkeep_AddsNoResearchProgress()
        {
            var harness = LabHarness(seed: 73, energyUpkeep: 1f);
            harness.Research.OnRunStarted(harness.Run);
            Assert.That(harness.Research.Start(harness.Run, Id("basic-construction")).Success, Is.True);

            harness.Economy.ApplyTick(harness.Run);
            harness.Research.ApplyScience(harness.Run, harness.Economy.ProducedScience);

            Assert.That(harness.Economy.ProducedScience, Is.EqualTo(0f));
            Assert.That(harness.Run.Timeline.Research.Points, Is.EqualTo(0f));
        }

        [Test]
        public void OfferRefreshes_DifferAcrossDrawsAndReplay()
        {
            var projects = new[]
            {
                Project("one", 10f, TechnologyEffect.Capability("one")),
                Project("two", 10f, TechnologyEffect.Capability("two")),
                Project("three", 10f, TechnologyEffect.Capability("three")),
                Project("four", 10f, TechnologyEffect.Capability("four")),
            };
            var first = Harness(projects);
            var replay = Harness(projects);
            first.Research.OnRunStarted(first.Run);
            replay.Research.OnRunStarted(replay.Run);
            Assert.That(Ids(replay.Run), Is.EqualTo(Ids(first.Run)));

            var initial = Ids(first.Run);
            var differed = false;
            for (var i = 0; i < 8 && !differed; i++)
            {
                first.Research.OnRunStarted(first.Run);
                replay.Research.OnRunStarted(replay.Run);
                Assert.That(Ids(replay.Run), Is.EqualTo(Ids(first.Run)));
                differed = Ids(first.Run) != initial;
            }

            Assert.That(differed, Is.True);
        }

        [Test]
        public void StartAndCompletion_ReplaceOffers()
        {
            var harness = Harness(
                Project("one", 4f, TechnologyEffect.Capability("one")),
                Project("two", 4f, TechnologyEffect.Capability("two")),
                Project("three", 4f, TechnologyEffect.Capability("three")),
                Project("four", 4f, TechnologyEffect.Capability("four")));
            harness.Research.OnRunStarted(harness.Run);
            var seeded = harness.Run.Timeline.Research.RefreshCount;
            var started = harness.Run.Timeline.Research.Offers[0];

            Assert.That(harness.Research.Start(harness.Run, started).Success, Is.True);

            Assert.That(harness.Run.Timeline.Research.RefreshCount, Is.EqualTo(seeded + 1));
            Assert.That(harness.Run.Timeline.Research.IsOffered(started), Is.False);
            Assert.That(harness.Run.Timeline.Research.Offers, Has.Count.EqualTo(3));

            harness.Research.ApplyScience(harness.Run, 4f);

            Assert.That(harness.Run.Timeline.Research.RefreshCount, Is.EqualTo(seeded + 2));
            Assert.That(harness.Run.Timeline.Research.HasActive, Is.False);
            Assert.That(harness.Run.Timeline.Research.IsOffered(started), Is.False);
            Assert.That(harness.Run.Archive.Research.Contains(started), Is.True);
        }

        [Test]
        public void MissingProject_RejectsStartAndIgnoresScience()
        {
            var harness = Harness(Project("basic-construction", 20f, TechnologyEffect.Capability("construction")));
            harness.Research.OnRunStarted(harness.Run);

            var rejected = harness.Research.Start(harness.Run, Id("ghost"));

            Assert.That(rejected.Success, Is.False);
            Assert.That(rejected.Error.Code, Is.EqualTo(ProjectStarwatch.Core.Commands.CommandErrorCode.InvalidArgument));
            Assert.That(harness.Run.Timeline.Research.HasActive, Is.False);

            harness.Run.Timeline.Research.Start(Id("ghost"));
            LogAssert.Expect(
                LogType.Warning,
                "ResearchService ignored Science for active project 'ghost' because it is not in the catalog.");

            harness.Research.ApplyScience(harness.Run, 5f);

            Assert.That(harness.Run.Timeline.Research.Points, Is.EqualTo(0f));
            Assert.That(harness.Run.Timeline.Research.ActiveId, Is.EqualTo(Id("ghost")));
            Assert.That(harness.Run.Archive.Research.Contains(Id("ghost")), Is.False);
        }

        [Test]
        public void Completion_RecordsKnowledgeAndChangesTheProductionQuote()
        {
            var harness = Harness(Project("basic-construction", 4f, TechnologyEffect.Output(ResourceKind.Industry, 1.1f)));
            harness.Research.OnRunStarted(harness.Run);
            Assert.That(harness.Research.Start(harness.Run, Id("basic-construction")).Success, Is.True);
            harness.Research.ApplyScience(harness.Run, 4f);

            Assert.That(harness.Run.Archive.Research.Contains(Id("basic-construction")), Is.True);
            Assert.That(harness.Effects.OutputMultiplier(ResourceKind.Industry), Is.EqualTo(1.1f).Within(0.001f));
            var colony = new ColonyState(Id("home-prime"), ColonyPresence.Colony);
            colony.AddBuilding(new ColonyBuildingState(Id("industry-works"), 1));
            var quote = harness.Production.Quote(colony, Planet(1f));
            Assert.That(quote.Output.Get(ResourceKind.Industry), Is.EqualTo(2.2f).Within(0.001f));
        }

        [Test]
        public void IndustryMultipliers_StackWithEachOtherAndThePlanet()
        {
            var harness = Harness(
                Project("basic-construction", 1f, TechnologyEffect.Output(ResourceKind.Industry, 2f)),
                Project("industrial-focus", 1f, TechnologyEffect.Output(ResourceKind.Industry, 1.5f), "basic-construction"));
            harness.Run.Archive.Research.TryAdd(Id("basic-construction"));
            harness.Run.Archive.Research.TryAdd(Id("industrial-focus"));
            harness.Effects.Rebuild(harness.Run, harness.Database.ResearchCatalog);

            var colony = new ColonyState(Id("home-prime"), ColonyPresence.Colony);
            colony.AddBuilding(new ColonyBuildingState(Id("industry-works"), 1));
            var quote = harness.Production.Quote(colony, Planet(2f));

            Assert.That(quote.Output.Get(ResourceKind.Industry), Is.EqualTo(12f).Within(0.001f));
        }

        [Test]
        public void TimelineReset_DropsProgressAndKeepsCompletedResearch()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(75));
            var offered = run.Timeline.Research.Offers[0];
            Assert.That(stack.Research.Start(run, offered).Success, Is.True);
            run.Archive.Research.TryAdd(ContentId.From("basic-construction"));
            run.Timeline.Research.AddPoints(3f);

            stack.Manager.RecreateTimeline(1);

            Assert.That(run.Timeline.Research.HasActive, Is.False);
            Assert.That(run.Timeline.Research.Points, Is.EqualTo(0f));
            Assert.That(run.Archive.Research.Contains(ContentId.From("basic-construction")), Is.True);
            Assert.That(stack.Research.IsEligible(run, ContentId.From("relic-foundry")), Is.True);
        }

        private static int TicksUntilComplete(ResearchHarness harness)
        {
            var active = harness.Run.Timeline.Research.ActiveId;
            for (var tick = 1; tick <= 20; tick++)
            {
                harness.Economy.ApplyTick(harness.Run);
                harness.Research.ApplyScience(harness.Run, harness.Economy.ProducedScience);
                if (harness.Run.Archive.Research.Contains(active))
                {
                    return tick;
                }
            }

            Assert.Fail("Project did not complete.");
            return 0;
        }

        private static string Ids(RunState run)
        {
            var offers = run.Timeline.Research.Offers;
            var text = string.Empty;
            for (var i = 0; i < offers.Count; i++)
            {
                text += offers[i].Value + ",";
            }

            return text;
        }

        private static ContentId Id(string value)
        {
            return ContentId.From(value);
        }

        private static ResearchHarness LabHarness(
            int seed,
            float energyUpkeep = 0f,
            params ResearchDefinitionSO[] projects)
        {
            var database = RunTestFixture.CreateObjectDatabase();
            var lab = ScriptableObject.CreateInstance<BuildingDefinitionSO>();
            lab.SetContent(
                "laboratory",
                new[] { new ResourceAmount(ResourceKind.Science, 2f) },
                new[] { new ResourceAmount(ResourceKind.Energy, energyUpkeep) });
            var works = ScriptableObject.CreateInstance<BuildingDefinitionSO>();
            works.SetContent(
                "works",
                new[] { new ResourceAmount(ResourceKind.Industry, 1f) },
                Array.Empty<ResourceAmount>());
            var settlement = ScriptableObject.CreateInstance<SettlementCatalogSO>();
            settlement.SetContent(new[] { lab, works });
            database.AssignDatabases(null, null, settlement, database.ResearchCatalog);
            if (projects == null || projects.Length == 0)
            {
                projects = new[] { Project("basic-construction", 20f, TechnologyEffect.Capability("construction")) };
            }

            var harness = Harness(database, seed, projects);
            var colony = new ColonyState(Id("home-prime"), ColonyPresence.Colony);
            colony.AddBuilding(new ColonyBuildingState(Id("laboratory"), 1));
            harness.Run.Timeline.TryAddColony(colony);
            return harness;
        }

        private static ResearchHarness Harness(params ResearchDefinitionSO[] projects)
        {
            return Harness(RunTestFixture.CreateObjectDatabase(), 71, projects);
        }

        private static ResearchHarness Harness(ProjectStarwatch.Core.Content.ObjectDatabaseSO database, params ResearchDefinitionSO[] projects)
        {
            return Harness(database, 71, projects);
        }

        private static ResearchHarness Harness(
            ProjectStarwatch.Core.Content.ObjectDatabaseSO database,
            int seed,
            params ResearchDefinitionSO[] projects)
        {
            var catalog = ScriptableObject.CreateInstance<ResearchCatalogSO>();
            catalog.SetContent(projects);
            database.AssignResearchCatalog(catalog);
            var effects = new EffectRegistry();
            var events = new DomainEventBus(new EventJournal());
            var clock = new SimulationClock();
            var production = new ProductionService(database, effects);
            var economy = new EconomyService(database, production, events, clock);
            var research = new ResearchService(database, effects, events, clock);
            var entityIds = new EntityIdFactory();
            var run = new RunState(
                RunSeed.FromValue(seed),
                new ProjectStarwatch.Core.Run.Universe.UniverseState("research", StartingEmpireDefinition.CreateDefault(), ProjectStarwatch.Core.Run.Universe.MapGraph.Empty),
                new TimelineState(entityIds),
                new ProjectStarwatch.Core.Run.Archive.RunArchive(),
                rewindIndex: 0,
                new DeterministicRandom(seed),
                entityIds);
            return new ResearchHarness(database, effects, economy, production, research, run);
        }

        private static ResearchDefinitionSO Project(
            string id,
            float cost,
            TechnologyEffect effect,
            string prerequisite = null)
        {
            var project = ScriptableObject.CreateInstance<ResearchDefinitionSO>();
            project.SetContent(
                id,
                id,
                ResearchDomain.Construction,
                cost,
                string.IsNullOrEmpty(prerequisite) ? null : new[] { prerequisite },
                effect);
            return project;
        }

        private static PlanetDefinition Planet(float industry)
        {
            return new PlanetDefinition(
                Id("home-prime"),
                Id("home-system"),
                "Home",
                "Test",
                footprintBudget: 4,
                modifiers: new PlanetModifiers(industry, 1f, 1f, 1f));
        }

        private sealed class ResearchHarness
        {
            public ProjectStarwatch.Core.Content.ObjectDatabaseSO Database { get; }

            public EffectRegistry Effects { get; }

            public EconomyService Economy { get; }

            public ProductionService Production { get; }

            public ResearchService Research { get; }

            public RunState Run { get; }

            public ResearchHarness(
                ProjectStarwatch.Core.Content.ObjectDatabaseSO database,
                EffectRegistry effects,
                EconomyService economy,
                ProductionService production,
                ResearchService research,
                RunState run)
            {
                Database = database;
                Effects = effects;
                Economy = economy;
                Production = production;
                Research = research;
                Run = run;
            }
        }
    }

}
