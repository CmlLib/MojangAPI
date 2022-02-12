# Mojang API

![Discord](https://img.shields.io/discord/795952027443527690?label=discord&logo=discord&style=for-the-badge) 

.NET Library for [Mojang API](https://wiki.vg/Mojang_API), [Mojang Authentication](https://wiki.vg/Authentication) and [Microsoft Xbox Authentication](https://wiki.vg/Microsoft_Authentication_Scheme)

-   Asynchronous API
-   Getting Player Data
-   Changing Player Name or Skin
-   Mojang Authentication
-   Microsoft Authentication
-   Security Question-Answer
-   Statistics

Support:

-   netstandard 2.0

## Install

Use Nuget package [MojangAPI](https://www.nuget.org/packages/MojangAPI) or download dll from [release](https://github.com/CmlLib/MojangAPI/releases).

## Dependencies

-   Newtonsoft.Json
-   System.Net.Http

## Usage

Include these namespaces : 

```csharp
using MojangAPI;
using MojangAPI.Model;
```

Sample program: [MojangAPISample](./MojangAPISample)  

### [MojangAPI](./docs/MojangAPI.md)

Getting player profile, Changing name or skin, Statistics, Blocked Server, Checking Game Ownership

### [Authentication](./docs/MojangAuth.md)

Mojang Yggdrasil authentication. 

### [XboxAuthentication](./docs/XboxAuthentication.md)

Microsoft Xbox Authentication

### [SecurityQuestion](./docs/SecurityQuestion.md)

Security question-answer flow
