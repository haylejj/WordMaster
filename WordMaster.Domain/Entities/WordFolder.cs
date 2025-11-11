namespace WordMaster.Domain.Entities;

public class WordFolder
{
    public int WordId { get; set; }
    public Word Word { get; set; } = null!;

    public int FolderId { get; set; }
    public Folder Folder { get; set; } = null!;
}

