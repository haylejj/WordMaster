using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordMaster.Domain.Entities;

namespace WordMaster.Infrastructure.EfCore.Configurations;

public class AllowedIpAddressConfiguration : IEntityTypeConfiguration<AllowedIpAddress>
{
    public void Configure(EntityTypeBuilder<AllowedIpAddress> builder)
    {
        builder.ToTable("AllowedIpAddresses");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.Property(x => x.IpAddress)
               .IsRequired()
               .HasMaxLength(45); // IPv6 için yeterli uzunluk

        builder.Property(x => x.Description)
               .HasMaxLength(500);

        builder.Property(x => x.CreatedAt)
               .IsRequired()
               .HasColumnType("datetime2");

        builder.Property(x => x.IsActive)
               .IsRequired()
               .HasDefaultValue(true);

        builder.HasIndex(x => x.IpAddress)
               .IsUnique();
    }
}

