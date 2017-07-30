using Data.Gtfs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace mapvsgeo
{
	class Program
	{
		private static List<MapLine> LosAngelesMapLines()
		{
			var feed = new ZipFileGtfsFeed("LACMTA.zip");

			return new List<MapLine>
			{
				//new MapLine("805",ColorFromRoute(feed,"805"),StopsFromTrip(feed,"43493095"))
			};
		}

		private static string ColorFromRoute(GtfsFeed feed, string routeId)
		{
			return feed.Routes.Single(r => r.RouteId == routeId).RouteColor;
		}

		private static List<MapLine> PhiladelphiaMapLines()
		{
			var feed = new AggregateGtfsFeed(new[]
			{
				new ZipFileGtfsFeed("gtfs_public/google_rail.zip"),
				new ZipFileGtfsFeed("gtfs_public/google_bus.zip"),
				//new ZipFileGtfsFeed("PortAuthorityTransitCorporation.zip")
			});
			return new List<MapLine>
			{
				//google_rail
				new MapLine("AIR",ColorFromRoute(feed,"AIR"), StopsFromTrip(feed,"AIR_401_V1_M")),
				new MapLine("CHE",ColorFromRoute(feed,"CHE"), StopsFromTrip(feed,"CHE_5712_V5_M")),
				new MapLine("CHW",ColorFromRoute(feed,"CHW"), StopsFromTrip(feed,"CHW_807_V5_M")),
				new MapLine("LAN",ColorFromRoute(feed,"LAN"), StopsFromTrip(feed,"LAN_516_V77_M").Take(5).Concat(StopsFromTrip(feed,"LAN_6506_V5_M").Skip(4)).ToArray()),
				new MapLine("MED",ColorFromRoute(feed,"MED"), StopsFromTrip(feed,"MED_309_V5_M")),
				new MapLine("FOX",ColorFromRoute(feed,"FOX"), StopsFromTrip(feed,"FOX_812_V5_M")),
				new MapLine("NOR",ColorFromRoute(feed,"NOR"), StopsFromTrip(feed,"NOR_216_V5_M")),
				new MapLine("PAO",ColorFromRoute(feed,"PAO"), StopsFromTrip(feed,"PAO_509_V5_M")),
				new MapLine("CYN",ColorFromRoute(feed,"CYN"), StopsFromTrip(feed,"CYN_1055_V5_M")),
				new MapLine("TRE",ColorFromRoute(feed,"TRE"), StopsFromTrip(feed,"TRE_705_V5_M")),
				new MapLine("WAR",ColorFromRoute(feed,"WAR"), StopsFromTrip(feed,"WAR_408_V5_M")),
				new MapLine("WIL",ColorFromRoute(feed,"WIL"), StopsFromTrip(feed,"WIL_9243_V5_M")),
				new MapLine("WTR",ColorFromRoute(feed,"WTR"), new []{ "90406"}.Concat(StopsFromTrip(feed,"WTR_6322_V5_M")).ToArray()),
				//google_bus
				/* Still need trolleys */
				new MapLine("16184",ColorFromRoute(feed,"16184"), StopsFromTrip(feed,"554917")), //101
				new MapLine("16186",ColorFromRoute(feed,"16186"), StopsFromTrip(feed,"555422")), //102
				new MapLine("16301",ColorFromRoute(feed,"16301"), StopsFromTrip(feed,"586267")), //BSL
				new MapLine("16301",ColorFromRoute(feed,"16301"), StopsFromTrip(feed,"586164")), //Broad Ridge Spur WARNING the wrong fairmount is being used
				new MapLine("16303",ColorFromRoute(feed,"16303"), StopsFromTrip(feed,"588669")), //MFL
				new MapLine("16210",ColorFromRoute(feed,"16210"), StopsFromTrip(feed,"666614")), //NHSL
				////patco
				//new MapLine("1",ColorFromRoute(feed,"1"), new string[0]) //PATCO
			};
		}

		public static Lazy<GtfsFeed> feed = new Lazy<GtfsFeed>(() => new AggregateGtfsFeed(new[]
		{
			new ZipFileGtfsFeed("gtfs_public/google_rail.zip"),
			new ZipFileGtfsFeed("gtfs_public/google_bus.zip"),
		    //new ZipFileGtfsFeed("PortAuthorityTransitCorporation.zip")
	    }));

		private static readonly Dictionary<GtfsFeed, Dictionary<string, StopTime[]>> stopTimeCache = new Dictionary<GtfsFeed, Dictionary<string, StopTime[]>>();

		private static string[] StopsFromTrip(GtfsFeed feed, string tripId)
		{
			if (!stopTimeCache.ContainsKey(feed))
				stopTimeCache[feed] = feed.StopTimes
						.GroupBy(st => st.TripId)
						.ToDictionary(x => x.Key, x => x.OrderBy(st => st.StopSequence).ToArray());

			return stopTimeCache[feed][tripId].Select(st => st.StopId).ToArray();
		}

		private static Dictionary<string, Point> GeoPoints => feed.Value.Stops.ToDictionary(s => s.StopId, s => new Point(s.StopLongitude, s.StopLatitude));

		static void Main(string[] args)
		{
			Files.Foo();

			var outputDirectory = "../../../";

			LosAngeles(outputDirectory: outputDirectory);
			Philadelphia(outputDirectory: outputDirectory);
		}
		static void LosAngeles(string outputDirectory)
		{
			General(
				outputDirectory: outputDirectory,
				name: "Los Angeles",
				mapImage: "http://s3-us-west-2.amazonaws.com/media.thesource.metro.net/wp-content/uploads/2016/09/07152145/rail_map.jpg",
				center: new Geo.Geometries.Point(34.039, -118.264),
				mapLines: LosAngelesMapLines()
			);
		}
		static void Philadelphia(string outputDirectory)
		{
			General(
				outputDirectory: outputDirectory,
				name: "Philadelphia",
				mapImage: "http://www.septa.org/site/images/system-map-1400-lrg-03.30.17.jpg",
				center: new Geo.Geometries.Point(39.952, -75.258),
				mapLines: PhiladelphiaMapLines()
			);
		}
		static void General(string outputDirectory, string name, string mapImage,
			Geo.Geometries.Point center, IReadOnlyList<MapLine> mapLines)
		{
			var mapNormalizer = Normalizer(0, 1400, 0, 1400);
			//These coordinates are approximately 60 miles by 60 miles
			//The background image is zoomed to about 90 miles by 90 miles
			//Therefore the image is stretched by 1.5 and offset by 0.25
			var geoEdgeLength = new Geo.Measure.Distance(60, Geo.Measure.DistanceUnit.Mile);
			var geoImageEdgeLength = new Geo.Measure.Distance(90, Geo.Measure.DistanceUnit.Mile);
			var geoImageScale = geoImageEdgeLength.SiValue / geoEdgeLength.SiValue;
			var geoImageOffset = 0 - (geoImageScale - 1) / 2;
			var top = Geo.GeoContext.Current.GeodeticCalculator.CalculateOrthodromicLine(center, 0, geoEdgeLength.SiValue / 2).Coordinate2.Latitude;
			var right = Geo.GeoContext.Current.GeodeticCalculator.CalculateOrthodromicLine(center, 90, geoEdgeLength.SiValue / 2).Coordinate2.Longitude;
			var bottom = Geo.GeoContext.Current.GeodeticCalculator.CalculateOrthodromicLine(center, 180, geoEdgeLength.SiValue / 2).Coordinate2.Latitude;
			var left = Geo.GeoContext.Current.GeodeticCalculator.CalculateOrthodromicLine(center, 270, geoEdgeLength.SiValue / 2).Coordinate2.Longitude;
			var geoNormalizer = Normalizer(left, right, top, bottom);

			const string duration = "2.5s";

			var geoImage = WebUtility.HtmlEncode($"http://maps.googleapis.com/maps/api/staticmap?size=640x640&center={center.Coordinate.Latitude},{center.Coordinate.Longitude}&zoom=9&scale=2");

			using (var stream = new FileStream(Path.Combine(outputDirectory, $"{name} mapvsgeo.svg"), FileMode.Create))
			using (var writer = new StreamWriter(stream))
			{
				writer.WriteLine("<svg viewBox='0 0 1 1' xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink'>");
				writer.WriteLine($@"
	<image xlink:href='{mapImage}' height='1' width='1'>
		<animate id='maptogeo' attributeName='opacity' from='1' to='0' dur='{duration}' begin='0; geotomap.end' />
		<animate id='geotomap' attributeName='opacity' from='0' to='1' dur='{duration}' begin='maptogeo.end' />
	</image>");
				writer.WriteLine($@"
	<image xlink:href='{geoImage}' height='{geoImageScale}' width='{geoImageScale}' x='{geoImageOffset}' y='{geoImageOffset}'>
		<animate attributeName='opacity' from='0' to='1' dur='{duration}' begin='0; geotomap.end' />
		<animate attributeName='opacity' from='1' to='0' dur='{duration}' begin='maptogeo.end' />
	</image>");

				foreach (var mapLine in mapLines)
				{
					var mapPoints = string.Join(" ", mapLine.Stops.Select(s => mapNormalizer(MapPoints.Get(s))).Select(p => p.X + "," + p.Y));
					var geoPoints = string.Join(" ", mapLine.Stops.Select(s => geoNormalizer(GeoPoints[s])).Select(p => p.X + "," + p.Y));

					writer.WriteLine($@"
	<polyline fill='none' stroke-width='0.005'>
		<animate attributeName='points' dur='{duration}' fill='freeze' begin='0; geotomap.end'
			from='{mapPoints}'
			to='{geoPoints}'
		/>
		<animate attributeName='points' dur='{duration}' fill='freeze' begin='maptogeo.end'
			from='{geoPoints}'
			to='{mapPoints}'
		/>
		<animate attributeName='stroke' dur='{duration}' fill='freeze' begin='0; geotomap.end'
			from='#6B93AC' to='#{mapLine.Color}' />
		<animate attributeName='stroke' dur='{duration}' fill='freeze' begin='maptogeo.end'
			from='#{mapLine.Color}' to='#6B93AC' />
	</polyline>");
				}

				writer.WriteLine("</svg>");
			}

			using (var stream = new FileStream(Path.Combine(outputDirectory, $"{name} map.svg"), FileMode.Create))
			using (var writer = new StreamWriter(stream))
			{
				writer.WriteLine("<svg viewBox='0 0 1 1' xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink'>");
				writer.WriteLine($@"<image xlink:href='{mapImage}' height='1' width='1' />");

				foreach (var mapLine in mapLines)
				{
					var mapPoints = string.Join(" ", mapLine.Stops.Select(s => mapNormalizer(MapPoints.Get(s))).Select(p => p.X + "," + p.Y));

					writer.WriteLine($@"<polyline fill='none' stroke-width='0.005' points='{mapPoints}' stroke='#6B93AC' />");
				}

				writer.WriteLine("</svg>");
			}

			using (var stream = new FileStream(Path.Combine(outputDirectory, $"{name} geo.svg"), FileMode.Create))
			using (var writer = new StreamWriter(stream))
			{
				writer.WriteLine("<svg viewBox='0 0 1 1' xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink'>");
				writer.WriteLine($@"<image xlink:href='{geoImage}' height='{geoImageScale}' width='{geoImageScale}' x='{geoImageOffset}' y='{geoImageOffset}' />");

				foreach (var mapLine in mapLines)
				{
					var geoPoints = string.Join(" ", mapLine.Stops.Select(s => geoNormalizer(GeoPoints[s])).Select(p => p.X + "," + p.Y));

					writer.WriteLine($@"<polyline fill='none' stroke-width='0.005' points='{geoPoints}' stroke='#{mapLine.Color}' />");
				}

				writer.WriteLine("</svg>");
			}

			using (var stream = new FileStream(Path.Combine(outputDirectory, $"{name} debug.svg"), FileMode.Create))
			using (var writer = new StreamWriter(stream))
			{
				writer.WriteLine("<svg viewBox='0 0 1 1' xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink'>");
				writer.WriteLine($"\t<image xlink:href='{mapImage}' height='1' width='1'/>");

				foreach (var stop in mapLines.SelectMany(r => r.Stops).Distinct())
				{
					var point = mapNormalizer(MapPoints.Get(stop));

					writer.WriteLine($"\t<circle cx='{point.X}' cy='{point.Y}'  r='0.005' stroke='black' stroke-width='0.001' fill='none'/>");
				}

				writer.WriteLine("</svg>");
			}
		}

		static Func<Point, Point> Normalizer(double leftX, double rightX, double topY, double bottomY)
		{
			return p => Normalize(p, leftX, rightX, topY, bottomY);
		}

		static Point Normalize(Point p, double leftX, double rightX, double topY, double bottomY)
		{
			return new Point(
				(p.X - leftX) / (rightX - leftX),
				(p.Y - topY) / (bottomY - topY)
			);
		}
	}
}
