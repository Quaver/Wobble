using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Wobble.Extended.HotReload.Screens.UI
{
    public class CompilingNotification : Sprite
    {
        /// <summary>
        /// </summary>
        public SpriteText Text { get; }

        /// <summary>
        /// </summary>
        public bool CompilationFailed { get; private set; }

        /// <summary>
        /// </summary>
        public CompilingNotification()
        {
            Tint = new Color(24, 24, 24);
            Size = new ScalableVector2(WindowManager.Width, 75);
            SetChildrenAlpha = true;

            Text = new SpriteText("Arial", "", 14)
            {
                Parent = this,
                Alignment = Alignment.MidCenter
            };

            SetCompilingText();
        }

        public void SetCompilingText()
        {
            Text.Text = $"Please wait while the project gets re-compiled...";
            Text.Tint = Color.White;
            CompilationFailed = false;
        }

        public void SetCompilationFailedText()
        {
            Text.Text = "Failed to compile project. Please see the logs!";
            Text.Tint = Color.Red;
            CompilationFailed = true;
        }
    }
}