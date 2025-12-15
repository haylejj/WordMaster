# Google OAuth 2.0 Entegrasyon Dokümantasyonu

Bu belge, WordMaster projesindeki Google ile Giriş (Google Login) özelliğinin teknik detaylarını, kullanılan teknolojileri ve `GoogleAuthService` metodlarının işleyişini açıklar.

## 1. Genel Bakış

Sistem, kullanıcıların Google hesaplarını kullanarak uygulamaya kayıt olmalarını veya giriş yapmalarını sağlar. Bu süreçte **OAuth 2.0 Authorization Code Flow** kullanılır.

### Akış Özeti:
1.  **Frontend:** Kullanıcı "Google ile Giriş Yap" butonuna tıklar.
2.  **Backend:** Google'ın güvenli giriş sayfasına yönlendiren bir URL oluşturur.
3.  **Google:** Kullanıcı giriş yapar ve izin verir.
4.  **Callback:** Google, kullanıcıyı backend'imize (veya frontend üzerinden backend'e) bir `code` ile yönlendirir.
5.  **Token Değişimi:** Backend, bu `code` parametresini kullanarak Google'dan `Access Token` ve `ID Token` alır.
6.  **Kullanıcı Senkronizasyonu (Best Practice):**
    - Önce `AspNetUserLogins` tablosunda Google Subject ID ile kullanıcı aranır.
    - Bulunamazsa email ile arama yapılır ve mevcut kullanıcıya Google login kaydı eklenir (hesap linking).
    - Hiç kullanıcı yoksa yeni oluşturulur ve Google login kaydı eklenir.
7.  **Oturum Açma:** Backend, kendi JWT (Access Token) ve Refresh Token'ını üretip kullanıcıya döner.

## 2. Kullanılan Teknolojiler ve Kütüphaneler

*   **ASP.NET Core Identity:** Kullanıcı yönetimi için.
*   **Google.Apis.Auth (v1.68.0):** Google OAuth akışını ve token doğrulama işlemlerini yönetmek için kullanılan resmi Google kütüphanesi.
*   **Newtonsoft.Json:** JSON web token (JWT) payload'larını işlemek için.

## 3. Yapılandırma (Configuration)

Google Cloud Console üzerinden alınan kimlik bilgileri `appsettings.json` (veya `appsettings.Development.json`) dosyasında saklanır.

```json
"Authentication": {
  "Google": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID",
    "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET",
    "RedirectUri": "http://localhost:3002/api/v1/auth/google-callback"
  }
}
```

*   **ClientId & Secret:** Google Cloud Console'dan alınır.
*   **RedirectUri:** Google Console'da "Authorized redirect URIs" listesine eklenmelidir. Backend'in callback endpoint'idir.

## 4. GoogleAuthService Detaylı Analizi

Bu servis `IGoogleAuthService` arayüzünü implemente eder ve tüm Google entegrasyon mantığını barındırır.

### `GetGoogleAuthUrl()`

Kullanıcıyı Google giriş sayfasına yönlendirmek için gereken URL'i oluşturur.

*   **Görevi:**
    *   Google'ın yetkilendirme sunucusuna (Authorization Server) yapılacak istek için parametreleri hazırlar.
    *   `scope` olarak `openid`, `profile` ve `email` talep eder. Bu sayede kullanıcının adını, soyadını ve e-postasını alabiliriz.
*   **İşleyişi:**
    *   `GoogleAuthorizationCodeFlow` nesnesi oluşturulur.
    *   `CreateAuthorizationCodeRequest` metodu ile Redirect URI ayarlanır.
    *   Geriye, kullanıcının tarayıcıda gideceği `https://accounts.google.com/o/oauth2/v2/auth?...` formatındaki URL döner.

### `HandleGoogleCallbackAsync(string code)`

En kritik metottur. Google'dan gelen `code` parametresini işler, kullanıcıyı doğrular ve sisteme giriş yaptırır.

