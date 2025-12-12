using Asp.Versioning;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.Validation.Word;
using WordMaster.Domain.Configuration;
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
    /// API Versioning yapılandırmasını ekler.
    /// URL path versioning kullanılır (örn: /api/v1/words)
    /// </summary>
    /// <param name="services">Servis koleksiyonu</param>
    /// <returns>Güncellenmiş servis koleksiyonu</returns>
    public static IServiceCollection AddApiVersioningConfigurations(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            // Varsayılan API versiyonu (versiyon belirtilmezse bu kullanılır)
            options.DefaultApiVersion = new ApiVersion(1, 0);

            // Versiyon belirtilmezse varsayılan versiyonu kullan
            options.AssumeDefaultVersionWhenUnspecified = true;

            // Response header'larında desteklenen ve deprecated versiyonları göster
            // api-supported-versions: 1.0, 2.0
            // api-deprecated-versions: 1.0
            options.ReportApiVersions = true;

            // Versiyon okuma stratejileri (URL path birincil)
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),           // /api/v1/words (birincil)
                new HeaderApiVersionReader("x-api-version") // Header ile de destekle (opsiyonel)
            );
        })
        .AddApiExplorer(options =>
        {
            // Swagger'da versiyon grupları için format: 'v'major[.minor][-status]
            // Örn: v1, v1.0, v2.0-beta
            options.GroupNameFormat = "'v'VVV";

            // Route'lardaki {version:apiVersion} placeholder'ını değiştir
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    /// <summary>
    /// Fluent Validation ve custom ValidationFilter yapılandırmasını ekler.
    /// </summary>
    public static IServiceCollection AddValidationConfigurations(this IServiceCollection services)
    {
        // Fluent Validation validator'larını assembly'den otomatik olarak kaydet
        services.AddValidatorsFromAssemblyContaining<CreateWordRequestValidator>();

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
    /// <summary>
    /// Swagger/OpenAPI dokümantasyon yapılandırmasını ekler.
    /// API endpoint'lerini test etmek ve dokümante etmek için Swagger UI kullanılır.
    /// JWT Bearer token authentication desteği ve API versiyonlama ile birlikte yapılandırılır.
    /// </summary>
    /// <param name="services">Servis koleksiyonu</param>
    /// <returns>Güncellenmiş servis koleksiyonu</returns>
    public static IServiceCollection AddSwaggerConfigurations(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        // Swagger versiyon konfigürasyonunu DI'a ekle
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

        services.AddSwaggerGen(c =>
        {
            // XML yorumlarını ekle
            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);

            // JWT Bearer authentication tanımı
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
    /// <summary>
    /// AppSettings.json'daki ayarları servis koleksiyonuna ekleyerek Options pattern'ini kullanır.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void AddConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.Configure<MailHogSettings>(configuration.GetSection("MailHog"));
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<UrlsSettings>(configuration.GetSection("URLs"));
    }
    /// <summary>
    /// Rate Limiting (hız sınırlama) yapılandırmasını ekler.
    /// Auth endpointleri için katı ("StrictPolicy") kurallar ve limit aşımı (429) durumunda özel JSON yanıtı döndürür.
    /// </summary>
    /// <param name="services">Servis koleksiyonu.</param>
    public static void AddRateLimitingConfigurations(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                string message = "Çok fazla istek gönderdiniz. Lütfen 1 dakika sonra tekrar deneyiniz.";

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter) && retryAfter.TotalSeconds > 0)
                {
                    double minutes = Math.Ceiling(retryAfter.TotalMinutes);
                    // Eğer 1 dakikadan azsa 1 dakika olarak gösterelim
                    if (minutes < 1) minutes = 1;
                    message = $"Çok fazla istek gönderdiniz. Lütfen {minutes} dakika sonra tekrar deneyiniz.";
                }

                ServiceResult result = ServiceResult.Failure(message, HttpStatusCode.TooManyRequests);

                JsonSerializerOptions jsonOptions = new()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                await context.HttpContext.Response.WriteAsync(JsonSerializer.Serialize(result, jsonOptions), token);
            };

            options.AddPolicy("StrictPolicy", context =>
            {
                string remoteIpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetSlidingWindowLimiter(remoteIpAddress,
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 10,                 // 10 istek hakkı
                        Window = TimeSpan.FromMinutes(1), // 1 dakikalık pencere
                        SegmentsPerWindow = 2,            // Pencereyi 2 parçaya böl (30sn'lik segmentler)
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0                    // Kuyruk yok, limit dolunca reddet
                    });
            });

            options.AddPolicy("GeneralPolicy", context =>
            {
                // Kullanıcı giriş yapmışsa User ID, yoksa IP adresi kullan
                string userKey = context.User.GetUserId().ToString()
                                 ?? context.Connection.RemoteIpAddress?.ToString()
                                 ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(userKey,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 60,                 // Dakikada 60 istek
                        Window = TimeSpan.FromMinutes(1), // 1 dakikalık pencere
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0                    // Kuyruk yok
                    });
            });
        });
    }

}
