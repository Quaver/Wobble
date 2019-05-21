using System;
using ManagedBass;

namespace Wobble.Audio.Samples
{
    /// <inheritdoc />
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
        ///     If the Bass channel is stopped, for example after the playback has finished.
        /// </summary>
        public bool IsStopped => Bass.ChannelIsActive(Id) == PlaybackState.Stopped;

        /// <summary>
        ///     The volume of the current stream as a percentage.
        /// </summary>
        public double Volume
        {
            get => Bass.ChannelGetAttribute(Id, ChannelAttribute.Volume) * 100;
            set => Bass.ChannelSetAttribute(Id, ChannelAttribute.Volume, (float)(value / 100f));
        }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="sample"></param>
        public AudioSampleChannel(AudioSample sample, bool isPitched = true, float rate = 1f)
        {
            Sample = sample ?? throw new ArgumentNullException();

            if (Sample.IsDisposed)
                throw new PlayableAudioDisposedException("Cannot create an AudioSampleChannel from a sample that is already disposed.");

            Id = Bass.SampleGetChannel(sample.Id);

            if (rate == 1f)
                return;

            // Apply rate.
            if (isPitched)
            {
                // When pitching is enabled, adjust rate using frequency.
                var frequency = Bass.ChannelGetInfo(Id).Frequency;
                Bass.ChannelSetAttribute(Id, ChannelAttribute.Frequency, frequency * rate);
            }
            else
            {
                // When pitching is disabled, adjust rate using tempo.

                // FIXME: BassFX can't be used with samples, so to make this work streams need to be used instead.
                // https://www.un4seen.com/forum/?topic=4932.msg118858#msg118858

                // Adjust with pitching enabled as a fall-back.
                var frequency = Bass.ChannelGetInfo(Id).Frequency;
                Bass.ChannelSetAttribute(Id, ChannelAttribute.Frequency, frequency * rate);
            }
        }

        /// <summary>
        ///     If the audio track has
        /// </summary>
        public bool HasPlayed { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Play()
        {
            if (Sample.HasPlayed && Sample.OnlyCanPlayOnce)
                throw new AudioEngineException($"You cannot play a sample more than once");

            if (Sample.IsDisposed)
                throw new PlayableAudioDisposedException("Cannot play a sample channel that is already disposed.");

            Bass.ChannelPlay(Id);
            Sample.HasPlayed = true;
            HasPlayed = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Pause() => Bass.ChannelPause(Id);

        /// <inheritdoc />
        /// <summary>
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

        public void Dispose()
        {
        }
    }
}