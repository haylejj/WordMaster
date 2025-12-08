# WordMaster API

WordMaster API, kelime öğrenme ve yönetimi uygulaması olan WordMaster'ın backend servisidir. En son **.NET 10** teknolojisi ve Clean Architecture prensipleri kullanılarak geliştirilmiştir. Güvenli, ölçeklenebilir ve performanslı bir RESTful API sunar.

## 🏁 Kurulum ve Çalıştırma (Getting Started)

Projeyi yerel ortamınızda çalıştırmak için aşağıdaki adımları izleyin.

### Gereksinimler
*   **.NET 10 SDK**
*   **Docker Desktop** (Redis ve MailHog servisleri için)
*   **SQL Server** (veya LocalDB)

### Adım 1: Altyapı Servislerini Başlatma
Proje, önbellekleme için **Redis** ve e-posta testleri için **MailHog** kullanır. Bu servisleri Docker ile kolayca ayağa kaldırabilirsiniz.

Terminali projenin ana dizininde açın ve şu komutu çalıştırın:

```bash
docker-compose up -d
```
Bu komut şunları başlatır:
*   **Redis**: `localhost:6379`
*   **RedisInsight** (Redis GUI): `localhost:5540`
*   **MailHog** (SMTP Test): `localhost:1025` (SMTP), `localhost:8025` (Web UI)

### Adım 2: Veritabanını Hazırlama
`appsettings.json` dosyasındaki `ConnectionStrings` bölümünü kendi SQL Server bağlantınıza göre düzenleyin. Ardından migration'ları uygulayın:

```bash
dotnet ef database update --project WordMaster.Infrastructure --startup-project WordMaster.API
```

### Adım 3: Uygulamayı Çalıştırma
API projesini çalıştırın:

```bash
cd WordMaster.API
dotnet run
```

Tarayıcınızda `https://localhost:3001/swagger` (port değişebilir) adresine giderek Swagger arayüzü üzerinden API'yi test edebilirsiniz.

---

## 🚀 Teknolojiler

Proje aşağıdaki modern teknolojiler üzerine inşa edilmiştir:

*   **.NET 10**: En yeni .NET sürümü.
*   **ASP.NET Core Web API**: RESTful servis altyapısı.
*   **Entity Framework Core**: ORM aracı.
*   **Docker & Docker Compose**: Redis ve MailHog gibi bağımlılıkların yönetimi.
*   **Redis**: Önbellekleme ve Security Stamp yönetimi.
*   **JWT (JSON Web Token)**: Güvenli kimlik doğrulama.
*   **Identity**: Kullanıcı yönetimi.
*   **FluentValidation**: Model doğrulama.
*   **Swagger**: API dokümantasyonu.

## 🏗 Mimari Yapı (Clean Architecture)

Proje 4 ana katmandan oluşur:

1.  **WordMaster.Domain**: Varlıklar (Entities) ve temel iş kuralları.
2.  **WordMaster.Application**: Servis arayüzleri, DTO'lar ve Validasyonlar.
3.  **WordMaster.Infrastructure**: Veritabanı (EF Core), Servis implementasyonları, JWT ve Redis entegrasyonları.
4.  **WordMaster.API**: Controller'lar ve uygulama giriş noktası.

## 🔐 Kimlik Doğrulama (Authentication)

API, JWT tabanlı güvenli bir kimlik doğrulama sistemi kullanır.

> ℹ️ **Not:** Yanında 🛡️ işareti bulunan endpoint'ler `[RequirePermission]` attribute'ü ile korunmaktadır ve veritabanı tabanlı gelişmiş yetkilendirme sistemi tarafından denetlenir.

### Auth Endpoint'leri

Tüm kimlik doğrulama işlemleri `/api/auth` altında toplanmıştır.

