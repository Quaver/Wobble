using System;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace Wobble.Graphics.ImGUI
{
    public abstract class SpriteImGui : IDrawable
    {
        /// <summary>
        ///     Used to render the imgui context to the screen.
        /// </summary>
        private ImGuiRenderer Renderer { get; }

        /// <summary>
        /// </summary>
        protected SpriteImGui()
        {
            Renderer = new ImGuiRenderer();
            Renderer.RebuildFontAtlas();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public void Draw(GameTime gameTime)
        {
            if (ImGui.GetCurrentContext() != Renderer.Context)
                ImGui.SetCurrentContext(Renderer.Context);

            Renderer.BeforeLayout(gameTime);
            RenderImguiLayout();
            Renderer.AfterLayout();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public virtual void Destroy() => Renderer.Dispose();

        /// <summary>
        ///     Renders the imgui layout
        /// </summary>
        protected abstract void RenderImguiLayout();
    }
}