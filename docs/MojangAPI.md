## class Mojang

If the request is failed, it throws appropriate exception. For example, `MojangException` is thrown when the mojang server returns error message.

### How to get `AccessToken` or `UUID`?

You can get these token by [Mojang Authentication](./MojangAuth.md) or [Microsoft Xbox Authentication](./XboxAuthentication.md)

`CmlLib.Core` can also get `AccessToken` and `UUID` using MLogin class. [doc](https://github.com/CmlLib/CmlLib.Core/wiki/Login-and-Sessions)

### Methods

#### GetUUID

username -> uuid

```csharp
Mojang mojang = new Mojang(new HttpClient());
PlayerUUID uuid = await mojang.GetUUID("username");

// uuid.UUID
// uuid.IsLegacy
// uuid.IsDemo
```

#### GetUUIDs

usernames -> uuids

```csharp
Mojang mojang = new Mojang(new HttpClient());
PlayerUUID[] uuids = await mojang.GetUUIDs(new string[] { "user1", "user2" });

foreach (PlayerUUID uuid in uuids)
{
    Console.WriteLine(uuid.UUID);
}
```

#### GetNameHistories
```csharp
Mojang mojang = new Mojang(new HttpClient());
NameHistory[] response = await mojang.GetNameHistories("uuid");

foreach (NameHistory item in response.Histories)
{
    // item.Name
    // item.ChangedToAt
    // item.ChangedTime
}
```

#### GetProfileUsingUUID

```csharp
Mojang mojang = new Mojang(new HttpClient());
PlayerProfile profile = await mojang.GetProfileUsingUUID("uuid");

// profile.UUID
// profile.Name
// profile.Skin
// profile.IsLegacy
```

#### GetProfileUsingAccessToken

*note: this api only works on xbox account*

```csharp
Mojang mojang = new Mojang(new HttpClient());
PlayerProfile profile = await mojang.GetProfileUsingAccessToken("accessToken");
```

#### GetPlayerAttributes

*note: this api only works on xbox account*

```csharp
Mojang mojang = new Mojang(new HttpClient());
PlayerAttributes attributes = await mojang.GetPlayerAttributes("accessToken");

// attributes.Privileges.OnlineChat
// attributes.Privileges.MultiplayerServer
// attributes.Privileges.MultiplayerRealms
// attributes.Privileges.Telemtry
// attributes.ProfanityFilterPreferences.ProfanityFilterOn
```

#### GetPlayerBlocklist

*note: this api only works on xbox account*

```csharp
Mojang mojang = new Mojang(new HttpClient());
string[] blocklists = await mojang.GetPlayerBlocklist("accessToken");
```

#### GetPlayerCertificates

*note: this api only works on xbox account*

```csharp
Mojang mojang = new Mojang(new HttpClient());
PlayerCertificates certs = await mojang.GetPlayerCertificates("accessToken");

// certs.KeyPair.PrivateKey
// certs.KeyPair.PublicKey
// certs.PublicKeySignature
// certs.ExpiresAt
// certs.RefreshedAfter
```

#### CheckNameAvailability

```csharp
Mojang mojang = new Mojang(new HttpClient());
string? result = await mojang.CheckNameAvailability("accessToken", "newName");
```

#### ChangeName

*note: this api only works on xbox account*

```csharp
Mojang mojang = new Mojang(new HttpClient());
PlayerProfile profile = await mojang.ChangeName("accessToken", "newName");
```

#### ChangeSkin

*note: this api only works on xbox account*

```csharp
Mojang mojang = new Mojang(new HttpClient());

// SkinType.Steve or SkinType.Alex
PlayerProfile response = await mojang.ChangeSkin("uuid", "accessToken", SkinType.Steve, "skinUrl");
```

#### UploadSkin

*note: this api only works on xbox account*

업로드 응답의 `PlayerProfile`을 반환한다. 반환값의 `UUID`, `Name`, `Skin.Url`, `Skin.Model`로 변경된 프로필을 확인할 수 있으며, 별도의 프로필 조회 요청은 보내지 않는다.

```csharp
Mojang mojang = new Mojang(new HttpClient());

// SkinType.Steve 또는 SkinType.Alex를 지정한다.
PlayerProfile profile = await mojang.UploadSkin("accessToken", SkinType.Steve, "skin_png_file_path");
```
```csharp
Mojang mojang = new Mojang(new HttpClient());
using Stream stream = File.OpenRead("skin.png");
PlayerProfile profile = await mojang.UploadSkin(
    "accessToken",
    SkinType.Steve,
    stream,
    "skin.png");
```

반환값이 필요 없는 기존 `await mojang.UploadSkin(...)` 코드는 그대로 사용할 수 있다. 다만 두 오버로드의 반환형이 `Task`에서 `Task<PlayerProfile>`로 변경되므로, 이전 버전으로 컴파일한 호출 애플리케이션은 다시 빌드해야 한다.

#### ResetSkin

*note: this api only works on xbox account*

```csharp
Mojang mojang = new Mojang(new HttpClient());
await mojang.ResetSkin("uuid", "accessToken");
```

#### GetBlockedServer

```csharp
Mojang mojang = new Mojang(new HttpClient());
string[] servers = await mojang.GetBlockedServer();
```

#### GetStatistics

*note: this api was obsoleted by mojang*

```csharp
Mojang mojang = new Mojang(new HttpClient());
Statistics stats = await mojang.GetStatistics(
    StatisticOption.ItemSoldMinecraft,
    StatisticOption.ItemSoldCobalt
);

// stats.Total
// stats.Last24h
// stats.SaleVelocityPerSeconds
```

#### CheckGameOwnership

*note1: this api only works on xbox account*  
*note2: this api does not check xbox game pass. if the user has xbox game pass instead of purchase minecraft, this api return `false`. but the user owns minecraft and can play it.*

```csharp
Mojang mojang = new Mojang(new HttpClient());
bool result = await mojang.CheckGameOwnership("accessToken");

if (result)
    Console.WriteLine("You have Minecraft JE");
else
    Console.WriteLine("You don't have Minecraft JE");
```
