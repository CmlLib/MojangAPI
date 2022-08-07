using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MojangAPI.Model
{
    public class PlayerBannedScope
    {
        [JsonPropertyName("banId")]
        public string? BanId { get; set; }

        /// <summary>
        /// Expire date as unix timestamp. null if the ban is permanent
        /// </summary>
        [JsonPropertyName("expires")]
        public long? Expires { get; set; }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }

        [JsonPropertyName("reasonMessage")]
        public string? ReasonMessage { get; set; }
    }

    public class PlayerBanStatus
    {
        /// <summary>
        /// known key: "MULTIPLAYER". BannedScopes["MULTIPLAYER"] will return object if the player is banned.
        /// </summary>
        [JsonPropertyName("bannedScopes")]
        public Dictionary<string, PlayerBannedScope?>? BannedScopes { get; set; }
    }
}
