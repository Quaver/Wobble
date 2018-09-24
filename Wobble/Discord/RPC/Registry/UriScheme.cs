using System.Diagnostics;

namespace Wobble.Discord.RPC.Registry
{
	internal static class UriScheme
	{	
		/// <summary>
		/// Gets the current location of the app
		/// </summary>
		/// <returns></returns>
		public static string GetApplicationLocation()
		{
			return Process.GetCurrentProcess().MainModule.FileName;
		}
	}
}
