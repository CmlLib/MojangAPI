using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MojangAPI.Model
{
    public class PlayerPrivileges
    {
        [JsonProperty("onlineChat")]
        public bool OnlineChat { get; set; }

        [JsonProperty("multiplayerServer")]
        public bool MultiplayerServer { get; set; }

        [JsonProperty("multiplayerRealms")]
        public bool MultiplayerRealms { get; set; }

        [JsonProperty("telemtry")]
        public bool Telemtry { get; set; }
    }
}
