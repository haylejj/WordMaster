namespace WordMaster.Application.Responses.Folder;

public class FolderWordResponse
{
    public long Id { get; set; }
    public string EnglishWord { get; set; } = default!;
    public string TurkishWord { get; set; } = default!;
}
