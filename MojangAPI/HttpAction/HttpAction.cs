using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace HttpAction
{
    public class HttpAction<T> : HttpRequestMessage
    {
        public string Host { get; set; }
        public string Path { get; set; }
        public HttpQueryCollection Queries { get; set; }
        public HttpHeaderCollection RequestHeaders { get; set; }

        public Func<HttpResponseMessage, Task<T>> ResponseHandler { get; set; }
            = HttpResponseHandlers.GetJsonHandler<T>();

        public Func<HttpResponseMessage, Task<T>> ErrorHandler { get; set; }
        //  = HttpResponseHandlers.GetDefaultErrorHandler<T>();
            = null;

        public virtual Uri CreateUri()
        {
            var ubuilder = new UriBuilder(this.Host);
            ubuilder.Path = this.Path;
            ubuilder.Query = this.Queries?.BuildQuery();
            return ubuilder.Uri;
        }
    }
}
