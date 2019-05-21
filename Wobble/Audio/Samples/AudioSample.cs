using System;
using System.IO;
using ManagedBass;
using Microsoft.Xna.Framework;
using Wobble.Helpers;

namespace Wobble.Audio.Samples
{
    /// <inheritdoc />
    /// <summary>
    ///     Should be used for small audio files such as sound effects.
    /// </summary>
    public class AudioSample : IDisposable
    {
        /// <summary>
        ///     The sample's id.
        /// </summary>
        public int Id { get; }

        /// <summary>
        ///     The default concurrent plays for this sample.
        /// </summary>
        public const int DEFAULT_CONCURRENCY = 10;

        /// <summary>
        ///     If the sample has already previously played.
        /// </summary>
        public bool HasPlayed { get; internal set; }

        /// <summary>
        ///     Determines if the audio stream can only be played one time.
        /// </summary>
        public bool OnlyCanPlayOnce { get; }

        /// <summary>
        ///     Keeps track of if the sample has been disposed of.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        ///     The master volume of all audio samples.
        /// </summary>
        public static double GlobalVolume
        {
            get => Bass.GlobalSampleVolume / 100f;
            set => Bass.GlobalSampleVolume = (int) (value * 100);
        }

        /// <summary>
        ///     Creates an audio sample from a local file.
        /// </summary>
        /// <param name="path">The path of the audio file.</param>
        /// <param name="onlyCanPlayOnce">If this sample is set to only play one time. (Default is true)</param>
        /// <param name="concurrentPlaybacks">The amount of concurrent playbacks possible for this sample</param>
        /// <exception cref="FileNotFoundException"></exception>
        public AudioSample(string path, bool onlyCanPlayOnce = false, int concurrentPlaybacks = DEFAULT_CONCURRENCY)
        {
            OnlyCanPlayOnce = onlyCanPlayOnce;
            Id = Load(path, concurrentPlaybacks);
        }

        /// <summary>
        ///     Creates an audio sample from a byte array.
        /// </summary>
        /// <param name="data">The byte array data of the sample</param>
        /// <param name="onlycanPlayOnce">If this sample is set to play only one time. (Default is true)</param>
        /// <param name="concurrentPlaybacks">The amount of concurrent playbacks possible for this sample</param>
        public AudioSample(byte[] data, bool onlycanPlayOnce = false, int concurrentPlaybacks = DEFAULT_CONCURRENCY)
        {
            OnlyCanPlayOnce = onlycanPlayOnce;
            Id = Load(data, concurrentPlaybacks);
        }

        /// <summary>
        ///     Creates an audio sample from a stream.
        /// </summary>
        /// <param name="data">The stream of data to be played</param>
        /// <param name="onlycanPlayOnce">If this sample is set to play only one time. (Default is true)></param>
        /// <param name="concurrentPlaybacks">The amount of concurrent playbacks possible for this sample</param>
        public AudioSample(Stream data, bool onlycanPlayOnce = false, int concurrentPlaybacks = DEFAULT_CONCURRENCY)
        {
            OnlyCanPlayOnce = onlycanPlayOnce;
            Id = Load(data.ToArray(), concurrentPlaybacks);
        }

        /// <summary>
        ///     Creates a new audio sample with undefined contents.
        /// </summary>
        /// <param name="length"></param>
        /// <param name="onlycanPlayOnce"></param>
        /// <param name="frequency"></param>
        /// <param name="channels"></param>
        /// <param name="concurrentPlaybacks"></param>
        public AudioSample(int length = 0, bool onlycanPlayOnce = false, int frequency = 44100, int channels = 2,
            int concurrentPlaybacks = DEFAULT_CONCURRENCY)
        {
            OnlyCanPlayOnce = onlycanPlayOnce;
            Id = Bass.CreateSample(length, frequency, channels, concurrentPlaybacks,
                BassFlags.Default | BassFlags.SampleOverrideLongestPlaying);
        }

        /// <summary>
        ///     Creates an audio sample channel to be played.
        /// </summary>
        /// <returns></returns>
        public AudioSampleChannel CreateChannel(bool isPitched = true, float rate = 1f)
        {
            return new AudioSampleChannel(this, isPitched, rate);
        }

        /// <summary>
        ///     Loads an audio sample from a file path and returns an Id to it.
        /// </summary>
        /// <param name="path">The file path to load the audio from</param>
        /// <param name="concurrentPlaybacks">The number of times the audio can be played concurrently.</param>
        /// <returns></returns>
        private static int Load(string path, int concurrentPlaybacks)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("AudioSample not found", path, new AudioEngineException());

            return Bass.SampleLoad(path, 0, 0, concurrentPlaybacks, BassFlags.Default | BassFlags.SampleOverrideLongestPlaying);
        }

        /// <summary>
        ///     Loads an audio sample from a byte array and returns an Id to the sample.
        /// </summary>
        /// <param name="data">The containing audio data.</param>
        /// <param name="concurrentPlaybacks">The total amount of concurrent playbacks allowed for the sample.</param>
        /// <returns></returns>
        private static int Load(byte[] data, int concurrentPlaybacks) => Bass.SampleLoad(data, 0, data.Length, concurrentPlaybacks,
                                                                            BassFlags.Default | BassFlags.SampleOverrideLongestPlaying);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Dispose()
        {
            Bass.SampleFree(Id);
            IsDisposed = true;
        }
    }
}