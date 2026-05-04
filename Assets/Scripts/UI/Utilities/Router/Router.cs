using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine.UIElements;

namespace __GAME_NAMESPACE__.UI.Utilities
{
    /// <summary>
    /// Reactive, idempotent, serialized navigation between named routes.
    /// All operations queue automatically — concurrent calls do not throw.
    /// </summary>
    public sealed class Router<TKey> : IDisposable where TKey : notnull
    {
        private readonly Dictionary<TKey, RouteEntry> _routes;
        private readonly List<RouteEntry> _stack = new();
        private readonly ReactiveProperty<TKey?> _current = new(default);
        private readonly ReactiveProperty<IReadOnlyList<TKey>> _stackKeys = new(Array.Empty<TKey>());
        private readonly IEqualityComparer<TKey> _comparer;

        private UniTask _operationChain = UniTask.CompletedTask;
        private bool _disposed;

        public Router(IEqualityComparer<TKey>? comparer = null)
        {
            _comparer = comparer ?? EqualityComparer<TKey>.Default;
            _routes = new Dictionary<TKey, RouteEntry>(_comparer);
        }

        public Observable<TKey?> Current => _current;
        public Observable<IReadOnlyList<TKey>> Stack => _stackKeys;

        public TKey? CurrentValue => _current.Value;
        public int Depth => _stack.Count;

        public void Register(TKey key, RouteSpec spec)
        {
            ThrowIfDisposed();

            if (_routes.ContainsKey(key))
            {
                throw new InvalidOperationException($"Route '{key}' is already registered.");
            }

            RouteEntry entry = new(key, spec);
            entry.Attach();
            _routes[key] = entry;
        }

        public UniTask GoTo(TKey key)
        {
            return Enqueue(() => DoGoTo(key));
        }

        public UniTask Back()
        {
            return Enqueue(DoBack);
        }

        public UniTask Reset(TKey rootKey)
        {
            return Enqueue(() => DoReset(rootKey));
        }

        public UniTask Clear()
        {
            return Enqueue(DoClear);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            foreach (RouteEntry entry in _routes.Values)
            {
                entry.Detach();
            }

            _routes.Clear();
            _stack.Clear();
            _current.Dispose();
            _stackKeys.Dispose();
        }

        private UniTask Enqueue(Func<UniTask> operation)
        {
            ThrowIfDisposed();
            UniTask previous = _operationChain;
            UniTask current = ChainOperation(previous, operation);
            _operationChain = current;
            return current;
        }

        private static async UniTask ChainOperation(UniTask previous, Func<UniTask> operation)
        {
            try
            {
                await previous;
            }
            catch
            {
                // Earlier operation failed — still run this one.
            }

            await operation();
        }

        private async UniTask DoGoTo(TKey key)
        {
            RouteEntry next = ResolveRoute(key);

            if (_stack.Count > 0 && _comparer.Equals(_stack[^1].Key, key))
            {
                return;
            }

            int existingIndex = IndexOf(key);
            if (existingIndex >= 0)
            {
                int popsNeeded = _stack.Count - 1 - existingIndex;
                for (int i = 0; i < popsNeeded; i++)
                {
                    await PopOnce();
                }

                return;
            }

            await PushOnce(next);
        }

        private async UniTask DoBack()
        {
            if (_stack.Count <= 1)
            {
                return;
            }

            await PopOnce();
        }

        private async UniTask DoReset(TKey rootKey)
        {
            RouteEntry root = ResolveRoute(rootKey);

            if (_stack.Count > 0)
            {
                await ClearStackInternal();
            }

            await PushOnce(root);
        }

        private async UniTask DoClear()
        {
            if (_stack.Count == 0)
            {
                return;
            }

            await ClearStackInternal();
            UpdateObservables();
        }

        private async UniTask PushOnce(RouteEntry next)
        {
            RouteEntry? previous = _stack.Count > 0 ? _stack[^1] : null;
            previous?.SetInteractive(false);
            next.SetInteractive(true);
            _stack.Add(next);

            try
            {
                UniTask coverTask = previous != null ? previous.Cover(CancellationToken.None) : UniTask.CompletedTask;
                await UniTask.WhenAll(next.Enter(CancellationToken.None), coverTask);
                next.RestoreFocus();
            }
            catch
            {
                _stack.RemoveAt(_stack.Count - 1);
                next.SetInteractive(false);

                if (previous != null)
                {
                    previous.SetInteractive(true);
                    previous.RestoreFocus();
                }

                UpdateObservables();
                throw;
            }

            UpdateObservables();
        }

        private async UniTask PopOnce()
        {
            RouteEntry current = _stack[^1];
            RouteEntry? next = _stack.Count > 1 ? _stack[^2] : null;
            next?.SetInteractive(false);

            Exception? failure = null;
            try
            {
                UniTask revealTask = next != null ? next.Reveal(CancellationToken.None) : UniTask.CompletedTask;
                await UniTask.WhenAll(current.Exit(CancellationToken.None), revealTask);
            }
            catch (Exception ex)
            {
                failure = ex;
            }

            _stack.RemoveAt(_stack.Count - 1);
            current.SetInteractive(false);

            if (next != null)
            {
                next.SetInteractive(true);
                next.RestoreFocus();
            }

            UpdateObservables();

            if (failure != null)
            {
                throw failure;
            }
        }

