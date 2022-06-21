using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MojangAPI
{
    public class MojangException : Exception
    {
        public MojangException(string message) : base(message)
        {

        }

        [JsonConstructor]
        public MojangException(string? error, string? errorMessage) : base($"{error} {errorMessage}")
        {
            this.Error = error;
            this.ErrorMessage = errorMessage;
        }

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; set; }

        public static Func<HttpResponseMessage, Exception?, Task> GetMojangErrorHandler() =>
            async (response, ex) =>
            {
                var resBody = await response.Content.ReadAsStringAsync();

                if (ex == null)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(resBody))
                        {
                            var obj = JsonSerializer.Deserialize<MojangException>(resBody);
                            if (obj != null)
                                throw obj;
                        }
                    }
                    catch (JsonException)
                    {
                        // ignore exception to execute next error handler
                    }
                }

                response.EnsureSuccessStatusCode();
                if (ex != null)
                    throw ex;

                throw new MojangException(resBody);
            };

        public static Func<HttpResponseMessage, Exception?, Task<T>> GetMojangErrorHandler<T>() =>
            async (response, ex) =>
            {
                await GetMojangErrorHandler().Invoke(response, ex);
                return default(T);
            };
    }
}
