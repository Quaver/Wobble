using System;
using System.Threading;
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
        ///     Whether this ImGui context has a hovered window.
        /// </summary>
        public bool IsMouseHovered => Renderer.IsMouseHovered;

        /// <summary>
        ///     Whether this ImGui context currently owns mouse input.
        /// </summary>
        public bool IsMouseInputOwner => Renderer.IsMouseInputOwner;

        /// <summary>
        ///     Whether this ImGui context currently owns keyboard input.
        /// </summary>
        public bool IsKeyboardInputOwner => Renderer.IsKeyboardInputOwner;

        /// <summary>
        ///     Whether ImGui requested mouse input for this context.
        /// </summary>
        public bool WantsMouseInput => Renderer.WantsMouseInput;

        /// <summary>
        ///     Whether ImGui requested keyboard input for this context.
        /// </summary>
        public bool WantsKeyboardInput => Renderer.WantsKeyboardInput;

        /// <summary>
        ///     Whether ImGui requested text input for this context.
        /// </summary>
        public bool WantsTextInput => Renderer.WantsTextInput;

        /// <summary>
        ///     Context/font atlas creation touches the GraphicsDevice, so it must happen on the
        ///     main thread - hop onto it and block if we're not already there.
        /// </summary>
        protected SpriteImGui(bool destroyContext = true, ImGuiOptions options = null, float scale = 1.0f)
        {
            Options = options;

            if (Thread.CurrentThread.ManagedThreadId == GameBase.Game.MainThreadId)
            {
                Renderer = new ImGuiRenderer(destroyContext, options, scale);
                Renderer.RebuildFontAtlas();
            }
            else
            {
                // Renderer is get-only, so assign it here rather than inside the lambda below.
                ImGuiRenderer renderer = null;
                using var completed = new ManualResetEventSlim(false);

                GameBase.Game.ScheduleRenderTargetDraw(() =>
                {
                    try
                    {
                        renderer = new ImGuiRenderer(destroyContext, options, scale);
                        renderer.RebuildFontAtlas();
                    }
                    finally
                    {
                        completed.Set();
                    }
                });

                completed.Wait();
                Renderer = renderer;
            }
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
