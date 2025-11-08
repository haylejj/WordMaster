using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.Services.Concrete;
using WordMaster.Infrastructure.EfCore.Repositories;
using WordMaster.Infrastructure.EfCore.UnitOfWork;
using WordMaster.Infrastructure.Services;

namespace WordMaster.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(connectionString));
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Generic Repository
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // Repositories
        services.AddScoped<IWordRepository, WordRepository>();
        services.AddScoped<IFavoriteRepository, FavoriteRepository>();
        services.AddScoped<IUnknowsRepository, UnknowsRepository>();

        // Services
        services.AddScoped<IWordService, WordService>();
        services.AddScoped<IFavoriteService, FavoriteService>();
        services.AddScoped<IUnknowsService, UnknowsService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRegisterService, RegisterService>();
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICacheService, RedisCacheService>();

        return services;
    }
}
