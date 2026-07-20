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
        protected ImGuiRenderer Renderer { get; }

        /// <summary>
        /// </summary>
        protected ImGuiOptions Options { get; }

        /// <summary>
        ///     Whether this ImGui context received the mouse press that started on the current frame.
        /// </summary>
        public bool WasActivatedByMouse => Renderer.WasActivatedByMouse;

        /// <summary>
        /// </summary>
        protected SpriteImGui(bool destroyContext = true, ImGuiOptions options = null, float scale = 1.0f)
        {
            Options = options;

            Renderer = new ImGuiRenderer(destroyContext, options, scale);
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
        public virtual void Draw(GameTime gameTime)
        {
            if (ImGui.GetCurrentContext() != Renderer.Context)
                ImGui.SetCurrentContext(Renderer.Context);

            try
            {
                Renderer.BeforeLayout(gameTime);
                RenderImguiLayout();
                Renderer.AfterLayout();
#if DEBUG
                global::Wobble.Graphics.UI.Debugging.PerformanceStats.RecordImGuiDrawData(Renderer.LastVertexCount, Renderer.LastIndexCount);
#endif
            }
            catch (Exception e)
            {
                // ignored
            }
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
