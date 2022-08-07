using HttpAction;
using MojangAPI.Model;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace MojangAPI
{
    public class Mojang
    {
        internal static readonly Lazy<HttpClient> DefaultClient = new Lazy<HttpClient>(() => new HttpClient());

        private readonly HttpClient client;

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
                Path = $"users/profiles/minecraft/{username ?? throw new ArgumentNullException(nameof(username))}",
                ErrorHandler = MojangException.GetMojangErrorHandler<PlayerUUID>()
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
                },

                ErrorHandler = MojangException.GetMojangErrorHandler<PlayerUUID>()
            });

        public Task<NameHistory[]> GetNameHistories(string uuid) =>
            client.SendActionAsync(new HttpAction<NameHistory[]>
            {
                Method = HttpMethod.Get,
                Host = "https://api.mojang.com",
                Path = $"user/profiles/{uuid?.Replace("-", "") ?? throw new ArgumentNullException(nameof(uuid))}/names",
                ResponseHandler = async (response) =>
                {
                    var handler = HttpResponseHandlers.GetJsonArrayHandler<NameHistory>();
                    return await handler.Invoke(response);
                },
                ErrorHandler = MojangException.GetMojangErrorHandler<NameHistory[]>()
            });

        public Task<PlayerUUID[]> GetUUIDs(string[] usernames) =>
            client.SendActionAsync(new HttpAction<PlayerUUID[]>
            {
                Method = HttpMethod.Post,
                Host = "https://api.mojang.com",
                Path = "profiles/minecraft",
                Content = new JsonHttpContent(usernames ?? throw new ArgumentNullException()),
                ResponseHandler = HttpResponseHandlers.GetJsonArrayHandler<PlayerUUID>(),
                ErrorHandler = MojangException.GetMojangErrorHandler<PlayerUUID[]>()
            });

        public Task<PlayerProfile> GetProfileUsingUUID(string uuid) =>
            client.SendActionAsync(new HttpAction<PlayerProfile>
            {
                Method = HttpMethod.Get,
                Host = "https://sessionserver.mojang.com",
                Path = $"session/minecraft/profile/{uuid ?? throw new ArgumentNullException()}",
                ResponseHandler = profileResponseHandlerFromUUID,
                ErrorHandler = MojangException.GetMojangErrorHandler<PlayerProfile>()
            });

        private async Task<PlayerProfile> profileResponseHandlerFromUUID(HttpResponseMessage response)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;

            var profile = root.Deserialize<PlayerProfile>() ?? new PlayerProfile();
            
            try
            {
                var innerValue = root.GetProperty("properties")
                                     .EnumerateArray()
                                     .First()
                                     .GetProperty("value")
                                     .GetString();

                var decoded = Convert.FromBase64String(innerValue);
                using var skinDocument = JsonDocument.Parse(decoded);
                var skinRoot = skinDocument.RootElement;

                var skinObj = skinRoot.GetProperty("textures").GetProperty("SKIN");
                var skinUrl = skinObj.GetProperty("url").GetString();
                var skinType = skinObj.GetProperty("metadata").GetProperty("model").GetString();

                profile.Skin = new Skin(skinUrl, skinType);
            }
            catch
            {
                SkinType? defaultSkinType = null;
                if (string.IsNullOrEmpty(profile?.UUID))
                    defaultSkinType = Skin.GetDefaultSkinType(profile!.UUID!);

                profile!.Skin = new Skin(null, defaultSkinType);
            }

            return profile;
        }

        public Task<PlayerProfile> GetProfileUsingAccessToken(string accessToken) =>
            client.SendActionAsync(new HttpAction<PlayerProfile>
            {
                Method = HttpMethod.Get,
                Host = "https://api.minecraftservices.com",
                Path = "minecraft/profile",

                RequestHeaders = new HttpHeaderCollection
                {
                    { "Authorization", "Bearer " + accessToken ?? throw new ArgumentNullException() },
                },

                ResponseHandler = profileResponseHandlerFromAccessToken,
                ErrorHandler = MojangException.GetMojangErrorHandler<PlayerProfile>()
            });

        private async Task<PlayerProfile> profileResponseHandlerFromAccessToken(HttpResponseMessage response)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;

            var profile = root.Deserialize<PlayerProfile>() ?? new PlayerProfile();

            try
            {
                var skinObj = root.GetProperty("skins").EnumerateArray().First();
                var skinUrl = skinObj.GetProperty("url").GetString();
                var skinType = skinObj.GetProperty("variant").GetString();
                profile.Skin = new Skin(skinUrl, skinType);
            }
            catch
            {
                SkinType? defaultSkinType = null;
                if (string.IsNullOrEmpty(profile?.UUID))
                    defaultSkinType = Skin.GetDefaultSkinType(profile!.UUID!);

                profile!.Skin = new Skin(null, defaultSkinType);
            }

            return profile;
        }

        public Task<PlayerAttributes> GetPlayerAttributes(string accessToken) =>
            client.SendActionAsync(new HttpAction<PlayerAttributes>
            {
                Method = HttpMethod.Get,
                Host = "https://api.minecraftservices.com",
                Path = "player/attributes",

                RequestHeaders = new HttpHeaderCollection
                {
                    { "Authorization", "Bearer " + accessToken }
                },

                ResponseHandler = HttpResponseHandlers.GetJsonHandler<PlayerAttributes>(),
                ErrorHandler = MojangException.GetMojangErrorHandler<PlayerAttributes>()
            });

        public Task<string[]> GetPlayerBlocklist(string accessToken) =>
            client.SendActionAsync(new HttpAction<string[]>
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
                    using var doc = JsonDocument.Parse(resbody);
                    return doc.RootElement.GetProperty("blockedProfiles")
                                          .EnumerateArray()
                                          .Select(elem => elem.GetString())
                                          .Where(elem => !string.IsNullOrEmpty(elem))
                                          .ToArray()!;
                },
                ErrorHandler = MojangException.GetMojangErrorHandler<string[]>()
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
                ErrorHandler = MojangException.GetMojangErrorHandler<PlayerCertificates>()
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
                    using var doc = JsonDocument.Parse(resbody);
                    return doc.RootElement.GetProperty("status").GetString();
                },
                ErrorHandler = MojangException.GetMojangErrorHandler<string?>()
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

                ResponseHandler = profileResponseHandlerFromAccessToken,
                ErrorHandler = MojangException.GetMojangErrorHandler<PlayerProfile>(),

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

                ResponseHandler = profileResponseHandlerFromAccessToken,
                ErrorHandler = MojangException.GetMojangErrorHandler<PlayerProfile>()
            });

        public Task UploadSkin(string accessToken, SkinType skinType, string skinPath)
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

        public Task UploadSkin(string accessToken, SkinType skinType, Stream skinStream, string filename) =>
            client.SendActionAsync(new HttpAction<bool>
            {
                Method = HttpMethod.Post,
                Host = "https://api.minecraftservices.com",
                Path = "minecraft/profile/skins",

                RequestHeaders = new HttpHeaderCollection
                {
                    { "Authorization", "Bearer " + accessToken }
                },

                Content = new MultipartFormDataContent
                {
                    { new StringContent(skinType.GetModelType()), "\"variant\"" },
                    { CreateStreamContent(skinStream, "image/png"), "\"file\"", filename }
                },

                CheckValidation = _ =>
                {
                    if (skinStream == null) return nameof(skinStream);
                    else if (string.IsNullOrEmpty(accessToken)) return nameof(accessToken);
                    else if (string.IsNullOrEmpty(filename)) return nameof(filename);
                    else return null;
                },

                ResponseHandler = HttpResponseHandlers.GetSuccessCodeResponseHandler(),
                ErrorHandler = MojangException.GetMojangErrorHandler<bool>()
            });

        private StreamContent CreateStreamContent(Stream stream, string contentType)
        {
            StreamContent content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            return content;
        }

        public Task ResetSkin(string uuid, string accessToken) =>
            client.SendActionAsync(new HttpAction<bool>
            {
                Method = HttpMethod.Delete,
                Host = "https://api.minecraftservices.com/",
                Path = $"minecraft/profile/skins/active",

                RequestHeaders = new HttpHeaderCollection
                {
                    { "Authorization", "Bearer " + accessToken }
                },

                CheckValidation = _ =>
                {
                    if (string.IsNullOrEmpty(uuid)) return nameof(uuid);
                    else if (string.IsNullOrEmpty(accessToken)) return nameof(accessToken);
                    else return null;
                },

                ResponseHandler = HttpResponseHandlers.GetSuccessCodeResponseHandler(),
                ErrorHandler = MojangException.GetMojangErrorHandler<bool>()
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
                ErrorHandler = MojangException.GetMojangErrorHandler<string[]>()
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
                ErrorHandler = MojangException.GetMojangErrorHandler<Statistics>()
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
                    using var doc = JsonDocument.Parse(responseContent);
                    return doc.RootElement.GetProperty("items").EnumerateArray().Any();
                },
                ErrorHandler = (res, ex) => Task.FromResult(false)
            });
    }
}
