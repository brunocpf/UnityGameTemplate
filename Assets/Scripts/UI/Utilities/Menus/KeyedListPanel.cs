using System.Collections.Generic;
using R3;
using UnityEngine.UIElements;

namespace __GAME_NAMESPACE__.UI.Utilities
{
    /// <summary>
    /// Base panel for rendering a stable, keyed list of items where the upstream owns a per-key
    /// <see cref="Observable{T}"/> for each item's state. Reconciles the visual tree on each list
    /// emission: existing keys keep their items, new keys produce new items, missing keys are removed.
    /// Per-row state updates flow through the inner streams without rebuilding rows.
    /// </summary>
    public abstract class KeyedListPanel<TState, TItem> : MenuPanel where TItem : VisualElement
    {
        private readonly Dictionary<string, TItem> _itemsByKey = new();
        private readonly List<string> _keysInOrder = new();
        private readonly List<TItem> _itemsInOrder = new();

        private CompositeDisposable _subscriptions = new();
        private VisualElement? _container;

        public IReadOnlyList<TItem> Items => _itemsInOrder;

        /// <summary>Designate the visual element that will host the list items.</summary>
        protected void SetContainer(VisualElement container)
        {
            _container = container;
        }

        /// <summary>Subscribe to the upstream list. Disposes any prior subscription and clears existing items.</summary>
        protected void BindList(Observable<IReadOnlyList<KeyedItem<TState>>> stream)
        {
            UnbindList();
            _subscriptions = new CompositeDisposable();
            stream.Subscribe(Render).AddTo(_subscriptions);
        }

        /// <summary>Tear down the subscription and remove all items from the container.</summary>
        protected void UnbindList()
        {
            DisposeSubscriptions();
            ClearAll();
        }

        /// <summary>
        /// Tear down only the subscription, leaving items in place. Useful from
        /// <see cref="DetachFromPanelEvent"/> handlers where the visual tree is going away anyway.
        /// </summary>
        protected void DisposeSubscriptions()
        {
            _subscriptions.Dispose();
            _subscriptions = new CompositeDisposable();
        }

        protected abstract TItem CreateItem(string key, Observable<TState> state);
        protected virtual void UpdateItemPosition(TItem item, int index, int total) { }
        protected virtual void OnItemRemoved(TItem item) { }

        private void Render(IReadOnlyList<KeyedItem<TState>> entries)
        {
            if (_container == null)
            {
                return;
            }

            HashSet<string> seen = new();
            for (int i = 0; i < entries.Count; i++)
            {
                KeyedItem<TState> entry = entries[i];
                seen.Add(entry.Key);

                if (!_itemsByKey.TryGetValue(entry.Key, out TItem item))
                {
                    item = CreateItem(entry.Key, entry.State);
                    _itemsByKey[entry.Key] = item;
                    _keysInOrder.Add(entry.Key);
                    _itemsInOrder.Add(item);
                    _container.Add(item);
                }

                UpdateItemPosition(item, i, entries.Count);
            }

            for (int i = _keysInOrder.Count - 1; i >= 0; i--)
            {
                string key = _keysInOrder[i];
                if (seen.Contains(key))
                {
                    continue;
                }

                TItem item = _itemsInOrder[i];
                _container.Remove(item);
                _keysInOrder.RemoveAt(i);
                _itemsInOrder.RemoveAt(i);
                _itemsByKey.Remove(key);
                OnItemRemoved(item);
            }
        }

        private void ClearAll()
        {
            if (_itemsInOrder.Count == 0)
            {
                return;
            }

            if (_container != null)
            {
                for (int i = 0; i < _itemsInOrder.Count; i++)
                {
                    _container.Remove(_itemsInOrder[i]);
                }
            }

            for (int i = 0; i < _itemsInOrder.Count; i++)
            {
                OnItemRemoved(_itemsInOrder[i]);
            }

            _itemsInOrder.Clear();
            _keysInOrder.Clear();
            _itemsByKey.Clear();
        }
    }
}
