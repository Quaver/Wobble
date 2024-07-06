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

        private Sprite GlobalUISprite { get; set; }

        private Sprite DefaultLayerSprite { get; set; }

        private Sprite NullLayerSprite { get; set; }

        /// <summary>
        ///     The background color for the scene.
        /// </summary>
        public override Color ClearColor { get; } = Color.CornflowerBlue;


        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TestLayerScreenView(Screen screen) : base(screen)
        {
            var layerManager = GameBase.Game.MainLayerManager;
            for (var i = 0; i < 5; i++)
            {
                _layers.Add(layerManager.NewLayer($"Layer {i}", screen));
            }

            // desired layer order: default -> 0 -> 1 -> 2 -> 3 -> 4 -> UI
            _layers[4].RequireBelow(layerManager.UILayer);
            _layers[0].RequireAbove(layerManager.DefaultLayer);

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
                    Parent = Container,
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

            GlobalUISprite = new Sprite
            {
                Parent = Container,
                Size = new ScalableVector2(0, 50, 1, 0),
                Alignment = Alignment.TopCenter,
                Tint = Color.Gray,
                Layer = layerManager.UILayer
            };

            DefaultLayerSprite = new Sprite
            {
                Parent = Container,
                Size = new ScalableVector2(200, 200),
                Position = new ScalableVector2(450, 450),
                Tint = Color.Green,
                Layer = layerManager.DefaultLayer
            };

            NullLayerSprite = new Sprite
            {
                Parent = Container,
                Size = new ScalableVector2(100, 100),
                Position = new ScalableVector2(450, 400),
                Tint = Color.Yellow
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Container?.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();
    }
}