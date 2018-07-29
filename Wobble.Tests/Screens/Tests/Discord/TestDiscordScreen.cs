using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wobble.Discord;
using Wobble.Discord.RPC;
using Wobble.Discord.RPC.Logging;
using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.Discord
{
    public class TestDiscordScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        /// </summary>
        public TestDiscordScreen()
        {
            DiscordManager.CreateClient("473243746922659842", LogLevel.None);
            DiscordManager.Client.SetPresence(new RichPresence()
            {
                Details = "TestDiscordScreen",
                State = "Testing"
            });

            View = new TestDiscordScreenView(this);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            DiscordManager.Dispose();
            base.Destroy();
        }
    }
}
