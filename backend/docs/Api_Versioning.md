# API Versioning Stratejisi

WordMaster API projesinde, API'nin evrimleşebilmesi ve geriye dönük uyumluluk (backward compatibility) sağlanabilmesi için **API Versioning** yapısı kurulmuştur.

Bu dokümanda, kullanılan versiyonlama stratejisi ve geliştiricilerin (frontend/mobil ekibi) buna nasıl uyum sağlayacağı anlatılmaktadır.

## 1. Versiyonlama Yöntemi: URL Path Versioning

Sektör standartlarına ve en iyi pratiklere (Best Practices) dayanarak, en açık ve anlaşılır yöntem olan **URL Path Versioning** tercih edilmiştir.

Bu yöntemde, versiyon numarası doğrudan URL yolunun bir parçasıdır.

### URL Formatı
```
/api/v{major}.{minor}/{controller}
```

Örnekler:
*   `GET /api/v1/words` -> Versiyon 1.0 (Varsayılan)
*   `GET /api/v2/words` -> Versiyon 2.0 (Gelecekteki versiyon)

**Neden Bu Yöntem Seçildi?**
*   **Açıklık (Explicitness):** URL'e bakıldığında hangi versiyonun kullanıldığı net bir şekilde anlaşılır.
*   **Cache Yönetimi:** CDN ve Proxy sunucuları, URL tabanlı farklılıkları çok daha kolay ve doğru bir şekilde önbellekler.
*   **Kolay Test:** Tarayıcı üzerinden veya Postman gibi araçlarla test ederken versiyonlar arası geçiş yapmak sadece URL'deki bir rakamı değiştirmek kadar basittir.

## 2. İstemci (Frontend/Mobile) Tarafı Kullanımı

Uygulamanız API'ye istek atarken, base URL yapısına versiyon bilgisini dahil etmelidir.

*   Mevcut tüm endpoint'ler varsayılan olarak **v1** kabul edilir.
*   Eğer URL'de versiyon belirtilmezse (örn: `/api/words`), sistem otomatik olarak bunu `v1.0` olarak işler. Ancak önerilen yöntem **her zaman versiyonu belirtmektir**.

**Örnek JavaScript (Axios) Konfigürasyonu:**

```javascript
const api = axios.create({
  baseURL: 'https://api.wordmaster.com/api/v1', // Versiyon burada belirtilir
  timeout: 10000,
});

// İstek: https://api.wordmaster.com/api/v1/words
api.get('/words'); 
```

Eğer ileride `v2` yayınlanırsa, sadece yeni özellikleri kullanacak olan modüllerde `baseURL` veya ilgili isteğin URL'i `/api/v2` olarak güncellenmelidir.

## 3. Swagger (API Dokümantasyonu) Kullanımı

API Versioning, Swagger UI ile tam entegre çalışır. `/swagger` adresine gittiğinizde sağ üst köşede bir versiyon seçici (Select a definition) göreceksiniz.

*   **v1**: Mevcut kararlı sürüm.
*   **v2 (Gelecek)**: Eğer bir endpoint'in yeni versiyonu yayınlanırsa, bu listede "v2" seçeneği belirecektir.

Swagger üzerinden endpoint'leri denerken, sistem seçilen versiyona göre URL'i otomatik oluşturur.

## 4. Backend Geliştirme Notları (Developer Guide)

Yeni bir controller veya versiyon eklerken dikkat edilmesi gerekenler:

1.  **Namespace:** Yeni versiyonları `Controllers/v2` gibi klasörlerde organize edin.
2.  **Attribute:** Controller sınıfına `[ApiVersion("2.0")]` attribute'unu ekleyin.
3.  **Route:** Route tanımının dinamik olduğundan emin olun:
    ```csharp
    [Route("api/v{version:apiVersion}/[controller]")]
    ```

**Deprecated (Kullanımdan Kaldırma):**
Eski bir versiyonu kullanımdan kaldırmak isterseniz `[Deprecated]` işaretlemesi yapabilirsiniz. Bu durumda Swagger'da ilgili endpoint silik görünecek ve "Deprecated" uyarısı çıkacaktır. ama API çalışmaya devam edecektir (Sunset policy uygulanana kadar).

## 5. Swagger Entegrasyonu Nasıl Çalışıyor? (Teknik Altyapı)

Bizim yazdığımız kodlarda Swagger'ın bu versiyonları "otomatik" olarak nasıl algıladığını merak ediyorsanız, arka plandaki akış şöyledir:

### Adım 1: Versiyonların Keşfedilmesi
Projeye eklediğimiz `Asp.Versioning.Mvc.ApiExplorer` paketi, uygulama başlarken tüm Controller'ları tarar. `[ApiVersion("...")]` attribute'larını bulur ve hangi Controller'ın hangi versiyona ait olduğunu gruplar.

### Adım 2: Belgelerin Oluşturulması (`ConfigureSwaggerOptions.cs`)
Bizim yazdığımız `ConfigureSwaggerOptions` sınıfı devreye girer. Bu sınıf, keşfedilen her versiyon grubu (v1, v2 vb.) için bir döngü kurar:
```csharp
foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
{
    // Her versiyon için (v1, v2) ayrı bir Swagger dokümanı "yaratır".
    options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
}
```
Bu sayede Swagger'ın elinde artık tek bir doküman değil, versiyon sayısı kadar (örneğin 2 tane) ayrı JSON dokümanı olur.

### Adım 3: Arayüzdeki Dropdown (`Program.cs`)
Swagger UI (arayüz) ayağa kalkarken, `Program.cs` içinde yazdığımız şu kod parçası çalışır:
```csharp
foreach (ApiVersionDescription description in apiVersionProvider.ApiVersionDescriptions)
{
    // Arayüzdeki (sağ üst) açılır kutuya seçenekleri ekler
    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
}
```
Bu kod, Swagger'a "Bak elimde v1 ve v2 var, bunları kullanıcıya seçtir" der. Kullanıcı listeden seçim yaptığında, Swagger UI ilgili versiyonun JSON dosyasını yükler ve ekranı günceller.

Özetle; **ApiExplorer** bulur, **ConfigureOptions** dokümanı hazırlar, **Program.cs** ise arayüze basar. Bizim tek yapmamız gereken Controller'a `[ApiVersion]` etiketini yapıştırmaktır.

## 6. Proje ve Assembly Versiyonlama

API'nin dış dünyada nasıl göründüğü (**API Versioning**) ile projenin kendi sürüm numarasının (**Assembly Version**) karıştırılmaması gerekir. 

`.csproj` dosyamızda projenin kendi sürümünü de takip ediyoruz:

```xml
<PropertyGroup>
    <!-- ... -->
    <Version>1.0.0</Version>
    <InformationalVersion>1.0.0-dev</InformationalVersion>
</PropertyGroup>
```

*   **Version**: Projenin derlenmiş sürüm (Assembly) numarasıdır. CI/CD süreçlerinde ve DLL versiyonlamasında kullanılır.
*   **InformationalVersion**: Daha detaylı bilgi (örn: `1.0.0-beta`, `1.0.0-rc1`) vermek için kullanılır.

Bu sürüm numarası, API URL'indeki `v1`'den bağımsızdır. Yani API `v1.0` kalırken, biz arka planda bug fix yapıp proje sürümünü `1.0.1`, `1.0.2` diye artırabiliriz. API arayüzü değişmediği sürece API versiyonunu (v1) artırmaya gerek yoktur.
