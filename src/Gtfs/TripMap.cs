using CsvHelper.Configuration;

namespace Data.Gtfs
{
	internal sealed class TripMap : CsvClassMap<Trip>
	{
		public TripMap()
		{
			Map(t => t.RouteId).Name("route_id");
			Map(t => t.ServiceId).Name("service_id");
			Map(t => t.TripId).Name("trip_id");
		}
	}
}