*   **Parametre:** `code` (Google'dan dönen yetkilendirme kodu).
*   **Dönüş Değeri:** `ServiceResult<LoginTokenResult>` (WordMaster Access Token ve Refresh Token).
*   **Adım Adım İşleyiş:**
    1.  **Token Değişimi (Exchange):** `ExchangeCodeForTokenAsync` metodu ile Google'a "Elimde bu kod var, bana token ver" denir. Google, `IdToken` ve `AccessToken` döner.
    2.  **Token Doğrulama:** `GoogleJsonWebSignature.ValidateAsync` metodu ile gelen `IdToken`'ın imzasını ve geçerliliğini Google sunucularıyla teyit eder. **Bu adım güvenlik için kritiktir; sahte tokenları engeller.**
    3.  **Bilgi Çıkarımı:** Doğrulanmış payload içinden `Email`, `GivenName` (Ad), `FamilyName` (Soyad) ve **`Subject`** (Google'ın unique user ID'si) bilgileri alınır.
    4.  **Kullanıcı Kontrolü (Best Practice - 3 Aşamalı):**
        *   **1. Aşama:** `FindByLoginAsync("Google", payload.Subject)` ile `AspNetUserLogins` tablosunda arama yapılır. Bu, email değişse bile kullanıcıyı bulmayı sağlar.
        *   **2. Aşama:** Login kaydı yoksa, `FindByEmailAsync` ile email'e göre arama yapılır. Bulunursa mevcut hesaba Google login kaydı eklenir (hesap linking).
        *   **3. Aşama:** Hiç kullanıcı yoksa yeni oluşturulur.
    5.  **Kayıt (Opsiyonel):**
        *   Eğer kullanıcı yoksa: Yeni bir `AppUser` oluşturulur. `FirstName`, `LastName` ve `Email` set edilir. Kullanıcı adı (UserName) benzersiz olacak şekilde `GenerateUniqueUsernameAsync` ile oluşturulur.
        *   `AddLoginAsync` ile `AspNetUserLogins` tablosuna Google login kaydı eklenir.
        *   Kullanıcı kaydedilir.
    6.  **Token Üretimi:** Kullanıcı bulunduktan (veya oluşturulduktan) sonra, projenin standart `IJwtService`'i kullanılarak WordMaster'a özgü JWT Access Token ve Refresh Token üretilir.
    7.  **Sonuç:** Oluşturulan tokenlar frontend'e dönülür.

### `FindOrCreateUserAsync(payload)` (Private)

Best Practice implementasyonu. Kullanıcı arama ve oluşturma mantığını yönetir.

```csharp
// 1. Google Subject ID ile ara (en güvenilir)
AppUser? user = await userManager.FindByLoginAsync("Google", payload.Subject);

// 2. Bulunamazsa email ile ara ve link et
if (user == null)
{
    user = await userManager.FindByEmailAsync(payload.Email);
    if (user != null)
        await userManager.AddLoginAsync(user, new UserLoginInfo("Google", payload.Subject, "Google"));
}

// 3. Hiç yoksa yeni oluştur
if (user == null)
    user = await CreateGoogleUserAsync(payload);
```

### `AddGoogleLoginAsync(user, googleSubjectId)` (Private)

Kullanıcıya Google external login kaydı ekler.

```csharp
UserLoginInfo loginInfo = new("Google", googleSubjectId, "Google");
return await userManager.AddLoginAsync(user, loginInfo);
```

## 5. External Login Tablosu (AspNetUserLogins)

ASP.NET Identity'nin `AspNetUserLogins` tablosu, external login provider'ları yönetmek için kullanılır.

### Tablo Yapısı

| Kolon | Açıklama | Örnek Değer |
|-------|----------|-------------|
| `LoginProvider` | Provider adı | `"Google"` |
| `ProviderKey` | Provider'ın unique user ID'si | `"117283746192837465012"` |
| `ProviderDisplayName` | Görüntüleme adı | `"Google"` |
| `UserId` | Sistemdeki kullanıcı ID'si | `"a1b2c3d4-..."` |

### Neden Email Yerine Subject ID Kullanıyoruz?

| Senaryo | Sadece Email ile | Subject ID ile (Best Practice) |
|---------|-----------------|-------------------------------|
| Kullanıcı Google'da email değiştirdi | ❌ Eşleşemez, yeni hesap oluşur | ✅ Subject ID ile bulunur |
| Aynı email farklı provider | ❌ Karışıklık | ✅ Her provider ayrı kayıt |
| İleride Facebook/Apple ekleme | ❌ Zor | ✅ Kolay (aynı pattern) |

### Örnek Kayıtlar

```
| LoginProvider | ProviderKey              | UserId                               |
|---------------|--------------------------|--------------------------------------|
| Google        | 117283746192837465012    | a1b2c3d4-e5f6-7890-abcd-ef1234567890 |
| Google        | 109876543210987654321    | b2c3d4e5-f6a7-8901-bcde-f23456789012 |
```

### Mevcut Kullanıcılar İçin Otomatik Migration

Halihazırda Google ile kayıt olmuş ancak `AspNetUserLogins` kaydı olmayan kullanıcılar, **ilk tekrar giriş** yaptıklarında otomatik olarak link edilir:

1. `FindByLoginAsync` → Bulamaz (kayıt yok)
2. `FindByEmailAsync` → Bulur (email eşleşir)
3. `AddLoginAsync` → Google login kaydı eklenir

Bu sayede manuel migration gerekmez.

## 6. Frontend Entegrasyonu (React)

Frontend tarafında kullanıcı deneyimini pürüzsüz hale getirmek için özel bir akış tasarlanmıştır.

1.  **Login/Register Sayfaları:** Buton, backend'den alınan Google Auth URL'ine yönlendirir.
2.  **Yönlendirme:** Kullanıcı Google'da giriş yapınca `http://localhost:3002/api/v1/auth/google-callback?code=...` adresine düşer.
3.  **Backend Callback:** Backend işlemi tamamlar ve kullanıcıyı frontend'in özel bir sayfasına (`/auth/google-callback`) yönlendirir. URL parametresi olarak tokenları ekler (`?token=...&expiresAt=...`).
4.  **GoogleCallbackPage.tsx:** Bu React sayfası, URL'deki token'ı yakalar, `useAuth` hook'u ile tarayıcı hafızasına (veya cookie'ye) kaydeder ve kullanıcıyı Dashboard'a yönlendirir.

## 7. Önemli Notlar

*   **Güvenlik:** Google Client Secret asla frontend kodunda bulunmamalıdır. Sadece backend tarafında saklanmalıdır.
*   **Redirect URI:** Development ve Production ortamları için farklı Redirect URI'lar Google Console'a eklenmelidir.
*   **Kullanıcı Bilgileri:** Google'dan gelen ad/soyad bilgileri sadece ilk kayıtta alınır. Kullanıcı sonradan profilinden bunları değiştirebilir (admin onayıyla), ancak bir sonraki Google girişinde bu bilgiler ezilmez.
*   **External Login Best Practice:** `AspNetUserLogins` tablosu kullanılarak Google Subject ID ile eşleştirme yapılır. Bu, email değişikliklerine karşı dayanıklıdır ve ileride başka provider'lar (Facebook, Apple) eklenebilir.
*   **AspNetUserTokens Tablosu:** Bu tablo Google'ın access/refresh token'larını saklamak içindir. Sadece Google API'lerine (Calendar, Drive vb.) erişmeniz gerekirse kullanılır. Şu an kullanılmıyor çünkü sadece authentication yapıyoruz.
