using Microsoft.AspNetCore.Identity;
using WordMaster.Domain.Entities;
using WordMaster.Infrastructure.EfCore;

namespace WordMaster.WebUI.Extensions;

public static class StartupExtensions
{
    public static void AddIdentityWithExtension(this IServiceCollection services)
    {
        services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromHours(1);
        });
        services.AddIdentity<AppUser, AppRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(3);
            options.Lockout.MaxFailedAccessAttempts = 5;
        })
        .AddDefaultTokenProviders()
        .AddEntityFrameworkStores<AppDbContext>();
    }
}
