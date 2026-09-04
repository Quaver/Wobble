using System;

namespace Wobble.Graphics.Sprites.Text
{
    /// <summary>
    ///     Event data for a link clicked in a <see cref="SpriteTextPlusFormattable"/>.
    /// </summary>
    public class TextLinkClickedEventArgs : EventArgs
    {
        /// <summary>
        ///     The clicked link range.
        /// </summary>
        public TextLinkRange Link { get; }

        /// <summary>
        ///     The target associated with the clicked link.
        /// </summary>
        public string Target => Link.Target;

        /// <summary>
        /// </summary>
        /// <param name="link"></param>
        public TextLinkClickedEventArgs(TextLinkRange link) => Link = link;
    }
}