using System.Collections.Generic;
using Wobble.Audio.Samples;
using Wobble.Audio.Tracks;

namespace Wobble.Managers
{
    public static class CachedAudioManager
    {
        /// <summary>
        /// </summary>
        public static Dictionary<string, AudioTrack> Tracks { get; } = new Dictionary<string, AudioTrack>();

        /// <summary>
        /// </summary>
        public static Dictionary<string, AudioSample> Samples { get; } = new Dictionary<string, AudioSample>();

        /// <summary>
        ///     Loads an AudioTrack and caches it for later use
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static AudioTrack LoadTrack(string name)
        {
            if (Tracks.ContainsKey(name))
                return Tracks[name];

            var track = new AudioTrack(GameBase.Game.Resources.Get(name));
            Tracks.Add(name, track);

            return track;
        }

        /// <summary>
        ///     Loads an AudioSample and caches it for later use
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static AudioSample LoadSample(string name)
        {
            if (Samples.ContainsKey(name))
                return Samples[name];

            var sample = new AudioSample(GameBase.Game.Resources.Get(name));
            Samples.Add(name, sample);

            return sample;
        }
    }
}