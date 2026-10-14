using System;

namespace EndlessHeros.Core.Identifiers
{
    /// <summary>
    /// Stable identity for a knowledge/visibility observer (player or faction).
    /// </summary>
    public readonly struct ObserverId : IEquatable<ObserverId>
    {
        public string Value { get; }

        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        public static ObserverId Player { get; } = new ObserverId("player");

        public ObserverId(string value)
        {
            Value = value ?? string.Empty;
        }

        /// <summary>
        /// Creates an ObserverId from a non-empty string value.
        /// </summary>
        public static ObserverId From(string value)
        {
            return new ObserverId(value);
        }

        public bool Equals(ObserverId other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is ObserverId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(Value ?? string.Empty);
        }

        public override string ToString()
        {
            return Value;
        }

        public static bool operator ==(ObserverId left, ObserverId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ObserverId left, ObserverId right)
        {
            return !left.Equals(right);
        }
    }
}
