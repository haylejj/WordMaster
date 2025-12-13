using System;

namespace WordMaster.Domain.Entities;

public class PracticeHistory
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    public int WordCount { get; set; }
    public int CorrectCount { get; set; }
    public int WrongCount { get; set; }
    public DateTime PracticeDate { get; set; } = DateTime.UtcNow;
    public AppUser? AppUser { get; set; }
}
