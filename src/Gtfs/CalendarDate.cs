using System;

namespace Data.Gtfs
{
	public class CalendarDate
	{
		public string ServiceId { get; set; }
		public DateTime Date { get; set; }
		public ExceptionType ExceptionType { get; set; }
	}

	public enum ExceptionType
	{
		Add = 1, Remove = 2
	}
}