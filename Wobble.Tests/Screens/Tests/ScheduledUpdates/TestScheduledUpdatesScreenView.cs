using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;
using Wobble.Managers;
using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.ScheduledUpdates
{
    public class TestScheduledUpdatesScreenView: ScreenView
    {
        private SpriteTextPlus Scheduled { get; }

        /// <summary>
        /// </summary>
        private bool IsScheduled { get; set; } = true;

        public TestScheduledUpdatesScreenView(Screen screen) : base(screen)
        {
            Scheduled = new SpriteTextPlus(FontManager.GetWobbleFont("exo2-semibold"),
                "", 36)
            {
                Alignment = Alignment.TopCenter,
                Y = 250
            };

            Task.Run(() =>
            {
                while (!Scheduled.IsDisposed)
                {
                    if (IsScheduled)
                        Scheduled.ScheduleUpdate(() => Scheduled.Text = $"Scheduled - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
                    else
                        Scheduled.Text = $"Unscheduled - {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
                }
            });

            new SpriteText("exo2-semibold", "Press 1 to toggle between scheduled & unscheduled", 32)
            {
                Parent = Container,
                Alignment = Alignment.BotCenter
            };
        }

        public override void Update(GameTime gameTime)
        {
            Container?.Update(gameTime);
            Scheduled.Update(gameTime);

            if (KeyboardManager.IsUniqueKeyPress(Keys.D1))
                IsScheduled = !IsScheduled;
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.CornflowerBlue);
            Container?.Draw(gameTime);

            Scheduled.Draw(gameTime);
        }

        public override void Destroy() => Container?.Destroy();
    }
}