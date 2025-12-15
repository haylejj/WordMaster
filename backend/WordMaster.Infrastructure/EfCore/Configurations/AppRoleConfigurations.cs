using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordMaster.Domain.Entities;

namespace WordMaster.Infrastructure.EfCore.Configurations;

public class AppRoleConfigurations : IEntityTypeConfiguration<AppRole>
{
    public void Configure(EntityTypeBuilder<AppRole> builder)
    {
        // Seed Roles
        builder.HasData(
            new AppRole { Id = Guid.Parse("995B39DB-6677-4542-8F7B-B584F514D88E"), Name = "admin", NormalizedName = "ADMIN", ConcurrencyStamp = "CDE3129C-856B-4E48-A35C-20E43CA37A6A" },
            new AppRole { Id = Guid.Parse("DD471AEB-7B14-4559-83A0-CD6057A19A0F"), Name = "user", NormalizedName = "USER", ConcurrencyStamp = "352B49E7-5194-4F57-9940-72EE8940E306" }
        );
    }
}
