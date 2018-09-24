using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ManagedBass;
using Wobble.Audio.Tracks;

namespace Wobble.Audio
{
    /// <summary>
    ///     Handles all of the AudioTracks that are currently active.
    /// </summary>
    public static class AudioManager
    {
        /// <summary>
        ///     The audio tracks that are currently loaded and available.
        /// </summary>
        public static List<AudioTrack> Tracks { get; private set; }

        /// <summary>
        ///     Initializes BASS and throws an exception if it fails.
        /// </summary>
        internal static void Initialize()
        {
            if (!Bass.Init())
                throw new AudioEngineException("BASS has failed to initialize! Are your platform-specific dlls present?");

            Tracks = new List<AudioTrack>();
        }

        /// <summary>
        ///     Disposes of any resources used by BASS.
        /// </summary>
        internal static void Dispose() => Bass.Free();

        /// <summary>
        ///     Updates the AudioManager and keeps things up-to-date.
        /// </summary>
        internal static void Update() => UpdateTracks();

        /// <summary>
        ///     Updates the real time for each track to keep updated.
        /// </summary>
        private static void UpdateTracks()
        {
            for (var i = Tracks.Count - 1; i >= 0; i--)
            {
                if (i > Tracks.Count)
                    break;

                var track = Tracks[i];

                // If the track is left over, we just want to dispose of it and remove it from our tracks.
                if (track.IsLeftOver)
                {
                    if (!track.IsDisposed)
                        track.Dispose();

                    Tracks.Remove(track);
                    continue;
                }

                track.CorrectTime(GameBase.Game.TimeSinceLastFrame);
            }
        }
    }
}
