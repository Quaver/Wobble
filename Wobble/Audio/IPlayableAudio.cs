using System;

namespace Wobble.Audio
{
    /// <summary>
    ///     An interface for all types of playable audio. This includes AudioTrack & AudioSampleChannel
    /// </summary>
    public interface IPlayableAudio
    {
        /// <summary>
        ///      Plays the audio
        /// </summary>
        void Play();

        /// <summary>
        ///     Pauses the audio
        /// </summary>
        void Pause();

        /// <summary>
        ///     Completely stops the audio and disposes of its resources
        /// </summary>
        void Stop();
    }
}