
using System;
using System.IO;
namespace JetImageLoader
{
    public class JetImageLoaderResult
    {
        /// <summary>
        /// Possible sources of ImageStream
        /// </summary>
        public enum Source
        {
            Network,
            StorageCache,
            MemoryCache,
            Assets
        }

        public Stream ImageStream { get; set; }

        public Source ImageStreamSource { get; set; }

        public Uri ImageUri { get; set; }
    }
}
