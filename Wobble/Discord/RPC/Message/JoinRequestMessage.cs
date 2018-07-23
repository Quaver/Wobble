using Newtonsoft.Json;

namespace Wobble.Discord.RPC.Message
{
	/// <summary>
	/// Called when some other person has requested access to this game. C -> D -> C.
	/// </summary>
	public class JoinRequestMessage : IMessage
	{
		public override MessageType Type { get { return MessageType.JoinRequest; } }

		/// <summary>
		/// The discord user that is requesting access.
		/// </summary>
		[JsonProperty("user")]
		public User User { get; internal set; }
	}
}
