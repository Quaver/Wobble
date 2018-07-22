using System;

namespace Wobble.Audio
{
    public class PlayableAudioDisposedException : Exception
    {
        public PlayableAudioDisposedException(string message) : base(message) {}
    }
}