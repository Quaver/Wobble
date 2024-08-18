using System;

namespace Wobble.Audio
{
    /// <summary>
    ///     Exception that is thrown when trying to access a disposed
    ///     audio stream/sample.
    /// </summary>
    public class PlayableAudioDisposedException : Exception
    {
        public PlayableAudioDisposedException(string message) : base(message) { }
    }
}