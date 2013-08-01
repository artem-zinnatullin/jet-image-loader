JetImageLoader load, cache, show, do it again!
================

fast and powerfull image loader with memory and storage caching for your Windows Phone 8+ projects!

####Like a boss:
* Load images
* Cache them in memory
* Cache them on storage
* Do it asynchronously

##Why JetImageLoader is super?##

1) __Easy to add to your project and start using it in 4 simple steps__:
* 1.1) __Add reference to JetImageLoader.dll__
* 1.2) __Extend JetImageLoaderConverter and configure it__:
<pre>
    public class MyAppJetImageLoaderConverter : BaseJetImageLoaderConverter
    {
        protected override JetImageLoaderConfig GetJetImageLoaderConfig()
        {
            return new JetImageLoaderConfig.Builder
            {
                IsLogEnabled     = true,
                CacheMode        = CacheMode.MemoryAndStorageCache,
                DownloaderImpl   = new HttpWebRequestDownloader(),
                MemoryCacheImpl  = new WeakMemoryCache<string, Stream>(),
                StorageCacheImpl = new LimitedStorageCache(IsolatedStorageFile.GetUserStoreForApplication(), 
                                   "\\image_cache", new SHA1CacheFileNameGenerator(), 1024 * 1024 * 10), // == 10 MB
            }.Build();
        }
    }
</pre>
* 1.3) __Declare MyAppJetImageLoaderConverter in App.xaml__:
<pre>
    ````<Application.Resources>
        <myApp:MyAppJetImageLoaderConverter x:Key="JetImageLoaderConverter"/>
    </Application.Resources>````
</pre>

* 1.4) __Set it as Converter for Image__:
<pre>
    ````<Image Source="{Binding UserAvatarUrl, Converter={StaticResource MyAppJetImageLoaderConverter}}"/>````
</pre>

aaand __that is all__, now it can load images from network, cache them in memory and storage and then load them from cache!

2) __You can use your own implementations__:
<pre>
    Downloader implementation    — just implement IDownloader interface
    Memory cache implementation  — just extend BaseMemoryCache abstract class
    Storage cache implementation — just extend BaseStorageCache abstract class
</pre>

3) __JetImageLoader already has basic implementations for all this things__:
<pre>
    1 Downloader implemetation     — HttpWebRequestDownloader based on HttpWebRequest class
    1 Memory cache implementation  — WeakMemoryCache based on ConditionalWeakTable with weak references
                                     and auto GC cleaning (very cool)
    2 Storage cache implementation — LimitedStorageCache with configurable limit in bytes to store on disk
                                     and stupid :) UnlimitedStorageCache implementation
</pre>
