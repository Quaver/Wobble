using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.BitmapFonts;
using Wobble.Graphics.Primitives;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Logging;
using Wobble.Screens;
using Wobble.Tests.Assets;

namespace Wobble.Tests.Screens.Tests.DrawingSprites
{
    public class TestDrawingSpritesScreenView : ScreenView
    {
        /// <summary>
        ///     Green box sprite.
        /// </summary>
        public Sprite GreenBox { get; }

        /// <summary>
        ///     Text that displays "Hello World!"
        /// </summary>
        public SpriteText HelloWorldText { get; }

        /// <summary>
        ///     Button that says click me! Click it to find out the surprise.
        /// </summary>
        public TextButton ClickMeButton { get; }

        /// <summary>
        ///     The background color for the scene.
        /// </summary>
        private Color BackgroundColor { get; set; } = Color.CornflowerBlue;

        /// <summary>
        ///     Random number generator.
        /// </summary>
        private Random Rng { get; } = new Random();

        /// <summary>
        ///     An animatable sprite with custom blend states.
        /// </summary>
        private AnimatableSprite AnimatingLighting { get; }

        /// <summary>
        ///     Sprite that has a semi_transparent shader attached to it.
        ///     (Orange Box)
        /// </summary>
        private Sprite SpriteWithShader { get; }

        /// <summary>
        ///     Dictates if the sprite with shader's width is decreasing in the shader animation
        /// </summary>
        private bool SpriteWithShaderWidthDecreasing { get; set; } = true;

        /// <summary>
        ///     Dictates if the sprite with shader's height is decreasing in the shader animation
        /// </summary>
        private bool SpriteWithShaderHeightDecreasing { get; set; } = true;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TestDrawingSpritesScreenView(Screen screen) : base(screen)
        {
#region GREEN_BOX
            GreenBox = new Sprite()
            {
                Parent = Container,
                Size = new ScalableVector2(50, 50),
                Tint = Color.Green,
                Position = new ScalableVector2(0, 10),
                Alignment = Alignment.TopCenter
            };

            GreenBox.AddBorder(Color.White, 2);
#endregion

#region HELLO_WORLD_TEXT
            HelloWorldText = new SpriteText("exo2-bold", "Hello, World!", 18)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = GreenBox.Height + GreenBox.Y + 40
            };
#endregion

#region CLICK_ME_BUTTON
            ClickMeButton = new TextButton(WobbleAssets.WhiteBox, "exo2-bold", "Click me!", 18, (sender, args) =>
                {
                    // Click event handler method goes here.
                    // Choose a random background color!
                    BackgroundColor = new Color(Rng.Next(0, 255), Rng.Next(0, 255), Rng.Next(0, 255));
                })
            {
                Parent = Container,
                Size = new ScalableVector2(200, 50),
                Tint = Color.Red,
                Text = { Tint = Color.White},
                Alignment = Alignment.TopCenter,
                Y = HelloWorldText.Y + 50
            };

            ClickMeButton.RightClicked += (sender, args) => Logger.Debug("Right Clicked", LogType.Runtime);
            ClickMeButton.MiddleMouseClicked += (sender, args) => Logger.Debug("Middle Mouse Clicked", LogType.Runtime);
#endregion

#region ANIMATING_LIGHTING
            AnimatingLighting = new AnimatableSprite(Textures.TestSpriteSheet)
            {
                Parent = Container,
                Size = new ScalableVector2(200, 200),
                Alignment = Alignment.MidRight,
                X = -20,
                // Here we will create new custom SpriteBatchOptions for this sprite.
                // This overrides SpriteBatch.Begin() to include these new SpriteBatchOptions ONLY for this sprite and any
                // sprites drawn after it that specify `UsePreviousSpriteBatchOptions`
                //
                // IMPORTANT!
                // Any sprites you want to have these same SpriteBatch.Begin options for WITHOUT creating a new SpriteBatch,
                // you must set `UsePreviousSpriteBatchOptions` to true.
                SpriteBatchOptions = new SpriteBatchOptions { BlendState = BlendState.Additive },
            };

