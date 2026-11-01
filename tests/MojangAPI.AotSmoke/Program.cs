using HttpAction;
using MojangAPI;
using MojangAPI.Cache;
using MojangAPI.Model;
using MojangAPI.SecurityQuestion;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

#if LEGACY_LIBRARY
if (!JsonSerializer.IsReflectionEnabledByDefault)
    throw new Exception("The legacy compatibility test requires JSON reflection.");
#else
if (JsonSerializer.IsReflectionEnabledByDefault)
    throw new Exception("JSON reflection must be disabled for this test.");
#endif

const string uuid = "853c80ef3c3749fdaa49938b674adae6";
const string profileJson = """
    {"id":"853c80ef3c3749fdaa49938b674adae6","name":"Player","skins":[{"url":"https://example.com/skin.png","variant":"SLIM"}]}
    """;
using var handler = new StubHandler();
using var client = new HttpClient(handler);
var api = new Mojang(client);

handler.Expect("GET", "/users/profiles/minecraft/Player", """{"id":"id","name":"Player"}""");
Check((await api.GetUUID("Player")).CurrentUsername == "Player", "UUID response");

handler.Expect("POST", "/profiles/minecraft", """[{"id":"id","name":"Player"}]""", body =>
{
    Check(body.RootElement[0].GetString() == "Player", "UUID array request");
});
Check((await api.GetUUIDs(new[] { "Player" }))[0].UUID == "id", "UUID array response");

handler.Expect("GET", $"/user/profiles/{uuid}/names", """[{"name":"Old","changedToAt":123}]""");
Check((await api.GetNameHistories(uuid))[0].ChangedToAt == 123, "Name history");

handler.Expect("POST", "/minecraft/profile/skins", profileJson, body =>
{
    Check(body.RootElement.GetProperty("variant").GetString() == "slim", "Skin variant");
    Check(body.RootElement.GetProperty("url").GetString() == "https://example.com/skin.png", "Skin URL");
});
var profile = await api.ChangeSkin(
    uuid,
    "token",
    SkinType.Alex,
    "https://example.com/skin.png");
Check(profile.Name == "Player" && profile.Skin?.Model == SkinType.Alex, "Skin response");

handler.ExpectUpload();
using var skin = new MemoryStream(Encoding.UTF8.GetBytes("skin-bytes"));
await api.UploadSkin(
    "token",
    SkinType.Steve,
    skin,
    "skin.png");

handler.Expect("DELETE", "/minecraft/profile/skins/active", "");
await api.ResetSkin(uuid, "token");

handler.Expect("GET", "/minecraft/profile", profileJson);
Check((await api.GetProfileUsingAccessToken("token")).Skin?.Url == "https://example.com/skin.png", "Token profile");

var texture = Convert.ToBase64String(Encoding.UTF8.GetBytes("""
    {"textures":{"SKIN":{"url":"https://example.com/skin.png","metadata":{"model":"slim"}}}}
    """));
handler.Expect("GET", $"/session/minecraft/profile/{uuid}",
    $$"""{"id":"{{uuid}}","name":"Player","properties":[{"value":"{{texture}}"}]}""");
Check((await api.GetProfileUsingUUID(uuid)).Skin?.Model == SkinType.Alex, "Session profile");

handler.Expect("GET", "/player/attributes", """
    {"privileges":{"onlineChat":{"enabled":true}},"banStatus":{"bannedScopes":{"MULTIPLAYER":{"banId":"ban"}}}}
    """);
var attributes = await api.GetPlayerAttributes("token");
Check(attributes.Privileges?.OnlineChat?.Enabled == true, "Nested privileges");
Check(attributes.BanStatus?.BannedScopes?["MULTIPLAYER"]?.BanId == "ban", "Ban dictionary");

handler.Expect("POST", "/player/certificates", """{"keyPair":{"publicKey":"key"}}""");
Check((await api.GetPlayerCertificates("token")).KeyPair?.PublicKey == "key", "Certificates");

handler.Expect("POST", "/orders/statistics", """{"Total":42}""", body =>
{
    Check(body.RootElement.GetProperty("metricKeys")[0].GetString() == "item_sold_minecraft", "Statistics request");
});
Check((await api.GetStatistics(StatisticOption.ItemSoldMinecraft)).Total == 42, "Statistics response");

handler.Expect("GET", "/users/profiles/minecraft/missing", """{"error":"NotFound","errorMessage":"missing"}""", status: HttpStatusCode.NotFound);
try
{
    await api.GetUUID("missing");
    throw new Exception("Expected MojangException");
}
catch (MojangException error)
{
    Check(error.StatusCode == 404 && error.Error == "NotFound", "Error response");
}

var auth = new MojangAuth(client, null);
handler.Expect("POST", "/authenticate", """{"accessToken":"token","selectedProfile":{"id":"id","name":"Player"}}""", body =>
{
    Check(body.RootElement.GetProperty("agent").GetProperty("name").GetString() == "Minecraft", "Authentication agent");
    Check(!body.RootElement.TryGetProperty("clientToken", out _), "Null request fields omitted");
});
Check((await auth.Authenticate("email", "password", null)).Session?.Username == "Player", "Authentication response");

var session = new Session { Username = "Player", UUID = "id", AccessToken = "token", ClientToken = "client" };
handler.Expect("POST", "/refresh", """{"accessToken":"new","selectedProfile":{"id":"id","name":"Player"}}""", body =>
{
    Check(body.RootElement.GetProperty("selectedProfile").GetProperty("id").GetString() == "id", "Refresh profile");
});
Check((await auth.Refresh(session)).Session?.AccessToken == "new", "Refresh response");

