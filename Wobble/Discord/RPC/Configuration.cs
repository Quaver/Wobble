using Newtonsoft.Json;

namespace Wobble.Discord.RPC
{
	public class Configuration
	{
		[JsonProperty("api_endpoint")]
		public string ApiEndpoint { get; set; }

		[JsonProperty("cdn_host")]
		public string CdnHost { get; set; }

		[JsonProperty("enviroment")]
		public string Enviroment { get; set; }
	}
}
