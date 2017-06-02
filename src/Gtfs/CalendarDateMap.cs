using System;
using CsvHelper.Configuration;
using System.Globalization;

namespace Data.Gtfs
{
	internal sealed class CalendarDateMap : CsvClassMap<CalendarDate>
	{
		public CalendarDateMap()
		{
			Map(cd => cd.ServiceId).Name("service_id");
			Map(cd => cd.Date).ConvertUsing(r => DateTime.ParseExact(r.GetField("date"), "yyyyMMdd", CultureInfo.InvariantCulture));
			Map(cd => cd.ExceptionType).ConvertUsing(r => (ExceptionType)r.GetField<int>("exception_type"));
		}
	}
}
