using Wobble.Discord.RPC.RPC.Payload;

namespace Wobble.Discord.RPC.RPC.Commands
{
	interface ICommand
	{
		IPayload PreparePayload(long nonce);
	}
}
