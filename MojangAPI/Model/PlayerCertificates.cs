using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MojangAPI.Model
{
    public class PlayerCertificates
    {
        public class PlayerKeyPair
        {
            [JsonPropertyName("privateKey")]
            public string? PrivateKey { get; set; }
            [JsonPropertyName("publicKey")]
            public string? PublicKey { get; set; }
        }

        [JsonPropertyName("keyPair")]
        public PlayerKeyPair? KeyPair { get; set; }

        [JsonPropertyName("publicKeySignature")]
        public string? PublicKeySignature { get; set; }

        [JsonPropertyName("ExpiresAt")]
        public DateTime? ExpiresAt { get; set; }

        [JsonPropertyName("RefreshedAfter")]
        public DateTime? RefreshedAfter { get; set; }
    }
}
