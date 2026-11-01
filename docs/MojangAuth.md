## What is Minecraft `Session`?

`Session` is an authentication information, containing `AccessToken`, `UUID` and `Username`.  
`AccessToken` is a temporary token issued after login. ([wiki](https://en.wikipedia.org/wiki/Access_token)).  
`UUID` is a unique value given to each player. This value cannot be changed. ([wiki](https://en.wikipedia.org/wiki/Universally_unique_identifier))  
`Username` is player name. This value can be changed by user.  

Some APIs (changing skin or name, etc) require you `Session`, and you can get this by login.  
`MojangAuth` class provides methods to login.

## class MojangAuth

Most methods return `MojangAuthResponse` or class inherited from `MojangAuthResponse`.  
You can check whether the request was successful or failed to check `IsSuccess` property in `MojangAuthResponse`.  
`Session` property is the result of authenticate. It contains `Username`, `AccessToken`, and `UUID`  
If `IsSuccess` is false, `Result`, `Error`, and `ErrorMessage` property tell you why the request failed.  

`MojangAuth` does not persist sessions. Callers must keep the returned `Session` and pass it explicitly to `TryAutoLogin(session)`, `Refresh(session)`, or `Validate(session)`.

There are two ways to login. One is `Yggdrasil Mojang Authentication` which is described below, and the other is `Microsoft Xbox Authentication` which is new way to login.  
[Microsoft Xbox Authentication](./XboxAuthentication.md)

Example: 

```csharp
HttpClient httpClient = new HttpClient();
MojangAuth auth = new MojangAuth(httpClient);

MojangAuthResponse res = await auth.Authenticate("mojang email", "password");

Console.WriteLine(res.ToString());

Session session = res.Session;
// session.Username
// session.UUID
// session.AccessToken
```

### Constructor

#### public MojangAuth(HttpClient client)

Sends authentication requests using the supplied `HttpClient`, or the default client if `null` is supplied. No session files are created or read.

### Methods

#### Authenticate

```csharp
// Generate a new client token for each authentication request.
MojangAuthResponse res = await auth.Authenticate("mojang email", "password");

// Use the supplied client token.
MojangAuthResponse res = await auth.Authenticate("mojang email", "password", "clientToken");

if (res.IsSuccess)
{
    // res.Session
}
else
{
    // res.Result
    // res.Error
    // res.ErrorMessage
}
```

#### TryAutoLogin

```csharp
// Validate the supplied session and refresh it if validation fails.
MojangAuthResponse res = await auth.TryAutoLogin(session);
```

#### Refresh

```csharp
// Pass the session or tokens managed by the caller.
MojangAuthResponse res = await auth.Refresh(session);
```

#### Validate

```csharp
// Pass the session or tokens managed by the caller.
MojangAuthResponse res = await auth.Validate(session);
```

#### Signout

```csharp
MojangAuthResponse res = await auth.Signout("mojang email", "password");
```

#### Invalidate

```csharp
// Pass the session or tokens managed by the caller.
MojangAuthResponse res = await auth.Invalidate("accessToken", "clientToken");
```
*note: 'clientToken' can be null*
