using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ManagedBass;
using Microsoft.Xna.Framework;
using Wobble.Audio.Tracks;
using Wobble.Logging;

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
        /// <param name="devicePeriod">Set to override the device period, milliseconds.</param>
        /// <param name="deviceBufferLength">Set to override the device buffer length, milliseconds.</param>
        internal static void Initialize(int? devicePeriod, int? deviceBufferLength)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Do not stop the output device to ensure consistent latency.
                //
                // Without this setting samples are played with lower latency when there's nothing else playing,
                // resulting in inconsistent hitsound and keysound latency.
                Bass.Configure(Configuration.DevNonStop, true);
            }

            if (devicePeriod.HasValue)
                Bass.Configure(Configuration.DevicePeriod, devicePeriod.Value);
            if (deviceBufferLength.HasValue)
                Bass.Configure(Configuration.DeviceBufferLength, deviceBufferLength.Value);

            Logger.Debug($"BASS options: DevicePeriod = {Bass.GetConfig(Configuration.DevicePeriod)}, DeviceBufferLength = {Bass.GetConfig(Configuration.DeviceBufferLength)}", LogType.Runtime);

            if (!Bass.Init())
            {
                var error = Bass.LastError;
                throw new AudioEngineException($"BASS has failed to initialize (error code: {(int) error}, name: \"{error}\")! Are your platform-specific dlls present?");
            }

            Logger.Debug($"BASS version: {Bass.Version}", LogType.Runtime);

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
            lock (Tracks)
            {
                for (var i = Tracks.Count - 1; i >= 0; i--)
                {
                    if (i > Tracks.Count)
                        break;

                    var track = Tracks[i];

                    // If the track is left over, we just want to dispose of it and remove it from our tracks.
                    if (track is AudioTrack t)
                    {
                        if ((t.IsLeftOver && t.AutoDispose) || t.IsDisposed)
                        {
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
}
