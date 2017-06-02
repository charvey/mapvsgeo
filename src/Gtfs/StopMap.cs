using CsvHelper.Configuration;

namespace Data.Gtfs
{
	internal sealed class StopMap : CsvClassMap<Stop>
	{
		public StopMap()
		{
			Map(s => s.StopId).Name("stop_id");
			Map(s => s.StopName).Name("stop_name");
			Map(s => s.StopDescription).Name("stop_desc");
			Map(s => s.StopLatitude).Name("stop_lat");
			Map(s => s.StopLongitude).Name("stop_lon");
		}
	}
}
