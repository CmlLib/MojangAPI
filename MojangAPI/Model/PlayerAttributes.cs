using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MojangAPI.Model
{
    public class PlayerAttributes
    {
        [JsonProperty("privileges")]
        public PlayerPrivileges? Privileges { get; set; }

        [JsonProperty("profanityFilterPreferences")]
        public PlayerProfanityFilterPreferences? ProfanityFilterPreferences { get;set;}
    }
}
