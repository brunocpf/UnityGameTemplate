using UnityEngine.UIElements;

namespace __GAME_NAMESPACE__.UI.Utilities
{
    /// <summary>
    /// Focuses a <see cref="Focusable"/> <see cref="VisualElement"/> when the pointer enters it.
    /// Apply via <c>element.AddManipulator(new HoverFocusManipulator())</c>.
    /// </summary>
    public sealed class HoverFocusManipulator : Manipulator
    {
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
        }

        private void OnPointerEnter(PointerEnterEvent evt)
        {
            if (target.canGrabFocus && target.focusController?.focusedElement != target)
            {
                target.Focus();
            }
        }
    }
}