| Metot | Endpoint | Açıklama |
| :--- | :--- | :--- |
| `POST` | `/api/auth/register` | Yeni kullanıcı kaydı oluşturur. |
| `POST` | `/api/auth/login` | Kullanıcı girişi yapar. Access ve Refresh Token döner. |
| `POST` | `/api/auth/admin-login` | Admin girişi yapar (Sadece admin paneli erişimi için). |
| `POST` | `/api/auth/refresh-token` | Süresi dolan Access Token'ı yeniler. |
| `POST` | `/api/auth/forget-password`| Şifre sıfırlama bağlantısı gönderir. |
| `POST` | `/api/auth/reset-password` | Şifre sıfırlama işlemini tamamlar. |
| `GET` | `/api/auth/session-check` | Oturumun (Token) geçerliliğini kontrol eder. |
| `POST` | `/api/auth/change-password` 🛡️ | Giriş yapmış kullanıcının şifresini değiştirir. |
| `POST` | `/api/auth/logout` 🛡️ | Çıkış yapar ve Refresh Token'ı siler. |

### User Profile Endpoint'leri

Kullanıcı profil yönetimi `/api/user` altında toplanmıştır.

| Metot | Endpoint | Açıklama |
| :--- | :--- | :--- |
| `GET` | `/api/user/profile` 🛡️ | Kullanıcının profil bilgilerini getirir. |
| `PUT` | `/api/user/profile` 🛡️ | Kullanıcının profil bilgilerini günceller. |

### Word Endpoint'leri

Kelime işlemleri `/api/words` altında toplanmıştır.

| Metot | Endpoint | Açıklama |
| :--- | :--- | :--- |
| `GET` | `/api/words` 🛡️ | Kullanıcının kelimelerini sayfalı olarak listeler. |
| `GET` | `/api/words/{id}` 🛡️ | Belirtilen ID'ye sahip kelimenin detaylarını getirir. |
| `POST` | `/api/words` 🛡️ | Yeni bir kelime ekler. |
| `PUT` | `/api/words` 🛡️ | Mevcut bir kelimeyi günceller. |
| `DELETE` | `/api/words/{id}` 🛡️ | Belirtilen kelimeyi siler. |
| `POST` | `/api/words/import-csv` 🛡️ | CSV dosyasından toplu kelime yükler. |
| `GET` | `/api/words/user-words` 🛡️ | Tüm kelimeleri basit liste olarak (Dropdown için) getirir. |
| `GET` | `/api/words/practice/random` 🛡️ | Pratik için rastgele kelime getirir. |
| `POST` | `/api/words/practice/check` 🛡️ | Pratik çeviri kontrolü yapar. |
| `POST` | `/api/words/practice/batch-update` 🛡️ | Pratik istatistiklerini toplu olarak günceller. |

### Folder Endpoint'leri

Klasör işlemleri `/api/folders` altında toplanmıştır.

| Metot | Endpoint | Açıklama |
| :--- | :--- | :--- |
| `GET` | `/api/folders` 🛡️ | Kullanıcının klasörlerini listeler. |
| `GET` | `/api/folders/{id}` 🛡️ | Klasör detaylarını getirir. |
| `POST` | `/api/folders` 🛡️ | Yeni klasör oluşturur. |
| `PUT` | `/api/folders` 🛡️ | Klasör ismini günceller. |
| `DELETE` | `/api/folders/{id}` 🛡️ | Klasörü siler. |
| `GET` | `/api/folders/{id}/words` 🛡️ | Klasör içeriğindeki kelimeleri listeler. |
| `POST` | `/api/folders/words` 🛡️ | Klasöre kelime ekler. |
| `DELETE` | `/api/folders/words` 🛡️ | Klasörden kelime çıkarır. |
| `POST` | `/api/folders/practice/check` 🛡️ | Klasör pratik çeviri kontrolü yapar. |

### Favorite Endpoint'leri

Favori kelime yönetimi `/api/favorites` altında toplanmıştır.

| Metot | Endpoint | Açıklama |
| :--- | :--- | :--- |
| `GET` | `/api/favorites` 🛡️ | Favori kelimeleri sayfalı listeler. |
| `POST` | `/api/favorites/toggle` 🛡️ | Kelimeyi favorilere ekler/çıkarır. |
| `GET` | `/api/favorites/practice/random` 🛡️ | Favorilerden pratik için rastgele kelime getirir. |
| `POST` | `/api/favorites/practice/check` 🛡️ | Favori pratik çeviri kontrolü yapar. |

