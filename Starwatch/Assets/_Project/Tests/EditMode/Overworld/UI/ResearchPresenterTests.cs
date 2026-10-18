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
            Assert.That(view.Model.Offers[0].CanStart, Is.False);
            Assert.That(run.Timeline.Research.ActiveId, Is.EqualTo(first));

            view.RaiseStart(view.Model.Offers[0].Id);

            Assert.That(run.Timeline.Research.ActiveId, Is.EqualTo(first));
            presenter.Dispose();
        }

        private sealed class FakeResearchView : IResearchView
        {
            public event System.Action<ContentId> StartRequested;

            public ResearchPanelModel Model { get; private set; }

            public void Apply(ResearchPanelModel model)
            {
                Model = model;
            }

            public void RaiseStart(ContentId researchId)
            {
                StartRequested?.Invoke(researchId);
            }
        }
    }
}
