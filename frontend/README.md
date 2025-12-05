# WordMaster Frontend

WordMaster Frontend, modern web teknolojileri kullanılarak geliştirilmiş, kullanıcı dostu ve performanslı bir kelime öğrenme platformu arayüzüdür.

## 🚀 Teknolojiler

Proje aşağıdaki temel teknolojiler üzerine inşa edilmiştir:

*   **React 18**: Kullanıcı arayüzü kütüphanesi.
*   **Vite**: Hızlı geliştirme ve build aracı.
*   **TypeScript**: Tip güvenliği ve daha iyi geliştirme deneyimi için.
*   **Tailwind CSS**: Hızlı ve esnek stillendirme için utility-first CSS framework'ü.
*   **Axios**: HTTP istekleri için.
*   **React Router DOM**: Sayfa yönlendirmeleri için.
*   **React Hook Form & Zod**: Form yönetimi ve validasyon için.
*   **Lucide React**: Modern ikon seti.
*   **Radix UI / Shadcn UI**: Erişilebilir ve özelleştirilebilir UI bileşenleri.

## 📂 Proje Yapısı

```
src/
├── components/        # Yeniden kullanılabilir UI bileşenleri (Button, Input, Modal vb.)
├── hooks/             # Custom React hook'ları
├── layouts/           # Sayfa düzenleri (AuthLayout, DashboardLayout vb.)
├── lib/               # Yardımcı kütüphaneler ve konfigürasyonlar (utils.ts vb.)
├── pages/             # Uygulama sayfaları (Login, Dashboard, WordList vb.)
├── services/          # API servisleri (AuthService, WordService vb.)
├── types/             # TypeScript tip tanımları ve interface'ler
├── App.tsx            # Ana uygulama bileşeni ve routing
└── main.tsx           # Uygulama giriş noktası
```

## 🔐 Kimlik Doğrulama ve Güvenlik

Frontend uygulaması, Backend ile güvenli bir şekilde iletişim kurmak için **JWT (JSON Web Token)** yapısını kullanır.

### Axios Interceptor Yapısı (`src/services/api.ts`)

Tüm HTTP istekleri merkezi bir `api` instance'ı üzerinden yönetilir.

1.  **Request Interceptor:**
    *   Her istekte `localStorage` kontrol edilir.
    *   Eğer `accessToken` varsa, isteğin `Authorization` header'ına `Bearer <token>` olarak eklenir.
    *   Login, Register gibi auth gerektirmeyen endpoint'ler bu işlemden muaf tutulur.

2.  **Response Interceptor:**
    *   **401 Unauthorized:** Sunucudan 401 hatası gelirse (Token süresi dolmuşsa):
        *   Sistem otomatik olarak `refreshToken` ile yeni bir `accessToken` almaya çalışır.
        *   Eğer başarılı olursa, başarısız olan ilk istek yeni token ile tekrar gönderilir (Kullanıcı kesinti hissetmez).
        *   Eğer yenileme başarısız olursa, kullanıcı çıkışa zorlanır ve Login sayfasına yönlendirilir.
    *   **403 Forbidden:** Kullanıcının yetkisi olmayan bir işlem yapması durumunda:
        *   Global bir `permission-denied` eventi fırlatılır.
        *   Bu event, kullanıcıya şık bir "Erişim Engellendi" modalı göstermek için dinlenir.

*   **403 Forbidden:** Kullanıcının yetkisi olmayan bir işlem yapması durumunda:
        *   Global bir `permission-denied` eventi fırlatılır.
        *   Bu event, kullanıcıya şık bir "Erişim Engellendi" modalı göstermek için dinlenir.

## 🚀 CI/CD (GitHub Actions)

Frontend projesi için otomatik derleme ve kontrol süreçleri **GitHub Actions** ile yönetilmektedir. `.github/workflows/audit.yaml` dosyasındaki `frontend-audit` işi şunları yapar:

*   **Node.js Kurulumu**: Node.js 20 sürümünü kurar.
*   **Bağımlılık Yükleme**: `npm ci` komutu ile bağımlılıkları temiz bir şekilde yükler.
*   **Build Kontrolü**: `npm run build` komutu ile projenin hatasız derlendiğini doğrular.

## 🛠 Kurulum ve Çalıştırma

Projeyi yerel ortamınızda çalıştırmak için:

1.  Bağımlılıkları yükleyin:
    ```bash
    npm install
    ```

2.  Geliştirme sunucusunu başlatın:
    ```bash
    npm run dev
    ```

3.  Tarayıcıda görüntüleyin:
    Genellikle `http://localhost:5173` adresinde çalışır.