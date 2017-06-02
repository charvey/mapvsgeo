using System;
using CsvHelper.Configuration;
using System.Globalization;

namespace Data.Gtfs
{
	internal sealed class CalendarMap : CsvClassMap<Calendar>
	{
		public CalendarMap()
		{
			Map(c => c.ServiceId).Name("service_id");
			Map(c => c.Monday).ConvertUsing(r => r.GetField<int>("monday") == 1);
			Map(c => c.Tuesday).ConvertUsing(r => r.GetField<int>("tuesday") == 1);
			Map(c => c.Wednesday).ConvertUsing(r => r.GetField<int>("wednesday") == 1);
			Map(c => c.Thursday).ConvertUsing(r => r.GetField<int>("thursday") == 1);
			Map(c => c.Friday).ConvertUsing(r => r.GetField<int>("friday") == 1);
			Map(c => c.Saturday).ConvertUsing(r => r.GetField<int>("saturday") == 1);
			Map(c => c.Sunday).ConvertUsing(r => r.GetField<int>("sunday") == 1);
			Map(c => c.StartDate).ConvertUsing(r => DateTime.ParseExact(r.GetField("start_date"), "yyyyMMdd", CultureInfo.InvariantCulture));
			Map(c => c.EndDate).ConvertUsing(r => DateTime.ParseExact(r.GetField("end_date"), "yyyyMMdd", CultureInfo.InvariantCulture));
		}
	}
}
