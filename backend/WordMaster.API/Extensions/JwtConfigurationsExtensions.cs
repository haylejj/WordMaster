using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.API.Extensions;

/// <summary>
/// JWT (JSON Web Token) authentication yapılandırma extension metodlarını içerir.
/// Token validasyonu, SecurityStamp kontrolü ve authentication event handler'larını yapılandırır.
/// </summary>
public static class JwtConfigurationsExtensions
{
    /// <summary>
    /// JWT (JSON Web Token) tabanlı authentication yapılandırmasını ekler.
    /// Gelen HTTP isteklerindeki JWT token'ları doğrular ve kullanıcı kimlik bilgilerini çıkarır.
    /// </summary>
    /// <param name="services">Servis koleksiyonu</param>
    /// <param name="configuration">Uygulama yapılandırması (appsettings.json'dan JWT ayarlarını okur)</param>
    /// <returns>Güncellenmiş servis koleksiyonu</returns>
    public static IServiceCollection AddJwtConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        // appsettings.json'daki "Jwt" bölümünü oku
        IConfigurationSection jwtSection = configuration.GetSection("Jwt");

        // JWT imzalama anahtarını byte dizisine çevir (token'ları şifrelemek/doğrulamak için kullanılır)
        byte[] key = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

        // Authentication servisini yapılandır
        services.AddAuthentication(options =>
        {
            // Varsayılan authentication şeması: JWT Bearer
            // Gelen isteklerdeki token'ları otomatik olarak JWT Bearer ile doğrular
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

            // Yetkisiz erişim durumunda kullanılacak şema
            // 401 Unauthorized yanıtı döndürülürken JWT Bearer challenge mekanizması devreye girer
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                // Token doğrulama parametrelerini ayarla
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Token'ın Issuer (token'ı oluşturan) bilgisini doğrula
                    ValidateIssuer = true,
                    ValidIssuer = jwtSection["Issuer"], // Kabul edilecek issuer değeri (örn: "WordMasterAPI")

                    // Token'ın Audience (token'ın hedef kitlesi) bilgisini doğrula
                    ValidateAudience = true,
                    ValidAudience = jwtSection["Audience"], // Kabul edilecek audience değeri (örn: "WordMasterClient")

                    // Token'ın dijital imzasını doğrula (token'ın değiştirilmediğinden emin olmak için)
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key), // İmza doğrulama için kullanılacak anahtar

                    // Token'ın geçerlilik süresini kontrol et (exp claim)
                    ValidateLifetime = true,

                    // Token'ın geçerlilik süresi kontrolünde tolerans süresi (varsayılan 5 dakika)
                    // TimeSpan.Zero: Hiç tolerans yok, token süresi dolduğu anda geçersiz olur
                    ClockSkew = TimeSpan.Zero,

                    // Claim type mapping (User.Identity.Name ve User.IsInRole için gerekli)
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    // 1. OnTokenValidated: Token teknik olarak geçerli (imza/süre tamam). 
                    // Burada veritabanı/cache kontrolü (SecurityStamp) ile oturumun mantıksal geçerliliğini (Logout olmuş mu? Şifre değişmiş mi?) teyit ediyoruz.
                    OnTokenValidated = async context =>
                    {
                        // Gerekli servisleri DI'dan al
                        UserManager<AppUser> userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<AppUser>>();
                        ICacheService cacheService = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();

                        // Token'daki kullanıcı ID'sini al
                        string? userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        if (!string.IsNullOrEmpty(userId))
                        {
                            string cacheKey = $"security_stamp:{userId}";
                            string? dbSecurityStamp = await cacheService.GetAsync<string>(cacheKey);

                            // Cache'de yoksa veritabanından al
                            if (string.IsNullOrEmpty(dbSecurityStamp))
                            {
                                AppUser? user = await userManager.FindByIdAsync(userId);
                                if (user != null)
                                {
                                    dbSecurityStamp = user.SecurityStamp;
                                    // Cache'e kaydet (1 saat geçerli)
                                    await cacheService.SetAsync(cacheKey, dbSecurityStamp, TimeSpan.FromHours(1));
                                }
                                else
                                {
                                    context.Fail("Kullanıcı bulunamadı.");
                                    return;
                                }
                            }

                            // Token'daki SecurityStamp ile karşılaştır
                            string? tokenSecurityStamp = context.Principal?.FindFirst("SecurityStamp")?.Value;

                            if (tokenSecurityStamp != dbSecurityStamp)
                            {
                                // SecurityStamp uyuşmuyorsa token geçersiz (kullanıcı şifresini değiştirmiş)
                                context.Fail("Security stamp geçersiz. Lütfen tekrar giriş yapın.");
                            }
                        }
                    },

                    // 2. OnChallenge: Yetkilendirme hatası olduğunda (Token yok veya geçersiz) devreye girer. 
                    // Standart davranışı ezip, istemciye kendi formatımızda 401 JSON cevabı dönmemizi sağlar.
                    OnChallenge = context =>
                    {
                        context.HandleResponse(); // Default davranışı engelle

                        // Response başlamışsa işlem yapma
                        if (context.Response.HasStarted)
                        {
                            return Task.CompletedTask;
                        }

                        ServiceResult result = ServiceResult.Failure("Yetkisiz erişim. Lütfen geçerli bir JWT token sağlayın.", HttpStatusCode.Unauthorized);

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        string json = JsonSerializer.Serialize(result);

                        return context.Response.WriteAsync(json);
                    },
                    // 3. OnForbidden: Kullanıcı giriş yapmış ama yetkisi yetmiyorsa (Örn: Admin rolü gerekiyor ama User rolünde) devreye girer.
                    // 403 Forbidden hatasını JSON formatında döner.
                    // 3. OnForbidden: Kullanıcı giriş yapmış ama yetkisi yetmiyorsa (Örn: Admin rolü gerekiyor ama User rolünde) devreye girer.
                    // 403 Forbidden hatasını JSON formatında döner.
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json; charset=utf-8";

                        ServiceResult result = ServiceResult.Failure("Bu işlemi yapmaya yetkiniz yok.", HttpStatusCode.Forbidden);

                        string json = JsonSerializer.Serialize(result);

                        return context.Response.WriteAsync(json);
                    },
                    // 4. OnAuthenticationFailed: Token'ın kendisinde bir sorun olduğunda (Süresi dolmuş, imza hatalı, format bozuk) devreye girer.
                    // Exception'ı yakalayıp sebebini belirterek 401 hatası döner.
                    OnAuthenticationFailed = context =>
                    {
                        context.NoResult(); // .NET’in kendi 401'i bastırılır

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json; charset=utf-8";

                        string message = "Token doğrulanamadı.";

                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            message = "Token süresi dolmuş.";
                            context.Response.Headers.Append("Token-Expired", "true");
                        }

                        if (context.Exception is SecurityTokenInvalidSignatureException)
                        {
                            message = "Token imzası geçersiz.";
                            context.Response.Headers.Append("Token-Invalid", "true");
                        }

                        ServiceResult result = ServiceResult.Failure(message, HttpStatusCode.Unauthorized);
                        string json = JsonSerializer.Serialize(result);
                        return context.Response.WriteAsync(json);
                    },
                };
            });
        return services;
    }
}
