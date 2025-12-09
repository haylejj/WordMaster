using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WordMaster.Domain.Entities;

namespace WordMaster.Infrastructure.BackgroundServices;

/// <summary>
/// Kayıt olduktan sonra 1 saat içinde e-posta adresini doğrulamayan pasif kullanıcıları
/// periyodik olarak silen arka plan servisi.
/// </summary>
public class UserCleanupService(IServiceProvider serviceProvider, ILogger<UserCleanupService> logger) : BackgroundService
{
    /// <summary>
    /// Servis çalıştığında tetiklenen ana metot. 2 saatte bir çalışacak şekilde yapılandırılmıştır.
    /// </summary>
    /// <param name="stoppingToken">Servisin durdurulması için kullanılan token.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        using PeriodicTimer timer = new(TimeSpan.FromHours(2));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await DoWorkAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("User Cleanup Service is stopping.");
        }
    }

    /// <summary>
    /// Veritabanındaki pasif kullanıcıları bulur ve siler.
    /// </summary>
    /// <param name="stoppingToken">İptal token'ı.</param>
    private async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        UserManager<AppUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        DateTime oneHourAgo = DateTime.UtcNow.AddMinutes(-65);

        // Pasif kullanıcıları bul: Email onylanmamış ve oluşturulma tarihi 65 dakika eski
        List<AppUser> passiveUsers = userManager.Users
            .Where(u => !u.EmailConfirmed && u.CreatedDate < oneHourAgo)
            .ToList();

        if (passiveUsers.Count != 0)
        {
            foreach (AppUser user in passiveUsers)
            {
                if (stoppingToken.IsCancellationRequested) break;

                IdentityResult result = await userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    logger.LogWarning("Failed to delete user {UserId}: {Errors}", user.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            logger.LogInformation("Deletion process completed. Deleted user count: {Count}. Deleted user IDs: {Ids}", passiveUsers.Count, string.Join(", ", passiveUsers.Select(u => u.Id)));
        }
        else
        {
            logger.LogInformation("User cleanup service ran, no passive users found to delete.");
        }
    }
}
