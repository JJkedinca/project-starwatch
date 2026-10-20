using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Overworld.UI;
using NUnit.Framework;

namespace ProjectStarwatch.Tests.EditMode.Overworld.UI
{
    /// <summary>
    /// Effect copy shown when the player hovers a research project.
    /// </summary>
    public sealed class ResearchEffectTextTests
    {
        [Test]
        public void Describe_NamesTheResourceAndThePercentBonus()
        {
            var text = ResearchEffectText.Describe(TechnologyEffect.Output(ResourceKind.Industry, 1.1f));

            Assert.That(text, Does.Contain(nameof(ResourceKind.Industry)));
            Assert.That(text, Does.Contain("10"));
        }

        [Test]
        public void Describe_NamesTheCapability()
        {
            var text = ResearchEffectText.Describe(TechnologyEffect.Capability("travel-i"));

            Assert.That(text, Does.Contain("travel-i"));
        }

        [Test]
        public void Describe_ReportsNoEffectWhenThePayloadDoesNothing()
        {
            var none = ResearchEffectText.Describe(default);
            var blankCapability = ResearchEffectText.Describe(TechnologyEffect.Capability(" "));
            var ignoredMultiplier = ResearchEffectText.Describe(TechnologyEffect.Output(ResourceKind.Energy, 0f));
            var travel = ResearchEffectText.Describe(TechnologyEffect.TravelSpeed(1.25f));

            Assert.That(blankCapability, Is.EqualTo(none));
            Assert.That(ignoredMultiplier, Is.EqualTo(none));
            Assert.That(travel, Does.Contain("25"));
            Assert.That(none, Is.Not.EqualTo(ResearchEffectText.Describe(TechnologyEffect.Capability("travel-i"))));
        }
    }
}
