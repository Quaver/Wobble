using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ManagedBass;

namespace Wobble.Audio
{
    public static class AudioEngine
    {
        /// <summary>
        ///     Initializes BASS and throws an exception if it fails.
        /// </summary>
        internal static void Initialize()
        {
            if (!Bass.Init())
                throw new AudioEngineException("BASS has failed to initialize! Are your platform-specific dlls present?");
        }

        /// <summary>
        ///     Disposes of any resources used by BASS.
        /// </summary>
        internal static void Dispose() => Bass.Free();
    }
}
