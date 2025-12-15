using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
    public DbSet<PracticeHistory> PracticeHistories { get; set; }
    public DbSet<AllowedIpAddress> AllowedIpAddresses { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        IEnumerable<EntityEntry<AppUser>> entries = ChangeTracker
            .Entries<AppUser>()
            .Where(e => e.State == EntityState.Modified);

        foreach (EntityEntry<AppUser>? entry in entries)
        {
            entry.Entity.UpdatedDate = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
