using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.Validation.Word;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;
using WordMaster.Infrastructure.EfCore;

namespace WordMaster.API.Extensions;

/// <summary>
/// IServiceCollection için extension metodları.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Fluent Validation ve custom ValidationFilter yapılandırmasını ekler.
    /// </summary>
    public static IServiceCollection AddValidationConfigurations(this IServiceCollection services)
    {
        // Fluent Validation validator'larını assembly'den otomatik olarak kaydet
        services.AddValidatorsFromAssemblyContaining<WordDtoValidator>();

        // Fluent Validation'ı otomatik validation için etkinleştir
        services.AddFluentValidationAutoValidation(o =>
        {
            o.DisableDataAnnotationsValidation = true;
        });

        // ApiController'ın ModelState hatalarında otomatik olarak 400 BadRequest + ProblemDetails döndürme davranışını devre dışı bırak
        // Bu sayede ModelState geçersiz olduğunda default pipeline çalışmaz, custom ValidationFilter devreye girer
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        return services;
    }

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
                    // Token doğrulandıktan SONRA SecurityStamp kontrolü yap
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
                                    // Cache'e kaydet (30 dakika geçerli)
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

                    OnChallenge = context =>
                    {
                        context.HandleResponse(); // Default davranışı engelle

                        ServiceResult result = ServiceResult.Failure("Yetkisiz erişim. Lütfen geçerli bir JWT token sağlayın.", HttpStatusCode.Unauthorized);

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        string json = JsonSerializer.Serialize(result);

                        return context.Response.WriteAsync(json);
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json; charset=utf-8";

                        ServiceResult result = ServiceResult.Failure("Bu işlemi yapmaya yetkiniz yok.", HttpStatusCode.Forbidden);

                        string json = JsonSerializer.Serialize(result);

                        return context.Response.WriteAsync(json);
                    },
                    OnAuthenticationFailed = context =>
                    {
                        // Token çözülürken hata oldu
                        context.NoResult(); // .NET’in kendi 401'i bastırılır

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json; charset=utf-8";

                        string message = "Token doğrulanamadı.";

                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            message = "Token süresi dolmuş.";
                        }

                        if (context.Exception is SecurityTokenInvalidSignatureException)
                        {
                            message = "Token imzası geçersiz.";
                        }

                        ServiceResult result = ServiceResult.Failure(message, HttpStatusCode.Unauthorized);
                        string json = JsonSerializer.Serialize(result);
                        return context.Response.WriteAsync(json);
                    },
                };
            });
        return services;
    }
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

        services.AddIdentity<AppUser, AppRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = true;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
            options.Lockout.MaxFailedAccessAttempts = 4;
        })
        .AddDefaultTokenProviders()
        .AddEntityFrameworkStores<AppDbContext>();

        // Security Stamp: Kullanıcı şifresi değiştiğinde veya önemli bir güvenlik olayında tüm oturumları sonlandırmak için kullanılır
        // Her 30 dakikada bir kontrol edilir, değişmişse kullanıcı otomatik logout olur
        services.Configure<SecurityStampValidatorOptions>(options =>
        {
            options.ValidationInterval = TimeSpan.FromMinutes(30);
        });

        return services;
    }
    /// <summary>
    /// Swagger/OpenAPI dokümantasyon yapılandırmasını ekler.
    /// API endpoint'lerini test etmek ve dokümante etmek için Swagger UI kullanılır.
    /// JWT Bearer token authentication desteği ile birlikte yapılandırılır.
    /// </summary>
    /// <param name="services">Servis koleksiyonu</param>
    /// <returns>Güncellenmiş servis koleksiyonu</returns>
    public static IServiceCollection AddSwaggerConfigurations(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);

            c.SwaggerDoc("v1", new() { Title = "API", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header. Örnek: 'Bearer {token}'"
            });
            // Bu ayar, Swagger'ın tanımlanan "Bearer" güvenlik şemasını tüm endpoint'lere otomatik olarak uygulamasını sağlar.
            // Yani kullanıcı Swagger UI'da Authorize butonuna token girdikten sonra, tüm API isteklerine "Authorization: Bearer {token}" header'ı otomatik eklenir.
            // Eğer bu ayarı koymazsak Swagger token'ı tanır ama çoğu endpoint'e göndermez.
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

        });

        return services;
    }
}
