using WordMaster.Application.Dto.Word;

namespace WordMaster.Application.ViewModels.Folder;

public class FolderDetailViewModel
{
    public long FolderId { get; set; }
    public string FolderName { get; set; } = string.Empty;

    public List<WordDto> Words { get; set; } = new();
    public List<WordDto> AllWords { get; set; } = new();
}


