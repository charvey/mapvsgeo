using System;
using CsvHelper.Configuration;
using System.Linq;

namespace Data.Gtfs
{
	internal sealed class StopTimeMap : CsvClassMap<StopTime>
	{
		public StopTimeMap()
		{
			Map(st => st.TripId).Name("trip_id");
			Map(st => st.ArrivalTime).ConvertUsing(r => ParseTime(r.GetField("arrival_time")));
			Map(st => st.DepartureTime).ConvertUsing(r => ParseTime(r.GetField("departure_time")));
			Map(st => st.StopId).Name("stop_id");
			Map(st => st.StopSequence).Name("stop_sequence");
		}

		private static TimeSpan ParseTime(string time)
		{
			var parts = time.Split(':').Select(int.Parse).ToArray();
			return TimeSpan.FromHours(parts[0])
			       + TimeSpan.FromMinutes(parts[1])
			       + TimeSpan.FromSeconds(parts[2]);
		}
	}
}
