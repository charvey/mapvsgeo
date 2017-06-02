using System.Collections.Generic;
using System.Linq;

namespace mapvsgeo
{
	static class StopNames
	{
		private static Dictionary<string, string> data = Program.feed.Value.Stops.ToDictionary(s => s.StopId, s => s.StopName);

		public static string Get(string stopId) => data[stopId];
	}
}
