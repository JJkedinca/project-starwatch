using EndlessHeros.Core.Data;
using EndlessHeros.Core.Run.Timeline;
using NUnit.Framework;

namespace EndlessHeros.Tests.EditMode.Core.Run
{
    /// <summary>
    /// EditMode coverage for per-resource totals.
    /// </summary>
    public sealed class ResourceInventoryTests
    {
        [Test]
        public void Add_AccumulatesOneTotalPerKind()
        {
            var inventory = new ResourceInventory();

            inventory.Add(ResourceKind.Industry, 1.5f);
            inventory.Add(ResourceKind.Industry, 1.5f);
            inventory.Add(ResourceKind.Energy, 0f);

            Assert.That(inventory.Get(ResourceKind.Industry), Is.EqualTo(3f));
            Assert.That(inventory.Get(ResourceKind.Energy), Is.EqualTo(0f));
            Assert.That(inventory.GetWhole(ResourceKind.Industry), Is.EqualTo(3));
        }

        [Test]
        public void TrySpend_LeavesTheStockpileUntouchedWhenUpkeepIsUnpaid()
        {
            var ledger = new ResourceLedger();
            var cost = new ResourceInventory();
            ledger.Add(ResourceKind.Energy, 2);
            cost.Add(ResourceKind.Energy, 3f);

            Assert.That(ledger.TrySpend(cost), Is.False);
            Assert.That(ledger.Get(ResourceKind.Energy), Is.EqualTo(2));

            cost.Clear();
            cost.Add(ResourceKind.Energy, 2f);
            Assert.That(ledger.TrySpend(cost), Is.True);
            Assert.That(ledger.Get(ResourceKind.Energy), Is.EqualTo(0));
        }

        [Test]
        public void TrySpend_RejectsNegativeAndUnknownCostsWithoutChangingStock()
        {
            var ledger = new ResourceLedger();
            ledger.Add(ResourceKind.Industry, 10);
            ledger.Add(ResourceKind.Energy, 4);
            var negative = new[] { new ResourceAmount(ResourceKind.Industry, -5) };
            var unknown = new[] { new ResourceAmount((ResourceKind)99, 5) };
            var mixed = new[]
            {
                new ResourceAmount(ResourceKind.Energy, 1),
                new ResourceAmount(ResourceKind.Industry, -5),
            };

            Assert.That(ledger.CanAfford(negative), Is.False);
            Assert.That(ledger.TrySpend(negative), Is.False);
            Assert.That(ledger.TrySpend(unknown), Is.False);
            Assert.That(ledger.TrySpend(mixed), Is.False);
            Assert.That(ledger.Get(ResourceKind.Industry), Is.EqualTo(10));
            Assert.That(ledger.Get(ResourceKind.Energy), Is.EqualTo(4));

            var quoted = new ResourceInventory();
            quoted.Add((ResourceKind)99, 1f);
            Assert.That(ledger.TrySpend(quoted), Is.False);
            Assert.That(ledger.Get(ResourceKind.Industry), Is.EqualTo(10));
        }

        [Test]
        public void Grant_SumsAKindBeforeKeepingTheWholePart()
        {
            var ledger = new ResourceLedger();

            ledger.Grant(new[]
            {
                new ResourceAmount(ResourceKind.Industry, 0.6f),
                new ResourceAmount(ResourceKind.Industry, 0.6f),
            });

            Assert.That(ledger.Get(ResourceKind.Industry), Is.EqualTo(1));
        }

        [Test]
        public void TrySpend_RejectsAFractionalCost()
        {
            var ledger = new ResourceLedger();
            ledger.Add(ResourceKind.Industry, 5);
            var fractional = new[] { new ResourceAmount(ResourceKind.Industry, 1.5f) };
            var halves = new[]
            {
                new ResourceAmount(ResourceKind.Industry, 0.5f),
                new ResourceAmount(ResourceKind.Industry, 0.5f),
            };

            Assert.That(ledger.CanAfford(fractional), Is.False);
            Assert.That(ledger.TrySpend(fractional), Is.False);
            Assert.That(ledger.Get(ResourceKind.Industry), Is.EqualTo(5));
            Assert.That(ledger.TrySpend(halves), Is.True);
            Assert.That(ledger.Get(ResourceKind.Industry), Is.EqualTo(4));
        }

        [Test]
        public void DepositFlooredInto_GrantsWholeResources()
        {
            var output = new ResourceInventory();
            var ledger = new ResourceLedger();
            output.Add(ResourceKind.Industry, 2.9f);

            output.DepositFlooredInto(ledger);

            Assert.That(ledger.Get(ResourceKind.Industry), Is.EqualTo(2));
        }

        [Test]
        public void RetainFraction_KeepsThePartBelowOne()
        {
            var inventory = new ResourceInventory();
            var ledger = new ResourceLedger();
            inventory.Add(ResourceKind.Industry, 2.9f);

            inventory.DepositFlooredInto(ledger);
            inventory.RetainFraction();

            Assert.That(ledger.Get(ResourceKind.Industry), Is.EqualTo(2));
            Assert.That(inventory.Get(ResourceKind.Industry), Is.EqualTo(0.9f).Within(0.0001f));
        }
    }
}
