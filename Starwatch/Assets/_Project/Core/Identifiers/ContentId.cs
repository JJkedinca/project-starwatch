using System;

namespace ProjectStarwatch.Core.Identifiers
{
    /// <summary>
    /// Stable identifier for authored content definitions (systems, ships, techs).
    /// </summary>
    public readonly struct ContentId : IEquatable<ContentId>
    {
        public string Value { get; }

        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        public ContentId(string value)
        {
            Value = value ?? string.Empty;
        }

        /// <summary>
        /// Creates a ContentId from a non-empty string value.
        /// </summary>
        public static ContentId From(string value)
        {
            return new ContentId(value);
        }

        public bool Equals(ContentId other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is ContentId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(Value ?? string.Empty);
        }

        public override string ToString()
        {
            return Value;
        }

        public static bool operator ==(ContentId left, ContentId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ContentId left, ContentId right)
        {
            return !left.Equals(right);
        }
    }
}
