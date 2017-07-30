using mapvsgeo;
using System;
using System.Collections.Generic;
using System.IO;

namespace mapvsgeo
{
	public class PageBuilder
	{
		public void BuildPage(string outputDirectory, IEnumerable<ComparisonDetails> comparisons)
		{
			using (var stream = new FileStream(Path.Combine(outputDirectory, "index.html"), FileMode.Create))
			using (var writer = new StreamWriter(stream))
			{
				writer.WriteLine(@"
<!DOCTYPE html>
<html lang='en'>
	<head>
		<title>Map vs Geo</title>
		<!-- Latest compiled and minified CSS -->
		<link rel='stylesheet' href='https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css' integrity='sha384-BVYiiSIFeK1dGmJRAkycuHAHRg32OmUcww7on3RYdg4Va+PmSTsz/K68vbdEjh4u' crossorigin='anonymous'>

		<!-- Optional theme -->
		<link rel='stylesheet' href='https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap-theme.min.css' integrity='sha384-rHyoN1iRsVXV4nD0JutlnGaslCJuC7uwjduW9SVrLvRYooPp2bWYgmgJQIXwl/Sp' crossorigin='anonymous'>

		<!-- Latest compiled and minified JavaScript -->
		<script src='https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js' integrity='sha384-Tc5IQib027qvyjSMfHjOMaLkfuWVxZxUPnCJA7l2mCWNIpG9mGCD8wGNIcPD7Txa' crossorigin='anonymous'></script>
	</head>
	<body>
		<div class='container'>
			<div class='page-header'>
				<h1>Map vs Geo</h1>
				<p class='lead'>Comparing the published maps and actual geography of various public transit systems.</p>
			</div>");
				foreach (var comparison in comparisons)
					writer.WriteLine(BuildComparisonRow(comparison));
				writer.WriteLine(@"
		</div>
	</body>
</html>");
			}
		}

		private string BuildComparisonRow(ComparisonDetails comparison)
		{
			return $@"
			<h2>{comparison.Name}</h2>
			<div class='row'>
				<div class='col-md-4'>
					<a href='{comparison.Name} map.svg' class='thumbnail'><img src='{comparison.MapImage}' alt='...'></a>
				</div>
				<div class='col-md-4'>
					<a href='{comparison.Name} mapvsgeo.svg' class='thumbnail'><img src='{comparison.Name} mapvsgeo.svg' alt='...'></a>
				</div>
				<div class='col-md-4'>
					<a href='{comparison.Name} geo.svg' class='thumbnail'><img src='http://maps.googleapis.com/maps/api/staticmap?size=640x640&center={comparison.Center.Coordinate.Latitude},{comparison.Center.Coordinate.Longitude}&zoom=9&scale=2' alt='...'></a>
				</div>
			</div>";
		}
	}
}