### Unknowns Endpoint'leri

Bilinmeyen kelime yönetimi `/api/unknows` altında toplanmıştır.

| Metot | Endpoint | Açıklama |
| :--- | :--- | :--- |
| `GET` | `/api/unknows` 🛡️ | Bilinmeyen kelimeleri sayfalı listeler. |
| `POST` | `/api/unknows/toggle` 🛡️ | Kelimeyi bilinmeyenlere ekler/çıkarır. |
| `GET` | `/api/unknows/practice/random` 🛡️ | Bilinmeyenlerden pratik için rastgele kelime getirir. |
| `POST` | `/api/unknows/practice/check` 🛡️ | Bilinmeyen pratik çeviri kontrolü yapar. |

### Admin Endpoint'leri

Admin paneli işlemleri `/api/admin` altında toplanmıştır. Sadece yöneticilerin erişebilmesi gereken servislere ev sahipliği yapar.

#### Admin Word Endpoint'leri

Adminlerin kelimeleri yönetmesi içindir.

| Metot | Endpoint | Açıklama |
| :--- | :--- | :--- |
| `GET` | `/api/admin/words` 🛡️ | Yönetim panelinde kelimeleri listeler. |
| `PUT` | `/api/admin/words/{id}` 🛡️ | Kelime bilgilerini günceller. |
| `DELETE` | `/api/admin/words/{id}` 🛡️ | Kelimeyi siler. |

#### Allowed IP Endpoint'leri

İzin verilen IP adreslerinin yönetimi.

| Metot | Endpoint | Açıklama |
| :--- | :--- | :--- |
| `GET` | `/api/admin/allowed-ips` 🛡️ | İzin verilen IP adreslerini listeler. |
| `GET` | `/api/admin/allowed-ips/{id}` 🛡️ | IP adresi detayını getirir. |
| `POST` | `/api/admin/allowed-ips` 🛡️ | Yeni bir IP adresi ekler. |
| `PUT` | `/api/admin/allowed-ips` 🛡️ | Mevcut IP adresini günceller. |
| `DELETE` | `/api/admin/allowed-ips/{id}` 🛡️ | IP adresini siler. |

#### Role Endpoint'leri

Sistem rollerinin yönetimi.

| Metot | Endpoint | Açıklama |
| :--- | :--- | :--- |
| `GET` | `/api/admin/roles` 🛡️ | Tüm rolleri listeler. |
| `GET` | `/api/admin/roles/{id}` 🛡️ | Rol detayını getirir. |
| `POST` | `/api/admin/roles` 🛡️ | Yeni rol oluşturur. |
| `PUT` | `/api/admin/roles` 🛡️ | Rolü günceller. |
| `DELETE` | `/api/admin/roles/{id}` 🛡️ | Rolü siler. |
| `GET` | `/api/admin/roles/assign/{userId}` 🛡️ | Kullanıcıya atanabilir rolleri gösterir. |
| `POST` | `/api/admin/roles/assign` 🛡️ | Kullanıcıya rol ataması yapar. |

#### Permission Endpoint'leri

Dinamik yetki (izin) yönetimi.

| Metot | Endpoint | Açıklama |
| :--- | :--- | :--- |
| `GET` | `/api/admin/permissions` 🛡️ | Tüm izinleri listeler. |
| `GET` | `/api/admin/permissions/role/{roleId}` 🛡️ | Role ait izinleri getirir. |
| `PUT` | `/api/admin/permissions/role` 🛡️ | Role izin tanımı yapar. |
| `POST` | `/api/admin/permissions/scan` 🛡️ | Koddaki yeni izinleri tarar ve veritabanına ekler. |

#### Dashboard Endpoint'leri

| Metot | Endpoint | Açıklama |
| :--- | :--- | :--- |
| `GET` | `/api/admin/dashboard` 🛡️ | Admin dashboard istatistiklerini getirir. |

#### User Management Endpoint'leri

Adminlerin kullanıcıları yönetmesi içindir.

