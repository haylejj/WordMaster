namespace WordMaster.Application.Requests.Folder;

public class AddWordToFolderRequest
{
    public long FolderId { get; set; }
    public long WordId { get; set; }
}
