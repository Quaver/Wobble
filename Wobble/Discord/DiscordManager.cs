using System;
using DiscordRPC;
using DiscordRPC.Logging;

namespace Wobble.Discord
{
    public static class DiscordManager
    {
        /// <summary>
        ///     The Discord RPC client.
        /// </summary>
        public static DiscordRpcClient Client { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public static string AppId { get; private set; }

        /// <summary>
        ///     Creates a new Discord RPC client to use throughout the game.
        ///     For more documentation on how to actually use the client: https://github.com/Lachee/discord-rpc-csharp
        /// </summary>
        /// <param name="appId">The application id, received from discordapp.com/developers</param>
        /// <param name="logLevel">The level of logging for the client.</param>
        public static void CreateClient(string appId, LogLevel logLevel = LogLevel.None)
        {
            if (Client != null && !Client.Disposed)
                throw new InvalidOperationException("DiscordRpcClient already is initialized and hasn't been disposed.");

            AppId = appId;

            Client = new DiscordRpcClient(AppId, true, -1)
            {
                Logger = new ConsoleLogger { Level = logLevel }
            };

            Client.Initialize();
        }

        /// <summary>
        ///     Disposes of the current client.
        /// </summary>
        public static void Dispose() => Client?.Dispose();
    }
}