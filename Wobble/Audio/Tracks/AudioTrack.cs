using System;
using System.IO;
using ManagedBass;
using ManagedBass.Fx;
using Wobble.Helpers;
using Wobble.Logging;

namespace Wobble.Audio.Tracks
{
    /// <summary>
    ///     This playable audio track should be used for bigger audio files such as songs.
    /// </summary>
    // ReSharper disable once InheritdocConsiderUsage
    public class AudioTrack : IAudioTrack, IPlayableAudio
    {
        /// <summary>
        ///     The method/input the track was loaded from.
        /// </summary>
        public AudioTrackLoadType Type { get; }

        /// <summary>
        ///     The original file path to the file if the user loaded the track with one.
        ///     Only works with <see cref="AudioTrackLoadType.FilePath"/>
        /// </summary>
        public string OriginalFilePath { get; }

        /// <summary>
        ///     The original byte array of the data if the user loaded the track with one.
        ///     Only works with <see cref="AudioTrackLoadType.ByteArray"/>
        /// </summary>
        public byte[] OriginalByteArray { get; }

        /// <summary>
        ///     The original Uri of the file if the user loaded the track with one.
        ///     Only works with <see cref="AudioTrackLoadType.Uri"/>
        /// </summary>
        public Uri OriginalUri { get; }

        /// <summary>
        ///     The currently loaded audio stream, if there is one.
        /// </summary>
        public int Stream { get; private set; }

        /// <summary>
        ///     The length of the current audio stream in milliseconds.
        /// </summary>
        private double _length = -1;
        public double Length
        {
            get => _length;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_length != -1)
                {
                    Logger.Error("Cannot set length of AudioTrack manually", LogType.Runtime);
                    return;
                }

