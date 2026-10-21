using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Toast kind for the overworld stack at bottom center.
    /// </summary>
    internal enum StatusKind
    {
        Error = 0,
        Result = 1,
        Event = 2,
    }

    /// <summary>
    /// One toast in the stack. Newest entries sit at index 0.
    /// </summary>
    internal readonly struct StatusToast
    {
        public int Id { get; }

        public string Message { get; }

        public StatusKind Kind { get; }

        /// <summary>
        /// Unscaled-time seconds when this toast should expire.
        /// </summary>
        public double ExpireAtUnscaledSeconds { get; }

        public StatusToast(int id, string message, StatusKind kind, double expireAtUnscaledSeconds)
        {
            Id = id;
            Message = message ?? string.Empty;
            Kind = kind;
            ExpireAtUnscaledSeconds = expireAtUnscaledSeconds;
        }

        public bool IsExpired(double nowUnscaledSeconds)
        {
            return nowUnscaledSeconds >= ExpireAtUnscaledSeconds;
        }

        public float RemainingSeconds(double nowUnscaledSeconds)
        {
            var remaining = ExpireAtUnscaledSeconds - nowUnscaledSeconds;
            return remaining > 0d ? (float)remaining : 0f;
        }
    }

    /// <summary>
    /// Kinded toast queue. Presenters post errors/results; event toasts come from
    /// <see cref="OverworldToastPresenter"/>. Hints stay on panel models, not here.
    /// </summary>
    internal sealed class OverworldStatus
    {
        internal const int kMaxToasts = 3;

        internal const float kErrorSeconds = 4f;

        internal const float kResultSeconds = 3f;

        internal const float kEventSeconds = 5f;

        internal const string kNoRun = "No active run.";

        public event Action Changed;

        private readonly List<StatusToast> _toasts = new();
        private int _nextId = 1;
        private Func<double> _clock = DefaultClock;

        /// <summary>
        /// Newest first. Do not hold the list; it is replaced when toasts change.
        /// </summary>
        public IReadOnlyList<StatusToast> Toasts => _toasts;

        /// <summary>
        /// Newest toast message, or empty when the stack is clear.
        /// </summary>
        public string Text => _toasts.Count > 0 ? _toasts[0].Message : string.Empty;

        /// <summary>
        /// Pushes a toast to the front and drops older entries past <see cref="kMaxToasts"/>.
        /// Expiry is fixed at post time so later posts do not extend older toasts.
        /// </summary>
        public void Post(string message, StatusKind kind = StatusKind.Result)
        {
            var text = message ?? string.Empty;
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var now = _clock();
            var expireAt = now + LifetimeSeconds(kind);
            _toasts.Insert(0, new StatusToast(_nextId++, text, kind, expireAt));
            while (_toasts.Count > kMaxToasts)
            {
                _toasts.RemoveAt(_toasts.Count - 1);
            }

            Changed?.Invoke();
        }

        /// <summary>
        /// Posts a result toast on success or an error toast on failure.
        /// </summary>
        public void PostCommand(bool success, string successMessage, string errorMessage)
        {
            if (success)
            {
                Post(successMessage, StatusKind.Result);
                return;
            }

            Post(errorMessage, StatusKind.Error);
        }

        /// <summary>
        /// Removes one toast by id.
        /// </summary>
        public void Dismiss(int id)
        {
            for (var i = 0; i < _toasts.Count; i++)
            {
                if (_toasts[i].Id != id)
                {
                    continue;
                }

                _toasts.RemoveAt(i);
                Changed?.Invoke();
                return;
            }
        }

        /// <summary>
        /// Removes every toast whose expiry has passed. Returns how many were removed.
        /// </summary>
        public int DismissExpired()
        {
            var now = _clock();
            var removed = 0;
            for (var i = _toasts.Count - 1; i >= 0; i--)
            {
                if (!_toasts[i].IsExpired(now))
                {
                    continue;
                }

                _toasts.RemoveAt(i);
                removed++;
            }

            if (removed > 0)
            {
                Changed?.Invoke();
            }

            return removed;
        }

        /// <summary>
        /// Lifetime in seconds for a toast kind.
        /// </summary>
        internal static float LifetimeSeconds(StatusKind kind)
        {
            switch (kind)
            {
                case StatusKind.Error:
                    return kErrorSeconds;
                case StatusKind.Event:
                    return kEventSeconds;
                default:
                    return kResultSeconds;
            }
        }

        /// <summary>
        /// Test hook so expiry rules can be asserted without waiting on realtime.
        /// </summary>
        internal void SetClockForTests(Func<double> clock)
        {
            _clock = clock ?? DefaultClock;
        }

        private static double DefaultClock()
        {
            return Time.realtimeSinceStartupAsDouble;
        }
    }
}
