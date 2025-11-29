# WordMaster Kimlik Doğrulama Sistemi (Authentication Architecture)

Bu doküman, WordMaster projesinde kullanılan **ASP.NET Core Identity** ve **JWT (JSON Web Token)** tabanlı kimlik doğrulama sisteminin mimarisini ve işleyişini açıklar.

## 1. Genel Bakış

Sistem, modern ve güvenli bir kimlik doğrulama altyapısı sunmak için tasarlanmıştır. Kullanıcıların güvenli bir şekilde sisteme giriş yapmasını, oturumlarını sürdürmesini ve yetkisiz erişimlerin engellenmesini sağlar.

**Temel Teknolojiler:**
*   **ASP.NET Core Identity:** Kullanıcı, rol ve parola yönetimi için standart kütüphane.
*   **JWT (JSON Web Token):** Stateless (sunucu tarafında oturum tutmayan) kimlik doğrulama mekanizması.
*   **Refresh Token:** Access token süresi dolduğunda kullanıcıyı tekrar giriş yapmaya zorlamadan oturumu yenilemek için kullanılır.

## 2. Temel Bileşenler

### 2.1. Varlıklar (Entities)
*   **AppUser:** `IdentityUser` sınıfından türetilmiştir. Kullanıcı bilgilerini (Email, PasswordHash, SecurityStamp vb.) ve ek olarak `RefreshToken` bilgilerini tutar.
*   **AppRole:** `IdentityRole` sınıfından türetilmiştir. Kullanıcı rollerini tanımlar.

### 2.2. Servisler
*   **LoginService:** Kullanıcı giriş, çıkış, şifre sıfırlama işlemlerini yönetir.
*   **JwtService:** JWT Access Token ve Refresh Token üretimi, doğrulaması ve yenileme işlemlerini yapar.
*   **UserService:** Kullanıcı profili ve token güncelleme işlemlerini yürütür.

## 3. Kimlik Doğrulama Akışı (Authentication Flow)

### 3.1. Kayıt (Registration)
1.  Kullanıcı `/api/auth/register` endpoint'ine bilgilerini gönderir.
2.  `RegisterService`, Identity kütüphanesini kullanarak kullanıcıyı oluşturur.
3.  Şifreler hash'lenerek güvenli bir şekilde saklanır.

### 3.2. Giriş (Login)
1.  Kullanıcı `/api/auth/login` endpoint'ine email ve şifresini gönderir.
2.  `LoginService`, `SignInManager` ile kullanıcıyı doğrular.
3.  Doğrulama başarılıysa:
    *   **Access Token** üretilir (Kullanıcı ID, Email, Roller ve SecurityStamp içerir).
    *   **Refresh Token** üretilir (Rastgele 64-byte string).
    *   Refresh Token veritabanına kaydedilir.
    *   Security Stamp, performans için Redis'e cache'lenir.
4.  Kullanıcıya Access Token ve Refresh Token dönülür.

### 3.3. Token Yenileme (Refresh Token Flow)
Access Token'ın ömrü kısadır (Örn: 15-60 dakika). Süresi dolduğunda:
1.  Frontend, `/api/auth/refresh-token` endpoint'ine süresi dolmuş Access Token ve elindeki Refresh Token'ı gönderir.
2.  `JwtService`:
    *   Access Token formatını doğrular (Süresi dolmuş olsa bile).
    *   İçindeki kullanıcı ID'sini alır.
    *   Veritabanındaki Refresh Token ile gelen token'ı karşılaştırır.
    *   Refresh Token süresinin dolup dolmadığını kontrol eder.
3.  Her şey geçerliyse, **yeni bir Access Token ve yeni bir Refresh Token** üretilir.
4.  Eski Refresh Token geçersiz kılınır (Refresh Token Rotation) ve yenisi kaydedilir.

### 3.4. Çıkış (Logout)
1.  Kullanıcı `/api/auth/logout` endpoint'ine istek atar.
2.  `LoginService`:
    *   Kullanıcının veritabanındaki Refresh Token bilgisini siler (`null` yapar).
    *   **Security Stamp**'i günceller. Bu işlem, o ana kadar üretilmiş tüm Access Token'ları anında geçersiz kılar.
    *   Redis'teki Security Stamp cache'ini temizler.

