namespace Wobble.Discord.RPC.Message
{
	/// <summary>
	/// Failed to establish any connection with discord. Discord is potentially not running?
	/// </summary>
	public class ConnectionFailedMessage : IMessage
	{
		public override MessageType Type { get { return MessageType.ConnectionFailed; } }

		/// <summary>
		/// The pipe we failed to connect too.
		/// </summary>
		public int FailedPipe { get; internal set; }
	}
}
