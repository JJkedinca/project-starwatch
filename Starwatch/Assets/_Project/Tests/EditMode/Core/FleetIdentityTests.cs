using System;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Core.Run.Timeline;
using NUnit.Framework;

namespace ProjectStarwatch.Tests.EditMode.Core
{
    public sealed class FleetIdentityTests
    {
        [Test]
        public void Constructor_KeepsId()
        {
            var id = new EntityId(4L);

            var identity = new FleetIdentity(id);

            Assert.That(identity.Id, Is.EqualTo(id));
        }

        [Test]
        public void Constructor_RejectsNone()
        {
            Assert.Throws<ArgumentException>(() => new FleetIdentity(EntityId.None));
        }
    }
}
