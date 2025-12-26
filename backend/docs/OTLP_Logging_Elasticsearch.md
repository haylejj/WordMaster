# 📊 OTLP ile Elasticsearch'e Loglama: Kapsamlı Kılavuz

Bu dökümantasyon, WordMaster projesinde **OpenTelemetry Protocol (OTLP)** kullanarak logların **Elasticsearch**'e gönderilmesi ve **Kibana**'da görüntülenmesi sürecini detaylı olarak açıklar.

## 📚 İçindekiler

1. [Mimari Genel Bakış](#mimari-genel-bakış)
2. [Serilog'dan OTLP'ye Geçiş](#serilogdan-otlpye-geçiş)
3. [Elasticsearch Temelleri](#elasticsearch-temelleri)
4. [Kibana Temelleri](#kibana-temelleri)
5. [Backend Konfigürasyonu](#backend-konfigürasyonu)
6. [OTLP Collector Konfigürasyonu](#otlp-collector-konfigürasyonu)
7. [Docker Compose Konfigürasyonu](#docker-compose-konfigürasyonu)
8. [Karşılaşılan Sorunlar ve Çözümleri](#karşılaşılan-sorunlar-ve-çözümleri)
9. [Kibana'da Log Görüntüleme](#kibanada-log-görüntüleme)

---

## Mimari Genel Bakış

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         LOGLAMA MİMARİSİ AKIŞI                              │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌───────────────┐                                                          │
│  │  WordMaster   │  (1) Log Üretir (_logger.LogInformation(...))           │
│  │   .NET API    │      • ILogger<T> kullanır                              │
│  │               │      • OpenTelemetry SDK ile entegre                    │
│  │  Port: 3002   │                                                          │
│  └───────┬───────┘                                                          │
│          │                                                                  │
│          │ (2) OTLP (gRPC - Port 4317)                                      │
│          │     Loglar binary format (Protobuf) ile gönderilir              │
│          ▼                                                                  │
│  ┌──────────────────┐                                                       │
│  │ OTel Collector   │  (3) Logları Alır, İşler ve Dağıtır                  │
│  │                  │      • Receiver: OTLP (gRPC/HTTP)                     │
│  │ Ports:           │      • Processor: Batch (toplu gönderim)              │
│  │  - 4317: gRPC    │      • Exporter: Elasticsearch                        │
│  │  - 4318: HTTP    │                                                       │
│  └──────────┬───────┘                                                       │
│             │                                                               │
│             │ (4) HTTP POST (Bulk API)                                      │
│             │     Loglar JSON olarak Elasticsearch'e gönderilir            │
│             ▼                                                               │
│  ┌──────────────────┐                                                       │
│  │  Elasticsearch   │  (5) Logları Depolar                                  │
│  │                  │      • Index: logs-generic-default-YYYY.MM.DD        │
│  │  Port: 9200      │      • Full-text search desteği                       │
│  │                  │      • Time-series veri yapısı                        │
│  └──────────┬───────┘                                                       │
│             │                                                               │
│             │ (6) Kibana Sorguları (KQL - Kibana Query Language)            │
│             ▼                                                               │
│  ┌──────────────────┐                                                       │
│  │     Kibana       │  (7) Görselleştirme ve Analiz                         │
│  │                  │      • Discover: Log arama/filtreleme                 │
│  │  Port: 5601      │      • Dashboards: Grafikler                          │
│  │                  │      • Alerts: Uyarılar                               │
│  └──────────────────┘                                                       │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Veri Akışı Özeti:

| Adım | Kaynak | Hedef | Protokol | Açıklama |
|------|--------|-------|----------|----------|
| 1 | API | OTel SDK | - | `ILogger` ile log yazılır |
| 2 | OTel SDK | Collector | gRPC (4317) | OTLP formatında iletim |
| 3 | Collector | Elasticsearch | HTTP (9200) | Bulk API ile index'leme |
| 4 | Elasticsearch | Kibana | HTTP (9200) | Query ve aggregation |

---

## Serilog'dan OTLP'ye Geçiş

### ❌ Eski Mimari: Serilog

```
.NET App ──Serilog──▶ Console
         ──Serilog──▶ File
         ──Serilog.Sinks.Elasticsearch──▶ Elasticsearch
```

**Serilog'un Dezavantajları:**

| Sorun | Açıklama |
|-------|----------|
| **Tight Coupling** | Her sink için ayrı NuGet paketi gerekli |
| **Kod Değişikliği** | Hedef değiştirmek = Kod değiştirmek |
| **Vendor Lock-in** | Elasticsearch'e özgü konfigürasyon |
| **Tek Sinyal** | Sadece loglar, metrics/traces ayrı çözümler gerektirir |

### ✅ Yeni Mimari: OpenTelemetry

```
.NET App ──OTLP──▶ [OTel Collector] ─┬─▶ Elasticsearch (Logs)
                                      ├─▶ Prometheus (Metrics)
                                      └─▶ Jaeger (Traces)
```

**OpenTelemetry'nin Avantajları:**

| Avantaj | Açıklama |
|---------|----------|
| **Loose Coupling** | Tek OTLP exporter, hedefler Collector'da tanımlanır |
| **Vendor Agnostic** | Standart protokol, herhangi bir backend'e geçiş kolay |
| **Merkezi Konfigürasyon** | Tüm routing Collector config'inde |
| **Unified Observability** | Logs + Metrics + Traces tek SDK ile |
| **Zengin Ekosistem** | Processors, receivers, exporters çeşitliliği |

### Neden Serilog'u Kaldırdık?

1. **Gereksiz Katman:** OpenTelemetry zaten `.NET ILogger` ile entegre çalışıyor
2. **Çakışma Riski:** İki logging altyapısı konfigürasyon karmaşıklığı yaratır
3. **Performans:** OTLP, gRPC ile binary format kullanır (daha hızlı)
4. **Tutarlılık:** Metrics OTLP ile gidiyor, loglar da aynı yoldan gitmeli

---

## Elasticsearch Temelleri

Elasticsearch, dağıtık bir **arama ve analitik motoru**dur. Logları depolamak ve sorgulamak için ideal bir çözümdür.

### Temel Kavramlar

#### 1. Index

**Tanım:** Veritabanındaki tablo gibi düşün. Loglar bir index içinde saklanır.

```
logs-generic-default-2025.12.26  ← Index adı (otomatik oluşturuldu)
├── _doc/abc123  ← Dokümant 1 (bir log kaydı)
├── _doc/def456  ← Dokümant 2
└── _doc/ghi789  ← Dokümant 3
```

**Index Adlandırma Stratejileri:**
- **Statik:** `logs-wordmaster` (tüm loglar tek index'te)
- **Tarih Bazlı:** `logs-wordmaster-2025.12.26` (günlük index)
- **Data Streams:** `logs-generic-default-*` (rolling index, Elasticsearch yönetir)

#### 2. Document (Dokümant)

**Tanım:** Tek bir log kaydı. JSON formatında saklanır.

```json
{
  "@timestamp": "2025-12-26T11:49:38.107Z",
  "message": "API Project is starting...",
  "log.level": "Information",
  "service.name": "wordmaster-api",
  "service.version": "1.0.0",
  "environment": "Development"
}
```

#### 3. Mapping

**Tanım:** Dokümandaki alanların veri tiplerini tanımlar (şema).

```json
{
  "mappings": {
    "properties": {
      "@timestamp": { "type": "date" },
      "message": { "type": "text" },
      "log.level": { "type": "keyword" },
      "service.name": { "type": "keyword" }
    }
  }
}
```

**Veri Tipleri:**
| Tip | Kullanım | Örnek |
|-----|----------|-------|
| `text` | Full-text arama | `message` |
| `keyword` | Exact match, aggregation | `service.name`, `log.level` |
| `date` | Tarih/zaman | `@timestamp` |
| `long/integer` | Sayısal değerler | `response.status_code` |

#### 4. Shard ve Replica

```
Index: logs-wordmaster
├── Primary Shard 0 ← Veri burada
│   └── Replica Shard 0 ← Kopya (başka node'da)
└── Primary Shard 1
    └── Replica Shard 1
```

**Neden Yellow Health?**
- `yellow` = Primary shardlar çalışıyor, replica yok
- Single-node kurulumda replica oluşturulamaz (başka node yok)
- **Development için tamamen normal!**

#### 5. Data Streams (Bizim Kullandığımız)

Data Streams, time-series veriler için optimize edilmiş özel bir index yapısıdır.

```
Data Stream: logs-generic-default
├── .ds-logs-generic-default-2025.12.25-000001  ← Backing index (dünkü)
└── .ds-logs-generic-default-2025.12.26-000001  ← Backing index (bugünkü)
```

**Avantajları:**
- Otomatik rolling (yeni index oluşturma)
- ILM (Index Lifecycle Management) ile uyumlu
- Append-only (sadece ekleme, güncelleme yok)

### Elasticsearch API Örnekleri

```bash
# Tüm indexleri listele
curl http://localhost:9200/_cat/indices?v

# Belirli pattern'e uyan indexler
curl http://localhost:9200/_cat/indices/logs-*?v

# Index içeriğini görüntüle (ilk 10 kayıt)
curl "http://localhost:9200/logs-generic-default/_search?size=10"

# Cluster sağlık durumu
curl http://localhost:9200/_cluster/health
```

---

## Kibana Temelleri

Kibana, Elasticsearch üzerindeki verileri görselleştirmek ve analiz etmek için kullanılan web arayüzüdür.

### Temel Kavramlar

#### 1. Data View (Eski adı: Index Pattern)

**Tanım:** Kibana'nın hangi Elasticsearch indexlerini sorgulaması gerektiğini tanımlar.

```
Data View: "logs-*"
├── logs-generic-default-2025.12.25-000001  ← Match eder
├── logs-generic-default-2025.12.26-000001  ← Match eder
└── .internal-alerts-*                       ← Match etmez
```

**Data View Oluşturma:**
1. Stack Management → Data Views
2. "Create data view"
3. Name: `WordMaster Logs`
4. Index pattern: `logs-*` veya `logs-generic-*`
5. Timestamp field: `@timestamp`

#### 2. Discover

**Tanım:** Logları arama, filtreleme ve inceleme arayüzü.

**Özellikler:**
- KQL (Kibana Query Language) ile sorgulama
- Zaman aralığı filtreleme
- Alan bazlı filtreleme
- Kayıtlı aramalar (Saved searches)

**KQL Örnekleri:**
```
# Belirli servis
service.name: "wordmaster-api"

# Hata logları
log.level: "Error" OR log.level: "Warning"

# Mesaj içinde arama
message: "database" AND environment: "Development"

# Zaman aralığı (Time picker'dan da yapılabilir)
@timestamp >= "2025-12-26" AND @timestamp < "2025-12-27"
```

#### 3. Saved Sessions / Saved Searches

**Tanım:** Sık kullandığın sorgu ve filtreleri kaydetme.

**Özellikler:**
- **Title:** Aramanın adı
- **Description:** Açıklama
- **Tags:** Etiketler (filtreleme için)
- **Store time:** Zaman aralığını da kaydet/kaydetme

### Kibana Loglama Workflow

```
1. Discover'ı Aç
      │
      ▼
2. Data View Seç (All logs veya Custom)
      │
      ▼
3. Zaman Aralığı Belirle (Last 15 minutes, Today, vb.)
      │
      ▼
4. KQL ile Filtrele (service.name: "wordmaster-api")
      │
      ▼
5. Gerekli Alanları Seç (environment, log.level, message, vb.)
      │
      ▼
6. Kaydet (Save Discover session)
```

---

## Backend Konfigürasyonu

### Gerekli NuGet Paketleri

```bash
# OpenTelemetry Core
dotnet add package OpenTelemetry

# Logging için
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol

# Metrics için (opsiyonel - metrics de kullanıyorsan)
dotnet add package OpenTelemetry.Extensions.Hosting
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
dotnet add package OpenTelemetry.Instrumentation.Http
dotnet add package OpenTelemetry.Instrumentation.Runtime
```

| Paket | Amaç |
|-------|------|
| `OpenTelemetry` | Core SDK |
| `OpenTelemetry.Exporter.OpenTelemetryProtocol` | OTLP exporter (gRPC/HTTP) |
| `OpenTelemetry.Extensions.Hosting` | Host entegrasyonu |
| `OpenTelemetry.Instrumentation.AspNetCore` | HTTP request/response otomatik izleme |
| `OpenTelemetry.Instrumentation.Http` | HttpClient çağrıları izleme |
| `OpenTelemetry.Instrumentation.Runtime` | GC, Thread Pool, Memory metrikleri |

### OpenTelemetryExtensions.cs

**Dosya:** `backend/WordMaster.API/Extensions/OpenTelemetryExtensions.cs`

```csharp
/// <summary>
/// OpenTelemetry logging servislerini yapılandırır ve OTLP exporter ekler.
/// Tüm application logları OTLP Collector'a gönderilir, oradan Elasticsearch'e route edilir.
/// </summary>
public static ILoggingBuilder AddOpenTelemetryLogging(
    this ILoggingBuilder logging, 
    IConfiguration configuration)
{
    // 1️⃣ Varsayılan provider'ları temizle (Console, Debug, vb.)
    logging.ClearProviders();

    // 2️⃣ Development ortamında console'da da logları göster
    var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
    if (environment == "Development")
    {
        logging.AddConsole();
    }

    // 3️⃣ OpenTelemetry logging provider'ı ekle
    logging.AddOpenTelemetry(options =>
    {
        // Resource: Logların "kimden" geldiğini tanımlar
        options.SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddService(
                serviceName: OpenTelemetryLogging.ServiceName,    // "wordmaster-api"
                serviceVersion: OpenTelemetryLogging.ServiceVersion // "1.0.0"
            )
            .AddAttributes(new Dictionary<string, object>
            {
                {"environment", AppInfo.EnvironmentName},  // "Development"
                {"version", AppInfo.Version}               // "1.0.0-dev"
            }));

        // 4️⃣ OTLP Exporter: Logları Collector'a gönder
        options.AddOtlpExporter(otlp =>
        {
            // gRPC protokolü (binary, hızlı)
            otlp.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            
            // Endpoint: Docker'da otel-collector:4317, local'de localhost:4317
            otlp.Endpoint = new Uri(
                configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317"
            );
            
            // Batch processing: Logları toplu gönder (performans)
            otlp.ExportProcessorType = ExportProcessorType.Batch;
        });

        // 5️⃣ Ek özellikler
        // Hem structured data hem formatted message
        options.IncludeFormattedMessage = true;
        
        // Log scope'larını dahil et (RequestId, CorrelationId vb.)
        options.IncludeScopes = true;
    });

    return logging;
}
```

### Satır Satır Açıklama

| Satır/Bölüm | Ne Yapar | Neden Gerekli |
|-------------|----------|---------------|
| `ClearProviders()` | Varsayılan provider'ları siler | Duplicate log önleme |
| `AddConsole()` | Console'a log yazar | Development'ta debug için |
| `SetResourceBuilder()` | Metadata ekler | Logları tanımlama (service.name) |
| `AddOtlpExporter()` | OTLP Collector'a gönderir | Merkezi log toplama |
| `Protocol.Grpc` | gRPC kullanır | HTTP'den 2-3x hızlı |
| `ExportProcessorType.Batch` | Toplu gönderim | Performans optimizasyonu |
| `IncludeFormattedMessage` | Mesaj formatını dahil eder | `"User 123 logged in"` |
| `IncludeScopes` | Scope bilgilerini dahil eder | RequestId ile izleme |

### Program.cs Entegrasyonu

```csharp
var builder = WebApplication.CreateBuilder(args);

// OpenTelemetry Logging
builder.Logging.AddOpenTelemetryLogging(builder.Configuration);

// OpenTelemetry Metrics
builder.Services.AddOpenTelemetryMetrics(builder.Configuration);

var app = builder.Build();
// ...
```

---

## OTLP Collector Konfigürasyonu

### otel-collector-config.yaml

**Dosya:** `observability/otel-collector-config.yaml`

```yaml
# ═══════════════════════════════════════════════════════════════════════════
# RECEIVERS: Veri kaynakları (Collector'a veri GELEN yerler)
# ═══════════════════════════════════════════════════════════════════════════
receivers:
  otlp:
    protocols:
      # gRPC: Binary format, en hızlı yol (Varsayılan ve önerilen)
      grpc:
        endpoint: 0.0.0.0:4317
      # HTTP: JSON format, debugging için kullanışlı
      http:
        endpoint: 0.0.0.0:4318

# ═══════════════════════════════════════════════════════════════════════════
# PROCESSORS: Veri işleme (Filtreleme, zenginleştirme, paketleme)
# ═══════════════════════════════════════════════════════════════════════════
processors:
  # batch: Veriyi hemen göndermez, biriktirir, toplu gönderir
  # Performansı artırır, ağ trafiğini azaltır
  batch:
    timeout: 10s           # Maksimum 10 saniye bekle
    send_batch_size: 1024  # 1024 kayıt birikince gönder

  # attributes: Tüm telemetri verilerine metadata ekler
  attributes:
    actions:
      - key: environment
        value: ${env:ENVIRONMENT}  # Docker env'den oku
        action: insert
      - key: service.name
        value: wordmaster-api
        action: insert

# ═══════════════════════════════════════════════════════════════════════════
# EXPORTERS: Veri hedefleri (Collector'dan veri GİDEN yerler)
# ═══════════════════════════════════════════════════════════════════════════
exporters:
  # Elasticsearch: Logları depolar
  elasticsearch:
    endpoint: "http://elasticsearch:9200"
    mapping:
      mode: ecs  # ECS (Elastic Common Schema) formatında

  # Prometheus: Metrics için (Pull modeli)
  prometheus:
    endpoint: "0.0.0.0:8889"

  # Debug: Container loglarına yazar (docker logs ile görülür)
  debug:
    verbosity: detailed

# ═══════════════════════════════════════════════════════════════════════════
# SERVICE: Pipeline tanımları (Veri akış haritası)
# ═══════════════════════════════════════════════════════════════════════════
service:
  pipelines:
    # LOG PIPELINE: OTLP → Batch + Attributes → Elasticsearch + Debug
    logs:
      receivers: [otlp]
      processors: [batch, attributes]
      exporters: [elasticsearch, debug]

    # METRIC PIPELINE: OTLP → Batch → Prometheus + Debug
    metrics:
      receivers: [otlp]
      processors: [batch]
      exporters: [prometheus, debug]

  # Collector'ın kendi telemetry'si
  telemetry:
    logs:
      level: info  # Collector'ın kendi logları
```

### Pipeline Akışı

```
                    LOGS PIPELINE
┌────────────────────────────────────────────────────────────────┐
│                                                                │
│  .NET API                                                      │
│     │                                                          │
│     │ OTLP (gRPC:4317)                                         │
│     ▼                                                          │
│  ┌─────────────────┐                                           │
│  │    RECEIVER     │  otlp (gRPC endpoint'i dinler)            │
│  └────────┬────────┘                                           │
│           │                                                    │
│           ▼                                                    │
│  ┌─────────────────┐                                           │
│  │   PROCESSORS    │  batch → attributes                       │
│  │                 │  • 10 saniye veya 1024 kayıt bekle        │
│  │                 │  • environment, service.name ekle         │
│  └────────┬────────┘                                           │
│           │                                                    │
│           ▼                                                    │
│  ┌─────────────────┐                                           │
│  │    EXPORTERS    │  elasticsearch + debug                    │
│  │                 │  • ES'e HTTP POST (Bulk API)              │
│  │                 │  • Container loglarına yaz                │
│  └─────────────────┘                                           │
│                                                                │
└────────────────────────────────────────────────────────────────┘
```

### Elasticsearch Exporter Detayları

| Ayar | Değer | Açıklama |
|------|-------|----------|
| `endpoint` | `http://elasticsearch:9200` | ES adresi (Docker network) |
| `mapping.mode` | `ecs` | ECS (Elastic Common Schema) formatı |

**ECS Nedir?**
- Elastic'in standart alan formatı
- `service.name`, `log.level`, `@timestamp` gibi alanlar
- Kibana'nın "All logs" data view'ı ile uyumlu

---

## Docker Compose Konfigürasyonu

### docker-compose.yml İlgili Servisler

```yaml
services:
  # ═══════════════════════════════════════════════════════════════════════════
  # OTEL-COLLECTOR: Telemetri verilerini toplayan merkezi hub
  # ═══════════════════════════════════════════════════════════════════════════
  otel-collector:
    image: otel/opentelemetry-collector-contrib:0.142.0
    container_name: wordmaster-otel-collector
    ports:
      - "4317:4317"  # gRPC - API buraya log/metric gönderir
      - "4318:4318"  # HTTP - Alternatif protokol
      - "8889:8889"  # Prometheus metrics endpoint
    command: ["--config=/etc/otel-collector-config.yaml"]
    volumes:
      - ./observability/otel-collector-config.yaml:/etc/otel-collector-config.yaml
    environment:
      - ENVIRONMENT=${ASPNETCORE_ENVIRONMENT:-Development}
    depends_on:
      - elasticsearch  # ES başlamadan Collector başlamasın
    networks:
      - wordmaster-network
    restart: unless-stopped

  # ═══════════════════════════════════════════════════════════════════════════
  # ELASTICSEARCH: Log depolama (Time-series database)
  # ═══════════════════════════════════════════════════════════════════════════
  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:9.2.3
    container_name: wordmaster-elasticsearch
    ports:
      - "9200:9200"  # REST API
    environment:
      - discovery.type=single-node      # Tek node (development)
      - xpack.security.enabled=false    # Güvenlik kapalı (development)
      - "ES_JAVA_OPTS=-Xms512m -Xmx512m" # Memory limit
    volumes:
      - elasticsearch-data:/usr/share/elasticsearch/data  # Veri kalıcılığı
    networks:
      - wordmaster-network
    restart: unless-stopped

  # ═══════════════════════════════════════════════════════════════════════════
  # KIBANA: Log görselleştirme ve analiz UI
  # ═══════════════════════════════════════════════════════════════════════════
  kibana:
    image: docker.elastic.co/kibana/kibana:9.2.3
    container_name: wordmaster-kibana
    ports:
      - "5601:5601"  # Web UI
    environment:
      - ELASTICSEARCH_HOSTS=http://elasticsearch:9200
    depends_on:
      - elasticsearch
    networks:
      - wordmaster-network
    restart: unless-stopped

  # ═══════════════════════════════════════════════════════════════════════════
  # API: OTLP ile log/metric gönderen uygulama
  # ═══════════════════════════════════════════════════════════════════════════
  api:
    # ... diğer ayarlar ...
    environment:
      - OpenTelemetry__Endpoint=${OTEL__ENDPOINT}  # http://otel-collector:4317
    depends_on:
      otel-collector:
        condition: service_started  # Collector başlamadan API başlamasın
```

### Port Açıklamaları

| Port | Servis | Protokol | Kullanım |
|------|--------|----------|----------|
| 4317 | OTel Collector | gRPC | API → Collector (Logs/Metrics) |
| 4318 | OTel Collector | HTTP | Alternatif iletim |
| 8889 | OTel Collector | HTTP | Prometheus scraping |
| 9200 | Elasticsearch | HTTP | REST API |
| 5601 | Kibana | HTTP | Web UI |

### Environment Variables

**.env dosyasında:**
```properties
# OTLP Collector adresi (Docker network içi)
OTEL__ENDPOINT=http://otel-collector:4317

# Environment adı (logları etiketlemek için)
ASPNETCORE_ENVIRONMENT=Development
```

**Neden `otel-collector:4317`?**
- Docker Compose'da containerlar birbirine `localhost` ile değil, **container_name** ile erişir
- `wordmaster-otel-collector` uzun olduğu için `otel-collector` service adı kullanılır

---

## Karşılaşılan Sorunlar ve Çözümleri

### Sorun 1: Index Not Found Exception

**Hata Mesajı:**
```
error.type: "index_not_found_exception"
failed to index document
index: "logs-wordmaster"
```

**Sebep:**
Eski konfigürasyonda sabit index adı kullanılıyordu:
```yaml
# ESKİ (HATALI)
elasticsearch:
  endpoint: "http://elasticsearch:9200"
  logs_index: "logs-wordmaster"  # ← Sabit index adı
```

`logs_index` ile sabit bir index adı verdiğinizde:
- Elasticsearch bu indexi **otomatik oluşturmuyordu**
- OTEL Collector bu indexe yazamıyordu

**Çözüm:**
Dynamic routing kullanarak indexlerin otomatik oluşturulmasını sağladık:
```yaml
# YENİ (DOĞRU)
elasticsearch:
  endpoint: "http://elasticsearch:9200"
  mapping:
    mode: ecs  # ECS formatı, otomatik index oluşturma
```

**Sonuç:**
- Index otomatik oluşturuldu: `logs-generic-default-2025.12.26-000001`
- Kibana'nın "All logs" data view'ı (`logs-*`) bu indexi otomatik yakaladı

### Sorun 2: Collector Config Çakışması

**Hata Mesajı:**
```
invalid configuration: must not specify both logs_index and logs_dynamic_index
```

**Sebep:**
Hem `logs_index` hem `logs_dynamic_index` belirtilmişti:
```yaml
# HATALI
elasticsearch:
  logs_index: "logs-wordmaster"
  logs_dynamic_index:
    enabled: true
```

**Çözüm:**
`logs_index`'i kaldırdık, sadece dynamic routing kullandık (varsayılan).

### Sorun 3: Yellow Health Status

**Gözlem:**
```
health status index
yellow open   .ds-logs-generic-default-2025.12.26-000001
```

**Sebep:**
- Single-node Elasticsearch
- Replica shard oluşturulamıyor (başka node yok)

**Durum:**
- ⚠️ **Yellow = Normal (Development için)**
- Primary shardlar çalışıyor, veri güvenli

**Production İçin:**
- Birden fazla node kullan
- Replica sayısını ayarla

---

## Kibana'da Log Görüntüleme

### Adım 1: Kibana'ya Erişim

```
URL: http://localhost:5601
```

### Adım 2: Discover'ı Aç

Sol menüden: **Analytics → Discover**

### Adım 3: Data View Seç

- Varsayılan olarak "All logs" (`logs-*`) mevcut
- Veya custom data view oluştur:
  - Stack Management → Data Views → Create data view
  - Name: `WordMaster Logs`
  - Index pattern: `logs-generic-*`
  - Timestamp field: `@timestamp`

### Adım 4: Logları Filtrele

**KQL Sorgu Örnekleri:**

```
# Sadece WordMaster servisi
service.name: "wordmaster-api"

# Error ve Warning logları
log.level: ("Error" OR "Warning")

# Belirli mesaj içeren
message: "database" OR message: "connection"

# Birleşik sorgu
service.name: "wordmaster-api" AND log.level: "Error" AND environment: "Development"
```

### Adım 5: Alanları Seç

Sol panelden görüntülemek istediğin alanları seç:
- `@timestamp`
- `log.level`
- `service.name`
- `message`
- `environment`

### Adım 6: Aramayı Kaydet

1. "Save" butonuna tıkla
2. Title: `WordMaster API Logs`
3. Description: `WordMaster backend servisinin logları`
4. Tags: `wordmaster`, `api`

---

## Özet ve Best Practices

### ✅ Doğru Yaklaşımlar

| Konu | Best Practice |
|------|---------------|
| **Protokol** | gRPC (4317) kullan, HTTP'den hızlı |
| **Batch Processing** | Logları toplu gönder (performans) |
| **Mapping Mode** | `ecs` kullan (Kibana uyumluluğu) |
| **Dynamic Index** | Sabit index yerine dynamic routing |
| **Environment Tags** | Loglara environment ekle (filtreleme) |

### ❌ Kaçınılması Gerekenler

| Konu | Kaçınılması Gereken |
|------|---------------------|
| **Dual Logging** | Serilog + OpenTelemetry birlikte kullanma |
| **Static Index** | Sabit `logs_index` kullanma |
| **Console in Prod** | Production'da console logger açık bırakma |