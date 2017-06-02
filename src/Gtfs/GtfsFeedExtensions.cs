using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.Gtfs
{
	public static class GtfsFeedExtensions
	{
		public static string Name(this GtfsFeed feed) => "Generic";

		public static IEnumerable<string> GetApplicableServiceIds(this GtfsFeed feed, DateTime now)
		{
			return feed.Calendars
				.Where(c => c.StartDate <= now && now <= c.EndDate)
				.Where(c => CalendarApplies(now.DayOfWeek, c))
				.Select(c => c.ServiceId)
				.Except(feed.CalendarDates.Where(c => c.Date == now.Date && c.ExceptionType == ExceptionType.Remove).Select(c => c.ServiceId))
				.Concat(feed.CalendarDates.Where(c => c.Date == now.Date && c.ExceptionType == ExceptionType.Add).Select(c => c.ServiceId))
				.Distinct();
		}

		private static bool CalendarApplies(DayOfWeek dayOfWeek, Calendar calendar)
		{
			switch (dayOfWeek)
			{
				case DayOfWeek.Sunday: return calendar.Sunday;
				case DayOfWeek.Monday: return calendar.Monday;
				case DayOfWeek.Tuesday: return calendar.Tuesday;
				case DayOfWeek.Wednesday: return calendar.Wednesday;
				case DayOfWeek.Thursday: return calendar.Thursday;
				case DayOfWeek.Friday: return calendar.Friday;
				case DayOfWeek.Saturday: return calendar.Saturday;
				default: throw new ArgumentOutOfRangeException(nameof(dayOfWeek));
			}
		}
	}
}
