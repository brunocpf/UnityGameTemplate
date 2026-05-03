using __GAME_NAMESPACE__.Core.Sample;
using NUnit.Framework;

namespace __GAME_NAMESPACE__.Core.Sample.Tests
{
    public sealed class GreeterTests
    {
        [Test]
        public void Greet_returns_hello_with_name()
        {
            Greeter sut = new();

            string result = sut.Greet("World");

            Assert.AreEqual("Hello, World!", result);
        }
    }
}
