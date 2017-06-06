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
            new MapLine("CHW", StopsFromTrip("CHW_807_V5_M")),
            new MapLine("LAN", StopsFromTrip("LAN_516_V77_M").Take(5).Concat(StopsFromTrip("LAN_6506_V5_M").Skip(4)).ToArray()),
            new MapLine("MED", StopsFromTrip("MED_309_V5_M")),
            new MapLine("FOX", StopsFromTrip("FOX_812_V5_M")),
            new MapLine("NOR", StopsFromTrip("NOR_216_V5_M")),
            new MapLine("PAO", StopsFromTrip("PAO_509_V5_M")),
            new MapLine("CYN", StopsFromTrip("CYN_1055_V5_M")),
            new MapLine("TRE", StopsFromTrip("TRE_705_V5_M")),
            new MapLine("WAR", StopsFromTrip("WAR_408_V5_M")),
            new MapLine("WIL", StopsFromTrip("WIL_9243_V5_M")),
            new MapLine("WTR", new []{ "90406"}.Concat(StopsFromTrip("WTR_6322_V5_M")).ToArray()),
		    //google_bus
		    /* Still need trolleys */
		    new MapLine("16184", StopsFromTrip("554917")), //101
            new MapLine("16186", StopsFromTrip("555422")), //102
            new MapLine("16301", StopsFromTrip("586267")), //BSL
            new MapLine("16301", StopsFromTrip("586164")), //Broad Ridge Spur WARNING the wrong fairmount is being used
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

            var center = new Geo.Geometries.Point(39.952, -75.258);
            if (!File.Exists("../../../geo.png"))
                Files.Download($"https://maps.googleapis.com/maps/api/staticmap?size=640x640&center={center.Coordinate.Latitude},{center.Coordinate.Longitude}&zoom=9&scale=2", "../../../geo.png");
            if (!File.Exists("../../../map.jpg"))
                Files.Download("http://www.septa.org/site/images/system-map-1400-lrg-03.30.17.jpg", "../../../map.jpg");

            var mapNormalizer = Normalizer(0, 1400, 0, 1400);
            //These coordinates are approximately 60 miles by 60 miles
            //The background image is zoomed to about 90 miles by 90 miles
            //Therefore the image is stretched by 1.5 and offset by 0.25
            var geoEdgeLength = new Geo.Measure.Distance(60, Geo.Measure.DistanceUnit.Mile);
            var geoImageEdgeLength = new Geo.Measure.Distance(90, Geo.Measure.DistanceUnit.Mile);
            var geoImageScale = geoImageEdgeLength.SiValue / geoEdgeLength.SiValue;
            var geoImageOffset = 0 - (geoImageScale - 1) / 2;
            var top = Geo.GeoContext.Current.GeodeticCalculator.CalculateOrthodromicLine(center, 0, geoEdgeLength.SiValue/2).Coordinate2.Latitude;
            var right= Geo.GeoContext.Current.GeodeticCalculator.CalculateOrthodromicLine(center, 90, geoEdgeLength.SiValue/2).Coordinate2.Longitude;
            var bottom = Geo.GeoContext.Current.GeodeticCalculator.CalculateOrthodromicLine(center, 180, geoEdgeLength.SiValue/2).Coordinate2.Latitude;
            var left= Geo.GeoContext.Current.GeodeticCalculator.CalculateOrthodromicLine(center, 270, geoEdgeLength.SiValue/2).Coordinate2.Longitude;
            var geoNormalizer = Normalizer(left, right, top, bottom);

            const string duration = "2.5s";

            using (var stream = new FileStream("../../../mapvsgeo.svg", FileMode.Create))
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine("<svg viewBox='0 0 1 1' xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink'>");
                writer.WriteLine($@"
	<image xlink:href='map.jpg' height='1' width='1'>
		<animate id='maptogeo' attributeName='opacity' from='1' to='0' dur='{duration}' begin='0; geotomap.end' />
		<animate id='geotomap' attributeName='opacity' from='0' to='1' dur='{duration}' begin='maptogeo.end' />
	</image>");
                writer.WriteLine($@"
	<image xlink:href='geo.png' height='{geoImageScale}' width='{geoImageScale}' x='{geoImageOffset}' y='{geoImageOffset}'>
		<animate attributeName='opacity' from='0' to='1' dur='{duration}' begin='0; geotomap.end' />
		<animate attributeName='opacity' from='1' to='0' dur='{duration}' begin='maptogeo.end' />
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

            using (var stream = new FileStream("../../../map.svg", FileMode.Create))
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine("<svg viewBox='0 0 1 1' xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink'>");
                writer.WriteLine($@"<image xlink:href='map.jpg' height='1' width='1' />");

                foreach (var mapLine in MapLines)
                {
	                var mapPoints = string.Join(" ", mapLine.Stops.Select(s => mapNormalizer(MapPoints.Get(s))).Select(p => p.X + "," + p.Y));

                    writer.WriteLine($@"<polyline fill='none' stroke-width='0.005' points='{mapPoints}' stroke='#6B93AC' />");
                }

                writer.WriteLine("</svg>");
            }

            using (var stream = new FileStream("../../../geo.svg", FileMode.Create))
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine("<svg viewBox='0 0 1 1' xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink'>");
                writer.WriteLine($@"<image xlink:href='geo.png' height='{geoImageScale}' width='{geoImageScale}' x='{geoImageOffset}' y='{geoImageOffset}' />");

                foreach (var mapLine in MapLines)
                {
                    var geoPoints = string.Join(" ", mapLine.Stops.Select(s => geoNormalizer(GeoPoints[s])).Select(p => p.X + "," + p.Y));

                    writer.WriteLine($@"<polyline fill='none' stroke-width='0.005' points='{geoPoints}' stroke='#{RouteColors[mapLine.RouteId]}' />");
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
