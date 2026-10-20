using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;

namespace ProjectStarwatch.Core.Run.Timeline
{
    /// <summary>
    /// Ships of one class inside a fleet.
    /// </summary>
    public readonly struct ShipStack
    {
        public ContentId ClassId { get; }

        public int Count { get; }

        public ShipStack(ContentId classId, int count)
        {
            if (classId.IsEmpty)
            {
                throw new ArgumentException("Ship class id is required.", nameof(classId));
            }

            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, "Ship count must be positive.");
            }

            ClassId = classId;
            Count = count;
        }
    }

    /// <summary>
    /// One fleet: where it is and the ships in it. Stacks change only through <see cref="FleetLedger"/>.
    /// </summary>
    public sealed class FleetComposition
    {
        private readonly List<ShipStack> _stacks;

        public EntityId Id { get; }

        public ContentId LocationSystemId { get; }

        public IReadOnlyList<ShipStack> Stacks => _stacks;

        public int ShipCount
        {
            get
            {
                var count = 0;
                for (var i = 0; i < _stacks.Count; i++)
                {
                    count += _stacks[i].Count;
                }

                return count;
            }
        }

        public FleetComposition(EntityId id, ContentId locationSystemId, IReadOnlyList<ShipStack> stacks)
        {
            if (id.IsNone)
            {
                throw new ArgumentException("Fleet id is required.", nameof(id));
            }

            if (locationSystemId.IsEmpty)
            {
                throw new ArgumentException("Location system id is required.", nameof(locationSystemId));
            }

            _stacks = Copy(stacks);
            if (_stacks.Count == 0)
            {
                throw new ArgumentException("A fleet needs at least one ship.", nameof(stacks));
            }

            Id = id;
            LocationSystemId = locationSystemId;
        }

        public int CountOf(ContentId classId)
        {
            var count = 0;
            for (var i = 0; i < _stacks.Count; i++)
            {
                if (_stacks[i].ClassId == classId)
                {
                    count += _stacks[i].Count;
                }
            }

            return count;
        }

        private static List<ShipStack> Copy(IReadOnlyList<ShipStack> source)
        {
            var copy = new List<ShipStack>();
            if (source == null)
            {
                return copy;
            }

            for (var i = 0; i < source.Count; i++)
            {
                var stack = source[i];
                if (stack.Count <= 0 || stack.ClassId.IsEmpty)
                {
                    continue;
                }

                var merged = false;
                for (var slot = 0; slot < copy.Count; slot++)
                {
                    if (copy[slot].ClassId != stack.ClassId)
                    {
                        continue;
                    }

                    copy[slot] = new ShipStack(stack.ClassId, copy[slot].Count + stack.Count);
                    merged = true;
                    break;
                }

                if (!merged)
                {
                    copy.Add(stack);
                }
            }

            return copy;
        }
    }
}
