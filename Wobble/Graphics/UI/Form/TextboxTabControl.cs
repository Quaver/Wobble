using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Wobble.Input;

namespace Wobble.Graphics.UI.Form
{
    public class TextboxTabControl : Container
    {
        /// <summary>
        ///     The textboxes in the tab control
        /// </summary>
        public List<Textbox> Textboxes { get; }

        /// <summary>
        /// </summary>
        /// <param name="textboxes"></param>
        public TextboxTabControl(List<Textbox> textboxes) => Textboxes = textboxes;

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleKeyPressTab();
            base.Update(gameTime);
        }

        /// <summary>
        ///     Adds a textbox to the control
        /// </summary>
        /// <param name="t"></param>
        public void AddTextbox(Textbox t) => Textboxes.Add(t);

        /// <summary>
        ///     Returns if tab input is allowed for the tab control
        /// </summary>
        /// <returns></returns>
        public virtual bool IsInputAllowed() => true;

        /// <summary>
        /// </summary>
        private void HandleKeyPressTab()
        {
            if (!IsInputAllowed())
                return;

            if (!KeyboardManager.IsUniqueKeyPress(Keys.Tab))
                return;

            var selectedTextbox = Textboxes.FindIndex(x => x.Focused);

            // Going backwards in the tab control while holding shift
            if (KeyboardManager.CurrentState.IsKeyDown(Keys.LeftShift) || KeyboardManager.CurrentState.IsKeyDown(Keys.RightShift))
            {
                if (selectedTextbox == 0 || selectedTextbox - 1 < 0)
                    return;

                Textboxes[selectedTextbox].Focused = false;
                Textboxes[selectedTextbox - 1].Focused = true;

                return;
            }

            // Going forwards in the tab control
            if (selectedTextbox == Textboxes.Count - 1 || selectedTextbox + 1 == Textboxes.Count)
                return;

            Textboxes[selectedTextbox].Focused = false;
            Textboxes[selectedTextbox + 1].Focused = true;
        }
    }
}