handler.Expect("POST", "/validate", "");
Check((await auth.Validate(session)).IsSuccess, "Validate response");
handler.Expect("POST", "/invalidate", "");
Check((await auth.Invalidate("token", "client")).IsSuccess, "Invalidate response");
handler.Expect("POST", "/signout", "");
Check((await auth.Signout("email", "password")).IsSuccess, "Signout response");
handler.Expect("POST", "/authenticate", """{"error":"ForbiddenOperationException","message":"bad credentials"}""", status: HttpStatusCode.Forbidden);
Check((await auth.Authenticate("email", "bad", null)).Result == MojangAuthResult.InvalidCredentials, "Authentication error");

#pragma warning disable CS0618
handler.Expect("POST", "/authentication/login_with_xbox", """{"access_token":"minecraft","expires_in":3600,"roles":[]}""");
Check((await auth.LoginWithXbox("uhs", "xsts")).AccessToken == "minecraft", "Xbox response");
#pragma warning restore CS0618

var questions = new QuestionFlow(client);
handler.Expect("GET", "/user/security/challenges", """[{"question":{"id":1,"question":"Question"},"answer":{"id":2}}]""");
var questionList = await questions.GetQuestionList("token");
foreach (var question in questionList)
    question.Answer = "Answer";

handler.Expect("POST", "/user/security/location", "true", body =>
{
    Check(body.RootElement[0].GetProperty("answer").GetString() == "Answer", "Security answer");
});
Check(await questions.SendAnswers(questionList, "token"), "Security response");

var cachePath = Path.Combine(Path.GetTempPath(), $"mojang-aot-{Guid.NewGuid()}.json");
try
{
    var cache = new SessionFileCacheManager(cachePath);
    cache.SaveCache(session);
    Check(cache.ReadCache().AccessToken == "token", "Session cache round trip");
    File.WriteAllText(cachePath, "invalid-json");
    Check(!string.IsNullOrEmpty(cache.ReadCache().ClientToken), "Corrupt cache fallback");

    var customCache = new JsonFileCacheManager<CustomPayload>(cachePath, SmokeJsonContext.Default.CustomPayload);
    customCache.SaveCache(new CustomPayload { Value = "custom" });
    Check(customCache.ReadCache().Value == "custom", "Custom cache metadata");
}
finally
{
    File.Delete(cachePath);
}

using var content = JsonHttpContent.Create(new CustomPayload { Value = "custom" }, SmokeJsonContext.Default.CustomPayload);
using var response = new HttpResponseMessage { Content = content };
var custom = await HttpResponseHandlers.GetJsonHandler(SmokeJsonContext.Default.CustomPayload)(response);
Check(custom.Value == "custom", "Custom response metadata");
var customError = await HttpResponseHandlers.GetJsonErrorHandler(new CustomPayload(), SmokeJsonContext.Default.CustomPayload)(response, null);
Check(customError.Value == "custom", "Custom error metadata");
using var arrayResponse = new HttpResponseMessage { Content = new StringContent("""[{"Value":"array"}]""") };
var customArray = await HttpResponseHandlers.GetJsonArrayHandler(SmokeJsonContext.Default.CustomPayload)(arrayResponse);
Check(customArray[0].Value == "array", "Custom array metadata");

#if LEGACY_LIBRARY
using var legacyContent = new JsonHttpContent(new { Value = "legacy" });
using var legacyResponse = new HttpResponseMessage { Content = legacyContent };
Check((await HttpResponseHandlers.GetJsonHandler<CustomPayload>()(legacyResponse)).Value == "legacy", "Legacy custom JSON reflection");
Console.WriteLine("All netstandard2.0 compatibility checks passed.");
#else
Console.WriteLine("All NativeAOT smoke checks passed (JSON reflection disabled).");
#endif

static void Check(bool condition, string name)
{
    if (!condition)
        throw new Exception($"Failed: {name}");
}

public sealed class CustomPayload
{
    public string? Value { get; set; }
}

[JsonSerializable(typeof(CustomPayload))]
internal partial class SmokeJsonContext : JsonSerializerContext
{
}

internal sealed class StubHandler : HttpMessageHandler
{
    private Func<HttpRequestMessage, Task<HttpResponseMessage>>? next;

    public void Expect(
        string method,
        string path,
        string response,
        Action<JsonDocument>? inspect = null,
        HttpStatusCode status = HttpStatusCode.OK)
    {
        next = async request =>
        {
            if (request.Method.Method != method || request.RequestUri?.AbsolutePath != path)
                throw new Exception($"Unexpected request: {request.Method} {request.RequestUri}");

            if (inspect != null)
            {
                using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
                inspect(body);
            }

            return new HttpResponseMessage(status) { Content = new StringContent(response) };
        };
    }

    public void ExpectUpload()
    {
        next = async request =>
        {
            if (request.Method != HttpMethod.Post || request.RequestUri?.AbsolutePath != "/minecraft/profile/skins")
                throw new Exception("Unexpected upload endpoint");

            if (request.Headers.Authorization?.ToString() != "Bearer token")
                throw new Exception("Missing Minecraft token");

            var parts = ((MultipartFormDataContent)request.Content!).ToArray();
            if (await parts[0].ReadAsStringAsync() != "classic" || await parts[1].ReadAsStringAsync() != "skin-bytes")
                throw new Exception("Invalid multipart skin data");

            if (parts[1].Headers.ContentType?.MediaType != "image/png")
                throw new Exception("Invalid skin content type");

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
        };
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var expectation = next ?? throw new Exception("No request expected");
        next = null;
        return expectation(request);
    }
}
