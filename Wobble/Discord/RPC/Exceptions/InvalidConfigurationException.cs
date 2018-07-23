using System;

namespace Wobble.Discord.RPC.Exceptions
{
	class InvalidConfigurationException : Exception
	{
		public InvalidConfigurationException(string message) : base(message) { }
	}
}
