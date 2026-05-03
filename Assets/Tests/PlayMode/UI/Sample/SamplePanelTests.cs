using System.Collections;
using __GAME_NAMESPACE__.UI.Sample;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace __GAME_NAMESPACE__.UI.Sample.PlayModeTests
{
    public sealed class SamplePanelTests
    {
        [UnityTest]
        public IEnumerator SamplePanel_renders_greeting_label()
        {
            SamplePanel panel = new();
            yield return null;

            Label greeting = panel.Q<Label>("greeting");

            Assert.IsNotNull(greeting, "Expected a Label named 'greeting' inside SamplePanel.");
            Assert.AreEqual("Hello, World!", greeting.text);
        }
    }
}
