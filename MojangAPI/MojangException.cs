using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MojangAPI
{
    public class MojangException : Exception
    {
        public MojangException(string message, int statusCode) : base(message) =>
            StatusCode = statusCode;

        public MojangException(string? error, string? errorMessage, int statusCode) : base(CreateMessageFromError(error, errorMessage))
            => (Error, ErrorMessage, StatusCode) = (error, errorMessage, statusCode);

        public string? Error { get; private set; }

        public string? ErrorMessage { get; private set; }

        public int StatusCode { get; private set; }

        private static string? CreateMessageFromError(string? error, string? errorMessage)
        {
            if (!string.IsNullOrEmpty(error) && !string.IsNullOrEmpty(errorMessage))
                return $"{error}, {errorMessage}";
            if (!string.IsNullOrEmpty(error))
                return error;
            if (!string.IsNullOrEmpty(errorMessage))
                return errorMessage;

            return null;
        }

        public static MojangException ParseFromResponse(string responseBody, int statusCode)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                string? error = null;
                string? errorMessage = null;
                if (root.TryGetProperty("error", out var errorProp) &&
                    errorProp.ValueKind == JsonValueKind.String)
                    error = errorProp.GetString();
                if (root.TryGetProperty("errorMessage", out var errorMessageProp) && 
                    errorMessageProp.ValueKind == JsonValueKind.String)
                    errorMessage = errorMessageProp.GetString();

                if (string.IsNullOrEmpty(error) && string.IsNullOrEmpty(errorMessage))
                    throw new FormatException();

                return new MojangException(error, errorMessage, statusCode);
            }
            catch (JsonException)
            {
                throw new FormatException();
            }
        }

        public static Func<HttpResponseMessage, Exception?, Task> GetMojangErrorHandler() =>
            async (response, ex) =>
            {
                if (ex == null)
                {
                    try
                    {
                        var resBody = await response.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(resBody))
                            ex = ParseFromResponse(resBody, (int)response.StatusCode);
                    }
                    catch (FormatException)
                    {
                        // ignore exception to execute next error handler
                    }
                }

                if (ex != null)
                    throw ex;

                throw new MojangException($"{(int)response.StatusCode}: {response.ReasonPhrase}", (int)response.StatusCode);
            };

        public static Func<HttpResponseMessage, Exception?, Task<T>> GetMojangErrorHandler<T>() =>
            async (response, ex) =>
            {
                await GetMojangErrorHandler().Invoke(response, ex);
                return default(T);
            };
    }
}
