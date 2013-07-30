
using System;
using System.IO;

namespace JetImageLoader.Network
{
    public class DownloadResult
    {
        public Exception Exception { get; set; }
        public Stream ResultStream { get; set; }
    }
}
