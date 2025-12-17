# WordMaster API

WordMaster API, kelime öğrenme ve yönetimi uygulaması olan WordMaster'ın backend servisidir. En son **.NET 10** teknolojisi ve Clean Architecture prensipleri kullanılarak geliştirilmiştir. Güvenli, ölçeklenebilir ve performanslı bir RESTful API sunar.

## 🏁 Kurulum ve Çalıştırma (Getting Started)

Projeyi yerel ortamınızda çalıştırmak için aşağıdaki adımları izleyin.

### Gereksinimler
*   **.NET 10 SDK**
*   **Docker Desktop** (Redis ve MailHog servisleri için)
*   **SQL Server** (veya LocalDB)

### Adım 1: Docker ile Kurulum (Önerilen)
Projenin API, Frontend, SQL Server, Redis ve MailHog servislerinin tamamını tek komutla ayağa kaldırabilirsiniz.

1.  **Environment Setup**: Kök dizindeki `.env.example` dosyasını `.env` olarak kopyalayın ve şifreleri nı ayarlayın.
    ```powershell
    copy .env.example .env
    ```
2.  **Servisleri Başlatın**:
    ```powershell
    docker-compose up -d
    ```

### 🛠️ Docker Yönetim Komutları

Docker kullanımı ile ilgili sık kullanılan komutlar:

#### 1. Build & Rebuild
Kodda değişiklik yaptığınızda image'ı yeniden oluşturmanız gerekir:
```powershell
# Sadece çalıştır (image varsa onu kullanır)
docker-compose up -d

# Yeniden build alarak çalıştır (kod değişikliği sonrası)
docker-compose up -d --build

# Sadece build al (çalıştırmadan)
docker-compose build
```

#### 2. Logları İzleme
Çalışan container'ların loglarını canlı izlemek için:

```powershell
# Tüm servislerin loglarını izle
docker-compose logs -f

# Sadece API loglarını izle
docker-compose logs -f api

# Sadece Database loglarını izle
docker-compose logs -f db
```

#### 3. Container İçine Girme (Exec)
Container içinde komut çalıştırmak veya dosya sistemini kontrol etmek için:

```powershell
# Database container'ına gir
docker exec -it wordmaster-db bash

# API container'ına gir
docker exec -it wordmaster-api /bin/sh
```

#### 4. Servisleri Durdurma/Yeniden Başlatma

```powershell
# Tüm servisleri durdur
docker-compose down

# Volume'ları (verileri) da silerek durdur (DİKKAT: DB sıfırlanır!)
docker-compose down -v

# Tek bir servisi yeniden başlat
docker-compose restart api
```

