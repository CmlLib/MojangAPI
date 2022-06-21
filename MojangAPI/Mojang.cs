using HttpAction;
using MojangAPI.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MojangAPI
{
    public class Mojang
    {
        internal static Lazy<HttpClient> DefaultClient = new Lazy<HttpClient>(() => new HttpClient());

        private HttpClient client;

        public Mojang()
        {
            client = DefaultClient.Value;
        }

        public Mojang(HttpClient client)
        {
            this.client = client;
        }

        public Task<PlayerUUID> GetUUID(string username) =>
            client.SendActionAsync(new HttpAction<PlayerUUID>
            {
                Method = HttpMethod.Get,
                Host = "https://api.mojang.com",
                Path = $"users/profiles/minecraft/{username ?? throw new ArgumentNullException(nameof(username))}"
            });

        public Task<PlayerUUID> GetUUID(string username, DateTimeOffset timestamp) =>
            GetUUID(username, timestamp.ToUnixTimeSeconds());

        public Task<PlayerUUID> GetUUID(string username, long timestamp) =>
            client.SendActionAsync(new HttpAction<PlayerUUID>
            {
                Method = HttpMethod.Get,
                Host = "https://api.mojang.com",
                Path = $"users/profiles/minecraft/{username ?? throw new ArgumentNullException(nameof(username))}",

                Queries = new HttpQueryCollection
                {
                    { "timestamp", timestamp.ToString() }
                }
            });

        public Task<NameHistoryResponse> GetNameHistories(string uuid) =>
            client.SendActionAsync(new HttpAction<NameHistoryResponse>
            {
                Method = HttpMethod.Get,
                Host = "https://api.mojang.com",
                Path = $"user/profiles/{uuid?.Replace("-", "") ?? throw new ArgumentNullException(nameof(uuid))}/names",
                ResponseHandler = async (response) =>
                {
                    var handler = HttpResponseHandlers.GetJsonArrayHandler<NameHistory>();
                    var histories = await handler.Invoke(response);
                    return new NameHistoryResponse(histories);
                },
                ErrorHandler = HttpResponseHandlers.GetJsonErrorHandler<NameHistoryResponse>()
            });

        public Task<PlayerUUID[]?> GetUUIDs(string[] usernames) =>
            client.SendActionAsync(new HttpAction<PlayerUUID[]?>
            {
                Method = HttpMethod.Post,
                Host = "https://api.mojang.com",
                Path = "profiles/minecraft",
                Content = new JsonHttpContent(usernames ?? throw new ArgumentNullException()),
                ResponseHandler = HttpResponseHandlers.GetJsonArrayHandler<PlayerUUID>(),
                ErrorHandler = (res, ex) => Task.FromResult<PlayerUUID[]?>(null)
            });

        public Task<PlayerProfile> GetProfileUsingUUID(string uuid) =>
            client.SendActionAsync(new HttpAction<PlayerProfile>
            {
                Method = HttpMethod.Get,
                Host = "https://sessionserver.mojang.com",
                Path = $"session/minecraft/profile/{uuid ?? throw new ArgumentNullException()}",
                ResponseHandler = uuidProfileResponseHandler,
                ErrorHandler = HttpResponseHandlers.GetJsonErrorHandler<PlayerProfile>()
            });

        private async Task<PlayerProfile> uuidProfileResponseHandler(HttpResponseMessage response)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            JObject job = JObject.Parse(responseContent);

            string? uuid = job["id"]?.ToString();
            string name = job["name"]?.ToString() ?? "";
            bool legacy = job["legacy"]?.ToString()?.ToLower() == "true";
            Skin? skin = null;

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
            {
                SkinType? defaultSkinType = null;
                if (string.IsNullOrEmpty(uuid))
                    defaultSkinType = Skin.GetDefaultSkinType(uuid ?? "");

                skin = new Skin(null, defaultSkinType);
            }

            return new PlayerProfile
            {
                UUID = uuid ?? "",
                Name = name,
                Skin = skin,
                IsLegacy = legacy
            };
        }

        public Task<PlayerProfile> GetProfileUsingAccessToken(string accessToken) =>
            client.SendActionAsync(new HttpAction<PlayerProfile>
            {
                Method = HttpMethod.Get,
                Host = "https://api.minecraftservices.com",
                Path = "minecraft/profile",

                RequestHeaders = new HttpHeaderCollection
                {
                    { "Authorization", "Bearer " + accessToken ?? throw new ArgumentNullException() }
                },

                ResponseHandler = atProfileResponseHandler,
                ErrorHandler = HttpResponseHandlers.GetJsonErrorHandler<PlayerProfile>()
            });

        private async Task<PlayerProfile> atProfileResponseHandler(HttpResponseMessage response)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            JObject job = JObject.Parse(responseContent);

            var skinObj = job["skins"]?[0];

            return new PlayerProfile
            {
                UUID = job["id"]?.ToString() ?? "",
                Name = job["name"]?.ToString() ?? "",
                Skin = new Skin
                (
                    url: skinObj?["url"]?.ToString(),
                    type: skinObj?["variant"]?.ToString()
                ),
                IsLegacy = false
            };
        }

        public Task<PlayerAttributes> GetPlayerAttributes(string accessToken) =>
            client.SendActionAsync(new HttpAction<PlayerAttributes>
            {
                Method = HttpMethod.Get,
                Host = "https://api.minecraftservices.com",
                Path = "players/attributes",

                RequestHeaders = new HttpHeaderCollection
                {
                    { "Authorization", "Bearer " + accessToken }
                },

                ResponseHandler = HttpResponseHandlers.GetJsonHandler<PlayerAttributes>(),
                ErrorHandler = HttpResponseHandlers.GetJsonErrorHandler<PlayerAttributes>()
            });

        public Task<string[]?> GetPlayerBlocklist(string accessToken) =>
            client.SendActionAsync(new HttpAction<string[]?>
            {
                Method = HttpMethod.Get,
                Host = "https://api.minecraftservices.com",
                Path = "privacy/blocklist",

                RequestHeaders = new HttpHeaderCollection
                {
                    { "Authorization", "Bearer " + accessToken }
                },

                ResponseHandler = async (res) =>
                {
                    var resbody = await res.Content.ReadAsStringAsync();
                    var job = JObject.Parse(resbody);
                    var arr = (JArray?)job["blockedProfiles"];
                    return arr?.Select(x => x.ToString())?.ToArray();
                },
                ErrorHandler = HttpResponseHandlers.GetDefaultErrorHandler<string[]?>()
            });

        public Task<PlayerCertificates> GetPlayerCertificates(string accessToken) =>
            client.SendActionAsync(new HttpAction<PlayerCertificates>
            {
                Method = HttpMethod.Post,
                Host = "https://api.minecraftservices.com",
                Path = "player/certificates",

                RequestHeaders = new HttpHeaderCollection
                {
                    { "Authorization", "Bearer " + accessToken }
                },

                ResponseHandler = HttpResponseHandlers.GetJsonHandler<PlayerCertificates>(),
                ErrorHandler = HttpResponseHandlers.GetJsonErrorHandler<PlayerCertificates>()
            });

        public Task<string?> CheckNameAvailability(string accessToken, string newName) =>
            client.SendActionAsync(new HttpAction<string?>
            {
                Method = HttpMethod.Get,
                Host = "https://api.minecraftservices.com",
                Path = $"minecraft/profile/name/{newName}/available",

                RequestHeaders = new HttpHeaderCollection
                {
                    { "Authorization", "Bearer " + accessToken }
                },

                ResponseHandler = async (res) =>
                {
                    var resbody = await res.Content.ReadAsStringAsync();
                    var job = JObject.Parse(resbody);
                    return job["status"]?.ToString();
                },
                ErrorHandler = HttpResponseHandlers.GetDefaultErrorHandler<string?>()
            });

        public Task<PlayerProfile> ChangeName(string accessToken, string newName) =>
            client.SendActionAsync(new HttpAction<PlayerProfile>
            {
                Method = HttpMethod.Put,
                Host = "https://api.minecraftservices.com",
                Path = $"minecraft/profile/name/{newName}",

                RequestHeaders = new HttpHeaderCollection
                {
                    { "Authorization", "Bearer " + accessToken }
                },

                ResponseHandler = atProfileResponseHandler,
                ErrorHandler = HttpResponseHandlers.GetJsonErrorHandler<PlayerProfile>(),

                CheckValidation = (h) =>
                {
                    if (string.IsNullOrEmpty(newName)) return nameof(newName);
                    else if (string.IsNullOrEmpty(accessToken)) return nameof(accessToken);
                    else return null;
                }
            });

        public Task<PlayerProfile> ChangeSkin(string uuid, string accessToken, SkinType skinType, string skinUrl) =>
            client.SendActionAsync(new HttpAction<PlayerProfile>
            {
                Method = HttpMethod.Post,
                Host = "https://api.minecraftservices.com",
                Path = "minecraft/profile/skins",

                RequestHeaders = new HttpHeaderCollection
                {
                    { "Authorization", "Bearer " + accessToken }
                },

                Content = new JsonHttpContent(new
                {
                    variant = skinType.GetModelType(),
                    url = skinUrl
                }),

                CheckValidation = (h) =>
                {
                    if (string.IsNullOrEmpty(uuid)) return nameof(uuid);
                    else if (string.IsNullOrEmpty(accessToken)) return nameof(accessToken);
                    else if (string.IsNullOrEmpty(skinUrl)) return nameof(skinUrl);
                    else return null;
                },

                ResponseHandler = atProfileResponseHandler,
                ErrorHandler = HttpResponseHandlers.GetJsonErrorHandler<PlayerProfile>()
            });

        public Task<MojangAPIResponse> UploadSkin(string accessToken, SkinType skinType, string skinPath)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentNullException(nameof(accessToken));
            if (string.IsNullOrEmpty(skinPath))
                throw new ArgumentNullException(nameof(skinPath));
            if (!File.Exists(skinPath))
                throw new FileNotFoundException();

            string filename = Util.ReplaceInvalidChars(Path.GetFileName(skinPath));
            Stream fileStream = File.OpenRead(skinPath);
            return UploadSkin(accessToken, skinType, fileStream, filename);
        }

        public Task<MojangAPIResponse> UploadSkin(string accessToken, SkinType skinType, Stream skinStream, string filename) =>
            client.SendActionAsync(new HttpAction<MojangAPIResponse>
            {
                Method = HttpMethod.Post,
                Host = "https://api.minecraftservices.com",
                Path = "minecraft/profile/skins",

                RequestHeaders = new HttpHeaderCollection
                {
                    { "Authorization", "Bearer " + accessToken }
                },

                Content = new MultipartFormDataContent()
                {
                    { new StringContent(skinType.GetModelType()), "\"variant\"" },
                    { CreateStreamContent(skinStream, "image/png"), "\"file\"", filename }
                },

                CheckValidation = (h) =>
                {
                    if (skinStream == null) return nameof(skinStream);
                    else if (string.IsNullOrEmpty(accessToken)) return nameof(accessToken);
                    else if (string.IsNullOrEmpty(filename)) return nameof(filename);
                    else return null;
                },

                ResponseHandler = async (handler) =>
                {
                    var res = await handler.Content.ReadAsStringAsync();
                    var defaultHandler = HttpResponseHandlers.GetDefaultResponseHandler<MojangAPIResponse>();
                    return await defaultHandler.Invoke(handler);
                },
                ErrorHandler = HttpResponseHandlers.GetJsonErrorHandler<MojangAPIResponse>()
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
                Host = "https://api.minecraftservices.com/",
                Path = $"minecraft/profile/skins/active",

                RequestHeaders = new HttpHeaderCollection
                {
                    { "Authorization", "Bearer " + accessToken }
                },

                CheckValidation = (h) =>
                {
                    if (string.IsNullOrEmpty(uuid)) return nameof(uuid);
                    else if (string.IsNullOrEmpty(accessToken)) return nameof(accessToken);
                    else return null;
                },

                ErrorHandler = HttpResponseHandlers.GetJsonErrorHandler<MojangAPIResponse>()
            });

        public Task<string[]> GetBlockedServer() =>
            client.SendActionAsync(new HttpAction<string[]>
            {
                Method = HttpMethod.Get,
                Host = "https://sessionserver.mojang.com",
                Path = "blockedservers",

                ResponseHandler = async (response) =>
                {
                    string content = await response.Content.ReadAsStringAsync();
                    return content.Split('\n').Select(x => x.Trim()).ToArray();
                },
                ErrorHandler = HttpResponseHandlers.GetDefaultErrorHandler<string[]>()
            });

        public Task<Statistics> GetStatistics(params StatisticOption[] options) =>
            GetStatistics(options.Select(x => x.Key).ToArray());

        public Task<Statistics> GetStatistics(string[] options) =>
            client.SendActionAsync(new HttpAction<Statistics>
            {
                Method = HttpMethod.Post,
                Host = "https://api.mojang.com",
                Path = "orders/statistics",

                Content = new JsonHttpContent(new
                {
                    metricKeys = options
                }),

                ResponseHandler = HttpResponseHandlers.GetJsonHandler<Statistics>(),
                ErrorHandler = HttpResponseHandlers.GetJsonErrorHandler<Statistics>()
            });

        public Task<bool> CheckGameOwnership(string accessToken) =>
            client.SendActionAsync(new HttpAction<bool>
            {
                Method = HttpMethod.Get,
                Host = "https://api.minecraftservices.com",
                Path = "entitlements/mcstore",

                RequestHeaders = new HttpHeaderCollection
                {
                    { "Authorization", "Bearer " + accessToken ?? throw new ArgumentNullException(nameof(accessToken)) }
                },

                ResponseHandler = async (response) =>
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    JObject job = JObject.Parse(responseContent);

                    var itemsCount = (job["items"] as JArray)?.Count ?? 0;
                    return itemsCount != 0;
                },
                ErrorHandler = (res, ex) => Task.FromResult(false)
            });
    }
}
