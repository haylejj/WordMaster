namespace WordMaster.Domain.Entities;

public class Folder
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedTime { get; set; }

    public string? UserId { get; set; }
    public AppUser? User { get; set; }

    public ICollection<WordFolder> WordFolders { get; set; } = new List<WordFolder>();
}

