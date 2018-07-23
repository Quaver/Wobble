using System;

namespace Wobble.Graphics.Shaders
{
    public class InvalidTypeParameterException : Exception
    {
        public InvalidTypeParameterException(string message) : base(message)
        {
        }
    }
}