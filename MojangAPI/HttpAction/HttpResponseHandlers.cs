using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HttpAction
{
    public class HttpResponseHandlers
    {
        public static Func<HttpResponseMessage, Task<T>> GetJsonHandler<T>() =>
            async (response) =>
            {
                string res = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(res, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
            };

        public static Func<HttpResponseMessage, Task<T>> GetDefaultErrorHandler<T>() =>
            (response) =>
            {
                response.EnsureSuccessStatusCode();
                return Task.FromResult(default(T));
            };

        private static JsonSerializer defaultSerializer = new JsonSerializer
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public static Func<HttpResponseMessage, Task<T[]>> GetJsonArrayHandler<T>() =>
            async (response) =>
            {
                string res = await response.Content.ReadAsStringAsync();
                JArray jarr = JArray.Parse(res);
                return jarr.Select(x => x.ToObject<T>(defaultSerializer)).ToArray();
            };
    }
}
