using UnityEngine.UIElements;

namespace __GAME_NAMESPACE__.UI.Utilities
{
    [UxmlElement]
    public abstract partial class MenuPanel : VisualElement
    {
        protected MenuPanel()
        {
            focusable = true;
            delegatesFocus = true;
            pickingMode = PickingMode.Position;
        }
    }
}
