using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mapvsgeo
{
    class Program
    {
        static List<Route> Routes = new List<Route>
        {
            new Route("NOR",new[]{"90004","90005","90006","90007","90008","90218","90219","90220","90221","90222","90223","90224","90225","90226","90227","90228" }),
            new Route("PAO",new[]{"90006","90005","90004","90522","90521","90520","90519","90518","90517","90516","90515","90514","90513","90512","90511","90510","90509","90508","90507","90506","90505","90504","90503","90502","90501"})
        };

        static Dictionary<string, Point> MapPoints = new Dictionary<string, Point>
        {
            {"90004",new Point(740,762)},
            {"90005",new Point(822,762)},
            {"90006",new Point(932,762)},
            {"90007",new Point(924,644)},
            {"90008",new Point(904,626)},
            {"90218",new Point(599,612)},
            {"90219",new Point(580,590)},
            {"90220",new Point(560,572)},
            {"90221",new Point(540,552)},
            {"90222",new Point(520,532)},
            {"90223",new Point(500,512)},
            {"90224",new Point(480,494)},
            {"90225",new Point(461,474)},
            {"90226",new Point(416,430)},
            {"90227",new Point(392,402)},
            {"90228",new Point(358,366)},

            {"90522",new Point(460,719)},
            {"90521",new Point(444,703)},
            {"90520",new Point(426,684)},
            {"90519",new Point(412,669)},
            {"90518",new Point(394,653)},
            {"90517",new Point(380,637)},
            {"90516",new Point(364,619)},
            {"90515",new Point(348,602)},
            {"90514",new Point(332,587)},
            {"90513",new Point(304,556)},
            {"90512",new Point(288,540)},
            {"90511",new Point(276,529)},
            {"90510",new Point(264,516)},
            {"90509",new Point(252,504)},
            {"90508",new Point(240,491)},
            {"90507",new Point(226,477)},
            {"90506",new Point(216,466)},
            {"90505",new Point(204,453)},
            {"90504",new Point(192,440)},
            {"90503",new Point(180,427)},
            {"90502",new Point(166,414)},
            {"90501",new Point(148,395)}
        };

        static Dictionary<string, Point> GeoPoints = File.ReadAllLines("google_rail/stops.txt").Skip(1)
            .Select(l => l.Split(','))
            .ToDictionary(l => l[0], l => new Point(double.Parse(l[4]), double.Parse(l[3])));

        static Dictionary<string, string> StopNames = File.ReadAllLines("google_rail/stops.txt").Skip(1)
            .Select(l => l.Split(',')).ToDictionary(l => l[0], l => l[1]);

        static Dictionary<string, string> RouteColors = File.ReadAllLines("google_rail/routes.txt").Skip(1)
            .Select(l => l.Split(',')).ToDictionary(l => l[0], l => l[6]);

        static void Main(string[] args)
        {
            var mapNormalizer = Normalizer(0, 1400, 0, 1400);
            var normalizedMapPoints = MapPoints.ToDictionary(x => x.Key, x => mapNormalizer(x.Value));
            var geoNormalizer = Normalizer(
                GeoPoints.Values.Min(p => p.X), GeoPoints.Values.Max(p => p.X),
                GeoPoints.Values.Max(p => p.Y), GeoPoints.Values.Min(p => p.Y)
            );
            var normalizedGeoPoints = GeoPoints.ToDictionary(x => x.Key, x => geoNormalizer(x.Value));

            using (var stream = new FileStream("../../../map.svg", FileMode.Create))
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine("<svg viewBox='0 0 1 1' xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink'>");
                writer.WriteLine("\t<image xlink:href='map.jpg' height='1' width='1'/>");

                foreach (var route in Routes)
                {
                    var missing = route.Stops.Where(s => !normalizedMapPoints.ContainsKey(s));
                    if (missing.Any())
                    {
                        foreach (var x in missing)
                            Console.WriteLine($"Find coordinates for {StopNames[x]} ({x})");
                        continue;
                    }

                    var mapPoints = string.Join(" ", route.Stops.Select(s => normalizedMapPoints[s]).Select(p => p.X + "," + p.Y));
                    var geoPoints = string.Join(" ", route.Stops.Select(s => normalizedGeoPoints[s]).Select(p => p.X + "," + p.Y));

                    writer.WriteLine($@"
<polyline fill='none' stroke-width='0.005'>
    <animate id='{route.Name}maptogeo' attributeName='points'
        dur='5s' fill='freeze'
        begin='0; {route.Name}geotomap.end'
        from='{mapPoints}'
        to='{geoPoints}'
    />
    <animate id='{route.Name}geotomap' attributeName='points'
        dur='5s' fill='freeze'
        begin='{route.Name}maptogeo.end'
        from='{geoPoints}'
        to='{mapPoints}'
    />
    <animate id='{route.Name}maptogeocolor' attributeName='stroke'
        from='#6b93ac' to='#{RouteColors[route.Name]}' dur='5s' fill='freeze' begin='0; {route.Name}maptogeocolor.end' />
    <animate id='{route.Name}geotomapcolor' attributeName='stroke'
        from='#{RouteColors[route.Name]}' to='#6b93ac' dur='5s' fill='freeze' begin='{route.Name}maptogeocolor.end' />
</polyline>");
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

    class Route
    {
        public string Name { get; }
        public string[] Stops { get; }
        
        public Route(string name, string[] stops)
        {
            this.Name = name;
            this.Stops = stops;
        }
    }

    class Point
    {
        public double X { get; }
        public double Y { get; }

        public Point(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}