## 4. Güvenlik Önlemleri

### 4.1. Security Stamp & Token İptali
JWT normalde stateless olduğu için sunucu tarafında iptal edilemez (süresi dolana kadar geçerlidir). Ancak WordMaster'da **Security Stamp** mekanizması ile bu aşılmıştır:
*   Her Access Token içine kullanıcının o anki `SecurityStamp` değeri gömülür.
*   Kritik işlemlerde (Şifre değişimi, Çıkış yapma, Rol değişimi) kullanıcının veritabanındaki Security Stamp değeri değiştirilir.
*   Gelen isteklerde middleware veya Identity, token içindeki stamp ile veritabanındakini (veya Redis'tekini) karşılaştırır. Eşleşmezse token reddedilir.

### 4.2. IP Adresi Kontrolü (Admin)
Admin paneli girişlerinde (`/api/auth/admin-login`), kullanıcının IP adresi `AllowedIpAddressService` ile kontrol edilir. İzin verilmeyen IP'lerden gelen admin girişleri engellenir.

### 4.3. Şifreleme
*   Şifreler ASP.NET Core Identity'nin standart hashing algoritmalarıyla saklanır.
*   Şifre sıfırlama işlemlerinde URL'deki hassas veriler (UserId) `DataProtectionHelper` ile şifrelenir.

## 5. Konfigürasyon ve Ayarlar

Projedeki JWT yapılandırması `appsettings.json` dosyasından okunur ve `ServiceCollectionExtensions.cs` içerisinde `AddJwtConfigurations` metodu ile sisteme entegre edilir.

### 5.1. appsettings.json Ayarları

```json
"Jwt": {
  "Key": "Buraya_En_Az_32_Karakterlik_Gizli_Bir_Anahtar_Girin",
  "Issuer": "WordMasterAPI",
  "Audience": "WordMasterClient",
  "ExpiresInMinutes": 60,
  "RefreshTokenExpiresInDays": 7
}
```

*   **Key (Gizli Anahtar):** Token'ların imzalanması (signing) ve doğrulanması için kullanılan simetrik anahtardır. Bu anahtarın güvenliği kritiktir; ele geçirilirse saldırganlar sahte token üretebilir.
*   **Issuer (Yayıncı):** Token'ı oluşturan servisin adıdır (Örn: `WordMasterAPI`). Gelen token'ın bu kaynaktan gelip gelmediği kontrol edilir.
*   **Audience (Hedef Kitle):** Token'ın hangi uygulama veya servis için üretildiğini belirtir (Örn: `WordMasterClient`).
*   **ExpiresInMinutes:** Access Token'ın geçerlilik süresidir. Güvenlik için kısa tutulması önerilir (Örn: 15-60 dk).
*   **RefreshTokenExpiresInDays:** Refresh Token'ın geçerlilik süresidir. Kullanıcının tekrar şifre girmeden ne kadar süre oturumunu yenileyebileceğini belirler.

### 5.2. Kod Tarafındaki Yapılandırma (ServiceCollectionExtensions)

`AddJwtConfigurations` metodu içerisinde aşağıdaki kritik ayarlar yapılır:

*   **ValidateIssuer & ValidateAudience:** Token'ın doğru kaynaktan geldiği ve doğru hedef için üretildiği kontrol edilir.
*   **ValidateLifetime:** Token süresinin dolup dolmadığı kontrol edilir.
*   **ClockSkew:** Sunucular arası saat farkı toleransıdır. `TimeSpan.Zero` yapılarak tolerans kaldırılmıştır; süre dolduğu an token geçersiz olur.
*   **Events (Olaylar):**
    *   **OnTokenValidated:** Token teknik olarak geçerli olsa bile, veritabanındaki `SecurityStamp` kontrol edilerek oturumun mantıksal geçerliliği (Logout olmuş mu? Şifre değişmiş mi?) teyit edilir.
    *   **OnChallenge:** Yetkisiz erişim (401) durumunda varsayılan boş yanıt yerine, standart `ServiceResult` formatında JSON hata mesajı dönülmesini sağlar.
    *   **OnForbidden:** Yetki yetersizliği (403) durumunda standart JSON hata mesajı dönülmesini sağlar.
