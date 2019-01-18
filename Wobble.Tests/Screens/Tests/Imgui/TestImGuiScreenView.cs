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
        private ImGuiRenderer ImGuiRenderer { get; }

        public TestImGuiScreenView(Screen screen) : base(screen)
        {
            ImGuiRenderer = new ImGuiRenderer();
            ImGuiRenderer.RebuildFontAtlas();

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

            // Call BeforeLayout first to set things up
            ImGuiRenderer.BeforeLayout(gameTime);

            // Draw our UI
            ImGuiLayout();

            // Call AfterLayout now to finish up and draw all the things
            ImGuiRenderer.AfterLayout();
        }

        protected virtual void ImGuiLayout()
        {
            // 1. Show a simple window
            // Tip: if we don't call ImGui.Begin()/ImGui.End() the widgets appears in a window automatically called "Debug"
            {
                ImGui.Begin("Wobble ImGUI Test");
                ImGui.Text("Hello, world!");
                ImGui.Text(string.Format("Application average {0:F3} ms/frame ({1:F1} FPS)", 1000f / ImGui.GetIO().Framerate, ImGui.GetIO().Framerate));

                if (ImGui.Button("Click Me"))
                {
                    Logger.Debug("CLICKEd", LogType.Runtime);
                }

                ImGui.End();
            }
        }

        public override void Destroy() => Container?.Destroy();
    }
}