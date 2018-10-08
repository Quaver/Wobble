using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Graphics;

namespace Wobble.Logging
{
    public static class LogManager
    {
        /// <summary>
        ///     The container that holds all logs.
        /// </summary>
        private static Container Container { get; set; }

        /// <summary>
        ///     Initializes the logger.
        /// </summary>
        internal static void Initialize() => Container = new Container()
        {
            SpriteBatchOptions = new SpriteBatchOptions()
            {
                BlendState = BlendState.NonPremultiplied
            }
        };

        /// <summary>
        ///     Add a log to the manager.
        /// </summary>
        /// <param name="m"></param>
        internal static void AddLog(string m, LogLevel level)
        {
            var log = new DrawableLog(m, level)
            {
                Parent = Container,
                Alignment = Alignment.TopLeft
            };

            log.Y = GetLogY(Container.Children.Count - 1);
        }

        /// <summary>
        ///     Updates the logs.
        /// </summary>
        /// <param name="gameTime"></param>
        internal static void Update(GameTime gameTime)
        {
            for (var i = 0; i < Container.Children.Count; i++)
            {
                Container.Children[i].Y = MathHelper.Lerp(Container.Children[i].Y, GetLogY(i),
                    (float) Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / 10, 1));
            }

           Container?.Update(gameTime);
        }

        /// <summary>
        ///     Draws the logs.
        /// </summary>
        /// <param name="gameTime"></param>
        internal static void Draw(GameTime gameTime) => Container?.Draw(gameTime);

        private static float GetLogY(int i)
        {
            if (i == 0)
                return 0;

            return Container.Children[i - 1].Y + Container.Children[i - 1].Height + 2;
        }
    }
}
