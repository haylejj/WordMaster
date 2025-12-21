using Microsoft.AspNetCore.Identity;
using System.Net;
using WordMaster.Application.Constants;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Responses.Statistics;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Concrete;

/// <summary>
/// Kullanıcı dashboard istatistiklerini sağlayan servis.
/// 1 dakikalık cache ile performans optimizasyonu sağlar.
/// </summary>
public class StatisticsService(
    IWordRepository wordRepository,
    IPracticeHistoryRepository practiceHistoryRepository,
    UserManager<AppUser> userManager,
    ICacheService cacheService) : IStatisticsService
{
    /// <summary>
    /// Kullanıcının dashboard istatistiklerini getirir.
    /// Cache'te varsa oradan döner (1 dakika TTL), yoksa DB'den çekip cache'ler.
    /// </summary>
    public async Task<ServiceResult<DashboardStatisticsResponse>> GetDashboardStatisticsAsync(Guid userId)
    {
        // Cache kontrolü
        string cacheKey = CacheKeys.UserStatistics(userId);
        DashboardStatisticsResponse? cached = await cacheService.GetAsync<DashboardStatisticsResponse>(cacheKey);
        if (cached != null)
        {
            return ServiceResult<DashboardStatisticsResponse>.Success(cached, HttpStatusCode.OK);
        }

        // 1. Fetch Aggregated Statistics directly from DB (Optimized)
        (int totalWords, int learnedWords, int totalCorrect, int totalWrong) = await wordRepository.GetUserGeneralStatsAsync(userId);

        // 2. Fetch Top 5 Best & Worst Words
        List<Word> bestWords = await wordRepository.GetUserBestWordsAsync(userId, 5);
        List<Word> worstWords = await wordRepository.GetUserWorstWordsAsync(userId, 5);

        // Fetch only last 14 days for the activity graph. Streak is now read from AppUser.
        DateTime twoWeeksAgo = DateTime.UtcNow.Date.AddDays(-13);
        List<PracticeHistory> history = await practiceHistoryRepository.GetHistoryByUserAsync(userId, twoWeeksAgo);

        // Basic calculations
        int totalAttempts = totalCorrect + totalWrong;
        double accuracy = totalAttempts > 0 ? (double)totalCorrect / totalAttempts * 100 : 0;

        // Map Best Words
        List<WordStatItem> topBest = bestWords.Select(w => new WordStatItem
        {
            Id = w.Id,
            EnglishWord = w.EnglishWord ?? "",
            TurkishWord = w.TurkishWord ?? "",
            CorrectCount = w.TotalCorrectCount,
            WrongCount = w.TotalWrongCount
        }).ToList();

        // Map Worst Words
        List<WordStatItem> topWorst = worstWords.Select(w => new WordStatItem
        {
            Id = w.Id,
            EnglishWord = w.EnglishWord ?? "",
            TurkishWord = w.TurkishWord ?? "",
            CorrectCount = w.TotalCorrectCount,
            WrongCount = w.TotalWrongCount
        }).ToList();

        // 4. Aktivite Grafiği (Son 14 gün)
        // Date range is already defined above: twoWeeksAgo

        // Filter history for only last 14 days for the graph
        List<DailyActivityStat> graphData = history
            .Where(h => h.PracticeDate.Date >= twoWeeksAgo)
            .GroupBy(h => h.PracticeDate.Date)
            .Select(g => new DailyActivityStat
            {
                Date = g.Key,
                Count = g.Sum(h => h.WordCount) // Total words practiced
            })
            .ToList();

        List<DailyActivityStat> lastActivities = [];
        for (int i = 0; i < 14; i++)
        {
            DateTime date = twoWeeksAgo.AddDays(i);
            DailyActivityStat? existing = graphData.FirstOrDefault(a => a.Date == date);
            lastActivities.Add(new DailyActivityStat
            {
                Date = date,
                Count = existing?.Count ?? 0
            });
        }

        // 5. Streak (Seri) - Get directly from AppUser
        int currentStreak = 0;
        AppUser? user = await userManager.FindByIdAsync(userId.ToString());
        if (user != null)
        {
            // Eğer son güncelleme dünden önceyse seri bozulmuştur
            if (user.LastStreakUpdateDate.HasValue && user.LastStreakUpdateDate.Value.Date < DateTime.UtcNow.Date.AddDays(-1))
            {
                currentStreak = 0;
            }
            else
            {
                currentStreak = user.CurrentStreak;
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

        // Cache'e kaydet (2 dakika)
        await cacheService.SetAsync(cacheKey, response, CacheDurations.Statistics);

        return ServiceResult<DashboardStatisticsResponse>.Success(response, HttpStatusCode.OK);
    }
}
