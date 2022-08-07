using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MojangAPI.Model
{
    public class PlayerAttributes
    {
        [JsonPropertyName("privileges")]
        public PlayerPrivileges? Privileges { get; set; }

        [JsonPropertyName("profanityFilterPreferences")]
        public PlayerProfanityFilterPreferences? ProfanityFilterPreferences { get; set; }

        [JsonPropertyName("banStatus")]
        public PlayerBanStatus? BanStatus { get; set; }
    }
}
