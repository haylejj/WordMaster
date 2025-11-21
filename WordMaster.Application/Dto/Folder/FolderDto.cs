namespace WordMaster.Application.Dto.Folder;

public class FolderDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedTime { get; set; }
    public int WordCount { get; set; }
}
