using System;
using ManagedBass;

namespace Wobble.Audio
{
    /// <summary>
    ///     A sample channel instance for a given audio sample.
    ///     Used to play sound effects.
    /// </summary>
    public class AudioSampleChannel : IPlayableAudio
    {
        /// <summary>
        ///     The audio sample that the channel is for.
        /// </summary>
        private AudioSample Sample { get; }

        /// <summary>
        ///     The id of the sample channel when it is created.
        /// </summary>
        private int Id { get; }

        /// <summary>
        ///     If the sample channel has already stopped.
        /// </summary>
        public bool HasStopped { get; private set; }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="sample"></param>
        public AudioSampleChannel(AudioSample sample)
        {
            Sample = sample;

            if (Sample.IsDisposed)
                throw new PlayableAudioDisposedException("Cannot create an AudioSampleChannel from a sample that is already disposed.");

            Id = Bass.SampleGetChannel(sample.Id);
        }

        /// <summary>
        ///     Plays the sample channel.
        /// </summary>
        public void Play()
        {
            if (Sample.HasPlayed && Sample.OnlyCanPlayOnce)
                throw new AudioEngineException($"You cannot play a sample more than once");

            if (Sample.IsDisposed)
                throw new PlayableAudioDisposedException("Cannot play a sample channel that is already disposed.");

            Bass.ChannelPlay(Id);
            Sample.HasPlayed = true;
        }

        /// <summary>
        ///     Pauses the sample channel.
        /// </summary>
        public void Pause() => Bass.ChannelPause(Id);

        /// <summary>
        ///     Stop the sample channel.
        /// </summary>
        public void Stop()
        {
            if (HasStopped)
                throw new AudioEngineException("Cannot stop sample channel while it is already stopped. Did you mean to dispose the sample?");

            if (Sample.IsDisposed)
                throw new PlayableAudioDisposedException("Cannot stop a sample channel that is already disposed.");

            Bass.ChannelStop(Id);
            HasStopped = true;
        }
    }
}