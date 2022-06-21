using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace HttpAction
{
    public class JsonHttpContent : StringContent
    {
        private const string DefaultMediaType = "application/json";

        public JsonHttpContent(object obj)
            : this(obj, null)
        {

        }

        public JsonHttpContent(object obj, Encoding? encoding)
            : this(obj, encoding, DefaultMediaType)
        {

        }

        public JsonHttpContent(object obj, Encoding? encoding, string mediaType)
            : base(serialize(obj), encoding, mediaType)
        {
            
        }

        public static string serialize(object obj)
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
        }
    }
}
