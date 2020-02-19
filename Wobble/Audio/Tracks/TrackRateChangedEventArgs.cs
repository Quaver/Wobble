using System;

namespace Wobble.Audio.Tracks
{
    public class TrackRateChangedEventArgs : EventArgs
    {
        public float Previous { get; }

        public float Current { get; }

        public TrackRateChangedEventArgs(float previous, float current)
        {
            Previous = previous;
            Current = current;
        }
    }
}