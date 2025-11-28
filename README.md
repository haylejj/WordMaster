# WordMaster API

WordMaster API, kelime öğrenme ve yönetimi uygulaması olan WordMaster'ın backend servisidir. En son **.NET 10** teknolojisi ve Clean Architecture prensipleri kullanılarak geliştirilmiştir. Güvenli, ölçeklenebilir ve performanslı bir RESTful API sunar.

## � Kurulum ve Çalıştırma (Getting Started)

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

### Auth Endpoint'leri

Tüm kimlik doğrulama işlemleri `/api/auth` altında toplanmıştır.

| Metot | Endpoint | Açıklama | Yetki |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/register` | Yeni kullanıcı kaydı oluşturur. | Public |
| `POST` | `/api/auth/login` | Kullanıcı girişi yapar. Access ve Refresh Token döner. | Public |
| `POST` | `/api/auth/refresh-token` | Süresi dolan Access Token'ı yeniler. | Public |
| `POST` | `/api/auth/forget-password`| Şifre sıfırlama bağlantısı gönderir (MailHog üzerinden izlenebilir). | Public |
| `POST` | `/api/auth/reset-password` | Şifre sıfırlama işlemini tamamlar. | Public |
| `POST` | `/api/auth/change-password`| Giriş yapmış kullanıcının şifresini değiştirir. | **User** |
| `POST` | `/api/auth/logout` | Çıkış yapar (Refresh Token'ı sunucudan siler). | **User** |
| `POST` | `/api/auth/admin-login` | Admin girişi yapar. Sadece admin rolüne sahip kullanıcılar giriş yapabilir. | Public |

### Word Endpoint'leri

Kelime işlemleri `/api/words` altında toplanmıştır. Tüm işlemler giriş yapmış kullanıcıya özeldir.

| Metot | Endpoint | Açıklama | Yetki |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/words` | Kullanıcının kelimelerini sayfalı olarak listeler. | **User** |
| `GET` | `/api/words/{id}` | Belirtilen ID'ye sahip kelimenin detaylarını getirir. | **User** |
| `POST` | `/api/words` | Yeni bir kelime ekler. | **User** |
| `PUT` | `/api/words` | Mevcut bir kelimeyi günceller. | **User** |
| `DELETE` | `/api/words/{id}` | Belirtilen kelimeyi siler. | **User** |
| `POST` | `/api/words/import-csv` | CSV dosyasından toplu kelime yükler. | **User** |
| `GET` | `/api/words/user-words` | Tüm kelimeleri basit liste olarak (Dropdown için) getirir. | **User** |
| `GET` | `/api/words/practice/random` | Pratik için rastgele kelime getirir. | **User** |
| `POST` | `/api/words/practice/check` | Pratik çeviri kontrolü yapar. | **User** |

### Folder Endpoint'leri

Klasör işlemleri `/api/folders` altında toplanmıştır.

| Metot | Endpoint | Açıklama | Yetki |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/folders` | Kullanıcının klasörlerini listeler. | **User** |
| `GET` | `/api/folders/{id}` | Klasör detaylarını getirir. | **User** |
| `POST` | `/api/folders` | Yeni klasör oluşturur. | **User** |
| `PUT` | `/api/folders` | Klasör ismini günceller. | **User** |
| `DELETE` | `/api/folders/{id}` | Klasörü siler. | **User** |
| `GET` | `/api/folders/{id}/words` | Klasör içeriğindeki kelimeleri listeler. | **User** |
| `POST` | `/api/folders/words` | Klasöre kelime ekler. | **User** |
| `DELETE` | `/api/folders/words` | Klasörden kelime çıkarır. | **User** |
| `POST` | `/api/folders/practice/check` | Klasör pratik çeviri kontrolü yapar. | **User** |

### Favorite & Unknowns Endpoint'leri

Favori ve bilinmeyen kelime yönetimi için kullanılır.

| Metot | Endpoint | Açıklama | Yetki |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/favorites` | Favori kelimeleri sayfalı listeler. | **User** |
| `POST` | `/api/favorites/toggle` | Kelimeyi favorilere ekler/çıkarır. | **User** |
| `DELETE` | `/api/favorites/{id}` | Favori kaydını siler. | **User** |
| `GET` | `/api/favorites/practice/random` | Favorilerden pratik için rastgele kelime getirir. | **User** |
| `POST` | `/api/favorites/practice/check` | Favori pratik çeviri kontrolü yapar. | **User** |
| `GET` | `/api/unknows` | Bilinmeyen kelimeleri sayfalı listeler. | **User** |
| `POST` | `/api/unknows/toggle` | Kelimeyi bilinmeyenlere ekler/çıkarır. | **User** |
| `DELETE` | `/api/unknows/{id}` | Bilinmeyen kaydını siler. | **User** |
| `GET` | `/api/unknows/practice/random` | Bilinmeyenlerden pratik için rastgele kelime getirir. | **User** |
| `POST` | `/api/unknows/practice/check` | Bilinmeyen pratik çeviri kontrolü yapar. | **User** |



