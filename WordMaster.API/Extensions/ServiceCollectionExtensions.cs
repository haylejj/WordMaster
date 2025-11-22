using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using WordMaster.Application.Validation.Word;
using WordMaster.Domain.Results;

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
                    OnChallenge = context =>
                    {
                        context.HandleResponse(); // Default davranışı engelle

                        ServiceResult result = ServiceResult.Failure("Yetkisiz erişim. Lütfen geçerli bir JWT token sağlayın.", HttpStatusCode.Unauthorized);

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        string json = JsonSerializer.Serialize(result, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });

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
}
