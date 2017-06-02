namespace mapvsgeo
{
	class MapLine
	{
		public string RouteId { get; }
		public string[] Stops { get; }
        
		public MapLine(string routeId, string[] stops)
		{
			this.RouteId = routeId;
			this.Stops = stops;
		}
	}
}