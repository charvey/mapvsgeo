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
                    new WebClient().DownloadFile("https://github.com/septadev/GTFS/releases/download/v20170423.1/gtfs_public.zip", "gtfs_public.zip");

                Extract("gtfs_public.zip");
            }

            if (!File.Exists("gtfs_public/google_bus.zip"))
            {
                if (!File.Exists("gtfs_public.zip"))
                    new WebClient().DownloadFile("https://github.com/septadev/GTFS/releases/download/v20170423.1/gtfs_public.zip", "gtfs_public.zip");

                Extract("gtfs_public.zip");
            }

            if (!File.Exists("PortAuthorityTransitCorporation.zip"))
                new WebClient().DownloadFile("http://www.ridepatco.org/developers/PortAuthorityTransitCorporation.zip", "PortAuthorityTransitCorporation.zip");
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
	}
}
