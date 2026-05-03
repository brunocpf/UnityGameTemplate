using __GAME_NAMESPACE__.Core.Sample;
using __GAME_NAMESPACE__.UI.Utilities;
using UnityEngine.UIElements;

namespace __GAME_NAMESPACE__.UI.Sample
{
    /// <summary>
    /// Sample VisualElement that wires a Core domain class to a UXML/USS markup pair
    /// loaded via <see cref="UiMarkupResources"/>. Replace with your own panels.
    /// </summary>
    [UxmlElement]
    public sealed partial class SamplePanel : VisualElement
    {
        private const string MARKUP_PATH = "UI/Sample/SampleMarkup";
        private const string ROOT_CLASS = "sample-panel";

        public SamplePanel()
        {
            UiMarkupResources.CloneInto(this, MARKUP_PATH, ROOT_CLASS);

            Greeter greeter = new();
            Label greeting = this.Q<Label>("greeting");
            if (greeting != null)
            {
                greeting.text = greeter.Greet("World");
            }
        }
    }
}
