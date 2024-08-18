using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wobble.Audio
{
    /// <inheritdoc />
    /// <summary>
    ///     An exception thrown when an illegal action is called during
    ///     audio engine usage.
    /// </summary>
    public class AudioEngineException : Exception
    {
        public AudioEngineException() { }
        public AudioEngineException(string message) : base(message) { }
    }
}
