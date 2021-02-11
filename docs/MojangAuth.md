## MojangAuth

Most methods return `MojangAuthResponse` or class inherited from `MojangAuthResponse`.  
You can check whether the request was successful or failed to check `IsSuccess` property in `MojangAuthResponse`.  
`Session` property is the result of authenticate. It contains `Username`, `AccessToken`, and `UUID`  
If `IsSuccess` is false, `Result`, `Error`, and `ErrorMessage` property tell you why the request failed.  

`MojangAuth` has its own session caching manager. You don't have to type mojang email and password everytime when you login. Login once using your mojang email and password, and `MojangAuth` will save your session information into json file(not contain your raw password, it contains only tokens). Next time when you login just call `TryAutoLogin`. it authenticate you using cached session without typing mojang email and password.

Example: 

```csharp
HttpClient httpClient = new HttpClient();
MojangAuth auth = new MojangAuth(httpClient);

MojangAuthResponse res;

res = await auth.TryAutoLogin(); // login using cached session
if (!res.IsSuccess) // failed to login using cached session
{
    Console.WriteLine(res.Result.ToString());

    // it will save your login information into json file
    res = await auth.Authenticate("mojang email", "password");
}

Console.WriteLine(res.ToString());

Session session = res.Session;
// session.Username
// session.UUID
// session.AccessToken
```

### Constructor

#### public MojangAuth(HttpClient client)

Same as `new MojangAuth(client, new SessionFileCacheManager("mojang_auth.json"))`

#### public MojangAuth(HttpClient httpClient, ICacheManager\<Session\> _cacheManager)

Initialize a new instance of the `MojangAuth` class with the specified cache manager.  
If `_cacheManager` is null, `MojangAuth` does not cache session.  
If you want to change cache file path, create a instance of `SessionFileCacheManager` with file path.  
Example: 
```csharp
MojangAuth auth = new MojangAuth(client, new SessionFileCacheManager("session_file_path.json"));
```

### Methods

#### Authenticate

```csharp
// using cached client token or create new client token if it was null.
MojangAuthResponse res = await auth.Authenticate("mojang email", "password");

// using the specified client token
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
// call Validate() and Refersh() using cached session.
MojangAuthResponse res = await auth.TryAutoLogin();
```

#### Refresh

```csharp
// using cached session
MojangAuthResponse res = await auth.Refresh();
```
```csharp
// using the specified session
MojangAuthResponse res = await auth.Refresh(session);
```

#### Validate

```csharp
// using cached session
MojangAuthResponse res = await auth.Validate();
```
```csharp
// using the specified session
MojangAuthResponse res = await auth.Validate(session);
```

#### Signout

```csharp
MojangAuthResponse res = await auth.Signout("mojang email", "password");
```

#### Invalidate

```csharp
// using cached session
MojangAuthResponse res = await auth.Invalidate();
```
```csharp
// using the specified session
MojangAuthResponse res = await auth.Invalidate("accessToken", "clientToken");
```
*note: 'clientToken' can be null*