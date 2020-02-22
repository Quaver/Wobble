using System;
using Microsoft.Xna.Framework;

namespace Wobble.Audio.Tracks
{
    public class AudioTrackVirtual : IAudioTrack
    {
        /// <summary>
        /// </summary>
        public int Stream { get; } = -1;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public double Length { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        private float _rate = 1.0f;
        public float Rate
        {
            get => _rate;
            set
            {
                var previous = _rate;
                _rate = value;
                
                RateChanged?.Invoke(this, new TrackRateChangedEventArgs(previous, value));
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool IsPlaying { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool IsPaused { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool IsStopped { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
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

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public double Time => CurrentTime;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public double Position => Time;

        /// <summary>
        /// </summary>
        public double Volume { get; set; }

        /// <summary>
        ///     The current time of the track
        /// </summary>
        private double CurrentTime { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="length"></param>
        public AudioTrackVirtual(double length)
        {
            Length = length;
            AudioManager.Tracks.Add(this);
        }

        /// <summary>
        ///     Updates the position/time of the audio track if necessary
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            if (IsPlaying)
            {
                var proposed = CurrentTime + gameTime.ElapsedGameTime.TotalMilliseconds * Rate;

                if (proposed >= 0 || proposed < Length)
                    CurrentTime = proposed;
            }
        }

        /// <summary>
        /// </summary>
        public void Play()
        {
            IsPlaying = true;
            IsPaused = false;
            IsStopped = false;
            HasPlayed = true;
        }

        /// <summary>
        /// </summary>
        public void Pause()
        {
            IsPaused = true;
            IsPlaying = false;
            IsStopped = false;
        }

        /// <summary>
        /// </summary>
        public void Stop()
        {
            IsPlaying = false;
            IsPaused = false;
            IsStopped = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="shouldPitch"></param>
        public void ApplyRate(bool shouldPitch)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="to"></param>
        /// <param name="time"></param>
        public void Fade(float to, int time)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="position"></param>
        public void Seek(double position)
        {
            if (position >= 0 || position < Length)
            {
                var previous = CurrentTime;
                CurrentTime = position;

                Seeked?.Invoke(this, new TrackSeekedEventArgs(previous, CurrentTime));
            }
            else
                throw new AudioEngineException("CAnnot seek below 0 or above the track's length");
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Dispose()
        {
            IsDisposed = true;
            Seeked = null;
            RateChanged = null;
        }
    }
}