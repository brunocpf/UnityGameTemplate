using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace __GAME_NAMESPACE__.UI.Utilities
{
    /// <summary>
    /// Loads and caches UI Toolkit markup assets from Resources paths.
    /// </summary>
    /// <remarks>
    /// Use this helper from custom <see cref="VisualElement"/> constructors to keep
    /// markup loading consistent across UI assemblies.
    /// </remarks>
    public static class UiMarkupResources
    {
        private static readonly Dictionary<string, VisualTreeAsset> _cache = new();

        /// <summary>
        /// Clones the markup asset at the provided Resources path into the target element.
        /// </summary>
        /// <param name="target">Target visual element that receives the cloned tree.</param>
        /// <param name="markupPath">Resources path without extension (for example, <c>UI/Battle/InventoryPanel/InventoryPanelMarkup</c>).</param>
        /// <param name="rootClass">Optional root class to add to the target element after cloning.</param>
        public static void CloneInto(VisualElement target, string markupPath, string? rootClass = null)
        {
            VisualTreeAsset asset = Load(markupPath);

            target.hierarchy.Clear();
            asset.CloneTree(target);

            if (!string.IsNullOrWhiteSpace(rootClass))
            {
                target.AddToClassList(rootClass);
            }
        }

        private static VisualTreeAsset Load(string markupPath)
        {
            if (string.IsNullOrWhiteSpace(markupPath))
            {
                throw new ArgumentException("markupPath cannot be null, empty, or whitespace.", nameof(markupPath));
            }

            if (_cache.TryGetValue(markupPath, out VisualTreeAsset markup))
            {
                return markup;
            }

            markup = Resources.Load<VisualTreeAsset>(markupPath);

            if (markup == null)
            {

                throw new InvalidOperationException(
                    $"Failed to load VisualTreeAsset at Resources path '{markupPath}'.");
            }

            _cache[markupPath] = markup;
            return markup;
        }
    }
}
