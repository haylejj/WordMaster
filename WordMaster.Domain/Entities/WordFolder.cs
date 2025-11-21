namespace WordMaster.Domain.Entities;

public class WordFolder
{
    public long WordId { get; set; }
    public Word Word { get; set; } = null!;

    public long FolderId { get; set; }
    public Folder Folder { get; set; } = null!;
}

