using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoEStashSorterModels
{
    class Http
    {
        private static readonly WebClient client;
        private static readonly int maxRetries = 3;

        static Http()
        {
            client = new WebClient();
            client.BaseAddress = PoeConnector.server.Url;
        }

        public static byte[] Get(string url)
        {
            int retries = 0;
            byte[] imageData = null;
            while (retries < maxRetries)
            {
                try
                {
                    imageData = client.DownloadData(url);
                    break;
                }
                catch {
                    retries++;
                    if (retries >= maxRetries)
                        throw new Exceptions.DownloadFailedException(Path.GetFileName(new Uri(url).LocalPath));
                    Console.WriteLine("Failed downloading icon. Retrying...({0})", retries);
                }
            }

            return imageData;
        }
    }
}
