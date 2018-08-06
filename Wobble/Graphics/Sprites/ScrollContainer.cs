using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Wobble.Input;

namespace Wobble.Graphics.Sprites
{
    public class ScrollContainer : Sprite
    {
        /// <summary>
        ///     The content that holds and is a parent of all sprites
        /// </summary>
        public Container ContentContainer { get; }

        /// <summary>
        ///     The scroll bar
        /// </summary>
        public Sprite Scrollbar { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public ScrollContainer(ScalableVector2 size, ScalableVector2 contentSize, bool startFromBottom = false)
        {
            Size = size;

            // Create the SpriteBatchOptions with scissor rect enabled.
            SpriteBatchOptions = new SpriteBatchOptions
            {
                RasterizerState = new RasterizerState { ScissorTestEnable = true }
            };

            // Create container in which all scrolling contents will be children of.
            ContentContainer = new Container(contentSize, new ScalableVector2(0, 0))
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true
            };

            // Choose starting location of the scroll container
            ContentContainer.Y = startFromBottom ? -ContentContainer.Height : 0;

            // Create the scroll bar.
            Scrollbar = new Sprite
            {
                Parent = this,
                Alignment = Alignment.BotRight,
                Width = 15,
                Tint = Color.Black,
                X = 1
            };
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // Set scrollbar heigt.
            Scrollbar.Height = Height / ContentContainer.Height * Height;

            // Set min scroll height to 30.
            if (Scrollbar.Height < 30)
                Scrollbar.Height = 30;

            // Handle scrolling
            if (MouseManager.CurrentState.ScrollWheelValue > MouseManager.PreviousState.ScrollWheelValue)
                ContentContainer.Y += 20;
            else if (MouseManager.CurrentState.ScrollWheelValue < MouseManager.PreviousState.ScrollWheelValue)
                ContentContainer.Y -= 20;

            // Make sure content container is clamped to the viewport.
            ContentContainer.Y = MathHelper.Clamp(ContentContainer.Y, -ContentContainer.Height + Height, 0);

            // Calculate the scrollbar's y position.
            var percentage = -ContentContainer.Y / ( -ContentContainer.Height + Height ) * 100;
            Scrollbar.Y = percentage / 100 * (Height - Scrollbar.Height);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            // Get the current scissor rectangle and save it so that we can reset it back.
            // to its original.
            var currentRect = GameBase.Game.GraphicsDevice.ScissorRectangle;

            // Temporarily set the scissor rect
            GameBase.Game.GraphicsDevice.ScissorRectangle = ScreenRectangle.ToRectangle();

            // Draw sprite + children.
            base.Draw(gameTime);

            // Reset scissor rect back to original
            GameBase.Game.GraphicsDevice.ScissorRectangle = currentRect;
        }

        /// <summary>
        ///     Adds a drawable that'll be contained in the ScrollContainer.
        /// </summary>
        public void AddContainedDrawable(Drawable drawable)
        {
            drawable.Parent = ContentContainer;

            // Set drawable and children to use the same SpriteBatch
            drawable.UsePreviousSpriteBatchOptions = true;
            drawable.Children.ForEach(x => x.UsePreviousSpriteBatchOptions = true);
        }
    }
}