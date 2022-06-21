using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MojangAPI.Model
{
    public class MojangAuthResponse : HttpAction.ActionResponse
    {
        public MojangAuthResponse(MojangAuthResult result)
        {
            this.Result = result;
        }

        [JsonPropertyName("error")]
        public string? Error { get; set; }
        [JsonPropertyName("message")]
        public string? ErrorMessage { get; set; }

        public MojangAuthResult Result { get; set; }
        public Session? Session { get; set; }
    }
}
