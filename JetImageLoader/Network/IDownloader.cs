
using System;
using System.Threading.Tasks;

namespace JetImageLoader.Network
{
    public delegate void DownloadResultCallback(DownloadResult downloadResult);

    public interface IDownloader
    {
        Task<DownloadResult> DownloadAsync(Uri uri);
    }
}
