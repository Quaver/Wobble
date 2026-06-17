#if DEBUG
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;

namespace Wobble.Graphics.UI.Debugging
{
    public static class SpriteTextPlusDebugRegistry
    {
        private static readonly object LockObject = new object();
        private static readonly List<Entry> Entries = new List<Entry>();
        private static int nextId = 1;

        public static int Register(SpriteTextPlus spriteText)
        {
            lock (LockObject)
            {
                var id = nextId++;
                Entries.Add(new Entry(id, spriteText));

                if (Entries.Count % 128 == 0)
                    SweepLocked();

                return id;
            }
        }

        public static void Unregister(SpriteTextPlus spriteText)
        {
            lock (LockObject)
                Entries.RemoveAll(x => !x.TryGetTarget(out var target) || ReferenceEquals(target, spriteText));
        }

        public static List<SpriteTextPlusDebugInfo> Snapshot(bool visibleOnly)
        {
            lock (LockObject)
            {
                var result = new List<SpriteTextPlusDebugInfo>(Entries.Count);

                for (var i = Entries.Count - 1; i >= 0; i--)
                {
                    var entry = Entries[i];

                    if (!entry.TryGetTarget(out var target) || target.IsDisposed)
                    {
                        Entries.RemoveAt(i);
                        continue;
                    }

                    if (visibleOnly && !target.Visible)
                        continue;

                    result.Add(new SpriteTextPlusDebugInfo(
                        entry.Id,
                        target.Text,
                        target.IsCached,
                        target.Visible,
                        target.FontSize,
                        target.MaxWidth,
                        target.Children.Count,
                        target.ScreenRectangle,
                        target.Size
                    ));
                }

                result.Sort((a, b) => a.Id.CompareTo(b.Id));
                return result;
            }
        }

        private static void SweepLocked() =>
            Entries.RemoveAll(x => !x.TryGetTarget(out var target) || target.IsDisposed);

        private class Entry
        {
            public int Id { get; }

            private WeakReference<SpriteTextPlus> Reference { get; }

            public Entry(int id, SpriteTextPlus target)
            {
                Id = id;
                Reference = new WeakReference<SpriteTextPlus>(target);
            }

            public bool TryGetTarget(out SpriteTextPlus target) => Reference.TryGetTarget(out target);
        }
    }

    public class SpriteTextPlusDebugInfo
    {
        public int Id { get; }

        public string Text { get; }

        public bool IsCached { get; }

        public bool Visible { get; }

        public int FontSize { get; }

        public float? MaxWidth { get; }

        public int LineCount { get; }

        public RectangleF ScreenRectangle { get; }

        public ScalableVector2 Size { get; }

        public SpriteTextPlusDebugInfo(int id, string text, bool isCached, bool visible, int fontSize, float? maxWidth,
            int lineCount, RectangleF screenRectangle, ScalableVector2 size)
        {
            Id = id;
            Text = text ?? "";
            IsCached = isCached;
            Visible = visible;
            FontSize = fontSize;
            MaxWidth = maxWidth;
            LineCount = lineCount;
            ScreenRectangle = screenRectangle;
            Size = size;
        }
    }
}
#endif
