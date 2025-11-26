namespace WordMaster.Application.Responses.Word;

public class AdminWordResponse
{
    public long Id { get; set; }
    public string? EnglishWord { get; set; }
    public string? TurkishWord { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime CreatedTime { get; set; }
}
