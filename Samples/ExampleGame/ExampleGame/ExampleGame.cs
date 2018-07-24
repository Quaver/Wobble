using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Wobble;
using Wobble.Audio;
using Wobble.Audio.Samples;
using Wobble.Audio.Tracks;
using Wobble.Discord;
using Wobble.Discord.RPC;
using Wobble.Discord.RPC.Logging;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Input;
using Wobble.Resources;
using Wobble.Screens;
using Wobble.Window;

namespace ExampleGame
{
    public class ExampleGame : WobbleGame
    {
        public Texture2D Spongebob;

        public AudioTrack Song;

        public List<Texture2D> TestSpritesheet;
        
        public ExampleGame()
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

            MouseManager.ShowCursor();
            Window.AllowUserResizing = true;

            /*
                DiscordManager.CreateClient("376180410490552320", LogLevel.Info);
                DiscordManager.Client.SetPresence(new RichPresence()
                {
                    Details = "Test"
                });
            */
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            Spongebob = ResourceLoader.LoadTexture2D(ResourceStore.spongebob, ImageFormat.Png);

            var line = ResourceLoader.LoadTexture2D(ResourceStore.test_spritesheet, ImageFormat.Png);
            TestSpritesheet = ResourceLoader.LoadSpritesheetFromTexture(line, 1, 12);

            Song = new AudioTrack(ResourceStore.Valence___Infinite)
            {
                Rate = 1.5f,
                Volume = 5
            };

            Song.Play();
            ScreenManager.AddScreen(new SampleScreen());
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
            Spongebob.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (MouseManager.IsUniqueClick(MouseButton.Left))
                WindowManager.ChangeScreenResolution(new Point(1600, 900));
            else if (MouseManager.IsUniqueClick(MouseButton.Right))
                WindowManager.ChangeScreenResolution(new Point(1920, 1080));
            else if (MouseManager.IsUniqueClick(MouseButton.Middle))
                WindowManager.ChangeScreenResolution(new Point(800, 600));

            //Person.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            //Person.Draw(gameTime);
        }
    }
}