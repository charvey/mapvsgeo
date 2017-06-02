using System;

namespace Data.Gtfs
{
	public class StopTime
	{
		public string TripId { get; set; }
		public TimeSpan ArrivalTime { get; set; }
		public TimeSpan DepartureTime { get; set; }
		public string StopId { get; set; }
		public int StopSequence { get; set; }
	}
}
