namespace WordMaster.Application.ViewModels.Practice;

public class PracticeFolderViewModel
{
    public long FolderId { get; set; }
    public string FolderName { get; set; } = string.Empty;
    public List<PracticeFolderWordViewModel> Words { get; set; } = [];
}

public class PracticeFolderWordViewModel
{
    public long WordId { get; set; }
    public string EnglishWord { get; set; } = string.Empty;
    public string TurkishWord { get; set; } = string.Empty;
}

