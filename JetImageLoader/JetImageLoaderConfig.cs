
using System;
using System.IO;
using JetImageLoader.Cache;
using JetImageLoader.Cache.Memory;
using JetImageLoader.Cache.Storage;
using JetImageLoader.Network;

namespace JetImageLoader
{
    public class JetImageLoaderConfig
    {
        public readonly bool IsLogEnabled;
        public readonly CacheMode CacheMode;
        public readonly IDownloader DownloaderImpl;
        public readonly BaseMemoryCache<string, Stream> MemoryCacheImpl;
        public readonly BaseStorageCache StorageCacheImpl;

        private JetImageLoaderConfig(Builder builder)
        {
            IsLogEnabled     = builder.IsLogEnabled;
            CacheMode        = builder.CacheMode;
            DownloaderImpl   = builder.DownloaderImpl;
            MemoryCacheImpl  = builder.MemoryCacheImpl;
            StorageCacheImpl = builder.StorageCacheImpl;
        }


        /// <summary>
        /// Implements Builder pattern
        /// </summary>
        /// <see cref="http://en.wikipedia.org/wiki/Builder_pattern"/>
        public class Builder
        {
            /// <summary>
            /// Enable/Disable log output for JetImageLoader
            /// Default - false
            /// </summary>
            public bool IsLogEnabled { get; set; }

            private CacheMode _cacheMode = CacheMode.MemoryAndStorageCache;

            /// <summary>
            /// Gets/Sets caching mode for JetImageLoader
            /// Default - CacheMode.MemoryAndStorageCache
            /// </summary>
            public CacheMode CacheMode { get { return _cacheMode; } set { _cacheMode = value; } }

            private IDownloader _downloaderImpl = new HttpWebRequestDownloader();

            /// <summary>
            /// Gets/Sets downloader implementation for JetImageLoader
            /// Default - HttpWebRequestDownloader
            /// </summary>
            public IDownloader DownloaderImpl
            {
                get { return _downloaderImpl; }
                set
                {
                    if (value == null) throw new ArgumentNullException();
                    _downloaderImpl = value;
                }
            }

            /// <summary>
            /// Gets/Sets memory cache implementation for JetImageLoader
            /// If you will leave it empty but CacheMode will require it, will be used WeakMemoryCache implementation 
            /// </summary>
            public BaseMemoryCache<string, Stream> MemoryCacheImpl { get; set; }

            /// <summary>
            /// Gets/Sets storage cache implementation for JetImageLoader
            /// If you will leave it empty but CacheMode will require it, exception will be thrown
            /// Default - null, I am sorry for that, but it requires IsolatedStorageFile instance, so you have to init it anyway
            /// </summary>
            public BaseStorageCache StorageCacheImpl { get; set; }

            public JetImageLoaderConfig Build()
            {
                CheckParams();
                return new JetImageLoaderConfig(this);
            }

            private void CheckParams()
            {
                if ((CacheMode == CacheMode.MemoryAndStorageCache || CacheMode == CacheMode.OnlyMemoryCache) && MemoryCacheImpl == null)
                {
                    throw new ArgumentException("CacheMode " + CacheMode + " requires MemoryCacheImpl");
                }

                if ((CacheMode == CacheMode.MemoryAndStorageCache || CacheMode == CacheMode.OnlyStorageCache) && StorageCacheImpl == null)
                {
                    throw new ArgumentException("CacheMode " + CacheMode + " requires StorageCacheImpl");
                }
            }
        }
    }
}
