using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;

namespace Wobble.Screens
{
    public static class ScreenManager
    {
        private static readonly object LockObject = new object();

        private static readonly Dictionary<string, Drawable> RegisteredElements =
            new Dictionary<string, Drawable>(StringComparer.Ordinal);

        /// <summary>
        ///     The screen that is currently being drawn.
        /// </summary>
        private static Screen CurrentScreen { get; set; }

        public static string CurrentScreenName => CurrentScreen?.GetType().Name ?? "None";

        /// <summary>
        ///     The screen and retained element keys queued for the next update.
        /// </summary>
        private static Screen QueuedScreen { get; set; }

        private static HashSet<string> QueuedRetainedElementKeys { get; set; } =
            new HashSet<string>(StringComparer.Ordinal);

        /// <summary>
        ///     Queues a screen switch without retaining registered elements.
        /// </summary>
        public static void ChangeScreen(Screen screen, bool switchImmediately = false) =>
            ChangeScreen(screen, Array.Empty<string>(), switchImmediately);

        /// <summary>
        ///     Queues a screen switch and retains the registered drawable roots named by
        ///     <paramref name="retainedElements"/> for this transition only.
        /// </summary>
        public static void ChangeScreen(Screen screen, IEnumerable<string> retainedElements,
            bool switchImmediately = false)
        {
            if (screen == null)
                throw new ArgumentNullException(nameof(screen));

            var retainedKeys = new HashSet<string>(retainedElements ?? Array.Empty<string>(),
                StringComparer.Ordinal);

            lock (LockObject)
            {
                foreach (var key in retainedKeys)
                {
                    if (!RegisteredElements.ContainsKey(key))
                        throw new ArgumentException($"No screen element is registered with the key '{key}'.",
                            nameof(retainedElements));
                }

                if (switchImmediately)
                {
                    SwitchScreen(screen, retainedKeys);
                    QueuedScreen = null;
                    QueuedRetainedElementKeys.Clear();
                    return;
                }

                QueuedScreen = screen;
                QueuedRetainedElementKeys = retainedKeys;
            }
        }

        /// <summary>
        ///     Registers a drawable root so a future screen transition can retain it by key.
        /// </summary>
        public static void RegisterElement(string key, Drawable drawable)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("A retained screen element key is required.", nameof(key));

            if (drawable == null)
                throw new ArgumentNullException(nameof(drawable));

            lock (LockObject)
            {
                if (RegisteredElements.ContainsKey(key))
                    throw new InvalidOperationException($"A screen element is already registered with the key '{key}'.");

                if (drawable.IsDisposed)
                    throw new ObjectDisposedException(nameof(drawable));

                if (!IsDescendantOf(drawable, CurrentScreen?.View.Container))
                    throw new InvalidOperationException("A registered screen element must belong to the current screen view.");

                foreach (var registeredDrawable in RegisteredElements.Values)
                {
                    if (IsDescendantOf(drawable, registeredDrawable) ||
                        IsDescendantOf(registeredDrawable, drawable))
                        throw new InvalidOperationException(
                            "A retained screen element cannot contain, or be contained by, another registered element.");
                }

                RegisteredElements.Add(key, drawable);
            }
        }

        public static bool TryGetElement<T>(string key, out T drawable) where T : Drawable
        {
            lock (LockObject)
            {
                if (RegisteredElements.TryGetValue(key, out var registeredDrawable) &&
                    registeredDrawable is T typedDrawable && !typedDrawable.IsDisposed)
                {
                    drawable = typedDrawable;
                    return true;
                }

                drawable = null;
                return false;
            }
        }

        public static T GetElement<T>(string key) where T : Drawable
        {
            if (TryGetElement<T>(key, out var drawable))
                return drawable;

            throw new KeyNotFoundException($"No active screen element with the key '{key}' and type {typeof(T).Name} exists.");
        }

        /// <summary>
        ///     Removes a registered element. By default, its drawable tree is destroyed.
        /// </summary>
        public static bool RemoveElement(string key, bool destroy = true)
        {
            lock (LockObject)
            {
                if (!RegisteredElements.TryGetValue(key, out var drawable))
                    return false;

                RegisteredElements.Remove(key);

                if (destroy && !drawable.IsDisposed)
                    drawable.Destroy();
                else if (!destroy)
                    DetachWithoutDestroying(drawable);

                return true;
            }
        }

        public static void Update(GameTime gameTime)
        {
            CurrentScreen?.Update(gameTime);

            lock (LockObject)
            {
                if (QueuedScreen == null)
                    return;

                SwitchScreen(QueuedScreen, QueuedRetainedElementKeys);
                QueuedScreen = null;
                QueuedRetainedElementKeys = new HashSet<string>(StringComparer.Ordinal);
            }
        }

        public static void Draw(GameTime gameTime) => CurrentScreen?.Draw(gameTime);

        private static void SwitchScreen(Screen screen, ISet<string> retainedKeys)
        {
            var retainedElements = RegisteredElements
                .Where(x => retainedKeys.Contains(x.Key) && !x.Value.IsDisposed)
                .ToArray();

            foreach (var element in retainedElements)
                DetachWithoutDestroying(element.Value);

            if (CurrentScreen?.AutomaticallyDestroyOnScreenSwitch != false)
                CurrentScreen?.Destroy();
            else
                ResetInteractionState(CurrentScreen.View.Container);

            foreach (var element in RegisteredElements
                         .Where(x => !retainedKeys.Contains(x.Key) || x.Value.IsDisposed).ToArray())
            {
                RegisteredElements.Remove(element.Key);

                if (!element.Value.IsDisposed)
                    element.Value.Destroy();
            }

            CurrentScreen = screen;

            foreach (var element in retainedElements)
                element.Value.Parent = CurrentScreen.View.Container;

            CurrentScreen.OnActivated();
        }

        private static bool IsDescendantOf(Drawable drawable, Drawable ancestor)
        {
            if (ancestor == null)
                return false;

            for (var current = drawable; current != null; current = current.Parent)
            {
                if (current == ancestor)
                    return true;
            }

            return false;
        }

        private static void DetachWithoutDestroying(Drawable drawable)
        {
            var destroyIfParentIsNull = drawable.DestroyIfParentIsNull;
            drawable.DestroyIfParentIsNull = false;
            drawable.Parent = null;
            drawable.DestroyIfParentIsNull = destroyIfParentIsNull;
        }

        private static void ResetInteractionState(Drawable drawable)
        {
            if (drawable is Button button)
                button.ResetInteractionState();

            foreach (var child in drawable.Children)
                ResetInteractionState(child);
        }
    }
}
