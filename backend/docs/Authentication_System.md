# WordMaster Kimlik Doğrulama Sistemi (Authentication Architecture)

Bu doküman, WordMaster projesinde kullanılan **ASP.NET Core Identity** ve **JWT (JSON Web Token)** tabanlı kimlik doğrulama sisteminin mimarisini ve işleyişini açıklar.

## 1. Genel Bakış

Sistem, modern ve güvenli bir kimlik doğrulama altyapısı sunmak için tasarlanmıştır. **Hibrit Token Stratejisi** kullanılarak maksimum güvenlik sağlanır.

**Temel Teknolojiler:**
*   **ASP.NET Core Identity:** Kullanıcı, rol ve parola yönetimi için standart kütüphane.
*   **JWT (JSON Web Token):** Stateless (sunucu tarafında oturum tutmayan) kimlik doğrulama mekanizması.
*   **Refresh Token:** HttpOnly cookie ile güvenli şekilde saklanır.
*   **Redis:** Security Stamp ve cache yönetimi için.

## 2. Hibrit Token Stratejisi

WordMaster, güvenlik için **hibrit token stratejisi** kullanır:

```
┌─────────────────────────────────────────────────────────────────┐
│                     HİBRİT TOKEN YAPISI                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ACCESS TOKEN                        REFRESH TOKEN               │
│  ─────────────                       ─────────────               │
│  • Süre: 15 dakika                   • Süre: 7 gün               │
│  • Saklama: Frontend Memory          • Saklama: HttpOnly Cookie  │
│  • Gönderim: Authorization header    • Gönderim: Browser otomatik│
│  • XSS Riski: Düşük                  • XSS Riski: YOK ✅          │
│                                                                  │
│  Cookie Özellikleri:                                             │
│   ✅ HttpOnly = true (JavaScript erişemez)                       │
│   ✅ Secure = true (Sadece HTTPS)                                │
│   ✅ SameSite = Strict (CSRF koruması)                           │
│   ✅ Path = /api (Sadece API isteklerinde)                       │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 2.1. Neden Bu Strateji?

| Depolama Yöntemi | XSS Riski | CSRF Riski | Notlar |
|-----------------|-----------|------------|--------|
| localStorage | ❌ Yüksek | ✅ Yok | JavaScript ile erişilebilir |
| sessionStorage | ❌ Yüksek | ✅ Yok | JavaScript ile erişilebilir |
| HttpOnly Cookie | ✅ Yok | ⚠️ Orta | SameSite ile CSRF önlenir |
| **Memory + Cookie** | ✅ En Güvenli | ✅ Korumalı | **Bizim yaklaşımımız** |

## 3. Temel Bileşenler

### 3.1. Varlıklar (Entities)
*   **AppUser:** `IdentityUser` sınıfından türetilmiştir. `RefreshToken` (hash'lenmiş) ve `RefreshTokenExpires` bilgilerini tutar.
*   **AppRole:** `IdentityRole` sınıfından türetilmiştir.

### 3.2. Servisler
*   **LoginService:** Kullanıcı giriş, çıkış, şifre sıfırlama işlemlerini yönetir. `LoginInternalResponse` döner.
*   **JwtService:** JWT Access Token ve Refresh Token üretimi, doğrulaması ve yenileme işlemlerini yapar.
*   **RefreshTokenCookieHelper:** HttpOnly cookie okuma/yazma/silme işlemlerini merkezi olarak yönetir.

### 3.3. Response Modelleri
*   **LoginResponse:** Sadece `AccessToken` ve `ExpiresAt` içerir. RefreshToken **body'de YER ALMAZ**.
*   **LoginInternalResponse:** Controller'a dönen internal model. Hem AccessToken hem RefreshToken içerir.
*   **RefreshTokenResponse:** Token yenileme sonucu dönen model. Sadece AccessToken içerir.

## 4. Kimlik Doğrulama Akışı (Authentication Flow)

### 4.1. Giriş (Login)
```
┌─────────┐          ┌─────────────┐          ┌───────────────┐
│ Frontend│          │AuthController│          │  LoginService │
└────┬────┘          └──────┬──────┘          └───────┬───────┘
     │  POST /auth/login    │                         │
     │─────────────────────>│                         │
     │                      │    LoginAsync()         │
     │                      │────────────────────────>│
     │                      │                         │
     │                      │  LoginInternalResponse  │
     │                      │  (AccessToken+RefreshToken)
     │                      │<────────────────────────│
     │                      │                         │
     │    Set-Cookie:       │                         │
     │    refreshToken=xxx  │                         │
     │    (HttpOnly,Secure) │                         │
     │<─────────────────────│                         │
     │                      │                         │
     │ JSON Response:       │                         │
     │ { accessToken, expiresAt }                     │
     │<─────────────────────│                         │