            // Start the animation loop forwards at 60FPS infinitely.
            AnimatingLighting.StartLoop(Direction.Forward, 60);
#endregion

#region SPRITE_WITH_SHADER
            SpriteWithShader = new Sprite
            {
                Image = WobbleAssets.WhiteBox,
                Size = new ScalableVector2(200, 200),
                Alignment = Alignment.TopCenter,
                Parent = Container,
                Tint = Color.Orange,
                Y = ClickMeButton.Y + ClickMeButton.Height + 40,
                SpriteBatchOptions = new SpriteBatchOptions
                {
                    SortMode = SpriteSortMode.Deferred,
                    BlendState = BlendState.NonPremultiplied,
                    SamplerState = SamplerState.PointClamp,
                    DepthStencilState = DepthStencilState.Default,
                    RasterizerState = RasterizerState.CullNone,
                    // The shader attached is to make the sprite semi transparent
                    // Shader created by "Vortex-" (https://github.com/VortexCoyote)
                    Shader = new Shader(GameBase.Game.Resources.Get("Wobble.Tests.Resources/Shaders/semi-transparent.mgfxo"),
                        new Dictionary<string, object>
                    {
                        {"p_dimensions", new Vector2(200, 200)},
                        {"p_position", new Vector2(0, 0)},
                        {"p_rectangle", new Vector2(100, 100)},
                        {"p_alpha", 0f}
                    })
                }
            };
#endregion
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            PerformShaderedSpriteAnimation(gameTime);
            Container?.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(BackgroundColor);
            Container?.Draw(gameTime);

            try
            {
                GameBase.Game.SpriteBatch.End();
            }
            catch (Exception)
            {
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();

        /// <summary>
        ///     Performs the animation which makes the orange box sprite more and less transparent.
        ///
        ///     This is something just to show how you can do animations with shader properties and also
        ///     lerping.
        /// </summary>
        private void PerformShaderedSpriteAnimation(GameTime gameTime)
        {
            // Get the time since the last frame so we can lerp with it.
            var timeSinceLastFrame = gameTime.ElapsedGameTime.TotalMilliseconds;

            // The current transparent rectangle for the sprite.
            var currentTransparentRect = (Vector2) SpriteWithShader.SpriteBatchOptions.Shader.Parameters["p_rectangle"];

            // When the width of the box is fully shown, we want to set it to be decreasing here.
            if (SpriteWithShaderWidthDecreasing && currentTransparentRect.X >= SpriteWithShader.Width - 0.01)
                SpriteWithShaderWidthDecreasing = false;
            // otherwise increase.
            else if (!SpriteWithShaderWidthDecreasing && currentTransparentRect.X <= 0.01)
                SpriteWithShaderWidthDecreasing = true;

            // When the height of the box is fully shown, we want to set it to be decreasing here.
            if (SpriteWithShaderHeightDecreasing && currentTransparentRect.Y >= SpriteWithShader.Height - 0.01)
                SpriteWithShaderHeightDecreasing = false;
            // Otherwise increase
            else if (!SpriteWithShaderHeightDecreasing && currentTransparentRect.Y <= 0.01)
                SpriteWithShaderHeightDecreasing = true;

            // The time used for each lerp animation.
            var animTime = (float) Math.Min(timeSinceLastFrame / 240, 1);

            // If we're decreasing the width in the shader, then we'll want to lerp the transparency rect closer to the width.
            // otherwise, we'll want to lerp it back to 0.
            var targetWidth = SpriteWithShaderWidthDecreasing ? SpriteWithShader.Width : 0;
            var targetHeight = SpriteWithShaderHeightDecreasing ? SpriteWithShader.Height : 0;

            var newWidth = MathHelper.Lerp(currentTransparentRect.X, targetWidth, animTime);
            var newHeight = MathHelper.Lerp(currentTransparentRect.Y, targetHeight, animTime);

            // Set the new rectangle shader parameter.
            SpriteWithShader.SpriteBatchOptions.Shader.SetParameter("p_rectangle", new Vector2(newWidth, newHeight), true);
        }
    }
}
