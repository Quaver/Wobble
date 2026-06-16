#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;

namespace Wobble.Graphics.UI.Debugging
{
    public static class DrawableDebugRegistry
    {
        private static readonly object LockObject = new object();
        private static readonly List<Entry> Entries = new List<Entry>();
        private static readonly Dictionary<int, Entry> EntriesById = new Dictionary<int, Entry>();
        private static readonly List<int> CurrentFrameDrawIds = new List<int>();
        private static int nextId = 1;

        public static int Register(Drawable drawable)
        {
            lock (LockObject)
            {
                var id = nextId++;
                var entry = new Entry(id, drawable);

                Entries.Add(entry);
                EntriesById[id] = entry;

                if (Entries.Count % 256 == 0)
                    SweepLocked();

                return id;
            }
        }

        public static void Unregister(Drawable drawable)
        {
            lock (LockObject)
            {
                for (var i = Entries.Count - 1; i >= 0; i--)
                {
                    var entry = Entries[i];

                    if (!entry.TryGetTarget(out var target) || ReferenceEquals(target, drawable))
                    {
                        Entries.RemoveAt(i);
                        EntriesById.Remove(entry.Id);
                    }
                }
            }
        }

        public static void ResetFrame()
        {
            CurrentFrameDrawIds.Clear();
        }

        public static void RecordDraw(Drawable drawable)
        {
            CurrentFrameDrawIds.Add(drawable.DebugId);
        }

        public static List<DrawableTypeDebugInfo> Snapshot(bool visibleOnly)
        {
            lock (LockObject)
            {
                SweepLocked();

                var frameCounts = new Dictionary<int, int>();
                for (var i = 0; i < CurrentFrameDrawIds.Count; i++)
                {
                    var id = CurrentFrameDrawIds[i];

                    if (!frameCounts.ContainsKey(id))
                        frameCounts[id] = 0;

                    frameCounts[id]++;
                }

                var groups = new Dictionary<string, TypeBuilder>();

                foreach (var pair in EntriesById)
                {
                    if (!pair.Value.TryGetTarget(out var drawable) || drawable.IsDisposed)
                        continue;

                    if (visibleOnly && !drawable.Visible)
                        continue;

                    var typeName = drawable.GetType().Name;

                    if (!groups.TryGetValue(typeName, out var builder))
                    {
                        builder = new TypeBuilder(typeName);
                        groups[typeName] = builder;
                    }

                    builder.LiveCount++;

                    if (!frameCounts.TryGetValue(pair.Key, out var drawCount))
                        continue;

                    var info = CreateInfo(drawable, drawCount);
                    builder.DrawnCount += drawCount;
                    builder.Objects.Add(info);
                }

                var result = groups.Values
                    .Where(x => x.DrawnCount > 0 || x.LiveCount > 0)
                    .Select(x => x.ToInfo())
                    .OrderByDescending(x => x.DrawnCount)
                    .ThenBy(x => x.TypeName)
                    .ToList();

                return result;
            }
        }

        private static DrawableDebugInfo CreateInfo(Drawable drawable, int drawCount)
        {
            string spriteTexture = null;
            string textPreview = null;
            bool? textCached = null;
            int? fontSize = null;
            int? lineCount = null;

            if (drawable is Sprite sprite)
                spriteTexture = DescribeTexture(sprite.Image);

            if (drawable is SpriteTextPlus text)
            {
                textPreview = Preview(text.Text);
                textCached = text.IsCached;
                fontSize = text.FontSize;
                lineCount = text.Children.Count;
            }

            return new DrawableDebugInfo(
                drawable.DebugId,
                drawable.GetType().Name,
                drawable.Parent?.GetType().Name ?? "None",
                drawCount,
                drawable.DrawOrder,
                drawable.Children.Count,
                drawable.Visible,
                drawable.IsDisposed,
                drawable.ScreenRectangle,
                drawable.Position,
                drawable.Size,
                drawable.Rotation,
                drawable.Scale,
                drawable.SpriteBatchOptions != null,
                spriteTexture,
                textPreview,
                textCached,
                fontSize,
                lineCount
            );
        }

