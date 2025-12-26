# 📝 Logging ve Serilog Yapılandırması

> ⚠️ **DEPRECATED (Kullanımdan Kaldırıldı)**
> 
> Bu dökümantasyon **artık geçerli değildir**. Serilog kaldırılmış ve 
> **OpenTelemetry ile OTLP** tabanlı loglama yapısına geçilmiştir.
> 
> Güncel dökümantasyon için bakınız: 
> - [`OTLP_Logging_Elasticsearch.md`](./OTLP_Logging_Elasticsearch.md) - OTLP ile Elasticsearch'e Loglama
> - [`OpenTelemetry_Observability.md`](./OpenTelemetry_Observability.md) - OpenTelemetry Genel Bakış
>
> **Geçiş Nedeni:** OpenTelemetry, tek bir SDK ile Logs + Metrics + Traces
> toplayarak merkezi observability sağlar. Serilog gereksiz katman haline gelmiştir.

---

*Aşağıdaki içerik arşiv amaçlıdır.*

---

WordMaster projesinde loglama altyapısı olarak **Serilog** kullanılmaktadır. Serilog, yapılandırılmış (structured) loglama sağlayan, güçlü ve esnek bir kütüphanedir.

## 📌 Serilog Nedir?

Serilog, .NET uygulamaları için geliştirilmiş, log verilerini düz metin yerine yapılandırılmış veri (JSON vb.) olarak kaydeden bir kütüphanedir. Bu sayede loglar üzerinde sorgulama, filtreleme ve analiz yapmak çok daha kolay hale gelir.

## ⚙️ Konfigürasyon

Serilog konfigürasyonu `appsettings.Development.json` (veya `appsettings.json`) dosyasında yönetilmektedir. Bu sayede kod değişikliği yapmadan log seviyeleri ve hedefleri değiştirilebilir.

### Örnek Konfigürasyon (`appsettings.Development.json`)

```json
"Serilog": {
  "Using": [
    "Serilog.Sinks.Console",
    "Serilog.Exceptions",
    "Serilog.Enrichers.ClientInfo",
    "Serilog.Enrichers.Environment"
  ],
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft": "Warning",
      "System": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "Enrich": [
    "FromLogContext",
    "WithExceptionDetails",
    "WithClientIp",
    "WithMachineName",
    "WithEnvironmentUserName",
    "WithCorrelationId"
  ],
  "WriteTo": [
    {
      "Name": "Console",
      "Args": {
        "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj} {Properties}{NewLine}{Exception}"
      }
    }
  ]
}
```

## ✨ Enrichers (Zenginleştiriciler)

Enricher'lar, log kayıtlarına otomatik olarak ek bağlam (context) verileri ekleyen bileşenlerdir. Projemizde kullanılan enricher'lar ve görevleri şunlardır:

*   **`FromLogContext`**: Kod içerisinde `LogContext.PushProperty` ile eklenen dinamik özellikleri loga dahil eder.
*   **`WithExceptionDetails`**: Hata durumlarında (Exception) standart mesajın ötesinde, stack trace ve inner exception gibi detaylı bilgileri loga ekler.
*   **`WithClientIp`**: İsteği yapan istemcinin IP adresini log kaydına ekler. Güvenlik ve izlenebilirlik için önemlidir.
*   **`WithMachineName`**: Uygulamanın çalıştığı sunucu veya makine adını ekler. Dağıtık sistemlerde hangi sunucunun log ürettiğini anlamak için kullanılır.
*   **`WithEnvironmentUserName`**: Uygulamanın çalıştığı ortamdaki işletim sistemi kullanıcı adını ekler.
*   **`WithCorrelationId`**: Her bir HTTP isteği için benzersiz bir ID oluşturur ve loga ekler. Bu sayede bir isteğin sistemdeki tüm yaşam döngüsü (tüm logları) tek bir ID ile takip edilebilir.

## 🏗 Host Entegrasyonu ve Yapılandırma Farkları