| Metot | Endpoint | Açıklama |
| :--- | :--- | :--- |
| `GET` | `/api/admin/users` 🛡️ | Kullanıcıları sayfalı listeler. |
| `GET` | `/api/admin/users/all` 🛡️ | Tüm kullanıcıları listeler. |
| `GET` | `/api/admin/users/{id}` 🛡️ | Kullanıcı düzenleme detaylarını getirir. |
| `GET` | `/api/admin/users/{id}/detail` 🛡️ | Kullanıcı görüntüleme detaylarını getirir. |
| `PUT` | `/api/admin/users` 🛡️ | Kullanıcı bilgilerini günceller. |
| `DELETE` | `/api/admin/users/{id}` 🛡️ | Kullanıcıyı siler. |
| `POST` | `/api/admin/users/{id}/reset-password` 🛡️ | Kullanıcının şifresini sıfırlar ve mail atar. |
| `POST` | `/api/admin/users/{id}/change-role` 🛡️ | Kullanıcının rollerini değiştirir. |

#### Log History & Database Endpoint'leri

Sistem logları ve veritabanı temizliği.

| Metot | Endpoint | Açıklama |
| :--- | :--- | :--- |
| `GET` | `/api/admin/logHistory` 🛡️ | Sistem loglarını filtreli/sayfalı listeler. |
| `POST` | `/api/admin/database/reset` 🛡️ | Belirtilen tabloyu sıfırlar (Admin şifresi gerektirir). |

---

## 🔧 Altyapı ve Kütüphaneler

### 1. FluentValidation & Otomatik Doğrulama
Gelen isteklerin (Request DTOs) doğrulanması için **FluentValidation** kütüphanesi kullanılmıştır.
*   **Otomatik Kontrol**: `ValidationFilter` sayesinde, Controller'a istek ulaştığında validasyon kuralları otomatik olarak çalıştırılır.
*   Eğer validasyon hatası varsa, Controller action'ı çalışmadan `400 Bad Request` ve hata detayları döner.

### 2. Authentication (Kimlik Doğrulama)
Kimlik doğrulama işlemleri **ASP.NET Core Identity** ve **JWT (JSON Web Token)** ile sağlanır.
*   **Identity**: Kullanıcı ve rol yönetimi için kullanılır.
*   **JWT**: Stateless bir yapı için Access ve Refresh Token mekanizması kurulmuştur.
*   Giriş yapan kullanıcıya bir JWT verilir ve sonraki isteklerde bu token `Authorization: Bearer <token>` header'ı ile gönderilir.

### 3. Authorization (Yetkilendirme) Sistemi
Projede **Dinamik Attribute Tabanlı** gelişmiş bir yetkilendirme sistemi mevcuttur.
*   **Role-Based Access Control (RBAC)** temel alınmıştır ancak izinler kod içerisindeki `[RequirePermission]` attribute'larından dinamik olarak taranır.
*   **PermissionService**: Kod tarafındaki izinleri tarayıp veritabanı ile senkronize eder.
*   Admin panelinden rollere bu izinler dinamik olarak atanabilir.
*   *Detaylı bilgi için `docs/Authorization_System.md` dosyasına bakabilirsiniz.*

### 4. Global Exception Handling
Hata yönetimi merkezi bir **Middleware** (`GlobalExceptionHandlerMiddleware`) üzerinden yapılır.
*   Uygulama genelinde fırlatılan tüm hatalar  bu katmanda yakalanır.
*   İstemciye her zaman standart bir hata formatı dönülür.

### 5. Redis Entegrasyonu
Performans artışı için **Redis** kullanılmıştır.
*   **Caching**: Sık erişilen veriler önbelleğe alınır.
*   **Security Stamp**: Kullanıcı oturum güvenliği için Identity Security Stamp bilgileri Redis'te tutulur.

### 6. Swagger (API Dokümantasyonu)
API endpoint'lerini test etmek ve belgelemek için **Swagger UI** entegre edilmiştir.
*   Geliştirme ortamında `/swagger` adresinden erişilebilir.
*   JWT token girişi için "Authorize" butonu aktiftir.

