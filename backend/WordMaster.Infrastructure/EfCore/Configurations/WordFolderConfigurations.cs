using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordMaster.Domain.Entities;

namespace WordMaster.Infrastructure.EfCore.Configurations;

public class WordFolderConfigurations : IEntityTypeConfiguration<WordFolder>
{
    public void Configure(EntityTypeBuilder<WordFolder> builder)
    {
        builder.ToTable("FolderWords");

        builder.HasKey(x => new { x.FolderId, x.WordId });

        builder.HasOne(x => x.Word)
            .WithMany(x => x.WordFolders)
            .HasForeignKey(x => x.WordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Folder)
            .WithMany(x => x.WordFolders)
            .HasForeignKey(x => x.FolderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

