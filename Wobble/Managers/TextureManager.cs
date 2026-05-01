using System.Collections.Generic;
using System.Net;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Assets;

namespace Wobble.Managers
{
    public static class TextureManager
    {
        /// <summary>
        /// </summary>
        public static Dictionary<string, Texture2D> Textures { get; } = new Dictionary<string, Texture2D>();

        /// <summary>
        /// </summary>
        public static Dictionary<string, List<Texture2D>> TextureAtlases { get; } = new Dictionary<string, List<Texture2D>>();

        /// <summary>
        ///    Loads a texture and caches it for later use
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Texture2D Load(string name)
        {
            if (Textures.ContainsKey(name))
                return Textures[name];

            var tex = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get(name));
            Textures.Add(name, tex);

            return tex;
        }

        /// <summary>
        ///     Loads a texture atlas and caches it for later use
        /// </summary>
        /// <param name="name"></param>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static List<Texture2D> LoadAtlas(string name, int rows, int columns)
        {
            if (TextureAtlases.ContainsKey(name))
                return TextureAtlases[name];

            var tex = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get(name));
            var textures = AssetLoader.LoadSpritesheetFromTexture(tex, rows, columns);
            tex.Dispose();

            TextureAtlases.Add(name, textures);

            return textures;
        }

        /// <summary>
        ///     Disposes all cached textures and clears the caches.
        /// </summary>
        internal static void Dispose()
        {
            foreach (var texture in Textures.Values)
                texture?.Dispose();

            foreach (var atlas in TextureAtlases.Values)
            {
                for (var i = 0; i < atlas.Count; i++)
                    atlas[i]?.Dispose();
            }

            Textures.Clear();
            TextureAtlases.Clear();
        }
    }
}
