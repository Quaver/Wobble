using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Resources;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Wobble.Resources
{
    /// <summary>
    ///     A way to load assets directly from embedded resources.
    /// </summary>
    public static class ResourceLoader
    {
        /// <summary>
        ///     Loads a texture of a given format from an embedded resource store.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static Texture2D LoadTexture2D(Bitmap image, ImageFormat format)
        {
            using (var stream = new MemoryStream())
            {
                image.Save(stream, format);
                return Texture2D.FromStream(GameBase.Game.GraphicsDevice, stream);
            }
        }

        /// <summary>
        ///     Loads a spritesheet in from a texture 2d given the number of rows and columns.
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static List<Texture2D> LoadSpritesheetFromTexture(Texture2D tex, int rows, int columns)
        {
            var frames = new List<Texture2D>();

            // Get the width and height of each individual texture.
            var imgWidth = tex.Width / columns;
            var imgHeight = tex.Height / rows;

            for (var i = 0; i < rows * columns; i++)
            {
                // Get the specific row and column from the index.
                var column = i / rows;
                var row = i % rows;

                var sourceRect = new Rectangle(imgWidth * column, imgHeight * row, imgWidth, imgHeight);
                var cropTexture = new Texture2D(GameBase.Game.GraphicsDevice, sourceRect.Width, sourceRect.Height);
                var data = new Color[sourceRect.Width * sourceRect.Height];
                tex.GetData(0, sourceRect, data, 0, data.Length);
                cropTexture.SetData(data);

                frames.Add(cropTexture);
            }

            return frames;
        }

        /// <summary>
        ///     Loads an image into a Texture2D from a local file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Texture2D LoadTexture2DFromFile(string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                return Texture2D.FromStream(GameBase.Game.GraphicsDevice, fileStream);
            }
        }

        /// <summary>
        ///     Loads a SpriteFont from an embedded resource.
        /// </summary>
        /// <returns></returns>
        public static SpriteFont LoadFont(ResourceManager manager, string fontName)
        {
            // Grab the current content manager.
            var oldManager = GameBase.Game.Content;

            // Set the new content manager.
            var resxContent = new ResourceContentManager(GameBase.Game.Services,manager);
            GameBase.Game.Content = resxContent;

            // Load up the font.
            var font = GameBase.Game.Content.Load<SpriteFont>(fontName);

            // Reset the content manager back to what it was.
            GameBase.Game.Content = oldManager;

            return font;
        }

        /// <summary>
        ///     Loads in a resource store property with a given name
        /// </summary>
        /// <param name="name">The name of the file to load</param>
        /// <typeparam name="T">The resx resource store to load from.</typeparam>
        /// <returns></returns>
        private static object GetProperty<T>(string name) => typeof(T).GetProperty(name.Replace("-", "_").Replace("@", "_"))?.GetValue(null, null);
    }
}