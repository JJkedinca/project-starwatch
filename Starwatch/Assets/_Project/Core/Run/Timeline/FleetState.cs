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
    /// Timeline-owned fleet. Stacks change only through <see cref="TimelineState"/>.
    /// </summary>
    public sealed class FleetState
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

        public FleetState(EntityId id, ContentId locationSystemId, IReadOnlyList<ShipStack> stacks)
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
            for (var i = 0; i < _stacks.Count; i++)
            {
                if (_stacks[i].ClassId == classId)
                {
                    return _stacks[i].Count;
                }
            }

            return 0;
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
                if (source[i].Count > 0 && !source[i].ClassId.IsEmpty)
                {
                    copy.Add(source[i]);
                }
            }

            return copy;
        }
    }
}
