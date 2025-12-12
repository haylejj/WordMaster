using StackExchange.Redis;
using WordMaster.Application.Constants;
using WordMaster.Infrastructure.EfCore;

namespace WordMaster.API.Extensions;

/// <summary>
/// Extension methods for the WebApplication class.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Logs the startup status of the application, including database and Redis connection checks.
    /// </summary>
    /// <param name="app">The WebApplication instance.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task LogStartupStatusAsync(this WebApplication app)
    {
        using (IServiceScope scope = app.Services.CreateScope())
        {
            IServiceProvider services = scope.ServiceProvider;
            ILogger logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

            try
            {
                logger.LogInformation("API Project is starting...");
                logger.LogInformation("API version: {Version}", AppInfo.Version);
                AppDbContext dbContext = services.GetRequiredService<AppDbContext>();
                if (await dbContext.Database.CanConnectAsync())
                {
                    logger.LogInformation("Successfully connected to the Database.");
                }
                else
                {
                    logger.LogError("Failed to connect to the Database.");
                }

                IConnectionMultiplexer redis = services.GetRequiredService<IConnectionMultiplexer>();
                if (redis.IsConnected)
                {
                    logger.LogInformation("Successfully connected to Redis.");
                }
                else
                {
                    logger.LogError("Failed to connect to Redis.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while checking connections during startup.");
            }
        }
    }
}
