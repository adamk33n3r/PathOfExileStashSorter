using System;
using System.IO;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoEStashSorterModels
{
    class FileCache
    {
        private static string cachePath;
        private static string iconCachePath;
        static FileCache()
        {
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PoEStashSorter");
            cachePath = Path.Combine(appDataPath, "Cache");
            iconCachePath = Path.Combine(cachePath, "Icons");
            Directory.CreateDirectory(iconCachePath);
        }

        public static string FromUrl(string url)
        {
            var uri = new Uri(url);
            var iconPath = uri.LocalPath.TrimStart('/');
            var fullIconPath = Path.Combine(iconCachePath, iconPath);
            if (!File.Exists(fullIconPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullIconPath));
                try
                {
                    var data = Http.Get(url);
                    using (var fstream = new FileStream(fullIconPath, FileMode.Create))
                    {
                        fstream.Write(data, 0, data.Length);
                    }
                } catch (Exceptions.DownloadFailedException ex)
                {
                    Console.WriteLine(ex);
                }
                catch { }
            }

            return fullIconPath;
        }
    }
}
