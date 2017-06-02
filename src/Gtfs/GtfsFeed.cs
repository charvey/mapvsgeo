using System.Collections.Generic;

namespace Data.Gtfs
{
	public interface GtfsFeed
	{
		IEnumerable<Calendar> Calendars { get; }
		IEnumerable<CalendarDate> CalendarDates { get; }
		IEnumerable<Route> Routes { get; }
		IEnumerable<Stop> Stops { get; }
		IEnumerable<StopTime> StopTimes { get; }
		IEnumerable<Trip> Trips { get; }
	}
}