        private static string DescribeTexture(Texture2D texture)
        {
            if (texture == null)
                return "none";

            var name = string.IsNullOrEmpty(texture.Name) ? "unnamed" : texture.Name;
            return $"{name} {texture.Width}x{texture.Height}";
        }

        private static string Preview(string text)
        {
            var preview = (text ?? "").Replace("\r", " ").Replace("\n", " ");

            if (preview.Length <= 48)
                return preview;

            return preview.Substring(0, 45) + "...";
        }

        private static void SweepLocked()
        {
            for (var i = Entries.Count - 1; i >= 0; i--)
            {
                var entry = Entries[i];

                if (entry.TryGetTarget(out var target) && !target.IsDisposed)
                    continue;

                Entries.RemoveAt(i);
                EntriesById.Remove(entry.Id);
            }
        }

        private class Entry
        {
            public int Id { get; }

            private WeakReference<Drawable> Reference { get; }

            public Entry(int id, Drawable drawable)
            {
                Id = id;
                Reference = new WeakReference<Drawable>(drawable);
            }

            public bool TryGetTarget(out Drawable drawable) => Reference.TryGetTarget(out drawable);
        }

        private class TypeBuilder
        {
            public string TypeName { get; }

            public int DrawnCount { get; set; }

            public int LiveCount { get; set; }

            public List<DrawableDebugInfo> Objects { get; } = new List<DrawableDebugInfo>();

            public TypeBuilder(string typeName) => TypeName = typeName;

            public DrawableTypeDebugInfo ToInfo()
            {
                Objects.Sort((a, b) =>
                {
                    var countCompare = b.DrawCount.CompareTo(a.DrawCount);
                    return countCompare != 0 ? countCompare : a.Id.CompareTo(b.Id);
                });

                return new DrawableTypeDebugInfo(TypeName, DrawnCount, LiveCount, Objects);
            }
        }
    }

    public class DrawableTypeDebugInfo
    {
        public string TypeName { get; }

        public int DrawnCount { get; }

        public int LiveCount { get; }

        public List<DrawableDebugInfo> Objects { get; }

        public DrawableTypeDebugInfo(string typeName, int drawnCount, int liveCount, List<DrawableDebugInfo> objects)
        {
            TypeName = typeName;
            DrawnCount = drawnCount;
            LiveCount = liveCount;
            Objects = objects;
        }
    }

    public class DrawableDebugInfo
    {
        public int Id { get; }
        public string TypeName { get; }
        public string ParentTypeName { get; }
        public int DrawCount { get; }
        public int DrawOrder { get; }
        public int ChildCount { get; }
        public bool Visible { get; }
        public bool IsDisposed { get; }
        public RectangleF ScreenRectangle { get; }
        public ScalableVector2 Position { get; }
        public ScalableVector2 Size { get; }
        public Microsoft.Xna.Framework.Vector2 Scale { get; }
        public float Rotation { get; }
        public bool HasSpriteBatchOptions { get; }
        public string SpriteTextureDescription { get; }
        public string TextPreview { get; }
        public bool? TextCached { get; }
        public int? FontSize { get; }
        public int? LineCount { get; }

        public DrawableDebugInfo(int id, string typeName, string parentTypeName, int drawCount, int drawOrder,
            int childCount, bool visible, bool isDisposed, RectangleF screenRectangle, ScalableVector2 position,
            ScalableVector2 size, float rotation, Microsoft.Xna.Framework.Vector2 scale, bool hasSpriteBatchOptions,
            string spriteTextureDescription, string textPreview, bool? textCached, int? fontSize, int? lineCount)
        {
            Id = id;
            TypeName = typeName;
            ParentTypeName = parentTypeName;
            DrawCount = drawCount;
            DrawOrder = drawOrder;
            ChildCount = childCount;
            Visible = visible;
            IsDisposed = isDisposed;
            ScreenRectangle = screenRectangle;
            Position = position;
            Size = size;
            Rotation = rotation;
            Scale = scale;
            HasSpriteBatchOptions = hasSpriteBatchOptions;
            SpriteTextureDescription = spriteTextureDescription;
            TextPreview = textPreview;
            TextCached = textCached;
            FontSize = fontSize;
            LineCount = lineCount;
        }
    }
}
#endif
