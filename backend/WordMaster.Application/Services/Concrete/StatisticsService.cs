using System.Net;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Responses.Statistics;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Concrete;

public class StatisticsService(IWordRepository wordRepository) : IStatisticsService
{
    public async Task<ServiceResult<DashboardStatisticsResponse>> GetDashboardStatisticsAsync(Guid userId)
    {
        // 1. Kullanıcının tüm kelimelerini getir
        List<Word> words = await wordRepository.GetWordsByUserAsync(userId);

        if (words == null || words.Count == 0)
        {
            // Hiç kelime yoksa boş istatistik dön
            return ServiceResult<DashboardStatisticsResponse>.Success(new DashboardStatisticsResponse(), HttpStatusCode.OK);
        }

        // 2. Temel hesaplamalar
        int totalWords = words.Count;
        int totalCorrect = words.Sum(w => w.TotalCorrectCount);
        int totalWrong = words.Sum(w => w.TotalWrongCount);
        int totalAttempts = totalCorrect + totalWrong;
        double accuracy = totalAttempts > 0 ? (double)totalCorrect / totalAttempts * 100 : 0;

        // Öğrenilmiş kabulü: En az 5 kez üst üste doğru bilinenler
        int learnedWords = words.Count(w => w.ConsecutiveCorrectCount >= 5);

        // 3. Sıralamalar (En iyiler ve En kötüler)
        // Hiç pratiği olmayanları (0 doğru 0 yanlış) elemeyi tercih edebiliriz veya sonda gösterebiliriz.
        // Burada sadece en az 1 kez işlem görmüş kelimeleri dikkate alalım ki liste boş görünmesin.

        List<WordStatItem> topBest = words
            .Where(w => w.TotalCorrectCount > 0)
            .OrderByDescending(w => w.TotalCorrectCount)
            .ThenBy(w => w.TotalWrongCount) // Eşitlik durumunda yanlışı az olan öne
            .Take(5)
            .Select(w => new WordStatItem
            {
                Id = w.Id,
                EnglishWord = w.EnglishWord ?? "",
                TurkishWord = w.TurkishWord ?? "",
                CorrectCount = w.TotalCorrectCount,
                WrongCount = w.TotalWrongCount
            })
            .ToList();

        List<WordStatItem> topWorst = words
            .Where(w => w.TotalWrongCount > 0)
            .OrderByDescending(w => w.TotalWrongCount)
            .ThenBy(w => w.TotalCorrectCount) // Eşitlik durumunda doğrusu az olan öne
            .Take(5)
            .Select(w => new WordStatItem
            {
                Id = w.Id,
                EnglishWord = w.EnglishWord ?? "",
                TurkishWord = w.TurkishWord ?? "",
                CorrectCount = w.TotalCorrectCount,
                WrongCount = w.TotalWrongCount
            })
            .ToList();

        // 4. Aktivite Grafiği (Son 14 gün)
        // Word tablosunda sadece LastPracticeDate var.
        // Bu yüzden "Bugün şu kadar kelimeye dokundun" verisini çıkarabiliriz.
        // Geçmiş günler için tam sayı veremeyiz ama "Son çalışılma tarihi X olan kelimeler" sayısını verebiliriz.
        // Bu tam bir "Günlük Pratik Sayısı" değildir ama kullanıcıya bir aktivite hissi verir.

        DateTime twoWeeksAgo = DateTime.UtcNow.Date.AddDays(-13);

        List<DailyActivityStat> activityGroups = words
            .Where(w => w.LastPracticeDate.HasValue && w.LastPracticeDate.Value.Date >= twoWeeksAgo)
            .GroupBy(w => w.LastPracticeDate!.Value.Date)
            .Select(g => new DailyActivityStat
            {
                Date = g.Key,
                Count = g.Count()
            })
            .ToList();

        // Grafikte boş günlerin 0 olarak görünmesi için listeyi tamamlayalım
        List<DailyActivityStat> lastActivities = new();
        for (int i = 0; i < 14; i++)
        {
            DateTime date = twoWeeksAgo.AddDays(i);
            DailyActivityStat? existing = activityGroups.FirstOrDefault(a => a.Date == date);
            lastActivities.Add(new DailyActivityStat
            {
                Date = date,
                Count = existing?.Count ?? 0
            });
        }

        // 5. Streak (Seri) Hesaplama
        // Word tablosundaki LastPracticeDate dağılımına bakarak tahmini bir seri çıkarıyoruz.
        // Eğer kullanıcı her gün farklı kelimelere dokunuyorsa bu yöntem %100 çalışır.
        // Eğer her gün kütüphanesindeki TÜM kelimeleri bitiriyorsa (zor ama) sadece 1 gün görünür.

        List<DateTime> distinctPracticeDates = words
            .Where(w => w.LastPracticeDate.HasValue)
            .Select(w => w.LastPracticeDate!.Value.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        int currentStreak = 0;
        DateTime checkDate = DateTime.UtcNow.Date;

        // Eğer bugün hiç pratik yapmadıysa, seriyi dünden itibaren kontrol etmeye başla
        // Böylece serisi bozulmamış olur, sadece bugün henüz yapmamış görünür.
        if (!distinctPracticeDates.Contains(checkDate))
        {
            // Bugün veri yok, düne bak
            checkDate = checkDate.AddDays(-1);
            // Ama eğer dünün verisi de yoksa streak 0 dır.
            if (!distinctPracticeDates.Contains(checkDate))
            {
                currentStreak = 0;
            }
            else
            {
                // Dün var, zinciri dünden başlat
                while (distinctPracticeDates.Contains(checkDate))
                {
                    currentStreak++;
                    checkDate = checkDate.AddDays(-1);
                }
            }
        }
        else
        {
            // Bugün veri var, zinciri bugünden başlat
            while (distinctPracticeDates.Contains(checkDate))
            {
                currentStreak++;
                checkDate = checkDate.AddDays(-1);
            }
        }

        DashboardStatisticsResponse response = new()
        {
            TotalWords = totalWords,
            TotalLearnedWords = learnedWords,
            TotalCorrectCount = totalCorrect,
            TotalWrongCount = totalWrong,
            AccuracyRate = Math.Round(accuracy, 2),
            CurrentStreak = currentStreak,
            TopBestWords = topBest,
            TopWorstWords = topWorst,
            LastActivities = lastActivities
        };

        return ServiceResult<DashboardStatisticsResponse>.Success(response, HttpStatusCode.OK);
    }
}