                _length = value;
            }
        }

        /// <summary>
        ///     The position of the current audio stream in milliseconds (from BASS library)
        /// </summary>
        public double Position
        {
            get
            {
                if (!StreamLoaded || IsDisposed)
                {
                    Logger.Warning("Cannot get track position if disposed or stream not loaded", LogType.Runtime);
                    return 0;
                }

                return Bass.ChannelBytes2Seconds(Stream, Bass.ChannelGetPosition(Stream)) * 1000;
            }
        }

        /// <summary>
        ///     The frequency of the file
        /// </summary>
        public int Frequency { get; private set; }

        /// <summary>
        /// </summary>
        public double Time => Bass.ChannelBytes2Seconds(Stream, Bass.ChannelGetPosition(Stream)) * 1000;

        /// <summary>
        ///     If the stream is currently loaded.
        /// </summary>
        public bool StreamLoaded => Stream != 0;

        /// <summary>
        ///     If the audio stream has already been disposed of.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        ///     If the audio track has already been played.
        /// </summary>
        public bool HasPlayed { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public event EventHandler<TrackSeekedEventArgs> Seeked;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public event EventHandler<TrackRateChangedEventArgs> RateChanged;

        /// <summary>
        ///     Returns if the audio stream is currently playing.
        /// </summary>
        public bool IsPlaying => Bass.ChannelIsActive(Stream) == PlaybackState.Playing;

        /// <summary>
        ///     Returns if the audio stream is currently paused
        /// </summary>
        public bool IsPaused => Bass.ChannelIsActive(Stream) == PlaybackState.Paused;

        /// <summary>
        ///     Returns if the audio stream is currently stopped.
        /// </summary>
        public bool IsStopped => Bass.ChannelIsActive(Stream) == PlaybackState.Stopped;

        /// <summary>
        ///     If the audio track is currently pitched based on the rate.
        /// </summary>
        public bool IsPitched { get; private set; } = true;

        /// <summary>
        ///     If the audio track has played and is now stopped.
        /// </summary>
        public bool IsLeftOver => HasPlayed && IsStopped;

        /// <summary>
        ///     If the audio track was loaded to be a preview. Fast loading, (not decoded or prescanned)
        /// </summary>
        public bool IsPreview { get; }

        /// <summary>
        ///     Will determine if the Audio Track will dispose automatically.
        /// </summary>
        public bool AutoDispose { get; }

        /// <summary>
        ///     The rate at which the audio plays at.
        /// </summary>
        private float _rate = 1.0f;
        public float Rate
        {
            get => _rate;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Cannot set rate to 0 or below.");

                var previous = _rate;

                _rate = value;
                ApplyRate(IsPitched);

                RateChanged?.Invoke(this, new TrackRateChangedEventArgs(previous, _rate));
            }
        }

        /// <summary>
        ///     The master volume of all audio streams.
        /// </summary>
        public static double GlobalVolume
        {
            get => Bass.GlobalStreamVolume / 100f;
            set => Bass.GlobalStreamVolume = (int)(value * 100);
        }

        /// <summary>
        ///     The volume of the current stream as a percentage.
        /// </summary>
        public double Volume
        {
            get => Bass.ChannelGetAttribute(Stream, ChannelAttribute.Volume) * 100;
            set => Bass.ChannelSetAttribute(Stream, ChannelAttribute.Volume, (float)(value / 100f));
        }

        /// <summary>
        ///     The percentage of how far the audio track is.
        /// </summary>
        public double ProgressPercentage => Time / Length * 100;

        /// <summary>
        ///     If set to false, it won't allow playback.
        /// </summary>
        public static bool AllowPlayback { get; set; } = true;

        /// <summary>
        ///    Loads an audio track from a file.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="preview"></param>
        /// <param name="autoDispose"></param>
        public AudioTrack(string path, bool preview = false, bool autoDispose = true)
        {
            Type = AudioTrackLoadType.FilePath;
            OriginalFilePath = path;

            IsPreview = preview;
            AutoDispose = autoDispose;

            var flags = preview ? 0 : BassFlags.Decode | BassFlags.Prescan;
            Stream = Bass.CreateStream(path, Flags: flags);

            AfterLoad();
        }

        /// <summary>
        ///     Loads an audio track from a byte array
        /// </summary>
        /// <param name="data"></param>
        /// <param name="preview"></param>
        /// <param name="autoDispose"></param>
        public AudioTrack(byte[] data, bool preview = false, bool autoDispose = true)
        {
            Type = AudioTrackLoadType.ByteArray;
            OriginalByteArray = data;

            IsPreview = preview;
            AutoDispose = autoDispose;

            var flags = preview ? 0 : BassFlags.Decode | BassFlags.Prescan;
            Stream = Bass.CreateStream(data, 0, data.Length, flags);

            AfterLoad();
        }

        /// <summary>
        ///     Loads an audio track from a stream
        /// </summary>
        /// <param name="data"></param>
        /// <param name="preview"></param>
        /// <param name="autoDispose"></param>
        public AudioTrack(Stream data, bool preview = false, bool autoDispose = true)
        {
            Type = AudioTrackLoadType.ByteArray;
            IsPreview = preview;
            AutoDispose = autoDispose;

            var byteArrayData = data.ToArray();
            OriginalByteArray = byteArrayData;

            var flags = preview ? 0 : BassFlags.Decode | BassFlags.Prescan;

            Stream = Bass.CreateStream(byteArrayData, 0, data.Length, flags);
            AfterLoad();
        }

        /// <summary>
        ///     Loads an audio track from a uri (internet resource)
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="preview"></param>
        /// <param name="autoDispose"></param>
        public AudioTrack(Uri uri, bool preview = false, bool autoDispose = true)
        {
            Type = AudioTrackLoadType.Uri;
            OriginalUri = uri;
            IsPreview = preview;
            AutoDispose = autoDispose;

            var flags = preview ? 0 : BassFlags.Decode | BassFlags.Prescan;
            Stream = Bass.CreateStream(uri.ToString(), 0, flags, null);
            AfterLoad();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Play()
        {
            if (!AllowPlayback)
            {
                Logger.Error("AllowPlayback is not enabled", LogType.Runtime);
                return;
            }

            CheckIfDisposed();

            if (IsPlaying)
            {
                Logger.Warning("Cannot play track if it is already playing.", LogType.Runtime);
                return;
            }

            var previous = Time;

            Bass.ChannelPlay(Stream);
            HasPlayed = true;

            if (Time < previous)
                Seeked?.Invoke(this, new TrackSeekedEventArgs(previous, Time));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Pause()
        {
            CheckIfDisposed();

            if (!IsPlaying || IsPaused)
            {
                Logger.Warning("Cannot pause audio track if it is not playing or if already paused.", LogType.Runtime);
                return;
            }

            Bass.ChannelPause(Stream);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Stop()
        {
            CheckIfDisposed();
            Bass.ChannelStop(Stream);

            if (AutoDispose)
                Dispose();
        }

        /// <summary>
        ///     Restarts the track from the beginning.
        /// </summary>
        public void Restart()
        {
            CheckIfDisposed();

            if (IsPlaying)
                Pause();

            Seek(0);
            Play();
        }

        /// <summary>
        ///     Seeks to a position in the track
        /// </summary>
        /// <param name="pos"></param>
        public void Seek(double pos)
        {
            CheckIfDisposed();

            if (pos > Length || pos < -1)
            {
                Logger.Error("You can only seek to a position greater than -1 and below its length.", LogType.Runtime);
                return;
            }

            var previous = Time;
            Bass.ChannelSetPosition(Stream, Bass.ChannelSeconds2Bytes(Stream, pos / 1000d));

            Seeked?.Invoke(this, new TrackSeekedEventArgs(previous, Time));
        }

        /// <summary>
        ///     Applies current rate with or without pitching.
        /// </summary>
        /// <param name="shouldPitch"></param>
        public void ApplyRate(bool shouldPitch)
        {
            CheckIfDisposed();

            IsPitched = shouldPitch;

            if (IsPitched)
            {
                // When pitching is enabled, adjust rate using frequency.
                Bass.ChannelSetAttribute(Stream, ChannelAttribute.Tempo, 0);
                Bass.ChannelSetAttribute(Stream, ChannelAttribute.Frequency, Frequency * _rate);
            }
            else
            {
                // When pitching is disabled, adjust rate using tempo.
                Bass.ChannelSetAttribute(Stream, ChannelAttribute.Frequency, Frequency);
                Bass.ChannelSetAttribute(Stream, ChannelAttribute.Tempo, _rate * 100 - 100);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException"></exception>
        public void Dispose()
        {
            Bass.StreamFree(Stream);
            Stream = 0;
            IsDisposed = true;
            Seeked = null;
            RateChanged = null;
        }

        /// <summary>
        ///     After initially loading the stream, it is advisable to call this to do any further initialization.
        /// </summary>
        /// <exception cref="AudioEngineException"></exception>
        private void AfterLoad()
        {
            if (!StreamLoaded)
            {
                Logger.Error("Cannot call AfterLoad if stream isn't loaded.", LogType.Runtime);
                return;
            }

            lock (AudioManager.Tracks)
                AudioManager.Tracks.Add(this);

            Length = Bass.ChannelBytes2Seconds(Stream, Bass.ChannelGetLength(Stream)) * 1000;
            Frequency = Bass.ChannelGetInfo(Stream).Frequency;

            if (!IsPreview)
            {
                Stream = BassFx.TempoCreate(Stream, BassFlags.FxFreeSource);

                // Settings from osu-framework. With default settings there's a huge offset on rates below 1.
                // With these settings there's still an offset on rates below 1, but it's not as bad (just like HT in osu!).
                Bass.ChannelSetAttribute(Stream, ChannelAttribute.TempoUseQuickAlgorithm, 1);
                Bass.ChannelSetAttribute(Stream, ChannelAttribute.TempoOverlapMilliseconds, 4);
                Bass.ChannelSetAttribute(Stream, ChannelAttribute.TempoSequenceMilliseconds, 30);
            }
        }

        /// <summary>
        ///    Checks if the audio stream is disposed and/or unloaded.
        /// </summary>
        private void CheckIfDisposed()
        {
            if (StreamLoaded && !IsDisposed)
                return;

            Logger.Error("You cannot change a disposed/unloaded track's position.", LogType.Runtime);
        }

        /// <summary>
        ///     Fades the current audio stream's volume to a given volume in a specified time frame.
        /// </summary>
        /// <param name="to"></param>
        /// <param name="time"></param>
        public void Fade(float to, int time) => Bass.ChannelSlideAttribute(Stream, ChannelAttribute.Volume, to / 100, time);

        /// <summary>
        ///     Fades the speed of the track to a given rate
        /// </summary>
        /// <param name="rate"></param>
        /// <param name="time"></param>
        public void FadeSpeed(float rate, int time)
        {
            if (IsPitched)
            {
                // When pitching is enabled, adjust rate using frequency.
                Bass.ChannelSlideAttribute(Stream, ChannelAttribute.Frequency, Frequency * rate, time);
                Bass.ChannelSetAttribute(Stream, ChannelAttribute.Tempo, 0);
            }
            else
            {
                Bass.ChannelSetAttribute(Stream, ChannelAttribute.Frequency, Frequency);
                Bass.ChannelSlideAttribute(Stream, ChannelAttribute.Tempo, rate * 100 - 100, time);
            }
        }
    }
}