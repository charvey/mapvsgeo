using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Data.Gtfs;

namespace mapvsgeo
{
    class Program
    {
	    private static List<MapLine> MapLines => new List<MapLine>
	    {
		    //google_rail
		    new MapLine("AIR", StopsFromTrip("AIR_401_V1_M")),
		    new MapLine("CHE", StopsFromTrip("CHE_5712_V5_M")),
		    new MapLine("CHW", new string[0]),
		    new MapLine("LAN", StopsFromTrip("LAN_516_V77_M").Take(5).Concat(StopsFromTrip("LAN_6506_V5_M").Skip(4)).ToArray()),
		    new MapLine("MED", new string[0]),
		    new MapLine("FOX", new string[0]),
		    new MapLine("NOR", StopsFromTrip("NOR_216_V5_M")),
		    new MapLine("PAO", StopsFromTrip("PAO_509_V5_M")),
		    new MapLine("CYN", new string[0]),
		    new MapLine("TRE", StopsFromTrip("TRE_705_V5_M")),
		    new MapLine("WAR", new string[0]),
		    new MapLine("WIL", StopsFromTrip("WIL_9243_V5_M")),
		    new MapLine("WTR", new string[0]),
		    //google_bus
		    /* Still need trolleys and chinatown subway */
		    new MapLine("16184", new string[0]), //101
		    new MapLine("16186", new string[0]), //102
		    new MapLine("16301", new string[0]), //BSL
		    new MapLine("16303", StopsFromTrip("588669")), //MFL
		    new MapLine("16210", StopsFromTrip("666614")), //NHSL
		    ////patco
		    //new MapLine("1", new string[0]) //PATCO
	    };

	    public static Lazy<GtfsFeed> feed = new Lazy<GtfsFeed>(() => new AggregateGtfsFeed(new[]
	    {
		    new ZipFileGtfsFeed("gtfs_public/google_rail.zip"),
		    new ZipFileGtfsFeed("gtfs_public/google_bus.zip"),
		    //new ZipFileGtfsFeed("PortAuthorityTransitCorporation.zip")
	    }));

	    private static Lazy<IReadOnlyList<StopTime>> stopTimes = new Lazy<IReadOnlyList<StopTime>>(() => feed.Value.StopTimes.ToList());

	    private static string[] StopsFromTrip(string tripId)
	    {
		    return stopTimes.Value
				.Where(st => st.TripId == tripId)
			    .OrderBy(st => st.StopSequence)
				.Select(s => s.StopId)
				.ToArray();
	    }

	    private static Dictionary<string, Point> GeoPoints => feed.Value.Stops.ToDictionary(s => s.StopId, s => new Point(s.StopLongitude, s.StopLatitude));

	    private static Dictionary<string, string> RouteColors => feed.Value.Routes.ToDictionary(r => r.RouteId, r => r.RouteColor);

        static void Main(string[] args)
        {
			Files.Foo();

            var mapNormalizer = Normalizer(0, 1400, 0, 1400);
            var geoNormalizer = Normalizer(
                GeoPoints.Values.Min(p => p.X), GeoPoints.Values.Max(p => p.X),
                GeoPoints.Values.Max(p => p.Y), GeoPoints.Values.Min(p => p.Y)
            );

	        const string duration = "2.5s";

            using (var stream = new FileStream("../../../map.svg", FileMode.Create))
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine("<svg viewBox='0 0 1 1' xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink'>");
                writer.WriteLine($@"
	<image xlink:href='map.jpg' height='1' width='1'>
		<animate id='maptogeo' attributeName='opacity' from='1' to='0' dur='{duration}' begin='0; geotomap.end' />
		<animate id='geotomap' attributeName='opacity' from='0' to='1' dur='{duration}' begin='maptogeo.end' />
	</image>");

                foreach (var mapLine in MapLines)
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
			from='#6B93AC' to='#{RouteColors[mapLine.RouteId]}' />
		<animate attributeName='stroke' dur='{duration}' fill='freeze' begin='maptogeo.end'
			from='#{RouteColors[mapLine.RouteId]}' to='#6B93AC' />
	</polyline>");
                }

                writer.WriteLine("</svg>");
            }

            using (var stream = new FileStream("../../../geomatch.svg", FileMode.Create))
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine("<svg viewBox='0 0 1 1' xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink'>");
                writer.WriteLine($@"
	<image xlink:href='geo2.jpg' height='1' width='1'>
	</image>");

                foreach (var mapLine in MapLines)
                {
                    var geoPoints = string.Join(" ", mapLine.Stops.Select(s => geoNormalizer(GeoPoints[s])).Select(p => p.X + "," + p.Y));

                    writer.WriteLine($@"
	<polyline fill='none' stroke-width='0.005'
        points='{geoPoints}' stroke='#{RouteColors[mapLine.RouteId]}' />");
                }

                writer.WriteLine("</svg>");
            }

            using (var stream = new FileStream("../../../debug.svg", FileMode.Create))
	        using (var writer = new StreamWriter(stream))
	        {
		        writer.WriteLine("<svg viewBox='0 0 1 1' xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink'>");
		        writer.WriteLine("\t<image xlink:href='map.jpg' height='1' width='1'/>");

		        foreach (var stop in MapLines.SelectMany(r => r.Stops).Distinct())
		        {
			        var point = mapNormalizer(MapPoints.Get(stop));

					writer.WriteLine($"\t<circle cx='{point.X}' cy='{point.Y}'  r='0.005' stroke='black' stroke-width='0.001' fill='none'/>");
			        //writer.WriteLine($"\t<text x='{point.X + 0.05}' fill='orange' y='{point.Y}'>{StopNames.Get(stop)}</text>");
			        //writer.WriteLine($"\t<text x='{point.X - 0.05}' fill='orange' y='{point.Y}'>{stop}</text>");
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
