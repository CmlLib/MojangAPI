using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MojangAPI.Model
{
    public class PlayerPrivilegeItem
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }
    }

    public class PlayerPrivileges
    {
        [JsonPropertyName("onlineChat")]
        public PlayerPrivilegeItem? OnlineChat { get; set; }

        [JsonPropertyName("multiplayerServer")]
        public PlayerPrivilegeItem? MultiplayerServer { get; set; }

        [JsonPropertyName("multiplayerRealms")]
        public PlayerPrivilegeItem? MultiplayerRealms { get; set; }

        [JsonPropertyName("telemtry")]
        public PlayerPrivilegeItem? Telemtry { get; set; }
    }
}
