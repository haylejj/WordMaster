using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using WordMaster.Application.Helpers;
using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.Services.Concrete;
using WordMaster.Infrastructure.EfCore.Repositories;
using WordMaster.Infrastructure.EfCore.UnitOfWork;
using WordMaster.Infrastructure.Helpers;
using WordMaster.Infrastructure.Services;

namespace WordMaster.Infrastructure.Extensions;

/// <summary>
/// Infrastructure katmanı servis koleksiyonu extension metodlarını içerir.
/// Redis, Repository, Service ve Helper kayıtlarını yönetir.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Redis bağlantı yapılandırmasını ekler.
    /// Connection string'i appsettings.json'dan okur, bulunamazsa localhost:6379 kullanır.
    /// </summary>
    /// <param name="services">Servis koleksiyonu.</param>
    /// <param name="configuration">Uygulama yapılandırması.</param>
    /// <returns>Güncellenmiş servis koleksiyonu.</returns>
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(connectionString));
        return services;
    }

    /// <summary>
    /// Uygulama servislerini DI container'a kaydeder.
    /// UnitOfWork, Repository, Service ve Helper sınıflarının Scoped yaşam döngüsü ile kaydını yapar.
    /// </summary>
    /// <param name="services">Servis koleksiyonu.</param>
    /// <returns>Güncellenmiş servis koleksiyonu.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Generic Repository
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // Repositories
        services.AddScoped<IWordRepository, WordRepository>();
        services.AddScoped<IFolderRepository, FolderRepository>();
        services.AddScoped<IWordFolderRepository, WordFolderRepository>();
        services.AddScoped<IFavoriteRepository, FavoriteRepository>();
        services.AddScoped<IUnknowsRepository, UnknowsRepository>();
        services.AddScoped<ILogHistoryRepository, LogHistoryRepository>();
        services.AddScoped<IPracticeHistoryRepository, PracticeHistoryRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();

        // Services
        services.AddScoped<IWordService, WordService>();
        services.AddScoped<IFolderService, FolderService>();
        services.AddScoped<IFavoriteService, FavoriteService>();
        services.AddScoped<IUnknowsService, UnknowsService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRegisterService, RegisterService>();
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<IRoleService, RoleService>();
        //services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IEmailService, MailHogEmailService>();
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<ILogHistoryService, LogHistoryService>();
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();
        services.AddScoped<IAllowedIpAddressService, AllowedIpAddressService>();
        services.AddScoped<IExcelService, ExcelService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IDatabaseService, DatabaseService>();
        services.AddScoped<IStatisticsService, StatisticsService>();

        // Helpers
        services.AddScoped<IDataProtectionHelper, DataProtectionHelper>();
        services.AddScoped<IRefreshTokenCookieHelper, RefreshTokenCookieHelper>();

        return services;
    }
}
