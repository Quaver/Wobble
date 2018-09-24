using System;

namespace Wobble.Discord.RPC.Exceptions
{
	class BadPresenceException : Exception
	{
		public BadPresenceException(string message) : base(message) { }
	}
}