Projemizde Serilog, doğrudan **.NET Host** üzerine entegre edilmiştir.

### `builder.Host.UseSerilog` (Bizim Yaptığımız)

Biz konfigürasyonu `Program.cs` içerisinde `builder.Host.AddSerilogConfigurations()` extension metodu ile yapıyoruz. Bu metot arka planda `host.UseSerilog(...)` kullanır.

```csharp
// Program.cs
builder.Host.AddSerilogConfigurations();
```

**Neden Host Üzerinden?**
*   **.NET'in varsayılan loglama mekanizmasını tamamen değiştirir.** Sadece ek bir logger eklemekle kalmaz, framework'ün ürettiği logları da Serilog üzerinden geçirir.
*   **Uygulama ayağa kalkarken (Startup) oluşan logları da yakalar.**
*   `appsettings.json` değişikliklerini dinamik olarak algılayabilir.

### `builder.Host.UseSerilog` vs `builder.Logging.AddSerilog` vs `builder.Services.AddSerilog`

Kısaca söylemek gerekirse:

*   **`builder.Host.UseSerilog()`** → Host seviyesinde Serilog kullan
*   **`builder.Logging.AddSerilog()`** → Microsoft logging pipeline’ına Serilog provider ekle
*   **`builder.Services.AddSerilog()`** → DI konteynerine Serilog hizmetlerini ekle (çoğu senaryoda gerekmez)

Aşağıda detaylı, net bir açıklama var:

#### ✅ 1. `builder.Host.UseSerilog()`

🔸 **Doğru ve en yaygın kullanılan yöntemdir.**
ASP.NET Core’un Host (Generic Host) mekanizmasına Serilog’u bağlar.

**Ne anlama gelir?**
*   Uygulama daha ayağa kalkmadan (Startup/Program çalışmadan) Serilog aktif olur.
*   Host seviyesindeki tüm loglar (Kestrel, Dependency injection, Hosting, app init logları…) Serilog’a gider.
*   `appsettings.json` üzerinden Serilog konfigürasyonunu okuyabilir.

**Ne zaman kullanılır?**
*   ✔ Her zaman.
*   ✔ Modern .NET uygulamalarında tek tercih edilmesi gereken yöntem.

#### ✅ 2. `builder.Logging.AddSerilog()`

Bu yöntem Serilog’u `Microsoft.Extensions.Logging` altyapısına bir provider olarak ekler.

**Ne anlama gelir?**
*   `ILogger` üzerinden yapılan loglar Serilog’a yönlendirilir (provider gibi).
*   Ancak Host loglarını yakalayamaz.
*   Genellikle `UseSerilog` kullanıyorsan buna gerek yoktur.

**Ne zaman kullanılır?**
*   ❗ `UseSerilog` kullanmıyorsan (Örn: manuel host kuruyorsun).
*   ❗ Eski tarz `ConfigureLogging` yapıyorsan.

**Ne zaman kullanmamalısın?**
*   ❌ Hem `UseSerilog` hem `AddSerilog` kullanmak gereksizdir. Zaten `UseSerilog` → Logging pipeline içine Serilog’u ekler.

#### ✅ 3. `builder.Services.AddSerilog()`

Bu çoğu kişiyi yanıltır. Bu fonksiyon Serilog’u DI konteynerine bir servis olarak ekler.

**Ne anlama gelir?**
*   Serilog’un kendi `ILogger` implementasyonunu (`Serilog.ILogger`) DI üzerinden resolve etmek istersen kullanılır.
*   Yani `ILogger<T>` değil, doğrudan `Serilog.ILogger` enjekte edeceksen.

> **Not:** `app.UseSerilogRequestLogging()` ise bir middleware'dir ve HTTP isteklerini loglamak için kullanılır. Yukarıdaki yapılandırma metotlarından farklı olarak, uygulamanın çalışma zamanında (runtime) devreye girer.
