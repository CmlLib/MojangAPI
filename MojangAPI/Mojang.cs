using HttpAction;
using MojangAPI.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MojangAPI
{
    public class Mojang // e4e31cdd28364168a693d05d2deb3fd7
    {
        private static HttpClient defaultClient;
        private HttpClient client;

        public Mojang()
        {
            if (defaultClient == null)
                defaultClient = new HttpClient();

            client = defaultClient;
        }

        public Mojang(HttpClient client)
        {
            this.client = client;
        }

        public Task<UserUUID> GetUUID(string username) =>
            client.SendActionAsync(new HttpAction<UserUUID>
            {
                Method = HttpMethod.Get,
                Host = "https://api.mojang.com",
                Path = $"users/profiles/minecraft/{username}"
            });

        public Task<UserUUID> GetUUID(string username, DateTimeOffset timestamp) =>
            GetUUID(username, timestamp.ToUnixTimeSeconds());

        public Task<UserUUID> GetUUID(string username, long timestamp) =>
            client.SendActionAsync(new HttpAction<UserUUID>
            {
                Method = HttpMethod.Get,
                Host = "https://api.mojang.com",
                Path = $"users/profiles/minecraft/{username}",

                Queries = new HttpQueryCollection
                {
                    { "timestamp", timestamp.ToString() }
                }
            });

        public Task<NameHistory[]> GetNameHistories(string uuid) =>
            client.SendActionAsync(new HttpAction<NameHistory[]>
            {
                Method = HttpMethod.Get,
                Host = "https://api.mojang.com",
                Path = $"user/profiles/{uuid.Replace("-", "")}/names",
                ResponseHandler = HttpResponseHandlers.GetJsonArrayHandler<NameHistory>()
            });

        public Task<UserUUID[]> GetUUIDs(string[] usernames) =>
            client.SendActionAsync(new HttpAction<UserUUID[]>
            {
                Method = HttpMethod.Post,
                Host = "https://api.mojang.com",
                Path = "profiles/minecraft",
                Content = new JsonHttpContent(usernames),
                ResponseHandler = HttpResponseHandlers.GetJsonArrayHandler<UserUUID>()
            });

        public Task<UserProfile> GetProfileUsingUUID(string uuid) =>
            client.SendActionAsync(new HttpAction<UserProfile>
            {
                Method = HttpMethod.Get,
                Host = "https://sessionserver.mojang.com",
                Path = $"session/minecraft/profile/{uuid}",
                ResponseHandler = uuidProfileResponseHandler
            });

        private async Task<UserProfile> uuidProfileResponseHandler(HttpResponseMessage response)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            JObject job = JObject.Parse(responseContent);

            var uuid = job["id"]?.ToString();
            var name = job["name"]?.ToString();
            var legacy = job["legacy"]?.ToString()?.ToLower() == "true";
            Skin skin = null;

            var propValue = job["properties"]?[0]?["value"];
            if (propValue != null)
            {
                var decoded = Convert.FromBase64String(propValue.ToString());
                JObject propObj = JObject.Parse(Encoding.UTF8.GetString(decoded));

                var skinObj = propObj["textures"]?["SKIN"];

                if (skinObj != null)
                {
                    skin = new Skin
                    (
                        url: skinObj["url"]?.ToString(),
                        type: skinObj["metadata"]?["model"]?.ToString()
                    );
                }
            }

            if (skin == null)
                skin = new Skin(null, Skin.GetDefaultSkinType(uuid));

            return new UserProfile
            {
                UUID = uuid,
                Name = name,
                Skin = skin,
                IsLegacy = legacy
            };
        }

        public Task<UserProfile> GetProfileUsingAccessToken(string accessToken) =>
            client.SendActionAsync(new HttpAction<UserProfile>
            {
                Method = HttpMethod.Get,
                Host = "https://api.minecraftservices.com",
                Path = "minecraft/profile",

                RequestHeaders = new HttpHeaderCollection
                {
                    { "Authorization", "Bearer " + accessToken }
                },

                ResponseHandler = atProfileResponseHandler
            });

        private async Task<UserProfile> atProfileResponseHandler(HttpResponseMessage response)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            JObject job = JObject.Parse(responseContent);

            var skinObj = job["skins"]?[0];

            return new UserProfile
            {
                UUID = job["id"]?.ToString(),
                Name = job["name"]?.ToString(),
                Skin = new Skin
                (
                    url: skinObj?["url"]?.ToString(),
                    type: skinObj?["alias"]?.ToString()
                ),
                IsLegacy = false
            };
        }

        public Task<UserProfile> ChangeName(string accessToken, string newName) =>
            client.SendActionAsync(new HttpAction<UserProfile>
            {
                Method = HttpMethod.Put,
                Host = "https://api.minecraftservices.com",
                Path = $"minecraft/profile/name/{newName}",

                RequestHeaders = new HttpHeaderCollection
                {
                    { "Authorization", "Bearer " + accessToken }
                },

                ResponseHandler = atProfileResponseHandler
            });

        public Task<MojangAPIResponse> ChangeSkin(string uuid, string accessToken, SkinType skinType, string skinUrl) =>
            client.SendActionAsync(new HttpAction<MojangAPIResponse>
            {
                Method = HttpMethod.Post,
                Host = "https://api.mojang.com",
                Path = $"user/profile/${uuid}/skin",

                RequestHeaders = new HttpHeaderCollection
                {
                    { "Authorization", "Bearer " + accessToken }
                },

                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "model", skinType.GetModelType() },
                    { "url", skinUrl }
                })
            });

        public Task<MojangAPIResponse> UploadSkin(string accessToken, SkinType skinType, string skinPath)
        {
            if (!File.Exists(skinPath))
                throw new FileNotFoundException();

            string filename = Util.ReplaceInvalidChars(Path.GetFileName(skinPath));
            Stream fileStream = File.OpenRead(skinPath);
            return UploadSkin(accessToken, skinType, fileStream, filename);
        }

        public Task<MojangAPIResponse> UploadSkin(string accessToken, SkinType skinType, Stream skinStream, string filename) =>
            client.SendActionAsync(new HttpAction<MojangAPIResponse>
            {
                Method = HttpMethod.Put,
                Host = "https://api.minecraftservices.com",
                Path = "minecraft/profile/skins",

                RequestHeaders = new HttpHeaderCollection
                {
                    { "Authorization", "Bearer " + accessToken }
                },

                Content = new MultipartFormDataContent()
                {
                    { new StringContent(skinType.GetModelType()), "model" },
                    { CreateStreamContent(skinStream, "image/png"), "file", filename }
                }
            });

        private StreamContent CreateStreamContent(Stream stream, string contentType)
        {
            StreamContent content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            return content;
        }

        public Task<MojangAPIResponse> ResetSkin(string uuid, string accessToken) =>
            client.SendActionAsync(new HttpAction<MojangAPIResponse>
            {
                Method = HttpMethod.Delete,
                Host = "https://api.mojang.com",
                Path = $"user/profile/{uuid}/skin",

                RequestHeaders = new HttpHeaderCollection
                {
                    { "Authorization", "Bearer " + accessToken }
                }
            });

        public Task<bool> CheckGameOwnership(string accessToken) =>
            client.SendActionAsync(new HttpAction<bool>
            {
                Method = HttpMethod.Get,
                Host = "https://api.minecraftservices.com",
                Path = "entitlements/mcstore",

                RequestHeaders = new HttpHeaderCollection
                {
                    { "Authorization", "Bearer " + accessToken }
                },

                ResponseHandler = async (response) =>
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    JObject job = JObject.Parse(responseContent);

                    var itemsCount = (job["items"] as JArray)?.Count ?? 0;
                    return itemsCount != 0;
                }
            });
    }
}
