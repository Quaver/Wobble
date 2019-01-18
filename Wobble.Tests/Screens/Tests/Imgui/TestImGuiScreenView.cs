using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.ImGUI;
using Wobble.Graphics.UI.Buttons;
using Wobble.Logging;
using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.Imgui
{
    public class TestImGuiScreenView : ScreenView
    {
        private HelloImGui HelloImGui { get; set; }

        public TestImGuiScreenView(Screen screen) : base(screen)
        {
            HelloImGui = new HelloImGui();

            // Make a button
            // ReSharper disable once ObjectCreationAsStatement
            new ImageButton(WobbleAssets.WhiteBox, (sender, args) => Logger.Important("CLICKED", LogType.Runtime, false))
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(100, 100),
                Tint = Color.Crimson
            };
        }

        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.CornflowerBlue);
            Container?.Draw(gameTime);
            HelloImGui.Draw(gameTime);
        }

        public override void Destroy() => Container?.Destroy();
    }
}