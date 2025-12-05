namespace WordMaster.Application.Responses.Word;

public class WordImportSummaryResponse
{
    public int TotalProcessed { get; set; }
    public int AddedCount { get; set; }
    public int DuplicateCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> FailedRows { get; set; } = new();
}
