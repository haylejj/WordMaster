# WordMaster Yetkilendirme ve İzin Sistemi (Authorization Architecture)

Bu doküman, WordMaster projesinde kullanılan dinamik, attribute tabanlı yetkilendirme sisteminin mimarisini ve çalışma mantığını açıklar.

## 1. Genel Bakış

Sistem, **Role-Based Access Control (RBAC)** (Rol Tabanlı Erişim Kontrolü) prensibine dayanır ancak izinler (Permissions) statik stringler yerine kod içerisindeki **Attribute**'lardan dinamik olarak türetilir. Bu sayede, geliştirici kod tarafında bir metoda yetki kuralı eklediğinde, bu kural veritabanına otomatik olarak yansıtılabilir ve admin panelinden yönetilebilir hale gelir.

## 2. Temel Bileşenler

### 2.1. Veritabanı Varlıkları (Entities)

*   **AppUser**: Sisteme giriş yapan kullanıcı.
*   **AppRole**: Kullanıcı rolleri (Örn: Admin, User).
*   **Permission**: Sistemdeki her bir yetkiyi temsil eder.
    *   `Key`: Benzersiz anahtar (Örn: `Admin_Role_Create_POST`).
    *   `AreaName`, `ControllerName`, `ActionName`, `HttpMethod`: İznin hangi endpoint'e ait olduğu.
    *   `Description`: İzin açıklaması.
*   **RolePermission**: Hangi rolün hangi izne sahip olduğunu tutan ara tablo (Many-to-Many).

### 2.2. Backend Mimarisi

#### RequirePermissionAttribute
Bu bir **Action Filter**'dır. Controller metodlarının üzerine eklenir ve istek geldiğinde devreye girer.

```csharp
[RequirePermission("Admin", "Roles", "CreateRole", "POST", "Rol oluştur")]
public async Task<IActionResult> CreateRole(...)
```

**Çalışma Mantığı:**
1.  Kullanıcının giriş yapıp yapmadığını kontrol eder.
2.  Kullanıcının ID'sini alır.
3.  Metodun üzerindeki attribute bilgilerinden benzersiz bir **Key** oluşturur.
4.  `PermissionService.HasPermissionAsync` metodunu çağırarak kullanıcının bu izne sahip olup olmadığını sorar.
5.  Eğer yetki yoksa `403 Forbidden` döner.

#### PermissionService (Senkronizasyon Motoru)
Sistemin kalbidir. `ScanAndSavePermissionsAsync` metodu ile kod ve veritabanı arasındaki senkronizasyonu sağlar.

*   **Scan (Tarama):** Projedeki tüm Controller'ları gezer ve `[RequirePermission]` attribute'una sahip metodları bulur.
*   **Sync (Senkronizasyon):**
    *   **Yeni İzinler:** Kodda olup DB'de olmayanları DB'ye ekler.
    *   **Silinen İzinler:** DB'de olup artık kodda bulunmayanları (silinmiş endpoint'ler) DB'den temizler.

### 3. Frontend Entegrasyonu (React)

#### Admin Paneli (Yetki Yönetimi)
Adminler, `AdminPermissionsPage` üzerinden bu sistemi yönetir.

1.  **İzinleri Tara (Scan):** Backend'deki `Scan` metodunu tetikler. Yeni eklenen endpoint'ler otomatik olarak listeye düşer.
2.  **Rol Seçimi:** Bir rol seçildiğinde, o role ait mevcut izinler işaretli gelir.
3.  **Kaydet:** Yapılan değişiklikler `RolePermission` tablosuna işlenir.

#### Yetki Kontrolü ve UX
Kullanıcı yetkisi olmayan bir işlem yapmaya çalıştığında:
1.  Backend `403 Forbidden` hatası döner.
2.  Frontend'deki `axios` interceptor bu hatayı yakalar.
3.  Global bir event (`permission-denied`) fırlatır.
4.  `PermissionDeniedModal` bileşeni bu event'i dinler ve kullanıcıya şık bir "Erişim Engellendi" uyarısı gösterir.

## 4. Nasıl Yeni Bir Yetki Eklenir?

Geliştirici olarak yapmanız gereken tek şey, korumak istediğiniz Controller metodunun üzerine attribute eklemektir:

```csharp
[HttpGet("secret-data")]
[RequirePermission("Admin", "Secret", "GetData", "GET", "Gizli verileri görüntüle")]
public IActionResult GetSecretData() { ... }
```

Ardından Admin panelinden "İzinleri Tara" butonuna basıldığında bu yetki otomatik olarak sisteme eklenir ve rollere atanabilir hale gelir.

## 5. Avantajları

*   **Dinamik:** Veritabanına manuel SQL insert yapmaya gerek kalmaz.
*   **Tutarlı:** Koddan silinen yetkiler veritabanında çöp olarak kalmaz.
*   **Merkezi:** Tüm yetki kontrolleri tek bir Attribute üzerinden yönetilir.
*   **Esnek:** İleride yeni roller veya izin grupları kolayca eklenebilir.
