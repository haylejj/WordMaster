namespace WordMaster.Application.Responses.Folder;

public class FolderResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedTime { get; set; }
    public int WordCount { get; set; }
}
