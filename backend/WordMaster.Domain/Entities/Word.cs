namespace WordMaster.Domain.Entities;

public class Word
{
    public long Id { get; set; }
    public string? EnglishWord { get; set; }
    public string? TurkishWord { get; set; }
    public DateTime CreatedTime { get; set; }
    public Guid? UserId { get; set; }
    public AppUser? User { get; set; }
    public Favorite? Favorite { get; set; }
    public Unknows? Unknows { get; set; }
    public ICollection<WordFolder> WordFolders { get; set; } = new List<WordFolder>();

    // Öğrenme Takibi
    public bool? IsLastAnswerCorrect { get; set; } // Son cevap doğru mu?
    public int ConsecutiveCorrectCount { get; set; } // Ard arda kaç kere doğru bildi
    public int ConsecutiveWrongCount { get; set; } // Ard arda kaç kere yanlış bildi
    public int TotalCorrectCount { get; set; } // Toplam doğru sayısı
    public int TotalWrongCount { get; set; } // Toplam yanlış sayısı
    public DateTime? LastPracticeDate { get; set; } // Son pratik yapılan tarih
}
