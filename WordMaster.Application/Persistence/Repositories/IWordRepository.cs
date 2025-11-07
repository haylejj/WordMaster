using WordMaster.Domain.Entities;
namespace WordMaster.Application.Persistence.Repositories;

public interface IWordRepository : IGenericRepository<Word>
{
    Task<Word?> GetWordForUserAsync(int wordId, string userId);
    Task<Word?> GetWordForUserTrackedAsync(int wordId, string userId);
    Task<Word?> GetWordByNormalizedEnglishAsync(string userId, string normalizedEnglishWord);
    Task<List<Word>> GetWordsByUserAsync(string userId);
    Task<(List<Word> Words, int TotalCount)> GetPagedWordsAsync(string userId, string? search, int page, int pageSize);
    Task<Word?> GetLastWord();
}
