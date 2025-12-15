using Microsoft.AspNetCore.Identity;
using WordMaster.Domain.Entities;
using WordMaster.Infrastructure.EfCore;

namespace WordMaster.API.Extensions;

/// <summary>
/// ASP.NET Core Identity yapılandırma extension metodlarını içerir.
/// Kullanıcı yönetimi, şifre politikaları ve hesap kilitleme ayarlarını yapılandırır.
/// </summary>
public static class IdentityConfigurationsExtensions
{
    /// <summary>
    /// ASP.NET Core Identity yapılandırmasını ekler.
    /// Kullanıcı yönetimi, şifre politikaları, hesap kilitleme (lockout) ve token ayarlarını içerir.
    /// Ayrıca SecurityStamp validasyonu ile güvenlik kontrol sıklığını belirler.
    /// </summary>
    /// <param name="services">Servis koleksiyonu</param>
    /// <returns>Güncellenmiş servis koleksiyonu</returns>
    public static IServiceCollection AddIdentityConfigurations(this IServiceCollection services)
    {
        // Email doğrulama, şifre sıfırlama gibi token'ların geçerlilik süresi (1 saat)
        services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromHours(1);
        });

        // jwt için IdentityCore kullanıyoruz, AddIdentity değil.Çünkü cookie tabanlı auth kullanmıyoruz.
        services.AddIdentityCore<AppUser>(options =>
        {
            options.SignIn.RequireConfirmedEmail = true;
            options.User.RequireUniqueEmail = true;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = true;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
            options.Lockout.MaxFailedAccessAttempts = 4;
        })
        .AddRoles<AppRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders()
        .AddSignInManager();

        // Security Stamp: Kullanıcı şifresi değiştiğinde veya önemli bir güvenlik olayında tüm oturumları sonlandırmak için kullanılır
        // Her 30 dakikada bir kontrol edilir, değişmişse kullanıcı otomatik logout olur
        // burası cookie auth için kullanılıyor, JWT için değil.
        //services.Configure<SecurityStampValidatorOptions>(options =>
        //{
        //    options.ValidationInterval = TimeSpan.FromMinutes(30);
        //});

        return services;
    }
}
