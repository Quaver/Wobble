using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Resources;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

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
        /// <param name="gd"></param>
        /// <param name="image"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static Texture2D LoadTexture2D(Bitmap image, ImageFormat format)
        {
            using (var stream = new MemoryStream())
            {
                image.Save(stream, format);
                return Texture2D.FromStream(WobbleGame.Instance.GraphicsDevice, stream);
            }
        }

        /// <summary>
        ///     Loads a SoundEffect from a .wav of a resource store.
        /// </summary>
        /// <returns></returns>
        public static SoundEffect LoadSoundEffect<T>(string name) => SoundEffect.FromStream((UnmanagedMemoryStream)GetProperty<T>(name));

        /// <summary>
        ///     Loads in a resource store property with a given name 
        /// </summary>
        /// <param name="name">The name of the file to load</param>
        /// <typeparam name="T">The resx resource store to load from.</typeparam>
        /// <returns></returns>
        private static object GetProperty<T>(string name) => typeof(T).GetProperty(name.Replace("-", "_").Replace("@", "_"))?.GetValue(null, null);
    }
}