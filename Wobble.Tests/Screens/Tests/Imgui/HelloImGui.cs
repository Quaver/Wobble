using ImGuiNET;
using Wobble.Graphics.ImGUI;
using Wobble.Logging;

namespace Wobble.Tests.Screens.Tests.Imgui
{
    public class HelloImGui : SpriteImGui
    {
        protected override void RenderImguiLayout()
        {
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
    }
}