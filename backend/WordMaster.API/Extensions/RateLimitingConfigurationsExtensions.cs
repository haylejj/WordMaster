using System.Net;
using System.Text.Json;
using System.Threading.RateLimiting;
using WordMaster.Domain.Results;

namespace WordMaster.API.Extensions;

/// <summary>
/// Rate limiting (hız sınırlama) yapılandırma extension metodlarını içerir.
/// StrictPolicy ve GeneralPolicy ile farklı endpoint grupları için istek sınırlaması sağlar.
/// </summary>
public static class RateLimitingConfigurationsExtensions
{
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
