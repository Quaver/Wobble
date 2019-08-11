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
using Wobble.Tests.Screens.Tests.Audio;
using Wobble.Tests.Screens.Tests.Background;
using Wobble.Tests.Screens.Tests.BitmapFont;
using Wobble.Tests.Screens.Tests.BlurContainer;
using Wobble.Tests.Screens.Tests.BlurredBgImage;
using Wobble.Tests.Screens.Tests.Discord;
using Wobble.Tests.Screens.Tests.DrawingSprites;
using Wobble.Tests.Screens.Tests.EasingAnimations;
using Wobble.Tests.Screens.Tests.Imgui;
using Wobble.Tests.Screens.Tests.Primitives;
using Wobble.Tests.Screens.Tests.Scaling;
using Wobble.Tests.Screens.Tests.Scrolling;
using Wobble.Tests.Screens.Tests.SpriteMasking;
using Wobble.Tests.Screens.Tests.SpriteTextPlusNew;
using Wobble.Tests.Screens.Tests.TaskHandler;
using Wobble.Tests.Screens.Tests.TextInput;
using Wobble.Tests.Screens.Tests.TextSizes;
using Wobble.Window;

namespace Wobble.Tests.Screens.Selection
{
    public class SelectionScreenView : ScreenView
    {
        private static readonly ScalableVector2 ButtonSize = new ScalableVector2(150, 50);
        private static readonly float ButtonGap = 5;

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
            var buttonsInColumn = (int) ((WindowManager.VirtualScreen.Y - ButtonGap) / (ButtonSize.Y.Value + ButtonGap));

            var i = 0;
            foreach (var testScreens in screen.TestCasesScreens)
            {
                // Create a generic text button.
                var button = new TextButton(WobbleAssets.WhiteBox, "exo2-medium", testScreens.Value, 12)
                {
                    Parent = Container,
                    Size = ButtonSize,
                    Text =
                    {
                        Tint = Color.Black,
                    },
                    X = (i / buttonsInColumn) * (ButtonGap + ButtonSize.X.Value) + ButtonGap,
                    Y = (i % buttonsInColumn) * (ButtonGap + ButtonSize.Y.Value) + ButtonGap,
                };

                switch (testScreens.Key)
                {
                    case ScreenType.DrawingSprites:
                        button.Clicked += (o, e) => ScreenManager.ChangeScreen(new TestDrawingSpritesScreen());
                        break;
                    case ScreenType.EasingAnimations:
                        button.Clicked += (o, e) => ScreenManager.ChangeScreen(new TestEasingAnimationsScreen());
                        break;
                    case ScreenType.Audio:
                        button.Clicked += (o, e) => ScreenManager.ChangeScreen(new TestAudioScreen());
                        break;
                    case ScreenType.Discord:
                        button.Clicked += (o, e) => ScreenManager.ChangeScreen(new TestDiscordScreen());
                        break;
                    case ScreenType.Background:
                        button.Clicked += (o, e) => ScreenManager.ChangeScreen(new TestBackgroundImageScreen());
                        break;
                    case ScreenType.Scrolling:
                        button.Clicked += (o, e) => ScreenManager.ChangeScreen(new TestScrollContainerScreen());
                        break;
                    case ScreenType.BlurContainer:
                        button.Clicked += (o, e) => ScreenManager.ChangeScreen(new TestBlurContainerScreen());
                        break;
                    case ScreenType.BlurredBackgroundImage:
                        button.Clicked += (o, e) => ScreenManager.ChangeScreen(new TestBlurredBackgroundImageScreen());
                        break;
                    case ScreenType.TextInput:
                        button.Clicked += (o, e) => ScreenManager.ChangeScreen(new TestTextInputScreen());
                        break;
                    case ScreenType.SpriteMaskContainer:
                        button.Clicked += (o, e) => ScreenManager.ChangeScreen(new TestSpriteMaskingScreen());
                        break;
                    case ScreenType.BitmapFont:
                        button.Clicked += (o, e) => ScreenManager.ChangeScreen(new TestBitmapFontScreen());
                        break;
                    case ScreenType.Primitives:
                        button.Clicked += (o, e) => ScreenManager.ChangeScreen(new TestPrimitivesScreen());
                        break;
                    case ScreenType.ImGui:
                        button.Clicked += (o, e) => ScreenManager.ChangeScreen(new TestImGuiScreen());
                        break;
                    case ScreenType.Scaling:
                        button.Clicked += (o, e) => ScreenManager.ChangeScreen(new TestScalingScreen());
                        break;
                    case ScreenType.TextSizes:
                        button.Clicked += (o, e) => ScreenManager.ChangeScreen(new TestTextSizesScreen());
                        break;
                    case ScreenType.TaskHandler:
                        button.Clicked += (o, e) => ScreenManager.ChangeScreen(new TaskHandlerScreen());
                        break;
                    case ScreenType.SpriteTextPlus:
                        button.Clicked += (o, e) => ScreenManager.ChangeScreen(new TestSpriteTextPlusScreen());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                i++;
            }
        }
    }
}
