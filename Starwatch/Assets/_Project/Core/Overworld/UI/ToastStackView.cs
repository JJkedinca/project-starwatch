using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Renders up to three toasts at bottom center. Each toast keeps the expiry
    /// stamped at post time — later stack changes do not restart older timers.
    /// </summary>
    internal sealed class ToastStackView
    {
        private VisualElement _root;
        private OverworldStatus _status;
        private readonly Dictionary<int, VisualElement> _elements = new();
        private readonly Dictionary<int, CancellationTokenSource> _timers = new();

        public void Bind(VisualElement root, OverworldStatus status)
        {
            _root = root;
            if (_status != null)
            {
                _status.Changed -= OnChanged;
            }

            CancelAllTimers();
            _elements.Clear();
            _root?.Clear();
            _status = status;
            if (_status != null)
            {
                _status.Changed += OnChanged;
                OnChanged();
            }
        }

        public void Unbind()
        {
            if (_status != null)
            {
                _status.Changed -= OnChanged;
            }

            CancelAllTimers();
            _elements.Clear();
            _root?.Clear();
            _status = null;
            _root = null;
        }

        private void OnChanged()
        {
            if (_root == null || _status == null)
            {
                return;
            }

            var toasts = _status.Toasts;
            HashSet<int> keep = new();
            for (var i = 0; i < toasts.Count; i++)
            {
                keep.Add(toasts[i].Id);
            }

            List<int> remove = null;
            foreach (var pair in _elements)
            {
                if (keep.Contains(pair.Key))
                {
                    continue;
                }

                remove ??= new List<int>();
                remove.Add(pair.Key);
            }

            if (remove != null)
            {
                for (var i = 0; i < remove.Count; i++)
                {
                    RemoveElement(remove[i]);
                }
            }

            for (var i = 0; i < toasts.Count; i++)
            {
                var toast = toasts[i];
                if (_elements.TryGetValue(toast.Id, out var existing))
                {
                    if (existing.parent == _root)
                    {
                        _root.Remove(existing);
                    }

                    _root.Add(existing);
                    continue;
                }

                var row = CreateRow(toast);
                _elements[toast.Id] = row;
                _root.Add(row);
                ArmTimer(toast);
            }
        }

        private VisualElement CreateRow(StatusToast toast)
        {
            var row = new VisualElement();
            row.name = "toast-" + toast.Id;
            row.AddToClassList("toast");
            row.AddToClassList(ClassFor(toast.Kind));

            var accent = new VisualElement { pickingMode = PickingMode.Ignore };
            accent.AddToClassList("toast__accent");
            row.Add(accent);

            var label = new Label(toast.Message);
            label.AddToClassList("toast__label");
            label.pickingMode = PickingMode.Ignore;
            row.Add(label);

            var toastId = toast.Id;
            var dismiss = new Button(() => _status?.Dismiss(toastId));
            dismiss.name = "toast-dismiss-" + toast.Id;
            dismiss.text = "×";
            dismiss.AddToClassList("toast__dismiss");
            row.Add(dismiss);

            return row;
        }

        private void ArmTimer(StatusToast toast)
        {
            if (_timers.ContainsKey(toast.Id))
            {
                return;
            }

            var remaining = toast.RemainingSeconds(Time.realtimeSinceStartupAsDouble);
            var cts = new CancellationTokenSource();
            _timers[toast.Id] = cts;
            RunLifetime(toast.Id, remaining, cts.Token).Forget();
        }

        private async UniTaskVoid RunLifetime(int toastId, float remainingSeconds, CancellationToken token)
        {
            var ms = Mathf.Max(0, (int)(remainingSeconds * 1000f));
            try
            {
                if (ms > 0)
                {
                    await UniTask.Delay(ms, cancellationToken: token);
                }
            }
            catch (System.OperationCanceledException)
            {
                return;
            }

            if (token.IsCancellationRequested)
            {
                return;
            }

            _timers.Remove(toastId);
            _status?.Dismiss(toastId);
        }

        private void RemoveElement(int id)
        {
            if (_timers.TryGetValue(id, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
                _timers.Remove(id);
            }

            if (_elements.TryGetValue(id, out var element))
            {
                element.RemoveFromHierarchy();
                _elements.Remove(id);
            }
        }

        private void CancelAllTimers()
        {
            foreach (var pair in _timers)
            {
                pair.Value.Cancel();
                pair.Value.Dispose();
            }

            _timers.Clear();
        }

        private static string ClassFor(StatusKind kind)
        {
            switch (kind)
            {
                case StatusKind.Error:
                    return "toast--error";
                case StatusKind.Event:
                    return "toast--event";
                default:
                    return "toast--result";
            }
        }
    }
}
