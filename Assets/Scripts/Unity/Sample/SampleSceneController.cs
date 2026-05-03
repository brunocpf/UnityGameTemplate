using __GAME_NAMESPACE__.UI.Sample;
using UnityEngine;
using UnityEngine.UIElements;

namespace __GAME_NAMESPACE__.Unity.Sample
{
    /// <summary>
    /// Wires the <see cref="SamplePanel"/> into a scene's <see cref="UIDocument"/>.
    /// Replace this with your own scene controllers as the project grows.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class SampleSceneController : MonoBehaviour
    {
        private SamplePanel? _panel;

        private void OnEnable()
        {
            UIDocument document = GetComponent<UIDocument>();
            VisualElement root = document.rootVisualElement;

            _panel = new SamplePanel();
            root.Add(_panel);
        }

        private void OnDisable()
        {
            _panel?.RemoveFromHierarchy();
            _panel = null;
        }
    }
}
