using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordMaster.Domain.Entities;

namespace WordMaster.Infrastructure.EfCore.Configurations;

public class PracticeHistoryConfigurations : IEntityTypeConfiguration<PracticeHistory>
{
    public void Configure(EntityTypeBuilder<PracticeHistory> builder)
    {
        builder
            .HasOne(x => x.AppUser)
            .WithMany(u => u.PracticeHistories)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
