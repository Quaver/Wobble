using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Screens;
using Wobble.Window;

namespace Wobble.Extended.HotReload.Screens.UI
{
    public class HotLoadingBrowser : ScrollContainer
    {
        private Dictionary<string, Type> Screens { get; }

        /// <summary>
        /// </summary>
        private List<ImageButton> Buttons { get; } = new List<ImageButton>();

        /// <summary>
        /// </summary>
        private HotLoaderScreenView ScreenView { get; }

        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="screenview"></param>
        /// <param name="screens"></param>
        public HotLoadingBrowser(HotLoaderScreenView screenview, Dictionary<string, Type> screens)
            : base(new ScalableVector2(250, WindowManager.Height), new ScalableVector2(250, WindowManager.Height))
        {
            ScreenView = screenview;
            Screens = screens;
            Tint = Color.Black;
            Alpha = 0.85f;
            CreateButtons();
        }

        /// <summary>
        /// </summary>
        private void CreateButtons()
        {
            foreach (var test in Screens)
                AddButton(test.Key);
        }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        private void AddButton(string name)
        {
            var btn = new ImageButton(WobbleAssets.WhiteBox)
            {
                Size = new ScalableVector2(Width - 10, 30),
                Alignment = Alignment.TopCenter,
                Tint = new Color(60, 60, 60),
                Y = Buttons.Count * 30 + 10 * (Buttons.Count)
            };

            btn.Clicked += (o, e) =>
            {
                ScreenView.ChangeScreen(Screens[name]);
                Console.WriteLine($"Going to screen: {name}");
            };

            // ReSharper disable once ObjectCreationAsStatement
            new SpriteText("Arial", name, 14)
            {
                Parent = btn,
                UsePreviousSpriteBatchOptions = true,
                Alignment = Alignment.MidCenter,
            };

            Buttons.Add(btn);

            var totalHeight =  Buttons.Count * 30 + 10 * (Buttons.Count - 1);

            if (totalHeight > Height)
                ContentContainer.Height = totalHeight;
            else
                ContentContainer.Height = Height;

            AddContainedDrawable(btn);
        }
    }
}