using StackExchange.Redis;
using WordMaster.Infrastructure.EfCore;

namespace WordMaster.API.Extensions;

public static class WebApplicationExtensions
{
    public static async Task LogStartupStatusAsync(this WebApplication app)
    {
        using (IServiceScope scope = app.Services.CreateScope())
        {
            IServiceProvider services = scope.ServiceProvider;
            ILogger logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

            try
            {
                logger.LogInformation("API Project is starting...");

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
