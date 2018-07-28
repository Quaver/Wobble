using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Graphics.UI.Form;
using Wobble.Screens;
using Wobble.Tests.Assets;
using Wobble.Window;

namespace Wobble.Tests.Screens.Tests.EasingAnimations
{
    public class TestEasingAnimationsScreenView : ScreenView
    {
        /// <summary>
        ///     The box that will be used for the easing functions.
        /// </summary>
        public Sprite GreenBox { get; }

        /// <summary>
        ///     The type of easing function the green box will use.
        /// </summary>
        public HorizontalSelector EasingSelection { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TestEasingAnimationsScreenView(Screen screen) : base(screen)
        {
            GreenBox = new Sprite()
            {
                Parent = Container,
                Size = new ScalableVector2(75, 75),
                Alignment = Alignment.MidLeft,
                Tint = Color.LimeGreen,
                Transformations = { new Transformation(TransformationProperty.X, Easing.EaseInQuad, 0, WindowManager.VirtualScreen.X, 3000)}
            };

            // Create the horizontal selector
            EasingSelection = new HorizontalSelector(Enum.GetNames(typeof(Easing)).ToList(), new ScalableVector2(300, 45),
                                    Fonts.AllerRegular16, 0.95f, Textures.LeftButtonSquare, Textures.RightButtonSquare, new ScalableVector2(45, 45), 10,
            // Create the method that'll be called when a new option is selected.
            (val, index) =>
            {
                // Clear all existing transformations
                GreenBox.Transformations.Clear();

                // Reset the position of the box.
                GreenBox.X = 0;

                // Add a new transformation with the new easing type.
                var newEaseType = (Easing)Enum.Parse(typeof(Easing), val);
                GreenBox.Transformations.Add(new Transformation(TransformationProperty.X, newEaseType, GreenBox.X, WindowManager.VirtualScreen.X, 3000));
            })
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Y = GreenBox.Height + 40,
                Tint = Color.Red,
                Alpha = 0.35f,
            };

            EasingSelection.ButtonSelectLeft.Tint = Color.Red;
            EasingSelection.ButtonSelectRight.Tint = Color.Red;
            EasingSelection.SelectedItemText.TextColor = Color.White;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // If there aren't any transformations left, then we'll
            // want to loop it in this case.
            if (GreenBox.Transformations.Count == 0)
            {
                GreenBox.X = 0;

                // Add a new transformation with the new easing type.
                var newEaseType = (Easing)Enum.Parse(typeof(Easing), EasingSelection.Options[EasingSelection.SelectedIndex]);
                GreenBox.Transformations.Add(new Transformation(TransformationProperty.X, newEaseType, GreenBox.X, WindowManager.VirtualScreen.X, 3000));
            }

            Container?.Update(gameTime);
        } 

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.Black);
            Container?.Draw(gameTime);

            try
            {
                GameBase.Game.SpriteBatch.End();
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();
    }
}
