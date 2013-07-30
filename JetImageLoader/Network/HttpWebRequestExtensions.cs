
using System.Net;
using System.Threading.Tasks;

namespace JetImageLoader.Network
{
    static class HttpWebRequestExtensions
    {
        public static Task<HttpWebResponse> GetResponseAsync(this HttpWebRequest request)
        {
            var taskComplete = new TaskCompletionSource<HttpWebResponse>();
            request.BeginGetResponse(asyncResponse =>
            {
                try
                {
                    var responseRequest = (HttpWebRequest)asyncResponse.AsyncState;
                    var someResponse = (HttpWebResponse)responseRequest.EndGetResponse(asyncResponse);
                    taskComplete.TrySetResult(someResponse);
                }
                catch (WebException webExc)
                {
                    var failedResponse = (HttpWebResponse)webExc.Response;
                    taskComplete.TrySetResult(failedResponse);
                }
            }, request);

            return taskComplete.Task;
        }
    }
}
