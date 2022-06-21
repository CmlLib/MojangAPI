using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MojangAPI.Model
{
    public class PlayerPrivileges
    {
        [JsonPropertyName("onlineChat")]
        public bool OnlineChat { get; set; }

        [JsonPropertyName("multiplayerServer")]
        public bool MultiplayerServer { get; set; }

        [JsonPropertyName("multiplayerRealms")]
        public bool MultiplayerRealms { get; set; }

        [JsonPropertyName("telemtry")]
        public bool Telemtry { get; set; }
    }
}
