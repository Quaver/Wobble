using System;

namespace Wobble.Audio.Tracks
{
    public interface IAudioTrack : IDisposable
    {
        int Stream { get; }

        /// <summary>
        ///     The length of the track
        /// </summary>
        double Length { get; set; }

        /// <summary>
        ///     The playback rate of the track
        /// </summary>
        float Rate { get; set; }

        /// <summary>
        ///     If the audio track is currently playing
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        ///     If the audio track is currently paused
        /// </summary>
        bool IsPaused { get; }

        /// <summary>
        ///     If the audio track is currently stopped
        /// </summary>
        bool IsStopped { get; }

        /// <summary>
        ///     If the track is disposed of
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        ///     If thr audio track has played yet
        /// </summary>
        bool HasPlayed { get; }

        /// <summary>
        ///     Event invoked when the track has been seeked
        /// </summary>
        event EventHandler<TrackSeekedEventArgs> Seeked;

        /// <summary>
        ///     Event invoked when the track's rate has changed
        /// </summary>
        event EventHandler<TrackRateChangedEventArgs> RateChanged;

        /// <summary>
        ///     The time of the audio track
        /// </summary>
        double Time { get; }

        /// <summary>
        ///     The position of the audio track
        /// </summary>
        double Position { get; }

        /// <summary>
        ///     The volume of the track
        /// </summary>
        double Volume { get; set; }

        /// <summary>
        ///     Seeks to a specific point in the track
        /// </summary>
        /// <param name="position"></param>
        void Seek(double position);

        /// <summary>
        ///     Plays the track
        /// </summary>
        void Play();

        /// <summary>
        ///     Pauses the track
        /// </summary>
        void Pause();

        /// <summary>
        ///     Stops the track
        /// </summary>
        void Stop();

        /// <summary>
        /// </summary>
        /// <param name="shouldPitch"></param>
        void ApplyRate(bool shouldPitch);

        /// <summary>
        /// </summary>
        /// <param name="to"></param>
        /// <param name="time"></param>
        void Fade(float to, int time);
    }
}