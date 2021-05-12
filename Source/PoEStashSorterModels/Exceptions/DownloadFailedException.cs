using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoEStashSorterModels.Exceptions
{
    class DownloadFailedException : Exception
    {
        public DownloadFailedException(string name) : base("Failed to download file: " + name)
        {
        }

        public DownloadFailedException() : base("Failed to download file")
        {
        }
    }
}
