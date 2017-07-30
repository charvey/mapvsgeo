namespace mapvsgeo
{
	class MapLine
	{
		public string RouteId { get; }
		public string Color { get; }
		public string[] Stops { get; }

		public MapLine(string routeId, string color, string[] stops)
		{
			this.RouteId = routeId;
			this.Color = color;
			this.Stops = stops;
		}
	}
}