```

1.  Kullanıcı `/api/auth/login` endpoint'ine email ve şifresini gönderir.
2.  `LoginService` kullanıcıyı doğrular.
3.  Başarılıysa:
    *   **Access Token** üretilir (15 dakika geçerli).
    *   **Refresh Token** üretilir ve **hash'lenerek** veritabanına kaydedilir.
    *   Security Stamp Redis'e cache'lenir.
4.  `AuthController`:
    *   Refresh Token'ı **HttpOnly cookie** olarak set eder.
    *   Access Token'ı JSON response body'de döner.
5.  Frontend, Access Token'ı **memory'de** (React State) saklar.

### 4.2. Token Yenileme (Refresh Token Flow)
```
┌─────────┐          ┌─────────────┐          ┌───────────────┐
│ Frontend│          │AuthController│          │   JwtService  │
└────┬────┘          └──────┬──────┘          └───────┬───────┘
     │  POST /refresh-token │                         │
     │  + Cookie: refreshToken                        │
     │─────────────────────>│                         │
     │                      │                         │
     │                      │ Cookie'den refreshToken │
     │                      │ oku                     │
     │                      │                         │
     │                      │  RefreshAccessTokenAsync│
     │                      │────────────────────────>│
     │                      │                         │
     │                      │  RefreshTokenInternalResponse
     │                      │<────────────────────────│
     │                      │                         │
     │  Set-Cookie:         │                         │
     │  refreshToken=YENİ   │                         │
     │<─────────────────────│                         │
     │                      │                         │
     │ { accessToken, expiresAt }                     │
     │<─────────────────────│                         │
```

1.  Access Token süresi dolduğunda, Frontend `/api/auth/refresh-token` endpoint'ine istek atar.
2.  **Refresh Token, HttpOnly cookie olarak browser tarafından otomatik gönderilir.**
3.  `AuthController`, cookie'den refresh token'ı okur.
4.  `JwtService`:
    *   Veritabanındaki hash'li token ile karşılaştırır.
    *   Süre kontrolü yapar.
    *   Yeni Access Token ve **yeni Refresh Token** üretir (Rotation).
5.  Yeni Refresh Token cookie olarak set edilir, Access Token body'de döner.

### 4.3. Çıkış (Logout)
1.  Kullanıcı `/api/auth/logout` endpoint'ine istek atar.
2.  `LoginService`:
    *   Veritabanındaki Refresh Token bilgisini siler.
    *   Security Stamp'i günceller (tüm token'ları geçersiz kılar).
    *   Redis cache'ini temizler.
3.  `AuthController`, Refresh Token cookie'sini siler.
4.  Frontend, memory'deki Access Token'ı temizler.

## 5. Güvenlik Önlemleri

### 5.1. Refresh Token Güvenliği
*   **Hash'leme:** Refresh token veritabanına **SHA256 hash** olarak kaydedilir. Veritabanı sızsa bile plain token ele geçirilemez.
*   **Rotation:** Her yenilemede yeni refresh token üretilir, eski token geçersiz olur.
*   **HttpOnly Cookie:** JavaScript kesinlikle erişemez (XSS koruması).
*   **Secure Flag:** Sadece HTTPS üzerinden gönderilir.
*   **SameSite=Strict:** Cross-site isteklerde gönderilmez (CSRF koruması).

### 5.2. Security Stamp & Token İptali
*   Her Access Token içine kullanıcının `SecurityStamp` değeri gömülür.
*   Kritik işlemlerde (şifre değişimi, çıkış) stamp güncellenir.
*   Middleware, token içindeki stamp ile Redis/DB'dekini karşılaştırır.

### 5.3. IP Adresi Kontrolü (Admin)
Admin paneli girişlerinde IP adresi `AllowedIpAddressService` ile kontrol edilir.

### 5.4. Response Headers
Token doğrulama hatalarında özel header'lar eklenir:
*   `Token-Expired: true` - Token süresi dolmuş
*   `Token-Invalid: true` - Token imzası geçersiz

## 6. Konfigürasyon

### 6.1. appsettings.json Ayarları

```json
"Jwt": {
  "Key": "En_Az_32_Karakterlik_Gizli_Anahtar",
  "Issuer": "WordMasterAPI",
  "Audience": "WordMasterClient",
  "ExpiresInMinutes": 15,
  "RefreshTokenExpiresInDays": 7
}
```

| Ayar | Açıklama | Önerilen Değer |
|------|----------|----------------|
| `ExpiresInMinutes` | Access Token süresi | 15 dakika |
| `RefreshTokenExpiresInDays` | Refresh Token süresi | 7 gün |

### 6.2. Cookie Ayarları (RefreshTokenCookieHelper)

```csharp
var cookieOptions = new CookieOptions
{
    HttpOnly = true,           // JavaScript erişemez
    Secure = true,             // Sadece HTTPS
    SameSite = SameSiteMode.Strict, // CSRF koruması
    Path = "/api",             // Sadece API isteklerinde
    IsEssential = true         // GDPR
};
```

## 7. Frontend Entegrasyonu

### 7.1. Token Saklama (Memory-Based)
```typescript
// Access Token: React State (Memory)
const [accessToken, setAccessToken] = useState<string | null>(null);

// Refresh Token: Browser tarafından HttpOnly cookie olarak yönetilir
// Frontend refresh token'a ERİŞEMEZ (bu güvenlik için iyi!)
```

### 7.2. API İstekleri
```typescript
// Axios instance - credentials: true ile cookie'ler otomatik gönderilir
const api = axios.create({
    baseURL: BASE_URL,
    withCredentials: true, // HttpOnly cookie'leri gönder
});

// Her istekte Authorization header eklenir
config.headers.Authorization = `Bearer ${accessToken}`;
```

### 7.3. Sayfa Yenileme
Sayfa yenilendiğinde Access Token (memory'de olduğu için) kaybolur. Frontend otomatik olarak `/auth/refresh-token` çağırır ve cookie'deki refresh token ile yeni access token alır.

## 8. Event'ler (JWT Bearer)

| Event | Açıklama |
|-------|----------|
| `OnTokenValidated` | SecurityStamp kontrolü yapılır |
| `OnChallenge` | 401 için custom JSON response |
| `OnForbidden` | 403 için custom JSON response |
| `OnAuthenticationFailed` | Token expired/invalid header'ları eklenir |
