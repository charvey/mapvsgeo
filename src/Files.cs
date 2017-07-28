using System;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace mapvsgeo
{
	public class Files
	{
        public static void Foo()
        {
            if (!File.Exists("gtfs_public/google_rail.zip"))
            {
                if (!File.Exists("gtfs_public.zip"))
                    Download("https://github.com/septadev/GTFS/releases/download/v20170701/gtfs_public.zip", "gtfs_public.zip");

                Extract("gtfs_public.zip");
            }

            if (!File.Exists("gtfs_public/google_bus.zip"))
            {
                if (!File.Exists("gtfs_public.zip"))
                    Download("https://github.com/septadev/GTFS/releases/download/v20170701/gtfs_public.zip", "gtfs_public.zip");

                Extract("gtfs_public.zip");
            }

            if (!File.Exists("PortAuthorityTransitCorporation.zip"))
                Download("http://www.ridepatco.org/developers/PortAuthorityTransitCorporation.zip", "PortAuthorityTransitCorporation.zip");

			if (!File.Exists("LACMTA.zip"))
				Download("https://gitlab.com/LACMTA/gtfs_rail/blob/master/gtfs_rail.zip", "LACMTA.zip");
        }

		private static void Extract(string zipPath)
		{
			using (var file = ZipFile.OpenRead(zipPath))
			{
				var dest = Path.Combine(Path.GetDirectoryName(zipPath), Path.GetFileNameWithoutExtension(zipPath));
				if (Directory.Exists(dest)) Directory.Delete(dest, true);
				file.ExtractToDirectory(dest);
			}
		}

        private static WebClient webClient = new WebClient();
        internal static void Download(string address, string fileName) => webClient.DownloadFile(address, fileName);
	}
}
