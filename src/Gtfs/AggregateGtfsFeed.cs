using System.Collections.Generic;
using System.Linq;

namespace Data.Gtfs
{
	public class AggregateGtfsFeed : GtfsFeed
	{
		private readonly IEnumerable<GtfsFeed> feeds;

		public AggregateGtfsFeed(IEnumerable<GtfsFeed> feeds)
		{
			this.feeds = feeds;
		}

		public IEnumerable<Calendar> Calendars => feeds.SelectMany(x => x.Calendars);
		public IEnumerable<CalendarDate> CalendarDates => feeds.SelectMany(x => x.CalendarDates);
		public IEnumerable<Route> Routes => feeds.SelectMany(x => x.Routes);
		public IEnumerable<Stop> Stops => feeds.SelectMany(x => x.Stops);
		public IEnumerable<StopTime> StopTimes => feeds.SelectMany(x => x.StopTimes);
		public IEnumerable<Trip> Trips => feeds.SelectMany(x => x.Trips);

		public string Name() => "Instance Aggregate";
	}
}