using System;

namespace Wobble.Audio
{
    public interface IPlayableAudio
    {
        void Play();
        void Pause();
        void Stop();
    }
}