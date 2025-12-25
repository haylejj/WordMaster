using Microsoft.EntityFrameworkCore;
using System.Reflection;
using WordMaster.Infrastructure.EfCore;

namespace WordMaster.API.Extensions;

/// <summary>
/// Entity Framework DbContext yapılandırması için extension metodları.
/// </summary>
public static class DbContextConfigurationsExtensions
{
    /// <summary>
    /// SQL Server ile AppDbContext yapılandırmasını ekler. Migration assembly ve retry policy içerir.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Application configuration (connection string için)</param>
    public static void AddDbContextConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(x =>
        {
            x.UseSqlServer(configuration.GetConnectionString("SqlServer"), option =>
            {
                option.MigrationsAssembly(Assembly.GetAssembly(typeof(AppDbContext))!.GetName().Name);
                option.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            });
        });
    }
}
