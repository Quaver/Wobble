using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Input;
using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.Layering
{
    public class TestLayerScreenView : ScreenView
    {
        /// <summary>
        ///     Green box sprite.
        /// </summary>
        private readonly List<Layer> _layers = new();

        private readonly List<Sprite> _sprites = new();

        private readonly LayeredContainer _layeredContainer;

        private Sprite DefaultLayerSprite { get; set; }

        private Sprite NullLayerSprite { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TestLayerScreenView(Screen screen) : base(screen)
        {
            _layeredContainer = new LayeredContainer { Parent = Container };
            for (var i = 0; i < 5; i++)
            {
                _layers.Add(_layeredContainer.LayerManager.NewLayer($"Layer {i}"));
            }

            // desired layer order: default -> 0 -> 1 -> 2 -> 3 -> 4 -> Top
            _layers[4].RequireBelow(_layeredContainer.LayerManager.TopLayer);
            _layers[0].RequireAbove(_layeredContainer.LayerManager.DefaultLayer);

            _layers[3].RequireAbove(_layers[2]);
            _layers[2].RequireAbove(_layers[0]);
            _layers[0].RequireBelow(_layers[4]);
            _layers[3].RequireBelow(_layers[4]);
            _layers[0].RequireBelow(_layers[1]);
            _layers[1].RequireBelow(_layers[2]);

            // Cycle would be ignored
            _layers[4].RequireBelow(_layers[0]);

            for (var i = 4; i >= 0; i--)
            {
                var size = (5 - i) * 100;
                var tint = (float)(i + 1) / 5;
                var sprite = new Sprite
                {
                    Size = new ScalableVector2(size, size),
                    Parent = _layeredContainer,
                    Tint = new Color(tint, tint, tint, 1),
                    Layer = _layers[i]
                };
                _sprites.Add(sprite);
                new SpriteText("exo2-bold", $"Layer {i}", 18)
                {
                    Parent = sprite,
                    Alignment = Alignment.BotRight,
                    Tint = Color.Red
                };
            }

            DefaultLayerSprite = new Sprite
            {
                Parent = _layeredContainer,
                Size = new ScalableVector2(200, 200),
                Position = new ScalableVector2(450, 450),
                Tint = Color.Green,
                Layer = _layeredContainer.LayerManager.DefaultLayer
            };

            NullLayerSprite = new Sprite
            {
                Parent = Container,
                Size = new ScalableVector2(100, 100),
                Position = new ScalableVector2(450, 400),
                Tint = new Color(128, 128, 0, 128)
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Container?.Update(gameTime);
            if (KeyboardManager.IsUniqueKeyPress(Keys.D))
                _layeredContainer.LayerManager.Dump();
        }

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