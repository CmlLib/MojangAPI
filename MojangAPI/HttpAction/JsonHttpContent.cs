using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace HttpAction
{
    public class JsonHttpContent : StringContent
    {
        private const string DefaultMediaType = "application/json";

        public JsonHttpContent(object? obj, JsonTypeInfo typeInfo)
            : this(obj, typeInfo, null)
        {
        }

        public JsonHttpContent(object? obj, JsonTypeInfo typeInfo, Encoding? encoding)
            : this(
                obj,
                typeInfo,
                encoding,
                DefaultMediaType)
        {
        }

        public JsonHttpContent(
            object? obj,
            JsonTypeInfo typeInfo,
            Encoding? encoding,
            string mediaType)
            : base(serialize(obj, typeInfo), encoding, mediaType)
        {
        }

        public static string serialize(object? obj, JsonTypeInfo typeInfo)
        {
            return JsonSerializer.Serialize(obj, typeInfo);
        }
    }
}
