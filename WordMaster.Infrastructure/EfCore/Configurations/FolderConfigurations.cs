using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordMaster.Domain.Entities;

namespace WordMaster.Infrastructure.EfCore.Configurations;

public class FolderConfigurations : IEntityTypeConfiguration<Folder>
{
    public void Configure(EntityTypeBuilder<Folder> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(60);
        builder.Property(x => x.CreatedTime).IsRequired();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany<WordFolder>()
            .WithOne(x => x.Folder)
            .HasForeignKey(x => x.FolderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

