using System;

namespace Wobble.Audio.Tracks
{
    public class TrackSeekedEventArgs : EventArgs
    {
        public double PreviousTime { get; }

        public double CurrentTime { get; }

        public TrackSeekedEventArgs(double previousTime, double currentTime)
        {
            PreviousTime = previousTime;
            CurrentTime = currentTime;
        }
    }
}