# 🔐 SecurityStamp + JWT Entegrasyonu

## ❓ Sorun: SecurityStamp JWT ile Nasıl Çalışır?

**Kısa Cevap:** SecurityStamp normalde **cookie-based auth** için çalışır, **JWT ile direkt çalışmaz**. Ama biz JWT'ye entegre ettik!

---

## 🎯 Çözüm: 3 Adımlı Entegrasyon

### **1. SecurityStamp'i JWT Token'a Claim Olarak Ekle**

```csharp
// JwtService.cs - GenerateAccessToken()
claims.Add(new Claim("SecurityStamp", securityStamp));
```

**Ne zaman:** Token oluştururken (Login, RefreshToken)  
**Nerede:** JWT token içinde claim olarak saklanır

---

### **2. Token Validation Sırasında SecurityStamp Kontrolü**

```csharp
// ServiceCollectionExtensions.cs - OnTokenValidated Event
OnTokenValidated = async context =>
{
    // 1. UserManager'dan kullanıcıyı al
    var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<AppUser>>();
    var user = await userManager.FindByIdAsync(userId);
    
    // 2. Token'daki SecurityStamp ile DB'deki SecurityStamp'i karşılaştır
    string? tokenSecurityStamp = context.Principal?.FindFirst("SecurityStamp")?.Value;
    
    if (tokenSecurityStamp != user.SecurityStamp)
    {
        // SecurityStamp uyuşmuyorsa token geçersiz!
        context.Fail("Security stamp geçersiz. Lütfen tekrar giriş yapın.");
    }
}
```

**Ne zaman:** Her API isteğinde, token doğrulandıktan SONRA  
**Nerede:** JWT Bearer Events - OnTokenValidated

---

### **3. Şifre Değiştiğinde SecurityStamp Güncelle**

```csharp
// UserService.cs veya AuthService.cs
await userManager.UpdateSecurityStampAsync(user);
```

**Ne zaman:** 
- Şifre değiştirildiğinde
- Email değiştirildiğinde
- 2FA ayarları değiştirildiğinde

**Sonuç:** Tüm eski JWT token'lar geçersiz olur!

---

## 🔄 Akış Diyagramı

```
┌─────────────────────────────────────────────────────────────┐
│ 1. LOGIN                                                    │
├─────────────────────────────────────────────────────────────┤
│ User Login → SecurityStamp: "ABC123"                        │
│ JWT Token Oluştur:                                          │
│   Claims: {                                                 │
│     UserId: "1",                                            │
│     Email: "user@example.com",                              │
│     SecurityStamp: "ABC123"  ← DB'den alındı               │
│   }                                                         │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ 2. API İSTEĞİ (Token Geçerli)                              │
├─────────────────────────────────────────────────────────────┤
│ Request Header: Authorization: Bearer <JWT>                 │
│ OnTokenValidated Event:                                     │
│   Token SecurityStamp: "ABC123"                             │
│   DB SecurityStamp:    "ABC123"  ✅ EŞLEŞIYOR              │
│ Sonuç: İstek başarılı                                       │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ 3. ŞİFRE DEĞİŞTİRME                                        │
├─────────────────────────────────────────────────────────────┤
│ User şifresini değiştirdi                                   │
│ UpdateSecurityStampAsync() çağrıldı                         │
│ DB SecurityStamp: "ABC123" → "XYZ789"  🔄 DEĞİŞTİ         │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ 4. ESKİ TOKEN İLE API İSTEĞİ (Token Geçersiz!)            │
├─────────────────────────────────────────────────────────────┤
│ Request Header: Authorization: Bearer <ESKİ JWT>            │
│ OnTokenValidated Event:                                     │
│   Token SecurityStamp: "ABC123"  ← Eski                    │
│   DB SecurityStamp:    "XYZ789"  ❌ EŞLEŞMIYOR             │
│ context.Fail() → 401 Unauthorized                           │
│ Mesaj: "Security stamp geçersiz. Lütfen tekrar giriş yapın."│
└─────────────────────────────────────────────────────────────┘
```

---

## ✅ Avantajlar

### **1. Otomatik Logout (Tüm Cihazlar)**
- Kullanıcı şifresini değiştirdiğinde
- Tüm cihazlardaki (telefon, tablet, bilgisayar) eski token'lar geçersiz olur
- Kullanıcı her cihazda tekrar login olmalı

### **2. Güvenlik**
- Çalınan token'lar şifre değiştirildiğinde otomatik geçersiz olur
- Şüpheli aktivite tespit edildiğinde SecurityStamp güncellenebilir

### **3. Merkezi Kontrol**
- Tek bir yerden (SecurityStamp) tüm oturumları yönetebilirsin

---

## ⚠️ Performans Notu

**Her API isteğinde veritabanı sorgusu yapılıyor!**

```csharp
AppUser? user = await userManager.FindByIdAsync(userId);  // DB sorgusu
```

### **Optimizasyon Seçenekleri:**

#### **1. Redis Cache (Önerilen)**
```csharp
// Önce cache'e bak
string? cachedSecurityStamp = await redis.GetAsync($"user:{userId}:securitystamp");

if (cachedSecurityStamp == null)
{
    // Cache'de yoksa DB'den al
    var user = await userManager.FindByIdAsync(userId);
    cachedSecurityStamp = user.SecurityStamp;
    
    // Cache'e kaydet (5 dakika)
    await redis.SetAsync($"user:{userId}:securitystamp", cachedSecurityStamp, TimeSpan.FromMinutes(5));
}

// Token ile karşılaştır
if (tokenSecurityStamp != cachedSecurityStamp) { ... }
```

#### **2. Memory Cache (Basit)**
```csharp
var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
string cacheKey = $"securitystamp_{userId}";

if (!cache.TryGetValue(cacheKey, out string? cachedStamp))
{
    var user = await userManager.FindByIdAsync(userId);
    cachedStamp = user.SecurityStamp;
    cache.Set(cacheKey, cachedStamp, TimeSpan.FromMinutes(5));
}
```

#### **3. Kontrol Sıklığını Azalt**
```csharp
// Her istekte değil, 5 dakikada bir kontrol et
var lastCheck = context.Principal?.FindFirst("LastSecurityCheck")?.Value;
if (DateTime.TryParse(lastCheck, out var checkTime))
{
    if (DateTime.UtcNow - checkTime < TimeSpan.FromMinutes(5))
    {
        return; // Henüz kontrol etme
    }
}
```

---

## 🎯 Özet

| Özellik | Cookie Auth | JWT (Bizim Çözüm) |
|---------|-------------|-------------------|
| SecurityStamp Kontrolü | ✅ Otomatik (ASP.NET Core) | ✅ Manuel (OnTokenValidated) |
| Veritabanı Sorgusu | Her 30 dakika | Her istek (cache ile optimize edilebilir) |
| Tüm Oturumları Sonlandırma | ✅ | ✅ |
| Performans | Yüksek | Orta (cache ile yüksek) |

**Sonuç:** SecurityStamp artık JWT ile de çalışıyor! 🎉

Şifre değiştiğinde tüm eski token'lar otomatik geçersiz oluyor.
