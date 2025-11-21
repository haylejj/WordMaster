using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using WordMaster.Domain.Entities;

namespace WordMaster.Infrastructure.EfCore;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser, AppRole, Guid>(options)
{
    public DbSet<Word> Words { get; set; }
    public DbSet<Folder> Folders { get; set; }
    public DbSet<WordFolder> WordFolders { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<Unknows> Unknows { get; set; }
    public DbSet<LogHistory> LogHistories { get; set; }
    public DbSet<AllowedIpAddress> AllowedIpAddresses { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Seed Roles
        modelBuilder.Entity<AppRole>().HasData(
            new AppRole { Id = Guid.Parse("995B39DB-6677-4542-8F7B-B584F514D88E"), Name = "admin", NormalizedName = "ADMIN", ConcurrencyStamp = "CDE3129C-856B-4E48-A35C-20E43CA37A6A" },
            new AppRole { Id = Guid.Parse("DD471AEB-7B14-4559-83A0-CD6057A19A0F"), Name = "user", NormalizedName = "USER", ConcurrencyStamp = "352B49E7-5194-4F57-9940-72EE8940E306" }
        );

        // Seed Allowed IP Addresses for Local Development
        modelBuilder.Entity<AllowedIpAddress>().HasData(
            new AllowedIpAddress
            {
                Id = 1,
                IpAddress = "127.0.0.1",
                Description = "Localhost IPv4 - Local Development",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true
            },
            new AllowedIpAddress
            {
                Id = 2,
                IpAddress = "::1",
                Description = "Localhost IPv6 - Local Development",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true
            }
        );

    }
}
