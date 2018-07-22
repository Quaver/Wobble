using System;

namespace Wobble
{
    public static class GameBase
    {
        /// <summary>
        ///     Contains a reference to the game.
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