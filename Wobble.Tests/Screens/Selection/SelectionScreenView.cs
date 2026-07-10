using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;
using Wobble.Screens;
using Wobble.Tests.Assets;
using Wobble.Tests.Screens.Tests.Audio;
using Wobble.Tests.Screens.Tests.Background;
using Wobble.Tests.Screens.Tests.BlurContainer;
using Wobble.Tests.Screens.Tests.BlurredBgImage;
using Wobble.Tests.Screens.Tests.ButtonPerformance;
using Wobble.Tests.Screens.Tests.Discord;
using Wobble.Tests.Screens.Tests.DrawableScaling;
using Wobble.Tests.Screens.Tests.DrawingSprites;
using Wobble.Tests.Screens.Tests.EasingAnimations;
using Wobble.Tests.Screens.Tests.Joystick;
using Wobble.Tests.Screens.Tests.NavigationBars;
using Wobble.Tests.Screens.Tests.Imgui;
using Wobble.Tests.Screens.Tests.Primitives;
using Wobble.Tests.Screens.Tests.Rotation;
using Wobble.Tests.Screens.Tests.Scaling;
using Wobble.Tests.Screens.Tests.ScheduledUpdates;
using Wobble.Tests.Screens.Tests.Scrolling;
using Wobble.Tests.Screens.Tests.SpriteMasking;
using Wobble.Tests.Screens.Tests.SpriteAlphaMaskingBlend;
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
        private static readonly float ButtonStartY = 60;
        private static readonly CultureInfo[] LanguageCultures =
        {
            CultureInfo.GetCultureInfo("en"),
            CultureInfo.GetCultureInfo("bg")
        };
        private static readonly string[] LanguageNameKeys =
        {
            "Language_English",
            "Language_Bulgarian"
        };

        private SpriteTextPlus TitleText { get; set; }

        private SpriteTextPlus LanguageLabel { get; set; }

        private HorizontalSelector LanguageSelector { get; set; }

        private List<KeyValuePair<RoundedButton, string>> ScreenButtons { get; } = new List<KeyValuePair<RoundedButton, string>>();

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
            var screen = (SelectionScreen)Screen;
            var buttonsInColumn = (int)((WindowManager.VirtualScreen.Y - ButtonStartY - ButtonGap) / (ButtonSize.Y.Value + ButtonGap));

            TitleText = new SpriteTextPlus(FontManager.GetWobbleFont("inter-semibold"), LocalizationManager.Get("Selection_Title"), 26)
            {
                Parent = Container,
                X = ButtonGap,
                Y = 12
            };

            LanguageLabel = new SpriteTextPlus(FontManager.GetWobbleFont("inter-semibold"), LocalizationManager.Get("Selection_LanguageLabel"), 18)
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                X = -235,
                Y = 17
            };

            LanguageSelector = new HorizontalSelector(CreateLanguageOptions(),
                new ScalableVector2(130, 35), FontManager.GetWobbleFont("inter-semibold"), 18, Textures.LeftButtonSquare,
                Textures.RightButtonSquare, new ScalableVector2(30, 30), 5, (value, index) =>
                {
                    LocalizationManager.SetCurrentCulture(LanguageCultures[index]);
                    RefreshLocalizedText();
                }, useRoundedButtons: true)
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                X = -55,
                Y = 10
            };

            var i = 0;
            foreach (var testScreens in screen.TestCasesScreens)
            {
                var button = new RoundedButton
                {
                    Parent = Container,
                    Size = ButtonSize,
                    Tint = Color.White,
                    X = (i / buttonsInColumn) * (ButtonGap + ButtonSize.X.Value) + ButtonGap,
                    Y = (i % buttonsInColumn) * (ButtonGap + ButtonSize.Y.Value) + ButtonStartY,
                };
                button.SetLabel(FontManager.GetWobbleFont("inter-medium"), LocalizationManager.Get(testScreens.Value), 12,
                    Color.Black);

                ScreenButtons.Add(new KeyValuePair<RoundedButton, string>(button, testScreens.Value));

                switch (testScreens.Key)
                {
                    case ScreenType.DrawingSprites:
                        button.Clicked += (o, e) => ScreenManager.ChangeScreen(new TestDrawingSpritesScreen());
                        break;
                    case ScreenType.Rotation:
                        button.Clicked += (o, e) => ScreenManager.ChangeScreen(new TestRotationScreen());
                        break;
                    case ScreenType.DrawableScaling:
                        button.Clicked += (o, e) => ScreenManager.ChangeScreen(new TestDrawableScalingScreen());
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
                    case ScreenType.SpriteAlphaMaskingBlend:
                        button.Clicked += (o, e) => ScreenManager.ChangeScreen(new TestSpriteAlphaMaskingBlendScreen());
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
                    case ScreenType.ScheduledUpdates:
                        button.Clicked += (o, e) => ScreenManager.ChangeScreen(new TestScheduledUpdatesScreen());
                        break;
                    case ScreenType.Joystick:
                        button.Clicked += (o, e) => ScreenManager.ChangeScreen(new TestJoystickScreen());
                        break;
                    case ScreenType.ButtonPerformance:
                        button.Clicked += (o, e) => ScreenManager.ChangeScreen(new TestButtonPerformanceScreen());
                        break;
                    case ScreenType.NavigationBar:
                        button.Clicked += (o, e) => ScreenManager.ChangeScreen(new TestNavigationBarScreen());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                i++;
            }
        }

        private void RefreshLocalizedText()
        {
            TitleText.Text = LocalizationManager.Get("Selection_Title");
            LanguageLabel.Text = LocalizationManager.Get("Selection_LanguageLabel");

            for (var i = 0; i < LanguageSelector.Options.Count; i++)
                LanguageSelector.Options[i] = LocalizationManager.Get(LanguageNameKeys[i]);

            LanguageSelector.SelectIndex(GetCurrentLanguageIndex());

            foreach (var button in ScreenButtons)
                button.Key.SetLabel(FontManager.GetWobbleFont("inter-medium"), LocalizationManager.Get(button.Value), 12,
                    Color.Black);
        }

        private static List<string> CreateLanguageOptions()
        {
            var options = new List<string>();

            foreach (var key in LanguageNameKeys)
                options.Add(LocalizationManager.Get(key));

            return options;
        }

        private static int GetCurrentLanguageIndex()
        {
            for (var i = 0; i < LanguageCultures.Length; i++)
            {
                if (LanguageCultures[i].Name == LocalizationManager.CurrentCulture.Name)
                    return i;
            }

            return 0;
        }
    }
}
