using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;
using Wobble.Screens;
using Wobble.Tests.Assets;
using Wobble.Tests.Screens.Tests.DrawingSprites;
using Wobble.Tests.Screens.Tests.EasingAnimations;

namespace Wobble.Tests.Screens.Selection
{
    public class SelectionScreenView : ScreenView
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public SelectionScreenView(Screen screen) : base(screen) => CreateSelectionButtons();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.OliveDrab);
            Container?.Draw(gameTime);

            try
            {
                GameBase.Game.SpriteBatch.End();
            }
            catch (Exception)
            {
                // ignored.
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();

        /// <summary>
        ///     Creates the selection buttons to navigate from test screens.
        /// </summary>
        private void CreateSelectionButtons()
        {
            var screen = (SelectionScreen) Screen;

            var i = 0;
            foreach (var testScreens in screen.TestCasesScreens)
            {
                // Create a generic text button.
                var button = new TextButton(WobbleAssets.WhiteBox, Fonts.AllerRegular16, testScreens.Value)
                {
                    Parent = Container,
                    Size = new ScalableVector2(150, 50),
                    Text =
                    {
                        TextColor = Color.Black,
                        TextScale = 0.75f
                    },
                    X = 5,
                    Y = i * 50 + i * 10 + 10
                };

                switch (testScreens.Key)
                {
                    case ScreenType.DrawingSprites:
                        button.Clicked += (o, e) => ScreenManager.ChangeScreen(new TestDrawingSpritesScreen());
                        break;
                    case ScreenType.EasingAnimations:
                        button.Clicked += (o, e) => ScreenManager.ChangeScreen(new TestEasingAnimationsScreen());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                i++;
            }
        }
    }
}
