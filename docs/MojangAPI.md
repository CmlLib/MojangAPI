## Mojang

Most methods return `MojangAPIResponse` or class inherited from `MojangAPIResponse`.  
You can check whether the request was successful or failed to check `IsSuccess` property in `MojangAPIResponse`.  
If `IsSuccess` is false, `Error` and `ErrorMessage` property tell you why the request failed.  

Example: 
```csharp
HttpClient httpClient = new HttpClient();
Mojang mojang = new Mojang(httpClient);

MojangAPIResponse response = await mojang.something();
if (response.isSuccess)
{
    Console.WriteLine("Success!");
}
else
{
    Console.WriteLine(response.Error);
    Console.WriteLine(response.ErrorMessage);
}
```

#### GetUUID

```csharp
PlayerUUID uuid = await mojang.GetUUID("username");

// uuid.UUID
// uuid.IsLegacy
// uuid.IsDemo
```

#### GetUUIDs
```csharp
PlayerUUID[] uuids = await mojang.GetUUIDs(new string[] { "user1", "user2" });

if (uuids == null)
    Console.WriteLine("Failed to request");
else
{
    foreach (PlayerUUID uuid in uuids)
    {
        Console.WriteLine(uuid.UUID);
    }
}
```

*note: this method return null if the request was failed*

#### GetNameHistories
```csharp
NameHistoryResponse response = await mojang.GetNameHistories("uuid");

if (response.IsSuccess)
{
    foreach (NameHistory item in response.Histories)
    {
        // item.Name
        // item.ChangedToAt
        // item.ChangedTime
    }
}
else
{
    Console.WriteLine(response.Error);
    Console.WriteLine(response.ErrorMessage);
}
```

#### GetProfileUsingUUID

```csharp
PlayerProfile profile = await mojang.GetProfileUsingUUID("uuid");

// profile.UUID
// profile.Name
// profile.Skin
// profile.IsLegacy
```

#### GetProfileUsingAccessToken

```csharp
PlayerProfile profile = await mojang.GetProfileUsingAccessToken("accessToken");
```

#### ChangeName

```csharp
PlayerProfile profile = await mojang.ChangeName("accessToken", "newName");
```

#### ChangeSkin

```csharp
MojangAPIResponse response = await mojang.ChangeSkin("uuid", "accessToken", SkinType.Steve, "skinUrl");
```

#### UploadSkin

```csharp
MojangAPIResponse response = await mojang.UploadSkin("accessToken", SkinType.Steve, "skin_png_file_path");
```
```csharp
Stream stream; // create stream for uploading skin
MojangAPIResponse response = await mojang.UploadSkin("accessToken", SkinType.Steve, stream, "file_name");
```

#### ResetSkin

```csharp
MojangAPIResponse response = await mojang.ResetSkin("uuid", "accessToken");
```

#### GetBlockedServer

```csharp
string[] servers = await mojang.GetBlockedServer();
```

#### GetStatistics

```csharp
Statistics stats = await mojang.GetStatistics(
    StatisticOption.ItemSoldMinecraft,
    StatisticOption.ItemSoldCobalt
);

// stats.Total
// stats.Last24h
// stats.SaleVelocityPerSeconds
```

#### CheckGameOwnership

```csharp
bool result = await mojang.CheckGameOwnership("accessToken");

if (result)
    Console.WriteLine("You have Minecraft JE");
else
    Console.WriteLine("You don't have Minecraft JE");
```

*note: this method works only accessToken from Microsoft login*