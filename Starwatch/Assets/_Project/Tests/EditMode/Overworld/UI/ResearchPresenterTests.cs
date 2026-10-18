using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run;
using EndlessHeros.Overworld.UI;
using EndlessHeros.Tests.EditMode.Core;
using NUnit.Framework;

namespace EndlessHeros.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// Research presenter intents and query flags against a fake view.
    /// </summary>
    public sealed class ResearchPresenterTests
    {
        [Test]
        public void Describe_FlagsDroppedScienceOnlyWhenTheHandOffWasPositive()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(80));
            var query = new ResearchQuery(RunTestFixture.CreateObjectDatabase(), stack.Economy);

            var idle = query.Describe(run);

            Assert.That(idle.HasActive, Is.False);
            Assert.That(idle.ScienceDropped, Is.False);
            Assert.That(idle.Offers, Has.Count.EqualTo(3));
            Assert.That(idle.Offers[0].CanStart, Is.True);

            stack.Economy.ApplyTick(run);

            var dropped = query.Describe(run);

            Assert.That(dropped.HasActive, Is.False);
            Assert.That(dropped.ScienceDropped, Is.True);
            Assert.That(dropped.SciencePerSecond, Is.EqualTo(0f));
        }

        [Test]
        public void Describe_ListsCompletedResearchByName()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(83));
            var database = RunTestFixture.CreateObjectDatabase();
            var id = ContentId.From("basic-construction");
            Assert.That(database.ResearchCatalog.TryGet(id, out var project), Is.True);
            run.Archive.Research.TryAdd(id);

            var model = new ResearchQuery(database, stack.Economy).Describe(run);

            Assert.That(model.CompletedNames, Does.Contain(project.DisplayName));
            Assert.That(model.CompletedTooltip, Does.Contain(project.DisplayName));
            Assert.That(model.CompletedTooltip, Does.Contain(ResearchEffectText.Describe(project.Effect)));
        }

        [Test]
        public void Describe_OffersCarryTheEffectTooltip()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(85));
            var database = RunTestFixture.CreateObjectDatabase();
            var model = new ResearchQuery(database, stack.Economy).Describe(run);

            Assert.That(model.Offers, Is.Not.Empty);
            for (var i = 0; i < model.Offers.Count; i++)
            {
                Assert.That(database.ResearchCatalog.TryGet(model.Offers[i].Id, out var project), Is.True);
                Assert.That(model.Offers[i].EffectText, Is.EqualTo(ResearchEffectText.Describe(project.Effect)));
            }
        }

        [Test]
        public void Start_RefreshesThePanelAndRejectsASecondProject()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(81));
            var view = new FakeResearchView();
            var status = new OverworldStatus();
            var presenter = new ResearchPresenter(
                view,
                status,
                stack.Manager,
                stack.Research,
                new ResearchQuery(RunTestFixture.CreateObjectDatabase(), stack.Economy),
                stack.Events);
            presenter.Start();
            var first = view.Model.Offers[0].Id;

            view.RaiseStart(first);

            Assert.That(view.Model.HasActive, Is.True);
            Assert.That(view.Model.ActiveId, Is.EqualTo(first));
            Assert.That(
                RunTestFixture.CreateObjectDatabase().ResearchCatalog.TryGet(first, out var started),
                Is.True);
            Assert.That(view.Model.ActiveEffectText, Is.EqualTo(ResearchEffectText.Describe(started.Effect)));
            Assert.That(view.Model.Offers[0].CanStart, Is.False);
            Assert.That(run.Timeline.Research.ActiveId, Is.EqualTo(first));

            var other = view.Model.Offers[0].Id;
            view.RaiseStart(other);

            Assert.That(run.Timeline.Research.ActiveId, Is.EqualTo(first));
            Assert.That(view.PromptedId, Is.EqualTo(other));

            view.ConfirmSwitch();

            Assert.That(run.Timeline.Research.ActiveId, Is.EqualTo(other));
            Assert.That(run.Timeline.Research.Points, Is.EqualTo(0f));
            presenter.Dispose();
        }

        [Test]
        public void ScienceTick_SkipsAClosedPanelAndRedrawsWhenOpen()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(84));
            var view = new FakeResearchView();
            var presenter = new ResearchPresenter(
                view,
                new OverworldStatus(),
                stack.Manager,
                stack.Research,
                new ResearchQuery(RunTestFixture.CreateObjectDatabase(), stack.Economy),
                stack.Events);
            presenter.Start();
            var afterStart = view.ApplyCount;

            stack.Research.ApplyScience(stack.Manager.CurrentRun, 0f);
            stack.Research.ApplyScience(stack.Manager.CurrentRun, 0f);

            Assert.That(view.ApplyCount, Is.EqualTo(afterStart));

            view.Open();

            Assert.That(view.ApplyCount, Is.EqualTo(afterStart + 1));

            stack.Economy.ApplyTick(stack.Manager.CurrentRun);
            stack.Research.ApplyScience(stack.Manager.CurrentRun, stack.Economy.ProducedScience);

            Assert.That(view.ApplyCount, Is.EqualTo(afterStart + 2));
            Assert.That(view.Model.ScienceDropped, Is.True);
            presenter.Dispose();
        }

        [Test]
        public void Completion_DismissesAnOpenSwitchPrompt()
        {
            var stack = RunTestFixture.CreateOverworldStack();
            var run = stack.Manager.CreateRun("authored-vertical-slice", RunSeed.FromValue(86));
            var view = new FakeResearchView();
            var presenter = new ResearchPresenter(
                view,
                new OverworldStatus(),
                stack.Manager,
                stack.Research,
                new ResearchQuery(RunTestFixture.CreateObjectDatabase(), stack.Economy),
                stack.Events);
            presenter.Start();
            var first = view.Model.Offers[0].Id;
            view.RaiseStart(first);
            view.RaiseStart(view.Model.Offers[0].Id);

            Assert.That(view.PromptedId, Is.EqualTo(view.Model.Offers[0].Id));

            stack.Research.ApplyScience(run, 1000f);

            Assert.That(run.Timeline.Research.HasActive, Is.False);
            Assert.That(view.PromptedId.IsEmpty, Is.True);
            presenter.Dispose();
        }

        private sealed class FakeResearchView : IResearchView
        {
            public event System.Action<ContentId> StartRequested;

            public event System.Action<ContentId> SwitchConfirmed;

            public event System.Action Opened;

            public ContentId PromptedId { get; private set; }

            public bool IsOpen { get; set; }

            public ResearchPanelModel Model { get; private set; }

            public int ApplyCount { get; private set; }

            public void Apply(ResearchPanelModel model)
            {
                ApplyCount++;
                Model = model;
            }

            public void Open()
            {
                IsOpen = true;
                Opened?.Invoke();
            }

            public void PromptSwitch(ContentId researchId)
            {
                PromptedId = researchId;
            }

            public void DismissSwitch()
            {
                PromptedId = default;
            }

            public void RaiseStart(ContentId researchId)
            {
                StartRequested?.Invoke(researchId);
            }

            public void ConfirmSwitch()
            {
                SwitchConfirmed?.Invoke(PromptedId);
            }
        }
    }
}
