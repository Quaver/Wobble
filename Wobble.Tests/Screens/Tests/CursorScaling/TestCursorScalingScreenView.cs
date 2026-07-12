using Microsoft.Xna.Framework;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Form;
using Wobble.Screens;

namespace Wobble.Tests.Screens.Tests.CursorScaling
{
    public class TestCursorScalingScreenView : ScreenView
    {
        private const int DefaultScalePercent = 100;

        private BindableInt ScalePercent { get; }
        private SpriteText ValueText { get; }

        public TestCursorScalingScreenView(Screen screen) : base(screen)
        {
            var cursor = GameBase.Game.GlobalUserInterface.Cursor;

            GameBase.Game.IsMouseVisible = false;
            cursor.Image = WobbleAssets.WhiteBox;
            cursor.Visible = true;
            cursor.Alpha = 1;
            cursor.SizeScale = 1f;
            cursor.Show(1);

            ScalePercent = new BindableInt(DefaultScalePercent,
                (int)(Graphics.UI.Cursor.MinimumSizeScale * 100),
                (int)(Graphics.UI.Cursor.MaximumSizeScale * 100));
            ScalePercent.ValueChanged += (_, args) =>
            {
                GameBase.Game.GlobalUserInterface.Cursor.SizeScale = args.Value / 100f;
                ValueText.Text = $"Cursor scale: {args.Value}%";
            };

            ValueText = new SpriteText("inter-semibold", $"Cursor scale: {DefaultScalePercent}%", 24)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Y = -55
            };

            new Slider(ScalePercent, new Vector2(500, 8), WobbleAssets.WhiteBox)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Tint = new Color(70, 70, 80),
                Y = 10
            }.ChangeColor(Color.CornflowerBlue);

            new SpriteText("inter-regular", "Drag the slider, then click anywhere to test the pressed expansion.", 18)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Y = 65
            };
        }

        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(new Color(25, 25, 30));
            Container?.Draw(gameTime);
            GameBase.Game.GlobalUserInterface.Cursor.Draw(gameTime);
            GameBase.Game.TryEndBatch();
        }

        public override void Destroy()
        {
            GameBase.Game.GlobalUserInterface.Cursor.SizeScale = 1f;
            GameBase.Game.GlobalUserInterface.Cursor.Hide(0);
            GameBase.Game.IsMouseVisible = true;
            Container?.Destroy();
        }
    }
}
