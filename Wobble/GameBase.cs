using System;

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
    }
}