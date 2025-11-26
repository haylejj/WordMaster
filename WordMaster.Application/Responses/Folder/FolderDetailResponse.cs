using WordMaster.Application.Responses.Word;

namespace WordMaster.Application.Responses.Folder;

public class FolderDetailResponse
{
    public long FolderId { get; set; }
    public string FolderName { get; set; } = string.Empty;

    public List<WordResponse> Words { get; set; } = new();
    public List<WordResponse> AllWords { get; set; } = new();
}
