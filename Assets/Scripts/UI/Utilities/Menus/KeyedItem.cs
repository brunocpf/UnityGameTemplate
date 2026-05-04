using R3;

namespace __GAME_NAMESPACE__.UI.Utilities
{
    /// <summary>
    /// Stable identity + per-key state stream for use with <see cref="KeyedListPanel{TState,TItem}"/>.
    /// The producer (a viewmodel) owns the inner <see cref="Observable{T}"/>; subscribers reuse the
    /// same instance across list emissions so per-row updates flow without rebuilding rows.
    /// </summary>
    public sealed record KeyedItem<TState>(string Key, Observable<TState> State);
}
