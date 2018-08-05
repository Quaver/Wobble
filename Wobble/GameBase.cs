using System;
using Wobble.Graphics;

namespace Wobble
{
    /// <summary>
    ///     Contains all global objects needed for references throughout the game's life.
    /// </summary>
    public static class GameBase
    {
        /// <summary>
        ///     Contains a reference to the game itself. It is a singleton, so there can only
        ///     ever be one game in existence.
        /// </summary>
        private static WobbleGame _game;
        public static WobbleGame Game
        {
            get => _game;
            internal set
            {
                if (_game != null)
                    throw new InvalidOperationException("There can only ever be one game in existence.");

                _game = value;
            }
        }

         /// <summary>
        ///    The default SpriteBatch options that'd be used on every drawable that doesn't
        ///    have it overwritten.
        /// </summary>
        public static SpriteBatchOptions DefaultSpriteBatchOptions { get; set;  } = new SpriteBatchOptions();

        /// <summary>
        ///     Dictates if the default SpriteBatch is currently in use.
        /// </summary>
        public static bool DefaultSpriteBatchInUse { get; internal set; }
    }
}