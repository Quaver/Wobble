using System;
using System.Collections.Generic;
using System.Text;

namespace Wobble.Platform
{
    /// <summary>
    ///     https://github.com/ppy/osu-framework/blob/master/osu.Framework/Platform/Clipboard.cs
    /// </summary>
    public abstract class Clipboard
    {
        public abstract string GetText();

        public abstract void SetText(string selectedText);
    }
}
