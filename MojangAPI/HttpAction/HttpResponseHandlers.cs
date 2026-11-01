using MojangAPI.Serialization;
using System.Text.Json.Serialization.Metadata;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace HttpAction
{
    public static class HttpResponseHandlers
    {
        public static Func<HttpResponseMessage, Task<T>> GetDefaultResponseHandler<T>()
        {
            // Each type check guarantees that the delegate result type matches T.
            if (typeof(T) == typeof(bool))
                return (Func<HttpResponseMessage, Task<T>>)(object)GetSuccessCodeResponseHandler();
            if (typeof(T) == typeof(int))
                return (Func<HttpResponseMessage, Task<T>>)(object)GetStatusCodeResponseHandler();
            if (typeof(T) == typeof(string))
                return (Func<HttpResponseMessage, Task<T>>)(object)GetStringResponseHandler();
            if (typeof(T) == typeof(Stream))
                return (Func<HttpResponseMessage, Task<T>>)(object)GetStreamResponseHandler();

            return response => GetJsonHandler(MojangJson.GetTypeInfo<T>())(response);
        }

        public static Func<HttpResponseMessage, Task<bool>> GetSuccessCodeResponseHandler() =>
            (response) =>
            {
                return Task.FromResult(response.IsSuccessStatusCode);
            };

        public static Func<HttpResponseMessage, Task<bool>> GetSuccessCodeResponseHandler(int successCode) =>
            (response) =>
            {
                bool result = (int)response.StatusCode == successCode;
                return Task.FromResult(result);
            };

        public static Func<HttpResponseMessage, Task<int>> GetStatusCodeResponseHandler() =>
            (response) =>
            {
                return Task.FromResult((int)response.StatusCode);
            };

        public static Func<HttpResponseMessage, Task<string>> GetStringResponseHandler() =>
            (response) =>
            {
                return response.Content.ReadAsStringAsync();
            };

        public static Func<HttpResponseMessage, Task<Stream>> GetStreamResponseHandler() =>
            (response) =>
            {
                return response.Content.ReadAsStreamAsync();
            };

        public static Func<HttpResponseMessage, Task<T>> GetSuccessCodeResponseHandler<T>(T returnObj) =>
            (response) =>
            {
                if (response.IsSuccessStatusCode)
                    return Task.FromResult(returnObj);
                else
                    throw new HttpRequestException();
            };

        public static Func<HttpResponseMessage, Exception?, Task<T>> GetJsonErrorHandler<T>(JsonTypeInfo<T> typeInfo) where T : new() =>
            GetJsonErrorHandler(new T(), typeInfo);

        public static Func<HttpResponseMessage, Exception?, Task<T>> GetJsonErrorHandler<T>(T defaultObj, JsonTypeInfo<T> typeInfo)
        {
            if (typeInfo == null)
                throw new ArgumentNullException(nameof(typeInfo));

            return async (response, ex) =>
            {
                if (ex == null)
                {
                    try
                    {
                        var resBody = await response.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(resBody))
                        {
                            var obj = JsonSerializer.Deserialize(resBody, typeInfo);
                            if (obj != null)
                                return obj;
                        }
                    }
                    catch
                    {
                        // If the response cannot be parsed, fall back to the HTTP status and original exception.
                    }
                }

                response.EnsureSuccessStatusCode();
                if (ex != null)
                    throw ex;

                return defaultObj;
            };
        }

        public static Func<HttpResponseMessage, Task<T>> GetJsonHandler<T>(JsonTypeInfo<T> typeInfo)
        {
            if (typeInfo == null)
                throw new ArgumentNullException(nameof(typeInfo));

            return async response =>
            {
                string res = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(res))
                    res = "{}";

                return JsonSerializer.Deserialize(res, typeInfo)
                    ?? throw new JsonException("The response must contain a non-null JSON value.");
            };
        }

        public static Func<HttpResponseMessage, Exception?, Task<T?>> GetDefaultErrorHandler<T>() =>
            (response, ex) =>
            {
                response.EnsureSuccessStatusCode();
                if (ex != null)
                    throw ex;

                return Task.FromResult(default(T));
            };

        public static Func<HttpResponseMessage, Task<T[]>> GetJsonArrayHandler<T>(JsonTypeInfo<T> typeInfo)
        {
            if (typeInfo == null)
                throw new ArgumentNullException(nameof(typeInfo));

            return async response =>
            {
                string res = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(res))
                    res = "[]";

                using var doc = JsonDocument.Parse(res);
                var root = doc.RootElement;

                return root.EnumerateArray()
                    .Select(elem => JsonSerializer.Deserialize(elem.GetRawText(), typeInfo)
                        ?? throw new JsonException("The response array must not contain null elements."))
                    .ToArray();
            };
        }
    }
}
