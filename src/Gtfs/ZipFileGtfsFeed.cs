using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Data.Gtfs
{
	internal class ZipFileGtfsFeed : GtfsFeed
	{
		private readonly ZipArchive zipArchive;

		public ZipFileGtfsFeed(string filename)
		{
			zipArchive = ZipFile.OpenRead(filename);
		}

		public IEnumerable<Calendar> Calendars => Read("calendar.txt", new CalendarMap());
		public IEnumerable<CalendarDate> CalendarDates => Read("calendar_dates.txt", new CalendarDateMap());
		public IEnumerable<Route> Routes => Read("routes.txt", new RouteMap());
		public IEnumerable<Stop> Stops => Read("stops.txt", new StopMap());
		public IEnumerable<StopTime> StopTimes => Read("stop_times.txt", new StopTimeMap());
		public IEnumerable<Trip> Trips => Read("trips.txt", new TripMap());

		private IReadOnlyList<T> Read<T>(string filename, CsvClassMap<T> map)
		{
			using (var stream = zipArchive.GetEntry(filename).Open())
			using (var reader = new StreamReader(stream))
			using (var csvReader = new CsvReader(reader))
			{
				csvReader.Configuration.TrimFields = true;
				csvReader.Configuration.TrimHeaders = true;
				csvReader.Configuration.WillThrowOnMissingField = false;
				csvReader.Configuration.RegisterClassMap(map);
				return csvReader.GetRecords<T>().ToList();
			}
		}
	}
}