Bu komut şunları başlatır:
*   **Web Uygulaması (Frontend)**: [http://localhost:5173](http://localhost:5173)
*   **Backend API**: [http://localhost:3002](http://localhost:3002)
*   **Swagger UI**: [http://localhost:3002/swagger](http://localhost:3002/swagger)
*   **SQL Server**: `localhost,1434`
*   **Redis**: `localhost:6379`
*   **RedisInsight** (Redis GUI): [http://localhost:5540](http://localhost:5540)
*   **MailHog** (SMTP Test):
    *   Web UI: [http://localhost:8025](http://localhost:8025)
    *   SMTP Port: `1025`

---

## 🗄️ Veritabanı İşlemleri (Docker)

### Bağlantı Detayları
Docker'daki SQL Server'a **SSMS** veya **Azure Data Studio** ile bağlanmak için:

| Ayar | Değer |
|------|-------|
| **Server** | `localhost,1434` (Port numarasına dikkat edin) |
| **Authentication** | SQL Server Authentication |
| **Login** | `username` |
| **Password** | `.env` dosyasındaki `DB_PASSWORD` |

> **Neden Port 1434?** Container içi port 1433'tür ancak yerel SQL Server ile çakışmaması için dışarıya 1434 olarak açılmıştır.

### Backup Restore (Veri Kopyalama)
Elinizdeki bir `.bak` dosyasını Docker veritabanına yüklemek için:

1.  **Backup dosyasını container'a kopyalayın:**
    ```powershell
    # Klasör oluştur
    docker exec wordmaster-db mkdir -p /var/opt/mssql/backup
   
    # Dosyayı kopyala (Dosya yolunu kendinize göre düzenleyin)
    docker cp "C:\sqltemp\DB_WordMaster_API_Dev.bak" wordmaster-db:/var/opt/mssql/backup/
    ```

2.  **Restore komutunu çalıştırın:**
    ```powershell
    docker exec wordmaster-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "DB_PASSWORD" -C -Q "RESTORE DATABASE [DB_WordMaster_API_Dev] FROM DISK = '/var/opt/mssql/backup/DB_WordMaster_API_Dev.bak' WITH REPLACE, MOVE 'DB_WordMaster_API_Dev' TO '/var/opt/mssql/data/DB_WordMaster_API_Dev.mdf', MOVE 'DB_WordMaster_API_Dev_log' TO '/var/opt/mssql/data/DB_WordMaster_API_Dev_log.ldf'"
    ```

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

## ⚙️ Konfigürasyon ve Environment Variables

Proje, hassas bilgileri (Database şifreleri, API anahtarları vb.) yönetmek için **Environment Variables** kullanır. Kod içerisinde bu değerler `DotNetEnv` kütüphanesi ile yüklenir.

### Öncelik Sırası
Konfigürasyon aşağıdaki öncelik sırasına göre okunur (En yüksekten en düşüğe):

1.  **Environment Variables (`.env`)** 🏆 (En Yüksek Öncelik)
2.  `appsettings.Development.json` (Local Development)
3.  `appsettings.json` (Varsayılanlar)

### .env Dosyası
Kök dizindeki `.env` dosyası Git tarafından takip **edilmez** (gitignore). Bu dosyayı `.env.example` dosyasından kopyalayarak oluşturmalısınız.

```bash
# Proje kök dizininde
copy .env.example .env
```

**.env Formatı (.NET Uyumlu):**
.NET'in hiyerarşik yapıyı anlaması için `__` (çift alt çizgi) kullanılır.

```env
# Database
ConnectionStrings__SqlServer=Server=...
ConnectionStrings__Redis=localhost:6379

# JWT
Jwt__Key=GizliAnahtar...
Jwt__Issuer=WordMasterApi
```

---

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
| `GET` | `/api/words/quiz` 🛡️ | Test (Quiz) için soru ve şıklar getirir. |
| `GET` | `/api/words/practice/distractors` 🛡️ | Pratik için yanlış şık (çeldirici) kelimeler getirir. |

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
| `GET` | `/api/favorites/quiz` 🛡️ | Favorilerden test (Quiz) sorusu getirir. |

### Unknowns Endpoint'leri

Bilinmeyen kelime yönetimi `/api/unknows` altında toplanmıştır.

| Metot | Endpoint | Açıklama |
| :--- | :--- | :--- |
| `GET` | `/api/unknows` 🛡️ | Bilinmeyen kelimeleri sayfalı listeler. |
| `POST` | `/api/unknows/toggle` 🛡️ | Kelimeyi bilinmeyenlere ekler/çıkarır. |
| `GET` | `/api/unknows/practice/random` 🛡️ | Bilinmeyenlerden pratik için rastgele kelime getirir. |
| `POST` | `/api/unknows/practice/check` 🛡️ | Bilinmeyen pratik çeviri kontrolü yapar. |
| `GET` | `/api/unknows/quiz` 🛡️ | Bilinmeyenlerden test (Quiz) sorusu getirir. |

### Statistics Endpoint'leri

Kullanıcı istatistikleri `/api/statistics` altında toplanmıştır.

| Metot | Endpoint | Açıklama |
| :--- | :--- | :--- |
| `GET` | `/api/statistics/dashboard` 🛡️ | Kullanıcının dashboard istatistiklerini (kelime sayısı, başarı oranı, grafik verisi, seri vb.) getirir. |

### System Endpoint'leri

Genel sistem bilgileri `/api/version` altında toplanmıştır.

| Metot | Endpoint | Açıklama |
| :--- | :--- | :--- |
| `GET` | `/api/version` | Uygulamanın anlık çalışan versiyonu, ortam bilgisi (Development/Production) ve API sürümünü döner. Login gerektirmez. |

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
### 2. Authentication (Kimlik Doğrulama) - Hibrit Token Stratejisi
Kimlik doğrulama işlemleri **ASP.NET Core Identity** ve **JWT (JSON Web Token)** ile sağlanır. Güvenlik için **Hibrit Token Stratejisi** uygulanmıştır.

#### Token Yapısı

| Token Türü | Süre | Saklama Yeri | Güvenlik |
|------------|------|--------------|----------|
| **Access Token** | 15 dakika | Frontend Memory (RAM) | XSS'e karşı korumalı |
| **Refresh Token** | 7 gün | HttpOnly Cookie | XSS'e %100 bağışık |

#### Güvenlik Özellikleri

*   **HttpOnly Cookie**: Refresh token JavaScript ile erişilemez. XSS saldırılarına karşı tam koruma.
*   **Secure Flag**: Cookie sadece HTTPS üzerinden gönderilir.
*   **SameSite=Strict**: Cross-site isteklerde cookie gönderilmez (CSRF koruması).
*   **Token Rotation**: Her yenilemede yeni refresh token üretilir.
*   **Hash'leme**: Refresh token veritabanına SHA256 hash olarak kaydedilir.
*   **Security Stamp**: Şifre değişikliği veya logout durumunda tüm token'lar anında geçersiz olur.

#### Akış

1.  **Login**: Kullanıcı giriş yapar → Access token JSON body'de, Refresh token HttpOnly cookie olarak döner.
2.  **API İstekleri**: Frontend, access token'ı `Authorization: Bearer <token>` header'ında gönderir.
3.  **Token Yenileme**: Access token süresi dolunca `/refresh-token` çağrılır. Refresh token cookie'den otomatik gönderilir.
4.  **Logout**: Hem sunucu tarafında token silinir, hem cookie temizlenir.

*Detaylı bilgi için [docs/Authentication_System.md](docs/Authentication_System.md) dosyasına bakabilirsiniz.*

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
*   **Not**: `AllowCredentials` HttpOnly cookie'lerin gönderilmesi için gereklidir.

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

### 12. Arka Plan İşlemleri (Background Jobs)
Sistemin belirli periyotlarla otomatik yapması gereken bakım işlemleri için **Hosted Service (BackgroundService)** yapısı kullanılmıştır.

*   **UserCleanupService**:
    *   **Görevi**: Kayıt olduktan sonra e-posta adresini doğrulamayan pasif ("hayalet") kullanıcıları temizlemek.
    *   **Çalışma Sıklığı**: Her 2 saatte bir tetiklenir.
    *   **Kural**: Oluşturulma tarihi 65 dakikadan daha eski olan ve `EmailConfirmed = false` olan kullanıcılar veritabanından kalıcı olarak silinir.
    *   **Amaç**: Doğrulanmamış gereksiz kullanıcı verilerinin veritabanını şişirmesini engellemek.

---

### 13. API Versioning (Versiyonlama)
Projenin sürdürülebilirliği ve ölçeklenebilirliği için **URL Path Versioning** stratejisi benimsenmiştir. Bu sayede mevcut istemcileri etkilemeden (Breaking Change) yeni özellikler geliştirilebilir.

*   **Format**: `/api/v1/words`, `/api/v2/words` şeklinde versiyon numarası URL'de açıkça belirtilir.
*   **Varsayılan**: Versiyon belirtilmeyen istekler otomatik olarak `v1.0` kabul edilir.
*   **Swagger Desteği**: Swagger arayüzünde sağ üst köşeden versiyonlar arası geçiş yapılabilir ve her versiyonun dokümantasyonu ayrı ayrı incelenebilir.
*   **Proje Sürümü**: Yazılımın derleme sürümü (`1.0.0-dev` vb.) `.csproj` üzerinden ayrıca takip edilir; bu, API versiyonundan (v1) bağımsızdır.

*Detaylı bilgi ve kullanım rehberi için [docs/Api_Versioning.md](docs/Api_Versioning.md) dosyasına bakabilirsiniz.*

---

### 14. Çoktan Seçmeli Test Modu (Quiz Mode)
Kullanıcıların kelime dağarcığını test etmeleri için geliştirilen "Test Modu", performans odaklı bir mimari ile sunulmuştur.

*   **Tek İstek (Single Request) Mimarisi**:
    *   Frontend'in her bir şık için ayrı ayrı istek atması yerine (N+1 problemi), `/quiz` endpoint'i üzerinden **Tek Seferde** soru ve 4 şık (1 doğru, 3 yanlış) döner.
    *   Bu yöntem, mobil cihazlarda veri kullanımını ve ağ gecikmesini (latency) minimize eder.
*   **Akıllı Şık Üretimi (Distractor Generation)**:
    *   Doğru cevap dışında kalan 3 yanlış şık, genel kelime havuzundan rastgele seçilir.
    *   Sistemin "Favoriler" veya "Bilinmeyenler" modunda çalışması fark etmeksizin, şıkların her zaman dolu gelmesi sağlanır.
*   **Güvenli Test**: Şıklar backend tarafında karıştırılır (shuffle), böylece doğru cevabın yeri tahmin edilemez.

### 15. Google OAuth 2.0 Entegrasyonu
Kullanıcıların Google hesapları ile hızlıca kayıt olabilmesi ve giriş yapabilmesi için Google OAuth entegrasyonu sağlanmıştır.
*   **Akış**: Authorization Code Flow kullanılır.
*   **Kütüphane**: `Google.Apis.Auth` ile token değişimi ve doğrulama yapılır.
*   **Kayıt**: Kullanıcı ilk kez giriş yaptığında Ad, Soyad ve E-posta bilgileri ile otomatik kayıt oluşturulur.
*   *Detaylı bilgi ve kurulum için `docs/Google_Auth_Integration.md` dosyasına bakabilirsiniz.*

---

*WordMaster Backend Team*
