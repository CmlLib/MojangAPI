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

`MojangAuth`는 세션을 저장하지 않습니다. 인증 결과의 `Session`은 호출자가 보관하고, 재인증 시 `TryAutoLogin(session)`, `Refresh(session)`, `Validate(session)`에 직접 전달해야 합니다.

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

지정한 `HttpClient`로 인증 요청을 전송합니다. `null`을 전달하면 기본 클라이언트를 사용합니다. 세션 파일은 생성하거나 읽지 않습니다.

### Methods

#### Authenticate

```csharp
// 호출마다 새로운 클라이언트 토큰을 생성한다.
MojangAuthResponse res = await auth.Authenticate("mojang email", "password");

// 지정한 클라이언트 토큰을 사용한다.
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
// 전달한 세션을 검증하고, 검증에 실패하면 갱신한다.
MojangAuthResponse res = await auth.TryAutoLogin(session);
```

#### Refresh

```csharp
// 호출자가 보관한 세션 또는 토큰을 전달한다.
MojangAuthResponse res = await auth.Refresh(session);
```

#### Validate

```csharp
// 호출자가 보관한 세션 또는 토큰을 전달한다.
MojangAuthResponse res = await auth.Validate(session);
```

#### Signout

```csharp
MojangAuthResponse res = await auth.Signout("mojang email", "password");
```

#### Invalidate

```csharp
// 호출자가 보관한 세션 또는 토큰을 전달한다.
MojangAuthResponse res = await auth.Invalidate("accessToken", "clientToken");
```
*note: 'clientToken' can be null*
