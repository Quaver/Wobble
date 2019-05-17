using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ManagedBass;
using Microsoft.Xna.Framework;
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
        public static List<IAudioTrack> Tracks { get; private set; }

        /// <summary>
        ///     Initializes BASS and throws an exception if it fails.
        /// </summary>
        internal static void Initialize()
        {
            if (!Bass.Init())
            {
                var error = Bass.LastError;
                throw new AudioEngineException($"BASS has failed to initialize (error code: {(int) error}, name: \"{error}\")! Are your platform-specific dlls present?");
            }

            Tracks = new List<IAudioTrack>();
        }

        /// <summary>
        ///     Disposes of any resources used by BASS.
        /// </summary>
        internal static void Dispose() => Bass.Free();

        /// <summary>
        ///     Updates the AudioManager and keeps things up-to-date.
        /// </summary>
        internal static void Update(GameTime gameTime) => UpdateTracks(gameTime);

        /// <summary>
        ///     Updates the real time for each track to keep updated.
        /// </summary>
        private static void UpdateTracks(GameTime gameTime)
        {
            for (var i = Tracks.Count - 1; i >= 0; i--)
            {
                if (i > Tracks.Count)
                    break;

                var track = Tracks[i];

                // If the track is left over, we just want to dispose of it and remove it from our tracks.
                if (track is AudioTrack t)
                {
                    if (t.IsLeftOver && t.AutoDispose)
                    {
                        if (!t.IsDisposed)
                            t.Dispose();

                        Tracks.Remove(t);
                        continue;
                    }
                }

                if (track is AudioTrackVirtual atv)
                {
                    if (atv.IsDisposed)
                    {
                        Tracks.Remove(atv);
                        continue;
                    }

                    atv.Update(gameTime);
                }
            }
        }
    }
}
