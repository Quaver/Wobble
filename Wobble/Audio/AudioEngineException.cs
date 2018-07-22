using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wobble.Audio
{
    public class AudioEngineException : Exception
    {
        public AudioEngineException() {}
        public AudioEngineException(string message) : base(message) {}
    }
}
