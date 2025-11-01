using Core.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Configurations;

public class WordConfigurations : IEntityTypeConfiguration<Word>
{
    public void Configure(EntityTypeBuilder<Word> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.EnglishWord).IsRequired().HasMaxLength(30);
        builder.Property(x => x.TurkishWord).IsRequired().HasMaxLength(30);
        builder.Property(x => x.CreatedTime).IsRequired();

        // Öğrenme Takibi - Varsayılan değerler
        builder.Property(x => x.ConsecutiveCorrectCount).HasDefaultValue(0);
        builder.Property(x => x.ConsecutiveWrongCount).HasDefaultValue(0);
        builder.Property(x => x.TotalCorrectCount).HasDefaultValue(0);
        builder.Property(x => x.TotalWrongCount).HasDefaultValue(0);

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
