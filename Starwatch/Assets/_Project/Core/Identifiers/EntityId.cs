using System;

namespace ProjectStarwatch.Core.Identifiers
{
    /// <summary>
    /// Stable runtime identity for mutable simulation entities such as fleets.
    /// </summary>
    public readonly struct EntityId : IEquatable<EntityId>
    {
        public long Value { get; }

        public bool IsNone => Value == 0L;

        public static EntityId None => new EntityId(0L);

        public EntityId(long value)
        {
            Value = value;
        }

        public bool Equals(EntityId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is EntityId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static bool operator ==(EntityId left, EntityId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EntityId left, EntityId right)
        {
            return !left.Equals(right);
        }
    }
}
