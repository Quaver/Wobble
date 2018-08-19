using Microsoft.Xna.Framework;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Screens;
using Wobble.Tests.Assets;

namespace Wobble.Tests.Screens.Tests.SpriteMasking
{
    public class TestSpriteMaskingScreenView : ScreenView
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TestSpriteMaskingScreenView(Screen screen) : base(screen)
        {
            // Create the container to mask its children in
            var maskContainer = new SpriteMaskContainer
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(200, 200),
                // The MASK image. In this case it's a circle!
                // IMPORTANT! That's why this exists!
                Image = Textures.CircleMask,
                Transformations =
                {
                    new Transformation(TransformationProperty.X, Easing.Linear, 0, 300, 5000)
                },
            };

            // Create a new sprite to be contained in the mask.
            maskContainer.AddContainedSprite(new Sprite()
            {
                Parent = maskContainer,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(500, 200),
                Image = WobbleAssets.Wallpaper,
            });

            maskContainer.AddContainedSprite(new SpriteText(Fonts.AllerRegular16, "This is masked!")
            {
                Parent = maskContainer,
                Alignment = Alignment.MidCenter,
            });
        }

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
            GameBase.Game.GraphicsDevice.Clear(Color.CornflowerBlue);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();
    }
}