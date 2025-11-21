using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordMaster.Domain.Entities;

namespace WordMaster.Infrastructure.EfCore.Configurations;

public class WordConfigurations : IEntityTypeConfiguration<Word>
{
    public void Configure(EntityTypeBuilder<Word> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.EnglishWord).IsRequired().HasMaxLength(30);
        builder.Property(x => x.TurkishWord).IsRequired().HasMaxLength(60);
        builder.Property(x => x.CreatedTime).IsRequired();


        builder.Property(x => x.ConsecutiveCorrectCount).HasDefaultValue(0);
        builder.Property(x => x.ConsecutiveWrongCount).HasDefaultValue(0);
        builder.Property(x => x.TotalCorrectCount).HasDefaultValue(0);
        builder.Property(x => x.TotalWrongCount).HasDefaultValue(0);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.EnglishWord });
        builder.HasIndex(x => new { x.UserId, x.TurkishWord });
        builder.HasIndex(x => new { x.UserId, x.CreatedTime });

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Favorite)
            .WithOne(x => x.Word)
            .HasForeignKey<Favorite>(x => x.WordId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Unknows)
            .WithOne(x => x.Word)
            .HasForeignKey<Unknows>(x => x.WordId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
