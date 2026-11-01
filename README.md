# Mojang API

![Discord](https://img.shields.io/discord/795952027443527690?label=discord&logo=discord&style=for-the-badge) 

.NET Library for [Mojang API](https://wiki.vg/Mojang_API), [Mojang Authentication](https://wiki.vg/Authentication) and [Microsoft Xbox Authentication](https://wiki.vg/Microsoft_Authentication_Scheme)

-   Asynchronous API
-   Getting Player Data
-   Changing Player Name or Skin
-   Security Question-Answer

Support:

-   netstandard 2.0
-   .NET 10 (NativeAOT compatible)

## Install

Install Nuget package [MojangAPI](https://www.nuget.org/packages/MojangAPI)

## Documentation

[Documentation](https://cmllib.github.io/CmlLib.Core-wiki/en/mojangapi/)

[한국어](https://cmllib.github.io/CmlLib.Core-wiki/ko/mojangapi/)

## .NET 10 및 NativeAOT

.NET 10 대상은 내장 JSON 메타데이터를 소스 생성하므로, JSON 리플렉션을 비활성화한 NativeAOT 앱에서도 스킨 변경·업로드, 프로필 조회, 인증 및 세션 캐시를 사용할 수 있습니다. 기존 `netstandard2.0` 대상도 유지합니다. 기존 인증 서버의 서비스 제공 여부는 이 호환성 변경의 범위에 포함되지 않습니다.

사용자 정의 형식을 `HttpAction`의 JSON 도우미나 `JsonFileCacheManager<T>`와 함께 사용할 때는 앱의 `JsonSerializerContext`에서 생성한 `JsonTypeInfo<T>`를 전달해야 합니다.

```csharp
var content = JsonHttpContent.Create(payload, AppJsonContext.Default.MyPayload);
var handler = HttpResponseHandlers.GetJsonHandler(AppJsonContext.Default.MyPayload);
var cache = new JsonFileCacheManager<MyPayload>("cache.json", AppJsonContext.Default.MyPayload);
```

오류 응답과 배열 응답에도 각각 `GetJsonErrorHandler(defaultValue, typeInfo)`, `GetJsonArrayHandler(typeInfo)`를 사용할 수 있습니다. 기존 `JsonHttpContent(object)` 생성자와 `serialize(object)`는 일반 JIT 앱과의 호환성을 위해 유지하며, .NET 10에서는 트리밍 및 동적 코드 경고를 표시합니다. NativeAOT에서는 `Create(value, typeInfo)`를 사용하세요.

오프라인 검증은 실제 계정이나 네트워크 요청 없이 실행됩니다. JSON 리플렉션을 끄고 라이브러리 전체를 트리밍 분석 대상으로 지정하며, AOT 및 트리밍 경고는 오류로 처리합니다.

```sh
dotnet build MojangAPI/MojangAPI.csproj -c Release
dotnet run --project tests/MojangAPI.LegacySmoke -c Release
dotnet publish tests/MojangAPI.AotSmoke -c Release -r win-x64 -o artifacts/aot
./artifacts/aot/MojangAPI.AotSmoke.exe
```

Linux에서는 RID를 `linux-x64`로 바꾸고 확장자 없는 실행 파일을 실행하세요. 네이티브 빌드 도구가 필요하며, CI에서 Windows와 Linux의 게시 및 실행을 검증합니다.
