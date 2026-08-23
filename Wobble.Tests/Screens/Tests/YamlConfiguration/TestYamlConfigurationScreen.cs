using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.YamlConfiguration
{
    public sealed class TestYamlConfigurationScreen : Screen
    {
        public sealed override ScreenView View { get; protected set; }

        public TestYamlConfigurationScreen() => View = new TestYamlConfigurationScreenView(this);
    }
}
