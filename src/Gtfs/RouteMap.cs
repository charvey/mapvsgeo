using CsvHelper.Configuration;

namespace Data.Gtfs
{
	internal sealed class RouteMap : CsvClassMap<Route>
	{
		public RouteMap()
		{
			Map(r => r.RouteId).Name("route_id");
			Map(r => r.RouteColor).Name("route_color");
		}
	}
}
