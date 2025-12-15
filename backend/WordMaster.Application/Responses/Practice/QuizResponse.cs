namespace WordMaster.Application.Responses.Practice;

public class QuizResponse
{
    public PracticeWordResponse Question { get; set; } = new();
    public List<string> Options { get; set; } = new();
}
