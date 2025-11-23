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



##  Kod Standartları

*   **Primary Constructors**: C# 12+ özelliği olan Primary Constructor yapısı kullanılır.
*   **ServiceResult Pattern**: Tüm servis metodları `ServiceResult<T>` döner.
*   **Async/Await**: Tüm işlemler asenkron olarak tasarlanmıştır.

---
*WordMaster Backend Team*
