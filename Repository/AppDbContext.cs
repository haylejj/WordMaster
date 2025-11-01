using Core.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Repository;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }
    public DbSet<Word> Words { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<Unknows> Unknows { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Seed Roles
        modelBuilder.Entity<AppRole>().HasData(
            new AppRole { Id = Guid.NewGuid().ToString(), Name = "admin", NormalizedName = "ADMIN", ConcurrencyStamp = Guid.NewGuid().ToString() },
            new AppRole { Id = Guid.NewGuid().ToString(), Name = "user", NormalizedName = "USER", ConcurrencyStamp = Guid.NewGuid().ToString() }
        );

        base.OnModelCreating(modelBuilder);
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
    }
}
