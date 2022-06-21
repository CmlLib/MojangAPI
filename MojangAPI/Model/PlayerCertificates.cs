using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MojangAPI.Model
{
    public class PlayerCertificates
    {
        public class PlayerKeyPair
        {
            [JsonProperty("privateKey")]
            public string? PrivateKey { get; set; }
            [JsonProperty("publicKey")]
            public string? PublicKey { get; set; }
        }

        [JsonProperty("keyPair")]
        public PlayerKeyPair? KeyPair { get; set; }

        [JsonProperty("publicKeySignature")]
        public string? PublicKeySignature { get; set; }

        [JsonProperty("ExpiresAt")]
        public DateTime? ExpiresAt { get; set; }

        [JsonProperty("RefreshedAfter")]
        public DateTime? RefreshedAfter { get; set; }
    }
}