### Admin Endpoint'leri

Admin paneli işlemleri `/api/admin` altında toplanmıştır. Tüm işlemler **admin** rolüne sahip kullanıcıya özeldir.

#### Allowed IP Endpoint'leri

| Metot | Endpoint | Açıklama | Yetki |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/admin/allowed-ips` | İzin verilen IP adreslerini listeler. | **Admin** |
| `GET` | `/api/admin/allowed-ips/{id}` | Belirtilen ID'ye sahip IP adresini getirir. | **Admin** |
| `POST` | `/api/admin/allowed-ips` | Yeni bir IP adresi ekler. | **Admin** |
| `PUT` | `/api/admin/allowed-ips` | Mevcut bir IP adresini günceller. | **Admin** |
| `DELETE` | `/api/admin/allowed-ips/{id}` | Belirtilen IP adresini siler. | **Admin** |

#### Role Endpoint'leri

| Metot | Endpoint | Açıklama | Yetki |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/admin/roles` | Tüm rolleri listeler. | **Admin** |
| `GET` | `/api/admin/roles/{id}` | Belirtilen ID'ye sahip rolü getirir. | **Admin** |
| `POST` | `/api/admin/roles` | Yeni bir rol oluşturur. | **Admin** |
| `PUT` | `/api/admin/roles` | Mevcut bir rolü günceller. | **Admin** |
| `DELETE` | `/api/admin/roles/{id}` | Belirtilen rolü siler. | **Admin** |
| `GET` | `/api/admin/roles/assign/{userId}` | Kullanıcıya atanabilecek rolleri listeler. | **Admin** |
| `POST` | `/api/admin/roles/assign` | Kullanıcıya rol ataması yapar. | **Admin** |

#### Dashboard Endpoint'leri

| Metot | Endpoint | Açıklama | Yetki |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/admin/dashboard` | Admin dashboard istatistiklerini getirir. | **Admin** |

#### User Endpoint'leri

| Metot | Endpoint | Açıklama | Yetki |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/admin/users` | Kullanıcıları sayfalı listeler (Arama destekler). | **Admin** |
| `GET` | `/api/admin/users/all` | Tüm kullanıcıları listeler. | **Admin** |
| `GET` | `/api/admin/users/{id}` | Kullanıcı detaylarını (Edit için) getirir. | **Admin** |
| `GET` | `/api/admin/users/{id}/detail` | Kullanıcı detaylarını (View için) getirir. | **Admin** |
| `PUT` | `/api/admin/users` | Kullanıcıyı günceller. | **Admin** |
| `DELETE` | `/api/admin/users/{id}` | Kullanıcıyı siler. | **Admin** |
| `POST` | `/api/admin/users/{id}/reset-password` | Kullanıcı şifresini sıfırlar. | **Admin** |

##  Kod Standartları

*   **Primary Constructors**: C# 12+ özelliği olan Primary Constructor yapısı kullanılır.
*   **ServiceResult Pattern**: Tüm servis metodları `ServiceResult<T>` döner.
*   **Async/Await**: Tüm işlemler asenkron olarak tasarlanmıştır.

---
*WordMaster Backend Team*
