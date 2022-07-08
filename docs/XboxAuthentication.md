## Microsoft Xbox Authentication

[wiki.vg](https://wiki.vg/Microsoft_Authentication_Scheme)

### Notice

Use [CmlLib.Core.Auth.Microsoft](https://github.com/CmlLib/CmlLib.Core.Auth.Microsoft) library.  
Microsoft xbox authenticating feature is updated only in CmlLib.Core.Auth.Microsoft library. 

### Reference

**This method is obsoleted. use CmlLib.Core.Auth.Microsoft**

[MojangAuth](./MojangAuth.md) provides methods to authenticate with Minecraft using Xbox game token.  
But it does not provide `Microsoft OAuth Flow` and `Xbox Authentication`.  
Use another library like [XboxAuthNet](https://github.com/AlphaBs/XboxAuthNet) or [xbox-webapi-csharp](https://github.com/OpenXbox/xbox-webapi-csharp).

You can acquire `XSTSToken` and `ush`(User Hash) through `Microsoft OAuth` and `Xbox Authentication`,.  
And then call `RequestSessionWithXbox` method  with these values.

Example: 
```csharp
HttpClient httpClient = new HttpClient();
MojangAuth auth = new MojangAuth(httpClient);

MojangAuthResponse res = await auth.RequestSessionWithXbox("ush", "XSTSToken");
if (res.IsSuccess)
{
    // res.Session.Username
    // res.Session.AccessToken
    // res.Session.UUID
}
else
{
    // res.Result
    // res.Error
    // res.ErrorMessage
}
```