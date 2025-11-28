# WordMaster – Code Style: Summary Documentation & Primary Constructors

Bu doküman, WordMaster Backend API içinde kod yazarken uygulanacak olan ortak standartları tanımlar.
Amaç, kodun okunabilirliğini artırmak, mimari tutarlılığı sağlamak ve .NET 8/9+ modern özelliklerinden maksimum fayda sağlamaktır.

---

## 1. Tüm Controller Action Metodlarında `/// <summary>` Kullanımı

### ✔ Kural:
Her public action metodunun *mutlaka* açıklayıcı bir özet (`/// <summary>`) bölümü olmalıdır. Swagger dokümantasyonu bu özetleri kullanır.

### ✔ Örnek:

```csharp
/// <summary>
/// Kullanıcının giriş yapmasını sağlar.
/// </summary>
/// <param name="request">Kullanıcı giriş bilgileri</param>
/// <returns>JWT access token ve kullanıcı bilgileri</returns>
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    // ...
}
```

---

## 2. Primary Constructors Kullanımı

### ✔ Kural:
Sınıf bağımlılıklarını (Dependency Injection) tanımlarken, klasik constructor yerine C# 12 ile gelen **Primary Constructors** yapısı tercih edilmelidir. Bu, kodun daha sade ve okunabilir olmasını sağlar.

### ❌ Eski Yöntem (Klasik Constructor):

```csharp
public class AuthController : BaseController
{
    private readonly ILoginService _loginService;

    public AuthController(ILoginService loginService)
    {
        _loginService = loginService;
    }
}
```

### ✔ Yeni Yöntem (Primary Constructor):

```csharp
public class AuthController(ILoginService loginService) : BaseController
{
    // _loginService field'ına gerek yoktur, parametre direkt kullanılabilir.
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await loginService.LoginAsync(request); // 'this.' veya '_' olmadan direkt erişim
        return CreateResult(result);
    }
}
```
