using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Wobble.Graphics.ImGUI;
using Wobble.Graphics.UI.Buttons;
using Wobble.Logging;

namespace Wobble.Tests.Screens.Tests.Imgui
{
    public class TestImGuiMenu : SpriteImGui
    {
        /// <summary>
        /// </summary>
        public bool Opened;

        /// <summary>
        /// </summary>
        public bool Rotation;

        /// <summary>
        /// </summary>
        public bool Lightshow;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (Opened)
            {
                Logger.Debug($"It was opened", LogType.Runtime, false);
                Opened = false;
            }

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void RenderImguiLayout()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    ImGui.MenuItem("Open", null, ref Opened);
                    ImGui.Separator();
                    ImGui.MenuItem("Save");
                    ImGui.MenuItem("Exit");
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Effects"))
                {
                    ImGui.MenuItem("Rotation", null, ref Rotation);
                    ImGui.MenuItem("Lightshow", null, ref Lightshow);
                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
                Button.IsGloballyClickable = !ImGui.IsAnyItemHovered();
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            Button.IsGloballyClickable = true;
        }
    }
}