### 7. CORS Politikası
Farklı originlerden (örneğin Frontend uygulamasından) gelen isteklere izin vermek için **CORS** yapılandırılmıştır.
*   Belirlenen frontend URL'lerine (localhost:5173 vb.) `AllowCredentials` ile izin verilir.

### 8. Logging (Serilog)
Uygulama genelinde yapılandırılmış (structured) loglama için **Serilog** kullanılmıştır.

### 9. Thread-Safe Random Kullanımı
Uygulama genelinde ve özellikle pratik modüllerinde (rastgele kelime seçimi vb.) **Random.Shared** yapısına geçilmiştir.
*   **Thread Safety**: Statik `Random` nesnelerinin çoklu iş parçacığı (multi-thread) ortamlarında neden olabileceği sorunlar (race condition vb.) giderilmiştir.
*   **Performans**: .NET 6+ ile gelen `Random.Shared`, her thread için optimize edilmiş güvenli bir örnek sunar.

---

### 10. Memory-Efficient (Allocation-Free) Rastgele Kelime Seçimi
Pratik modüllerinde "Sıradaki Kelime" özelliği için kullanılan rastgele kelime seçim algoritması optimize edilmiştir.
*   **Sorun**:
    1.  **Ardışık Tekrar**: Özellikle az sayıda kelime içeren listelerde (örneğin 5 bilinmeyen kelime) pratik yaparken, rastgele seçim sonucunda aynı kelimenin arka arkaya (consecutive) gelmesi kullanıcı deneyimini olumsuz etkiliyordu.
    2.  **Bellek Yönetimi**: Bu durumu engellemek için yapılan geleneksel `Where(x => x.Id != excludeId).ToList()` filtrelemesi, her istekte bellekte yeni bir liste (kopya) oluşturarak Memory Allocation'a neden oluyordu.
*   **Çözüm**: "Allocation-Free" yaklaşımı ile hem tekrar engellendi hem de performans artırıldı:
    1.  Parametre olarak bir önceki kelimenin ID'si (`excludeWordId`) alınır.
    2.  Orijinal listeden rastgele bir indeks seçilir.
    3.  Eğer seçilen kelime, hariç tutulması gereken kelime ise, **filtreleme yapıp yeni liste oluşturmak yerine**, mevcut listedeki **bir sonraki eleman** (`(index + 1) % Count`) seçilir.
*   **Sonuç**: Bellekte gereksiz liste kopyaları oluşturulmadan O(1) bellek karmaşıklığı ile çalışır ve aynı kelimenin arka arkaya gelmesi %100 engellenir (listede 1'den fazla kelime varsa).

### 11. Rate Limiting (Hız Sınırlama)
Sistemi aşırı yük ve kaba kuvvet (brute-force) saldırılarından korumak için ASP.NET Core Rate Limiting middleware'i entegre edilmiştir. İki temel politika uygulanır:

1.  **StrictPolicy (Katı Kural)**:
    *   **Hedef**: Auth (Login, Register vb.) endpoint'leri.
    *   **Limit**: IP adresi başına dakikada **10 istek**.
    *   **Algoritma**: Sliding Window (Kayan Pencere).
    *   **Amaç**: Brute-force ve şifre deneme saldırılarını engellemek.

2.  **GeneralPolicy (Genel Kural)**:
    *   **Hedef**: Kelime, Favori, Klasör gibi kaynak tüketen endpoint'ler.
    *   **Limit**: Kullanıcı (User ID) veya IP başına dakikada **60 istek**.
    *   **Algoritma**: Fixed Window (Sabit Pencere).
    *   **Amaç**: Adil kullanım sağlamak ve sunucu kaynaklarını korumak.

> ℹ️ **Not:** Hız limiti aşıldığında (429 Too Many Requests), sistem standart hata sayfası yerine JSON formatında ve Türkçe olarak ne kadar beklemeniz gerektiğini söyleyen bir mesaj döner (Örn: "Lütfen 1 dakika sonra tekrar deneyiniz.").

---

*WordMaster Backend Team*