        private async UniTask ClearStackInternal()
        {
            RouteEntry[] entries = _stack.ToArray();
            foreach (RouteEntry entry in entries)
            {
                entry.SetInteractive(false);
            }

            UniTask[] exits = new UniTask[entries.Length];
            for (int i = 0; i < entries.Length; i++)
            {
                exits[i] = entries[i].Exit(CancellationToken.None);
            }

            try
            {
                await UniTask.WhenAll(exits);
            }
            finally
            {
                _stack.Clear();
            }
        }

        private RouteEntry ResolveRoute(TKey key)
        {
            if (!_routes.TryGetValue(key, out RouteEntry entry))
            {
                throw new InvalidOperationException($"Route '{key}' is not registered.");
            }

            return entry;
        }

        private int IndexOf(TKey key)
        {
            for (int i = 0; i < _stack.Count; i++)
            {
                if (_comparer.Equals(_stack[i].Key, key))
                {
                    return i;
                }
            }

            return -1;
        }

        private void UpdateObservables()
        {
            _current.Value = _stack.Count > 0 ? _stack[^1].Key : default;
            TKey[] snapshot = new TKey[_stack.Count];
            for (int i = 0; i < _stack.Count; i++)
            {
                snapshot[i] = _stack[i].Key;
            }

            _stackKeys.Value = snapshot;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Router<TKey>));
            }
        }

        private sealed class RouteEntry
        {
            private readonly EventCallback<FocusInEvent> _focusInHandler;
            private VisualElement? _lastFocused;
            private bool _attached;

            public RouteEntry(TKey key, RouteSpec spec)
            {
                Key = key;
                Spec = spec;
                _focusInHandler = OnFocusIn;
            }

            public TKey Key { get; }
            public RouteSpec Spec { get; }

            public void Attach()
            {
                if (_attached || Spec.Element == null)
                {
                    return;
                }

                Spec.Element.RegisterCallback(_focusInHandler);
                _attached = true;
                SetInteractive(false);
            }

            public void Detach()
            {
                if (!_attached || Spec.Element == null)
                {
                    return;
                }

                Spec.Element.UnregisterCallback(_focusInHandler);
                _attached = false;
            }

            public UniTask Enter(CancellationToken ct)
            {
                return RunHookAndTransition(Spec.OnEnter, Spec.Transition.Enter, ct);
            }

            public UniTask Exit(CancellationToken ct)
            {
                return RunHookAndTransition(Spec.OnExit, Spec.Transition.Exit, ct);
            }

            public UniTask Cover(CancellationToken ct)
            {
                return RunHookAndTransition(Spec.OnCover, Spec.Transition.Cover, ct);
            }

            public UniTask Reveal(CancellationToken ct)
            {
                return RunHookAndTransition(Spec.OnReveal, Spec.Transition.Reveal, ct);
            }

            public void SetInteractive(bool interactive)
            {
                VisualElement? element = Spec.Element;
                if (element == null)
                {
                    return;
                }

                element.SetEnabled(interactive);
                element.pickingMode = interactive ? PickingMode.Position : PickingMode.Ignore;

                if (!interactive)
                {
                    BlurFocusedDescendant(element);
                }
            }

            public void RestoreFocus()
            {
                VisualElement? element = Spec.Element;
                if (element == null)
                {
                    Spec.OnRestoreFocus?.Invoke();
                    return;
                }

                if (element.panel == null)
                {
                    RestoreFocusInternal();
                    return;
                }

                element.schedule.Execute(RestoreFocusInternal);
            }

            private void RestoreFocusInternal()
            {
                VisualElement? element = Spec.Element;
                if (element == null)
                {
                    Spec.OnRestoreFocus?.Invoke();
                    return;
                }

                if (_lastFocused?.panel != null && _lastFocused.canGrabFocus)
                {
                    _lastFocused.Focus();
                }
                else if (element.canGrabFocus)
                {
                    element.Focus();
                }

                Spec.OnRestoreFocus?.Invoke();
            }

            private void OnFocusIn(FocusInEvent evt)
            {
                if (evt.target is VisualElement child && child != Spec.Element)
                {
                    _lastFocused = child;
                }
            }

            private static void BlurFocusedDescendant(VisualElement element)
            {
                if (element.focusController?.focusedElement is VisualElement focused && element.Contains(focused))
                {
                    focused.Blur();
                }
            }

            private async UniTask RunHookAndTransition(
                Func<CancellationToken, UniTask>? hook,
                Func<VisualElement, CancellationToken, UniTask> transition,
                CancellationToken ct)
            {
                UniTask hookTask = hook != null ? hook(ct) : UniTask.CompletedTask;
                VisualElement? element = Spec.Element;
                if (element == null)
                {
                    await hookTask;
                    return;
                }

                await UniTask.WhenAll(hookTask, transition(element, ct));
            }
        }
    }
}
