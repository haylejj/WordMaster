namespace WordMaster.Application.Requests.Folder;

public class UpdateFolderRequest
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
}
