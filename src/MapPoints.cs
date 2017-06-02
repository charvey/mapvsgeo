using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mapvsgeo
{
	public static class MapPoints
	{
		private const string filename = "../../../data/MapPoints.csv";
		private static Dictionary<string, Point> data = null;

		public static Dictionary<string, Point> Get()
		{
			if (data == null)
				data = File.ReadAllLines(filename)
					.Select(l => l.Split(','))
					.ToDictionary(l => l[0], l => new Point(int.Parse(l[1]), int.Parse(l[2])));
			return data;
		}

		public static Point Get(string stopId)
		{
			while (true)
			{
				var points = Get();

				if (points.ContainsKey(stopId))
					return points[stopId];

				Console.WriteLine($"Can't find {StopNames.Get(stopId)} ({stopId})");
				var l = Console.ReadLine().Split(' ');
				Add(stopId, int.Parse(l[0]), int.Parse(l[1]));
			}
		}

		private static void Add(string stopId, int x, int y)
		{
			File.AppendAllText(filename, "\n" + stopId + "," + x + "," + y);
			data = null;
		}
	}
}
