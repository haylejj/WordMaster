using WordMaster.Domain.Entities;
namespace WordMaster.Application.Persistence.Repositories;

public interface IWordRepository : IGenericRepository<Word>
{
    Task<Word?> GetWordForUserAsync(long wordId, Guid userId);
    Task<Word?> GetWordForUserTrackedAsync(long wordId, Guid userId);
    Task<Word?> GetWordForUserTrackedWithFoldersAsync(long wordId, Guid userId);
    Task<Word?> GetWordForDeleteAsync(long wordId, Guid userId);
    Task<Word?> GetWordByNormalizedEnglishAsync(Guid userId, string normalizedEnglishWord);
    Task<List<Word>> GetWordsByUserAsync(Guid userId);
    Task<(List<Word> Words, int TotalCount)> GetPagedWordsAsync(Guid userId, string? search, int page, int pageSize);
    Task<(List<Word> Words, int TotalCount)> GetAdminPagedWordsAsync(string? search, int page, int pageSize);
    Task<(int TotalWords, int LearnedWords, int TotalCorrect, int TotalWrong)> GetUserGeneralStatsAsync(Guid userId);
    Task<List<Word>> GetUserBestWordsAsync(Guid userId, int count);
    Task<List<Word>> GetUserWorstWordsAsync(Guid userId, int count);
    void DeleteWordWithRelations(Word word);
}
