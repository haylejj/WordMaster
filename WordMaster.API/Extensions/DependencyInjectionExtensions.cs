using WordMaster.Application.Services.Abstract;
using WordMaster.Infrastructure.Services;

namespace WordMaster.API.Extensions;

/// <summary>
/// Dependency Injection servis kayıtları.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Uygulama servislerini DI container'a ekler.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}
