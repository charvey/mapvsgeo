using System.Collections.Generic;

namespace mapvsgeo
{
	public class ComparisonDetails
	{
		public string Name { get; set; }
		public Geo.Geometries.Point Center { get; set; }
		public string MapImage { get; set; }
		public IReadOnlyList<MapLine> MapLines { get; set; }
	}
}