using System.Net;
using System.Text;
using MojangAPI.Model;
using Xunit;

namespace MojangAPI.Tests;

public sealed class UploadSkinTests
{
    private const string Uuid = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string SkinUrl = "https://textures.minecraft.net/texture/uploaded-skin";
    private static readonly byte[] Png = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+a7WsAAAAASUVORK5CYII=");

    [Theory]
    [InlineData(false, SkinType.Steve, "classic", "CLASSIC")]
    [InlineData(false, SkinType.Alex, "slim", "SLIM")]
    [InlineData(true, SkinType.Steve, "classic", "CLASSIC")]
    [InlineData(true, SkinType.Alex, "slim", "SLIM")]
    public async Task Upload_returns_the_profile_from_the_upload_response(
        bool useFilePath,
        SkinType model,
        string requestVariant,
        string responseVariant)
    {
        var body = $$"""
            {
              "id": "{{Uuid}}",
              "name": "SkinPlayer",
              "skins": [{
                "id": "uploaded-skin-id",
                "state": "ACTIVE",
                "url": "{{SkinUrl}}",
                "variant": "{{responseVariant}}"
              }]
            }
            """;
        using var handler = new UploadHandler(HttpStatusCode.OK, body);
        using var client = new HttpClient(handler);
        var mojang = new Mojang(client);
        PlayerProfile profile;

        if (useFilePath)
        {
            var path = Path.GetTempFileName();
            try
            {
                await File.WriteAllBytesAsync(path, Png);
                profile = await mojang.UploadSkin("test-access-token", model, path);
            }
            finally
            {
                // 파일 오버로드가 연 스트림은 요청 본문에 있으므로 임시 파일 삭제 전에 해제한다.
                handler.Dispose();
                File.Delete(path);
            }
        }
        else
        {
            using var stream = new MemoryStream(Png);
            profile = await mojang.UploadSkin(
                "test-access-token",
                model,
                stream,
                "skin.png");
        }

        Assert.Equal(1, handler.RequestCount);
        Assert.Equal(requestVariant, handler.Variant);
        Assert.Equal(Png, handler.Uploaded);
        Assert.Equal(Uuid, profile.UUID);
        Assert.Equal("SkinPlayer", profile.Name);
        Assert.NotNull(profile.Skin);
        Assert.Equal(SkinUrl, profile.Skin.Url);
        Assert.Equal(model, profile.Skin.Model);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    public async Task Upload_preserves_upstream_errors(HttpStatusCode status)
    {
        using var handler = new UploadHandler(status, """{"error":"Rejected","errorMessage":"Upload failed"}""");
        using var client = new HttpClient(handler);
        using var stream = new MemoryStream(Png);
        var mojang = new Mojang(client);

        var exception = await Assert.ThrowsAsync<MojangException>(() => mojang.UploadSkin(
            "test-access-token",
            SkinType.Steve,
            stream,
            "skin.png"));

        Assert.Equal((int)status, exception.StatusCode);
        Assert.Equal("Rejected", exception.Error);
        Assert.Equal("Upload failed", exception.ErrorMessage);
        Assert.Equal(1, handler.RequestCount);
    }

    private sealed class UploadHandler(HttpStatusCode status, string responseBody) : HttpMessageHandler
    {
        private readonly List<HttpRequestMessage> _requests = [];

        public int RequestCount => _requests.Count;
        public string? Variant { get; private set; }
        public byte[]? Uploaded { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _requests.Add(request);
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("https://api.minecraftservices.com/minecraft/profile/skins", request.RequestUri!.AbsoluteUri);
            Assert.Equal("Bearer test-access-token", request.Headers.Authorization?.ToString());
            var multipart = Assert.IsType<MultipartFormDataContent>(request.Content);
            foreach (var part in multipart)
            {
                var name = part.Headers.ContentDisposition?.Name?.Trim('"');
                if (name == "variant")
                    Variant = await part.ReadAsStringAsync(cancellationToken);
                else if (name == "file")
                    Uploaded = await part.ReadAsByteArrayAsync(cancellationToken);
            }

            return new HttpResponseMessage(status)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var request in _requests)
                    request.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
