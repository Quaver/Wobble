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
        ///     Emitted when the output device is automatically changed.
        /// </summary>
        public static event Action<string> OutputDeviceChanged;

        /// <summary>
        ///     Initializes BASS and throws an exception if it fails.
        /// </summary>
        /// <param name="devicePeriod">Set to override the device period, milliseconds.</param>
        /// <param name="deviceBufferLength">Set to override the device buffer length, milliseconds.</param>
        public static void Initialize(int? devicePeriod, int? deviceBufferLength, int? device = -1)
        {
            Dispose();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Do not stop the output device to ensure consistent latency.
                //
                // Without this setting samples are played with lower latency when there's nothing else playing,
                // resulting in inconsistent hitsound and keysound latency.
                Bass.Configure(Configuration.DevNonStop, true);
            }

            // Follow system default audio source
            Bass.Configure(Configuration.IncludeDefaultDevice, true);

            if (devicePeriod.HasValue)
                Bass.Configure(Configuration.DevicePeriod, devicePeriod.Value);
            if (deviceBufferLength.HasValue)
                Bass.Configure(Configuration.DeviceBufferLength, deviceBufferLength.Value);

            Logger.Debug($"BASS options: DevicePeriod = {Bass.GetConfig(Configuration.DevicePeriod)}, DeviceBufferLength = {Bass.GetConfig(Configuration.DeviceBufferLength)}", LogType.Runtime);

            if (!Bass.Init(device.Value))
            {
                var error = Bass.LastError;

                if (error == Errors.Device)
                    throw new AudioEngineException("Quaver could not find an audio output device. Please connect or enable an audio output device and restart the game.");

                throw new AudioEngineException("Quaver could not find an audio output device. Please connect or enable an audio output device and restart the game.");
            }

            Logger.Debug($"BASS version: {Bass.Version}", LogType.Runtime);

            Tracks = new List<IAudioTrack>();
        }

        /// <summary>
        ///     Disposes of any resources used by BASS.
        /// </summary>
        internal static void Dispose()
        {
            if (Tracks != null)
            {
                lock (Tracks)
                {
                    for (var i = 0; i < Tracks.Count; i++)
                        Tracks[i]?.Dispose();

                    Tracks.Clear();
                }
            }

            Bass.Free();
        }

        /// <summary>
        ///     Updates the AudioManager and keeps things up-to-date.
        /// </summary>
        internal static void Update(GameTime gameTime)
        {
            CheckForLostOutputDevice();
            UpdateTracks(gameTime);
        }

        /// <summary>
        ///     Returns an audio device by its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static DeviceInfo? GetAudioDeviceByName(string name)
        {
            for (var i = 1; i < Bass.DeviceCount; i++)
            {
                var device = Bass.GetDeviceInfo(i);

                if (device.Name == name)
                    return device;
            }

            return null;
        }

        /// <summary>
        ///     Throws if the initialized output device is no longer available.
        /// </summary>
        private static void CheckForLostOutputDevice()
        {
            if (Tracks == null)
                return;

            if (IsCurrentOutputDeviceAvailable())
                return;

            if (TrySwitchToAvailableOutputDevice())
                return;

            throw new AudioEngineException("Quaver lost access to its audio output device and could not find another output device. Please connect or enable an audio output device and restart the game.");
        }

        /// <summary>
        ///     Checks if the current output device is still usable.
        /// </summary>
        private static bool IsCurrentOutputDeviceAvailable()
        {
            try
            {
                return IsOutputDeviceAvailable(Bass.CurrentDevice);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     Attempts to switch BASS and active tracks to another available output device.
        /// </summary>
        private static bool TrySwitchToAvailableOutputDevice()
        {
            var previousDevice = Bass.CurrentDevice;

            for (var i = 1; i < Bass.DeviceCount; i++)
            {
                if (i == previousDevice || !TryInitializeOutputDevice(i))
                    continue;

                try
                {
                    Bass.CurrentDevice = i;
                    MoveTracksToDevice(i);
                    var deviceName = Bass.GetDeviceInfo(i).Name;
                    Logger.Warning($"Lost audio output device. Switched to: {deviceName}", LogType.Runtime);
                    OutputDeviceChanged?.Invoke(deviceName);
                    return true;
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
            }

            return false;
        }

        /// <summary>
        ///     Returns if a BASS output device can be used.
        /// </summary>
        private static bool IsOutputDeviceAvailable(int device)
        {
            if (device < 1 || device >= Bass.DeviceCount)
                return false;

            var info = Bass.GetDeviceInfo(device);
            return info.IsEnabled && info.IsInitialized;
        }

        /// <summary>
        ///     Initializes an enabled output device if it is not already initialized.
        /// </summary>
        private static bool TryInitializeOutputDevice(int device)
        {
            if (device < 1 || device >= Bass.DeviceCount)
                return false;

            var info = Bass.GetDeviceInfo(device);

            if (!info.IsEnabled)
                return false;

            return info.IsInitialized || Bass.Init(device);
        }

        /// <summary>
        ///     Moves active track streams to another output device.
        /// </summary>
        private static void MoveTracksToDevice(int device)
        {
            lock (Tracks)
            {
                foreach (var track in Tracks)
                {
                    if (track is AudioTrack audioTrack && !audioTrack.IsDisposed)
                        Bass.ChannelSetDevice(audioTrack.Stream, device);
                }
            }
        }

        /// <summary>
        ///     Updates the real time for each track to keep updated.
        /// </summary>
        private static void UpdateTracks(GameTime gameTime)
        {
            lock (Tracks)
            {
                for (var i = Tracks.Count - 1; i >= 0; i--)
                {
                    var track = Tracks[i];

                    // If the track is left over, we just want to dispose of it and remove it from our tracks.
                    if (track is AudioTrack t)
                    {
                        if (t.IsLeftOver && t.AutoDispose)
                            t.Dispose();

                        if (t.IsDisposed)
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
