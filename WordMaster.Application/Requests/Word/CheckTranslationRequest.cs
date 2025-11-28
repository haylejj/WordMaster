namespace WordMaster.Application.Requests.Word;

public class CheckTranslationRequest
{
    public long WordId { get; set; }
    public string Answer { get; set; } = null!;
}

