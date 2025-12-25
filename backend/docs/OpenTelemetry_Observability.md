# OpenTelemetry ile Observability (Gözlemlenebilirlik)

## 📚 İçindekiler
1. [Observability Nedir?](#observability-nedir)
2. [OpenTelemetry (OTLP) Nedir?](#opentelemetry-otlp-nedir)
3. [Kullandığımız Araçlar](#kullandığımız-araçlar)
4. [Mimari Genel Bakış](#mimari-genel-bakış)
5. [Veri Akışı](#veri-akışı)
6. [Kod İçi Implementasyon](#kod-içi-implementasyon)
7. [Docker Compose Konfigürasyonu](#docker-compose-konfigürasyonu)
8. [OpenTelemetry Collector Konfigürasyonu](#opentelemetry-collector-konfigürasyonu)
9. [Prometheus Konfigürasyonu](#prometheus-konfigürasyonu)
10. [Grafana Kullanımı](#grafana-kullanımı)
11. [Sorun Giderme](#sorun-giderme)

---

## Observability Nedir?

**Observability (Gözlemlenebilirlik)**, bir sistemin içini "göremeden" dış çıktılarına bakarak o sistemin ne durumda olduğunu anlayabilme yeteneğidir.

### 3 Temel Sinyal (Telemetry Pillars):

```
┌────────────────────────────────────────────────────────────────┐
│                   OBSERVABILITY 3 SİNYALİ                      │
├────────────────────────────────────────────────────────────────┤
│                                                                │
│  1️⃣ METRICS (Ölçümler)                                         │
│     └─ "Ne kadar?" sorusuna cevap verir                        │
│     └─ Örnekler:                                               │
│        • Son 1 dakikada 150 request geldi                      │
│        • Ortalama response süresi 45ms                         │
│        • Aktif kullanıcı sayısı: 23                            │
│     └─ Kullanım: Trendleri görmek, alert kurmak               │
│                                                                │
│  2️⃣ TRACES (İzler)                                             │
│     └─ "Request nerelerden geçti?" sorusuna cevap             │
│     └─ Örnekler:                                               │
│        • Login isteği → Auth Service → DB → Cache             │
│        • Her adımın süresi: Auth(20ms) + DB(35ms) + Cache(2ms)│
│     └─ Kullanım: Performans darboğazlarını bulmak            │
│                                                                │
│  3️⃣ LOGS (Kayıtlar)                                            │
│     └─ "O anda ne oldu?" sorusuna cevap                       │
│     └─ Örnekler:                                               │
│        • [13:45:12] User 42 logged in successfully            │
│        • [13:45:15] ERROR: Database timeout                   │
│     └─ Kullanım: Hata ayıklama, audit                         │
│                                                                │
└────────────────────────────────────────────────────────────────┘
```

**Bu projede odaklandığımız:** `METRICS`.

---

## OpenTelemetry (OTLP) Nedir?

**OpenTelemetry**, telemetri verilerini (Metrics, Traces, Logs) toplamak, işlemek ve dışarı aktarmak için **açık kaynak, vendor-agnostic** (satıcı bağımsız) bir standarttır.

### Neden OpenTelemetry?

#### ❌ Eski Yöntem (Vendor Lock-in):
```
.NET App ──JaegerExporter──▶ Jaeger
         ──PrometheusExporter──▶ Prometheus
         ──DatadogExporter──▶ Datadog
```
- Her backend için ayrı NuGet paketi
- Backend değiştirmek = Kod değiştirmek
- Ayrı ayrı yapılandırma

#### ✅ OpenTelemetry Yöntemi:
```
.NET App ──OTLP──▶ [OpenTelemetry Collector] ─┬─▶ Jaeger
                                               ├─▶ Prometheus
                                               ├─▶ Datadog
                                               └─▶ Loki
```
- Tek exporter (OTLP)
- Backend değiştirmek = Sadece Collector config'i değiştirmek
- Merkezi yapılandırma

### OTLP (OpenTelemetry Protocol)

**OTLP**, OpenTelemetry'nin resmi iletişim protokolüdür.

- **Transport:** gRPC (Binary, hızlı) veya HTTP/JSON
- **Port:** 
  - `4317` → gRPC (Varsayılan ve önerilen)
  - `4318` → HTTP
- **Format:** Protocol Buffers (Protobuf)

---

## Kullandığımız Araçlar

### 1. **OpenTelemetry .NET SDK**
- **Görev:** Uygulama içinde metricleri toplamak
- **Nasıl?** `services.AddOpenTelemetry().WithMetrics(...)`
- **Ne Toplar?**
  - HTTP Request süreleri (`http_server_request_duration`)
  - Runtime bilgileri (GC, Thread Pool)
  - Özel (Custom) metricler

### 2. **OpenTelemetry Collector**
- **Görev:** Telemetri verilerini almak, işlemek ve dağıtmak (Hub)
- **Neden Gerekli?**
  - Uygulamayı backend'den ayırır (Loose coupling)
  - Birden fazla uygulamadan veri toplayabilir
  - Veriyi filtreleyebilir, zenginleştirebilir (Processor'lar ile)
- **Docker Image:** `otel/opentelemetry-collector-contrib`

### 3. **Prometheus**
- **Görev:** Time-series (zaman serisi) veritabanı - Metricleri saklar
- **Nasıl Çalışır?** "Pull" modeli (Scrape):
  - Belirli aralıklarla (örn: her 10sn) Collector'a gelip veriyi çeker
- **Port:** `9090` (Web UI)
- **Sorgu Dili:** PromQL (Prometheus Query Language)

### 4. **Grafana**
- **Görev:** Görselleştirme (Dashboards)
- **Prometheus'tan farkı:**
  - Prometheus: Ham veri + Basit grafikler
  - Grafana: Profesyonel dashboardlar, alerting, panel'ler
- **Port:** `3000`
- **Data Source:** Prometheus'a bağlanır
---

## Mimari Genel Bakış

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         OBSERVABILITY MİMARİSİ                          │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌───────────────┐                                                      │
│  │  WordMaster   │  (1) Metric Üretir (Counter, Histogram)             │
│  │   .NET API    │      • HTTP Request Count                            │
│  │               │      • Request Duration                              │
│  │  Port: 3002   │      • Runtime (GC, Memory)                          │
│  └───────┬───────┘                                                      │
│          │                                                              │
│          │ (2) OTLP (gRPC)                                              │
│          │     http://wordmaster-otel-collector:4317                    │
│          ▼                                                              │
│  ┌──────────────────┐                                                   │
│  │ OTel Collector   │  (3) Veriyi İşler (Processors)                    │
│  │                  │      • Batch: Toplu paketler                      │
│  │ Ports:           │      • (İleride: Filter, Attributes, vb.)         │
│  │  - 4317: gRPC    │                                                   │
│  │  - 8889: Metrics │  (4) Prometheus Formatında Sunar                  │
│  └──────────┬───────┘      http://wordmaster-otel-collector:8889/metrics│
│             │                                                           │
│             │ (5) Prometheus Scraping (Pull - Her 10sn)                 │
│             ▼                                                           │
│  ┌──────────────────┐                                                   │
│  │   Prometheus     │  (6) Veritabanı (Time-Series)                     │
│  │                  │      • Son 15 gün boyunca tüm metricler           │
│  │  Port: 9090      │      • PromQL ile sorgulanabilir                  │
│  └──────────┬───────┘                                                   │
│             │                                                           │
│             │ (7) Grafana Sorguları (PromQL)                            │
│             ▼                                                           │
│  ┌──────────────────┐                                                   │
│  │     Grafana      │  (8) Dashboardlar                                 │
│  │                  │      • Request Rate Grafiği                       │
│  │  Port: 3000      │      • Error Rate Paneli                          │
│  │  admin/admin     │      • CPU/Memory Kullanımı                       │
│  └──────────────────┘                                                   │
│             ▲                                                           │
│             │ (9) Kullanıcı Tarayıcıdan Bağlanır                        │
│  ┌──────────┴───────┐                                                   │
│  │   Developer      │  http://localhost:3000                            │
│  └──────────────────┘                                                   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Veri Akışı

### Adım Adım Akış:

```
[1] User API'ye istek atar: GET /api/v1/words
          │
          ▼
[2] ASP.NET Core Instrumentation metrici yakalar:
    • http_server_request_duration_seconds: 0.042 (42ms)
    • http.method: GET
    • http.route: /api/v1/words
    • http.status_code: 200
          │
          ▼
[3] OTel SDK metriği OTLP Exporter'a verir
          │
          ▼
[4] OTLP Exporter → Collector'a gRPC ile gönderir
    Endpoint: http://wordmaster-otel-collector:4317
          │
          ▼
[5] Collector metriği alır:
    • Receivers: otlp (gRPC 4317'yi dinler)
    • Processors: batch (Paketler, hemen göndermez)
          │
          ▼
[6] Collector metriği Prometheus formatına çevirir:
    • Exporters: prometheus (8889 portunda sunar)
    • Format: Text (Prometheus Exposition Format)
          │
          ▼
[7] Prometheus her 10 saniyede Collector'a gelir:
    HTTP GET http://wordmaster-otel-collector:8889/metrics
    Aldığı veriyi veritabanına yazar
          │
          ▼
[8] Grafana Prometheus'a sorgu atar:
    PromQL: rate(http_server_request_duration_seconds_count[1m])
    Prometheus: "Son 1 dakikada saniyede 2.5 request var"
          │
          ▼
[9] Grafana grafiği çizer, kullanıcıya gösterir
```

---

## Kod İçi Implementasyon

### 1. OpenTelemetryMetric.cs (Constants)

**Dosya:** `backend/WordMaster.Application/Constants/OpenTelemetryMetric.cs`

```csharp
using System.Diagnostics.Metrics;

namespace WordMaster.Application.Constants;

public static class OpenTelemetryMetric
{
    // MeterName: Metric'leri gruplamak için benzersiz ad (Namespace gibi)
    public const string MeterName = "WordMaster";
    
    // MeterVersion: Metric yapısının versiyonu
    public const string MeterVersion = "1.0.0";
    
    public const string ServiceName = "WordMaster";
    
    // Meter: Metric üreticisi (Factory)
    // Counter, Histogram gibi metricleri oluşturmak için kullanılır
    public static Meter Meter = new(MeterName, MeterVersion);
}
```

**Ne İşe Yarar?**
- `Meter`, metric üreten fabrika gibidir.
- İleride custom metric eklemek istersen: `OpenTelemetryMetric.Meter.CreateCounter("login_count")`
- Şu an sadece built-in metricler (ASP.NET Core) kullanıyoruz.

---

### 2. OpenTelemetryExtensions.cs (Konfigürasyon)

**Dosya:** `backend/WordMaster.API/Extensions/OpenTelemetryExtensions.cs`

```csharp
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using WordMaster.Application.Constants;

namespace WordMaster.API.Extensions;

public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddOpenTelemetryMetrics(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                // ─────────────────────────────────────────────────
                // ADIM 1: Custom Meter'ı Kaydet
                // ─────────────────────────────────────────────────
                // Eğer yazmazsak, custom metriclerimiz toplanmaz.
                metrics.AddMeter(OpenTelemetryMetric.MeterName);
                
                // ─────────────────────────────────────────────────
                // ADIM 2: Resource (Kimlik Kartı) Tanımla
                // ─────────────────────────────────────────────────
                // Bu verinin "kimden geldiğini" tanımlar.
                // Prometheus/Grafana'da "job", "service" etiketleri olarak görünür.
                metrics.ConfigureResource(resource =>
                {
                    resource.AddService(
                        serviceName: OpenTelemetryMetric.ServiceName,    // "WordMaster"
                        serviceVersion: OpenTelemetryMetric.MeterVersion // "1.0.0"
                    );
                    resource.AddAttributes(new List<KeyValuePair<string, object>>
                    {
                        new("environment", AppInfo.EnvironmentName) // "Development" / "Production"
                    });
                });
                
                // ─────────────────────────────────────────────────
                // ADIM 3: Built-in Instrumentation'lar
                // ─────────────────────────────────────────────────
                // Otomatik metricler:
                
                // ASP.NET Core: HTTP Request süreleri, sayıları
                metrics.AddAspNetCoreInstrumentation();
                
                // Runtime: GC sayısı, Thread Pool bilgisi, Bellek kullanımı
                metrics.AddRuntimeInstrumentation();
                
                // HttpClient: Dış API'lere yapılan isteklerin metrikleri
                metrics.AddHttpClientInstrumentation();
                
                // ─────────────────────────────────────────────────
                // ADIM 4: OTLP Exporter (Collector'a Gönderim)
                // ─────────────────────────────────────────────────
                metrics.AddOtlpExporter(options =>
                {
                    // Protokol: gRPC (Hızlı, Binary)
                    options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    
                    // Endpoint:
                    // - Docker'da: http://wordmaster-otel-collector:4317
                    // - Local'de: http://localhost:4317 (Default)
                    options.Endpoint = new Uri(
                        configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317"
                    );
                });
            });
        return services;
    }
}
```

**Satır Satır Açıklama:**

| Satır | Ne Yapar |
|-------|----------|
| `AddMeter(...)` | "WordMaster" isimli meter'dan gelen metrikleri topla |
| `ConfigureResource(...)` | Veriyi etiketler: service.name="WordMaster", environment="Development" |
| `AddAspNetCoreInstrumentation()` | HTTP isteklerini otomatik izler |
| `AddRuntimeInstrumentation()` | GC, CPU, Memory metriclerini toplar |
| `AddOtlpExporter(...)` | Veriyi Collector'a gönderir (gRPC ile) |

---

### 3. Program.cs (Extension Çağrısı)

**Dosya:** `backend/WordMaster.API/Program.cs`

```csharp
// ... diğer servisler

builder.Services.AddOpenTelemetryMetrics(builder.Configuration);

// ... diğer servisler
```

**Neden Burada?**
- DI (Dependency Injection) container'a OpenTelemetry servislerini kaydetmek için.
- Uygulama başladığında metricleri toplamaya başlar.

---

## Docker Compose Konfigürasyonu

### Eklenen Servisler:

#### 1. OpenTelemetry Collector

```yaml
otel-collector:
  image: otel/opentelemetry-collector-contrib:0.142.0
  container_name: wordmaster-otel-collector
  ports:
    - "4317:4317" # gRPC (API buraya gönderir)
    - "4318:4318" # HTTP (Alternatif)
    - "8889:8889" # Prometheus Metrics (Prometheus buradan çeker)
  command: ["--config=/etc/otel-collector-config.yaml"]
  volumes:
    - ./observability/otel-collector-config.yaml:/etc/otel-collector-config.yaml
  networks:
    - wordmaster-network
  restart: unless-stopped
```

**Açıklama:**
- `image`: Collector'ın contrib versiyonu (Daha fazla exporter/receiver içerir)
- `ports`: 
  - `4317`: API'nin OTLP ile bağlandığı port
  - `8889`: Prometheus'un veri çektiği port
- `command`: Collector'a config dosyasının yerini söyler
- `volumes`: Host'taki config dosyasını container içine mount eder

---

#### 2. Prometheus

```yaml
prometheus:
  image: prom/prometheus:v2.54.1
  container_name: wordmaster-prometheus
  ports:
    - "9090:9090" # Web UI
  volumes:
    - ./observability/prometheus.yml:/etc/prometheus/prometheus.yml
  networks:
    - wordmaster-network
  restart: unless-stopped
```

**Açıklama:**
- `9090`: Web UI ve API portu
- `prometheus.yml`: Hangi hedeflerden scrape edeceğini tanımlar

---

#### 3. Grafana

```yaml
grafana:
  image: grafana/grafana:11.3.1
  container_name: wordmaster-grafana
  ports:
    - "3000:3000"
  environment:
    - GF_SECURITY_ADMIN_USER=${GRAFANA__SECURITY__ADMIN__USER}
    - GF_SECURITY_ADMIN_PASSWORD=${GRAFANA__SECURITY__ADMIN__PASSWORD}
  volumes:
    - grafana-data:/var/lib/grafana
  networks:
    - wordmaster-network
  restart: unless-stopped
```

**Açıklama:**
- `3000`: Dashboard UI
- `environment`: Admin kullanıcı bilgileri (.env'den okunur)
- `volumes`: Grafana ayarları ve dashboardları kalıcı olarak saklanır

---

#### 4. API Servisine Eklenen Environment Variable

```yaml
api:
  # ... diğer ayarlar
  environment:
    # ... diğer env var'lar
    - OpenTelemetry__Endpoint=${OTEL__ENDPOINT}
```

**.env Dosyasında:**
```properties
OTEL__ENDPOINT=http://wordmaster-otel-collector:4317
```

**Neden Gerekli?**
- Docker içinde containerlar birbirine `localhost` ile değil, **container ismi** ile erişir.
- API, Collector'a `http://wordmaster-otel-collector:4317` adresiyle bağlanır.

---

## OpenTelemetry Collector Konfigürasyonu

**Dosya:** `observability/otel-collector-config.yaml`

```yaml
receivers:
  # otlp: API'den veri alır
  otlp:
    protocols:
      grpc:
        endpoint: 0.0.0.0:4317 # Tüm network interface'lerden dinle
      http:
        endpoint: 0.0.0.0:4318

processors:
  # batch: Veriyi hemen göndermez, biriktirir, toplu gönderir (Performans+)
  batch:

exporters:
  # debug: Container loglarına basar (docker logs wordmaster-otel-collector)
  # Hata ayıklama için. Production'da silinebilir.
  debug:
    verbosity: detailed
  
  # prometheus: Metricleri Prometheus formatında 8889 portunda sunar
  prometheus:
    endpoint: "0.0.0.0:8889"

service:
  pipelines:
    metrics:
      receivers: [otlp]        # Kaynak: API (OTLP)
      processors: [batch]      # İşleme: Paketleme
      exporters: [debug, prometheus]  # Hedef: Loglar + Prometheus
```

**Pipeline Mantığı:**
```
API Metriği → [Receiver: OTLP] → [Processor: Batch] → [Exporter: Prometheus]
```

### Processor Örnekleri (İleride Eklenebilir):

```yaml
processors:
  # Bellek limiti (Çökme önleme)
  memory_limiter:
    check_interval: 1s
    limit_mib: 1000
  
  # Hassas veriyi silme (GDPR vb.)
  attributes:
    actions:
      - key: user_password
        action: delete
  
  # Health check isteklerini filtreleme
  filter:
    metrics:
      exclude:
        match_type: strict
        metric_names:
          - http_server_request_duration_seconds{http.route="/health"}
```

---

## Prometheus Konfigürasyonu

**Dosya:** `observability/prometheus.yml`

```yaml
global:
  scrape_interval: 15s # Varsayılan scraping aralığı

scrape_configs:
  - job_name: 'otel-collector'  # İş adı (Prometheus'ta "job" etiketi olur)
    scrape_interval: 10s        # Bu job için özel interval (Daha sık)
    static_configs:
      - targets: ['wordmaster-otel-collector:8889']  # Hedef adres
```

**Açıklama:**
- `scrape_interval`: Prometheus her 10 saniyede bir Collector'a HTTP GET yapar.
- `targets`: Hangi adresten veri çekileceği. Docker network'ü sayesinde container ismi ile erişir.
- `job_name`: Metriklere `job="otel-collector"` etiketi ekler.

**Prometheus Ne Yapar?**
1. Her 10 saniyede `http://wordmaster-otel-collector:8889/metrics` adresine GET atar.
2. Dönen text formatındaki metricleri parse eder.
3. Veritabanına kaydeder (Time-series).

---

## Grafana Kullanımı

### İlk Kurulum:

1. **Giriş Yap:**
   - URL: `http://localhost:3000`
   - Kullanıcı: `admin`
   - Şifre: `.env` dosyasındaki `GRAFANA__SECURITY__ADMIN__PASSWORD`

2. **Data Source Ekle:**
   - Sol menü: `Connections` → `Data sources` → `Add data source`
   - **Prometheus** seç
   - URL: `http://wordmaster-prometheus:9090 veya http://prometheus:9090`
   - `Save & Test` → Yeşil ✅

3. **Dashboard Import Et:**
   - Sol menü: `Dashboards` → `New` → `Import`
   - Dashboard ID: `19924` (ASP.NET Core Dashboard)
   - Data source: `Prometheus` seç
   - `Import`

### Örnek PromQL Sorguları:

| Amaç | PromQL Sorgusu |
|------|----------------|
| Toplam Request Sayısı | `http_server_request_duration_seconds_count` |
| Saniyede Request Oranı | `rate(http_server_request_duration_seconds_count[1m])` |
| Ortalama Response Süresi | `rate(http_server_request_duration_seconds_sum[5m]) / rate(http_server_request_duration_seconds_count[5m])` |
| 95th Percentile | `histogram_quantile(0.95, http_server_request_duration_seconds_bucket)` |
| Endpoint'e Göre Hata Oranı | `sum(rate(http_server_request_duration_seconds_count{http_status_code=~"5.."}[5m])) by (http_route)` |

---

## Sorun Giderme

### 1. Grafana'da Veri Görünmüyor

**Kontrol Listesi:**
- API çalışıyor mu? (`docker ps`)
- API'ye request atıldı mı? (Metricler request gelince üretilir)
- Collector logları: `docker logs wordmaster-otel-collector`
  - `debug` exporter'da veri görüyor musun?
- Prometheus targets: `http://localhost:9090/targets`
  - `otel-collector` job'u UP durumda mı?

**Muhtemel Sebepler:**
- API, Collector'a ulaşamıyor: Endpoint yanlış (`OTEL__ENDPOINT` kontrol et)
- Prometheus, Collector'a ulaşamıyor: `prometheus.yml` targets kontrolü

---

### 2. "Connection Refused" Hatası

**Sebep:** Docker network içinde `localhost` kullanıldı.

**Çözüm:**
- `localhost:4317` yerine `wordmaster-otel-collector:4317` kullan.
- `.env` dosyasında `OTEL__ENDPOINT` kontrolü yap.

---


