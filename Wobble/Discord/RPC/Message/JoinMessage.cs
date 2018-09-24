using Newtonsoft.Json;

namespace Wobble.Discord.RPC.Message
{
	/// <summary>
	/// Called when the Discord Client wishes for this process to join a game. D -> C.
	/// </summary>
	public class JoinMessage : IMessage
	{
		public override MessageType Type { get { return MessageType.Join; } }

		/// <summary>
		/// The <see cref="Secrets.JoinSecret" /> to connect with. 
		/// </summary>
		[JsonProperty("secret")]
		public string Secret { get; internal set; }

		public JoinMessage() { }

	}
}
