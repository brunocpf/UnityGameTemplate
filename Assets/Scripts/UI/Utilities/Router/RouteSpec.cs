using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;

namespace __GAME_NAMESPACE__.UI.Utilities
{
    /// <summary>
    /// Registration data for a single <see cref="Router{TKey}"/> route.
    /// Use object-initializer syntax: <c>new RouteSpec { Element = ..., Transition = ... }</c>.
    /// </summary>
    public sealed class RouteSpec
    {
        /// <summary>
        /// Visual root for this route. When set, the router auto-manages enabled state, picking,
        /// MenuButton activation, and last-focused descendant restoration.
        /// </summary>
        public VisualElement? Element { get; init; }

        /// <summary>
        /// Visual transition applied to <see cref="Element"/> on enter/exit/cover/reveal.
        /// Defaults to <see cref="RouteTransitions.None"/>.
        /// </summary>
        public IRouteTransition Transition { get; init; } = RouteTransitions.None;

        public Func<CancellationToken, UniTask>? OnEnter { get; init; }
        public Func<CancellationToken, UniTask>? OnExit { get; init; }
        public Func<CancellationToken, UniTask>? OnCover { get; init; }
        public Func<CancellationToken, UniTask>? OnReveal { get; init; }

        /// <summary>
        /// Invoked after the router has restored focus on this route. Use this for routes without
        /// an <see cref="Element"/> to perform custom focus delegation.
        /// </summary>
        public Action? OnRestoreFocus { get; init; }
    }
}
