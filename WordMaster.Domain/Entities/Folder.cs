namespace WordMaster.Domain.Entities;

public class Folder
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedTime { get; set; }

    public Guid? UserId { get; set; }
    public AppUser? User { get; set; }

    public ICollection<WordFolder> WordFolders { get; set; } = new List<WordFolder>();
}

