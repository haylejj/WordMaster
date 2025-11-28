using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordMaster.Domain.Entities;

namespace WordMaster.Infrastructure.EfCore.Configurations;

public class LogHistoryConfiguration : IEntityTypeConfiguration<LogHistory>
{
    public void Configure(EntityTypeBuilder<LogHistory> builder)
    {
        builder.ToTable("LogHistories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AttemptedAt)
               .HasColumnType("datetime2");

        builder.Property(x => x.IpAddress)
               .HasMaxLength(64);

        builder.Property(x => x.Email)
               .HasMaxLength(256);

        builder.Property(x => x.Source)
               .HasMaxLength(64);

        builder.HasOne(x => x.AppUser)
               .WithMany()
               .HasForeignKey(x => x.AppUserId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

