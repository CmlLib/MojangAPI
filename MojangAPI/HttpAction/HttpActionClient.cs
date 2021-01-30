using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;

namespace HttpAction
{
    public static class HttpActionClient
    {
        public static Task<T> SendActionAsync<T>(this HttpClient client, HttpAction<T> httpAction) =>
            SendActionAsync(client, httpAction, HttpCompletionOption.ResponseContentRead, CancellationToken.None);

        public static Task<T> SendActionAsync<T>(this HttpClient client, HttpAction<T> httpAction, CancellationToken cancellationToken) =>
            SendActionAsync(client, httpAction, HttpCompletionOption.ResponseContentRead, cancellationToken);

        public static Task<T> SendActionAsync<T>(this HttpClient client, HttpAction<T> httpAction, HttpCompletionOption httpCompletionOption) =>
            SendActionAsync(client, httpAction, httpCompletionOption, CancellationToken.None);

        public static async Task<T> SendActionAsync<T>(this HttpClient client, HttpAction<T> httpAction, HttpCompletionOption httpCompletionOption, CancellationToken cancellationToken)
        {
            if (httpAction.RequestUri == null)
                httpAction.RequestUri = httpAction.CreateUri();

            if (httpAction.RequestHeaders != null)
                httpAction.RequestHeaders.AddToHeader(httpAction.Headers);

            HttpResponseMessage response
                = await client.SendAsync((HttpRequestMessage)httpAction, httpCompletionOption, cancellationToken);
            
            T result;
            if (response.IsSuccessStatusCode || httpAction.ErrorHandler == null)
                result = await httpAction.ResponseHandler(response);
            else
                result = await httpAction.ErrorHandler(response);

            ActionResponse actionResponse = result as ActionResponse;
            if (actionResponse != null)
                actionResponse.StatusCode = (int)response.StatusCode;

            return result;
        }
    }
}
