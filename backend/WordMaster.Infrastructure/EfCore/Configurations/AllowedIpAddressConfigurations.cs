using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordMaster.Domain.Entities;

namespace WordMaster.Infrastructure.EfCore.Configurations;

public class AllowedIpAddressConfigurations : IEntityTypeConfiguration<AllowedIpAddress>
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


        // Seed Allowed IP Addresses for Local Development
        builder.HasData(
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

