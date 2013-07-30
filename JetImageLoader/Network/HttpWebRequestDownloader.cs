
using System;
using System.Net;
using System.Threading.Tasks;

namespace JetImageLoader.Network
{
    public class HttpWebRequestDownloader : IDownloader
    {
        public Task<DownloadResult> DownloadAsync(Uri uri)
        {
            // ReSharper disable once AccessToStaticMemberViaDerivedType
            var request = HttpWebRequest.Create(uri);

            var taskComplete = new TaskCompletionSource<DownloadResult>();
            
            request.BeginGetResponse(asyncResult =>
            {
                try
                {
                    var response = (HttpWebResponse) ((HttpWebRequest) asyncResult.AsyncState).EndGetResponse(asyncResult);

                    using (response)
                    {
                        taskComplete.TrySetResult(new DownloadResult { ResultStream = response.GetResponseStream()});
                    }
                }
                catch (Exception e)
                {
                    taskComplete.TrySetResult(new DownloadResult { Exception = e });
                }
            }, request);

            return taskComplete.Task;
        }
    }
    
}
