using MojangAPI.Serialization;
using HttpAction;
using MojangAPI.Cache;
using MojangAPI.Model;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace MojangAPI
{
    public class MojangAuth
    {
        private readonly HttpClient client;
        private readonly ICacheManager<Session>? cacheManager;

        public MojangAuth() : this(null) { }

        public MojangAuth(HttpClient? client) :
            this(client, new SessionFileCacheManager("mojang_auth.json"))
        { }

        public MojangAuth(HttpClient? httpClient, ICacheManager<Session>? _cacheManager)
        {
            if (httpClient == null)
                this.client = Mojang.DefaultClient.Value;
            else
                this.client = httpClient;

            this.cacheManager = _cacheManager;
        }

        private string CreateClientToken()
        {
            return Guid.NewGuid().ToString();
        }

        private void SaveSession(Session session)
        {
            if (string.IsNullOrEmpty(session.ClientToken))
            {
                Session? cachedSession = cacheManager?.ReadCache();
                if (cachedSession == null || string.IsNullOrEmpty(cachedSession.ClientToken))
                    session.ClientToken = CreateClientToken();
                else
                    session.ClientToken = cachedSession.ClientToken;
            }
            cacheManager?.SaveCache(session);
        }

        private async Task<T> Request<T>(HttpAction<T> httpAction) where T : MojangAuthResponse
        {
            if (string.IsNullOrEmpty(httpAction.Host))
                httpAction.Host = "https://authserver.mojang.com";

            T response = await client.SendActionAsync(httpAction)
                .ConfigureAwait(false);

            if (response.IsSuccess && response.Session != null)
                SaveSession(response.Session);

            return response;
        }

        private async Task<MojangAuthResponse> authResponseHandler(HttpResponseMessage response)
        {
            string content = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            if (!root.TryGetProperty("selectedProfile", out var selectedProfile))
                return new MojangAuthResponse(MojangAuthResult.NoProfile);

            return new MojangAuthResponse(MojangAuthResult.Success)
            {
                Session = new Session
                {
                    AccessToken = root.GetProperty("accessToken").GetString(),
                    UUID = selectedProfile.GetProperty("id").GetString(),
                    Username = selectedProfile.GetProperty("name").GetString()
                }
            };
        }

        private async Task<MojangAuthResponse> errorResponseHandler(HttpResponseMessage? response, Exception? ex)
        {
            if (response == null)
                return new MojangAuthResponse(MojangAuthResult.UnknownError);

            try
            {
                string content = await response.Content.ReadAsStringAsync()
                    .ConfigureAwait(false);
                MojangAuthResponse authResponse = JsonSerializer.Deserialize(content, MojangJson.GetTypeInfo<MojangAuthResponse>())
                    ?? new MojangAuthResponse(MojangAuthResult.UnknownError);

                switch (authResponse.Error)
                {
                    case "Method Not Allowed":
                    case "Not Found":
                    case "Unsupported Media Type":
                        authResponse.Result = MojangAuthResult.BadRequest;
                        break;
                    case "IllegalArgumentException":
                    case "ForbiddenOperationException":
                        authResponse.Result = MojangAuthResult.InvalidCredentials;
                        break;
                    default:
                        authResponse.Result = MojangAuthResult.UnknownError;
                        break;
                }

                return authResponse;

            }
            catch
            {
                return new MojangAuthResponse(MojangAuthResult.UnknownError);
            }
        }

        public Task<MojangAuthResponse> Authenticate(string email, string password) =>
            Authenticate(email, password, cacheManager?.ReadCache()?.ClientToken);

        public Task<MojangAuthResponse> Authenticate(string email, string password, string? clientToken) =>
            Request(new HttpAction<MojangAuthResponse>
            {
                Method = HttpMethod.Post,
                Path = "authenticate",

                Content = JsonHttpContent.Create(new AuthenticationRequest
                {
                    Username = email,
                    Password = password,
                    ClientToken = clientToken
                }, MojangJson.GetTypeInfo<AuthenticationRequest>()),

                CheckValidation = (h) =>
                {
                    if (string.IsNullOrEmpty(email)) return nameof(email);
                    else if (string.IsNullOrEmpty(password)) return nameof(password);
                    else return null;
                },
                ResponseHandler = authResponseHandler,
                ErrorHandler = errorResponseHandler
            });

        public async Task<MojangAuthResponse> TryAutoLogin()
        {
            Session? cachedSession = cacheManager?.ReadCache();
            if (cachedSession == null || cachedSession.IsEmpty)
                return new MojangAuthResponse(MojangAuthResult.InvalidSession);
            else
                return await TryAutoLogin(cachedSession)
                    .ConfigureAwait(false);
        }

        public async Task<MojangAuthResponse> TryAutoLogin(Session session)
        {
            if (session.IsEmpty)
                return new MojangAuthResponse(MojangAuthResult.InvalidSession);

            MojangAuthResponse res = await Validate(session)
                .ConfigureAwait(false);
            if (!res.IsSuccess)
                res = await Refresh(session)
                    .ConfigureAwait(false);

            return res;
        }

        public Task<MojangAuthResponse> Refresh()
        {
            Session? cachedSession = cacheManager?.ReadCache();
            if (cachedSession == null)
                return Task.FromResult(new MojangAuthResponse(MojangAuthResult.InvalidSession));
            else
                return Refresh(cachedSession);
        }

        public Task<MojangAuthResponse> Refresh(Session session) =>
            Request(new HttpAction<MojangAuthResponse>
            {
                Method = HttpMethod.Post,
                Path = "refresh",

                Content = JsonHttpContent.Create(new TokenRequest
                {
                    AccessToken = session.AccessToken,
                    ClientToken = session.ClientToken,

                    SelectedProfile = new SelectedProfile
                    {
                        Id = session.UUID,
                        Name = session.Username
                    }
                }, MojangJson.GetTypeInfo<TokenRequest>()),

                CheckValidation = (h) =>
                {
                    if (session == null) return nameof(session);
                    else if (session.IsEmpty) return "Empty session";
                    else return null;
                },
                ResponseHandler = authResponseHandler,
                ErrorHandler = errorResponseHandler
            });

        public Task<MojangAuthResponse> Validate()
        {
            Session? cachedSession = cacheManager?.ReadCache();
            if (cachedSession == null || string.IsNullOrEmpty(cachedSession.AccessToken))
                return Task.FromResult(new MojangAuthResponse(MojangAuthResult.InvalidSession));
            else
                return Validate(cachedSession);
        }

        public async Task<MojangAuthResponse> Validate(Session session)
        {
            if (session.AccessToken == null)
                throw new ArgumentException("session.AccessToken");

            MojangAuthResponse res = await Validate(session.AccessToken, session.ClientToken)
                .ConfigureAwait(false);
            if (res.IsSuccess)
                res.Session = session;
            return res;
        }

        public Task<MojangAuthResponse> Validate(string accessToken, string? clientToken) =>
            Request(new HttpAction<MojangAuthResponse>
            {
                Method = HttpMethod.Post,
                Path = "validate",

                Content = JsonHttpContent.Create(new TokenRequest
                {
                    AccessToken = accessToken,
                    ClientToken = clientToken
                }, MojangJson.GetTypeInfo<TokenRequest>()),

                CheckValidation = (h) =>
                {
                    if (string.IsNullOrEmpty(accessToken)) return nameof(accessToken);
                    else return null;
                },
                ResponseHandler = HttpResponseHandlers.GetSuccessCodeResponseHandler(
                    new MojangAuthResponse(MojangAuthResult.Success)),
                ErrorHandler = errorResponseHandler
            });

        public Task<MojangAuthResponse> Signout(string email, string password) =>
            Request(new HttpAction<MojangAuthResponse>
            {
                Method = HttpMethod.Post,
                Path = "signout",

                Content = JsonHttpContent.Create(new CredentialsRequest
                {
                    Username = email,
                    Password = password
                }, MojangJson.GetTypeInfo<CredentialsRequest>()),

                CheckValidation = (h) =>
                {
                    if (string.IsNullOrEmpty(email)) return nameof(email);
                    else if (string.IsNullOrEmpty(password)) return nameof(password);
                    else return null;
                },
                ResponseHandler = HttpResponseHandlers.GetSuccessCodeResponseHandler(
                    new MojangAuthResponse(MojangAuthResult.Success)),
                ErrorHandler = errorResponseHandler
            });

        public Task<MojangAuthResponse> Invalidate()
        {
            Session? cachedSession = cacheManager?.ReadCache();
            if (cachedSession == null || cachedSession.AccessToken == null)
                return Task.FromResult(new MojangAuthResponse(MojangAuthResult.InvalidSession));
            else
                return Invalidate(cachedSession.AccessToken, cachedSession.ClientToken);
        }

        public Task<MojangAuthResponse> Invalidate(string accessToken, string? clientToken) =>
            Request(new HttpAction<MojangAuthResponse>
            {
                Method = HttpMethod.Post,
                Path = "invalidate",

                Content = JsonHttpContent.Create(new TokenRequest
                {
                    AccessToken = accessToken,
                    ClientToken = clientToken
                }, MojangJson.GetTypeInfo<TokenRequest>()),

                CheckValidation = (h) =>
                {
                    if (string.IsNullOrEmpty(accessToken)) return nameof(accessToken);
                    else return null;
                },
                ResponseHandler = HttpResponseHandlers.GetSuccessCodeResponseHandler(
                    new MojangAuthResponse(MojangAuthResult.Success)),
                ErrorHandler = errorResponseHandler
            });

        [Obsolete("Use CmlLib.Core.Auth.Microsoft. https://github.com/CmlLib/CmlLib.Core.Auth.Microsoft")]
        public Task<MicrosoftAuthResponse> LoginWithXbox(string uhs, string xstsToken) =>
            client.SendActionAsync(new HttpAction<MicrosoftAuthResponse>
            {
                Method = HttpMethod.Post,
                Host = "https://api.minecraftservices.com",
                Path = "authentication/login_with_xbox",

                Content = new StringContent($"{{\"identityToken\": \"XBL3.0 x={uhs};{xstsToken}\"}}"),

                ResponseHandler = async (response) =>
                {
                    string content = await response.Content.ReadAsStringAsync()
                        .ConfigureAwait(false);
                    MicrosoftAuthResponse authResponse = JsonSerializer.Deserialize(content, MojangJson.GetTypeInfo<MicrosoftAuthResponse>())
                        ?? new MicrosoftAuthResponse();
                    authResponse.ExpiresOn = DateTime.Now.AddSeconds(authResponse.ExpiresIn);
                    return authResponse;
                },

                CheckValidation = (h) =>
                {
                    if (string.IsNullOrEmpty(uhs)) return nameof(uhs);
                    else if (string.IsNullOrEmpty(xstsToken)) return nameof(xstsToken);
                    else return null;
                }
            });

        [Obsolete("Use CmlLib.Core.Auth.Microsoft. https://github.com/CmlLib/CmlLib.Core.Auth.Microsoft")]
        public async Task<MojangAuthResponse> RequestSessionWithXbox(string uhs, string xstsToken)
        {
            try
            {
                MicrosoftAuthResponse authResponse = await LoginWithXbox(uhs, xstsToken)
                    .ConfigureAwait(false);

                if (!authResponse.IsSuccess)
                    return new MojangAuthResponse(MojangAuthResult.InvalidCredentials);

                Mojang mojang = new Mojang(this.client);

                if (!await mojang.CheckGameOwnership(authResponse.AccessToken!).ConfigureAwait(false))
                    return new MojangAuthResponse(MojangAuthResult.NoProfile);

                PlayerProfile profile = await mojang.GetProfileUsingAccessToken(authResponse.AccessToken!)
                    .ConfigureAwait(false);
                Session session = new Session
                {
                    Username = profile.Name,
                    AccessToken = authResponse.AccessToken,
                    UUID = profile.UUID
                };

                SaveSession(session);

                return new MojangAuthResponse(MojangAuthResult.Success)
                {
                    Session = session
                };
            }
            catch
            {
                return new MojangAuthResponse(MojangAuthResult.UnknownError);
            }
        }
    }
}
