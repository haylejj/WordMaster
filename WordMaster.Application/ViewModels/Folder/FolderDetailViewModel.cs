namespace WordMaster.Application.ViewModels.Folder;

public class FolderDetailViewModel
{
    public int FolderId { get; set; }
    public string FolderName { get; set; } = string.Empty;

    public List<WordMaster.Domain.Entities.Word> Words { get; set; } = new();
    public List<WordMaster.Domain.Entities.Word> AllWords { get; set; } = new();
}


