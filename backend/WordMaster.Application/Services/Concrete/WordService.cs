using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.Net;
using WordMaster.Application.Constants;
using WordMaster.Application.Key;
using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Requests.Practice;
using WordMaster.Application.Requests.Word;
using WordMaster.Application.Responses.Practice;
using WordMaster.Application.Responses.Word;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Extensions;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Concrete;

public class WordService(IWordRepository wordRepository, IUnitOfWork unitOfWork, ICacheService cacheService, ILogger<WordService> logger, IPracticeHistoryRepository practiceHistoryRepository, UserManager<AppUser> userManager) : IWordService
{
    public async Task<ServiceResult<WordResponse>> GetWordForUserAsync(long id, Guid userId)
    {
        string cacheKey = CacheKeys.Word(id, userId);
        WordResponse? cachedWordDto = await cacheService.GetAsync<WordResponse>(cacheKey);
        if (cachedWordDto != null)
        {
            return ServiceResult<WordResponse>.Success(cachedWordDto, HttpStatusCode.OK);
        }

        Word? word = await wordRepository.GetWordForUserAsync(id, userId);
        if (word == null)
        {
            return ServiceResult<WordResponse>.Failure("Kelime bulunamadı.", HttpStatusCode.NotFound);
        }

        WordResponse wordDto = new()
        {
            Id = word.Id,
            EnglishWord = word.EnglishWord,
            TurkishWord = word.TurkishWord,
            FavoriteId = word.Favorite?.Id,
            UnknowsId = word.Unknows?.Id
        };
        await cacheService.SetAsync(cacheKey, wordDto, CacheDurations.Normal);
        return ServiceResult<WordResponse>.Success(wordDto, HttpStatusCode.OK);
    }
    public async Task<ServiceResult> AddWordAsync(CreateWordRequest request, Guid userId)
    {
        request.EnglishWord = request.EnglishWord?.NormalizeEnglishWord()!;
        request.TurkishWord = request.TurkishWord?.NormalizeTurkishWord()!;

        if (string.IsNullOrWhiteSpace(request.EnglishWord))
        {
            return ServiceResult.Failure("İngilizce kelime boş olamaz.", HttpStatusCode.BadRequest);
        }

        ServiceResult<bool> isDuplicate = await IsWordDuplicateAsync(request.EnglishWord, userId);
        if (isDuplicate.Data == true)
        {
            return ServiceResult.Failure("Bu kelime zaten sözlüğünüzde mevcut.", HttpStatusCode.Conflict);
        }

        Word word = new()
        {
            EnglishWord = request.EnglishWord,
            TurkishWord = request.TurkishWord,
            UserId = userId,
            CreatedTime = DateTime.UtcNow
        };

        await wordRepository.AddAsync(word);
        await unitOfWork.CommitAsync();

        // Cache invalidation
        await cacheService.RemoveAsync(CacheKeys.Words(userId));
        await cacheService.RemoveAsync(CacheKeys.UserWordsDropdown(userId));

        return ServiceResult.SuccessAsCreated();
    }
    public async Task<ServiceResult<WordResponse>> UpdateWordAsync(long wordId, UpdateWordRequest request, Guid userId)
    {
        Word? existingWord = await wordRepository.GetWordForUserTrackedWithFoldersAsync(wordId, userId);
        if (existingWord == null)
        {
            return ServiceResult<WordResponse>.Failure("Kelime bulunamadı veya size ait değil.", HttpStatusCode.NotFound);
        }

        request.EnglishWord = request.EnglishWord?.NormalizeEnglishWord()!;
        request.TurkishWord = request.TurkishWord?.NormalizeTurkishWord()!;

        if (string.IsNullOrWhiteSpace(request.EnglishWord))
        {
            return ServiceResult<WordResponse>.Failure("İngilizce kelime boş olamaz.", HttpStatusCode.BadRequest);
        }

        bool isDuplicate = await wordRepository.AnyAsync(x =>
            x.Id != wordId &&
            x.UserId == userId &&
            x.EnglishWord != null &&
            x.EnglishWord == request.EnglishWord);

        if (isDuplicate)
        {
            return ServiceResult<WordResponse>.Failure("Bu kelime zaten sözlüğünüzde mevcut.", HttpStatusCode.Conflict);
        }

        existingWord.EnglishWord = request.EnglishWord;
        existingWord.TurkishWord = request.TurkishWord;

        wordRepository.Update(existingWord);
        await unitOfWork.CommitAsync();

        // Cache invalidation
        await cacheService.RemoveAsync(CacheKeys.Word(wordId, userId));
        await cacheService.RemoveAsync(CacheKeys.Words(userId));
        await cacheService.RemoveAsync(CacheKeys.UserWordsDropdown(userId));
        await cacheService.RemoveAsync(CacheKeys.Favorites(userId));
        await cacheService.RemoveAsync(CacheKeys.Unknows(userId));

        // Invalidate folder caches
        if (existingWord.WordFolders != null)
        {
            foreach (WordFolder wordFolder in existingWord.WordFolders)
            {
                await cacheService.RemoveAsync(CacheKeys.FolderWords(wordFolder.FolderId, userId));
            }
        }

        WordResponse response = new()
        {
            Id = existingWord.Id,
            EnglishWord = existingWord.EnglishWord,
            TurkishWord = existingWord.TurkishWord
        };

        return ServiceResult<WordResponse>.Success(response, HttpStatusCode.OK);
    }
    public async Task<ServiceResult> DeleteWordAsync(long wordId, Guid userId)
    {
        Word? word = await wordRepository.GetWordForDeleteAsync(wordId, userId);
        if (word == null)
        {
            return ServiceResult.Failure("Kelime bulunamadı veya size ait değil.", HttpStatusCode.NotFound);
        }

        wordRepository.DeleteWordWithRelations(word);
        await unitOfWork.CommitAsync();

        // Cache invalidation
        await cacheService.RemoveAsync(CacheKeys.Word(wordId, userId));
        await cacheService.RemoveAsync(CacheKeys.Words(userId));
        await cacheService.RemoveAsync(CacheKeys.UserWordsDropdown(userId));
        await cacheService.RemoveAsync(CacheKeys.Favorites(userId));
        await cacheService.RemoveAsync(CacheKeys.Unknows(userId));

        // Invalidate folder caches
        if (word.WordFolders != null)
        {
            foreach (WordFolder wordFolder in word.WordFolders)
            {
                await cacheService.RemoveAsync(CacheKeys.FolderWords(wordFolder.FolderId, userId));
            }
        }

        return ServiceResult.Success(HttpStatusCode.NoContent);
    }
    public async Task<ServiceResult<bool>> IsWordDuplicateAsync(string englishWord, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(englishWord))
        {
            return ServiceResult<bool>.Success(false, HttpStatusCode.OK);
        }

        string normalizedWord = englishWord.NormalizeEnglishWord();
        Word? existingWord = await wordRepository.GetWordByNormalizedEnglishAsync(userId, normalizedWord);

        return ServiceResult<bool>.Success(existingWord != null, HttpStatusCode.OK);
    }
    /// <summary>
    /// Kullanıcı için rastgele bir pratik kelimesi getirir.
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si.</param>
    /// <param name="excludeWordId">Varsa, bu ID'ye sahip kelime hariç tutulur (Ardışık tekrarı önlemek için).</param>
    /// <returns>Rastgele seçilen kelime.</returns>
    /// <remarks>
    /// Performans Optimizasyonu:
    /// Rastgele seçim sırasında bellekte yeni bir liste oluşturmamak (allocation-free) için
    /// LINQ Where() yerine indeks tabanlı seçim ve kaydırma (retry) mantığı kullanılmıştır.
    /// Eğer rastgele seçilen indeks 'excludeWordId'ye denk gelirse, bir sonraki eleman seçilir.
    /// </remarks>
    public async Task<ServiceResult<PracticeWordResponse>> GetRandomWordAsync(Guid userId, long? excludeWordId = null)
    {
        string cacheKey = CacheKeys.Words(userId);
        List<PracticeWordKey>? cachedWords = await cacheService.GetAsync<List<PracticeWordKey>>(cacheKey);

        List<PracticeWordKey> practiceWords;
        if (cachedWords != null && cachedWords.Count > 0)
        {
            practiceWords = cachedWords;
        }
        else
        {
            List<Word> words = await wordRepository.GetWordsByUserAsync(userId);
            practiceWords = words.Select(w => new PracticeWordKey
            {
                Id = w.Id,
                EnglishWord = w.EnglishWord,
                TurkishWord = w.TurkishWord
            }).ToList();

            if (practiceWords.Count > 0)
            {
                await cacheService.SetAsync(cacheKey, practiceWords, CacheDurations.Practice);
            }
        }

        if (practiceWords.Count == 0)
        {
            return ServiceResult<PracticeWordResponse>.Failure("Kayıt bulunamadı.", HttpStatusCode.NotFound);
        }

        int index = Random.Shared.Next(0, practiceWords.Count);
        PracticeWordKey randomWord = practiceWords[index];

        if (excludeWordId.HasValue && randomWord.Id == excludeWordId.Value && practiceWords.Count > 1)
        {
            // If we hit the excluded word, pick the next one circular.
            // This avoids creating a new list or iterating with Where.
            index = (index + 1) % practiceWords.Count;
            randomWord = practiceWords[index];
        }

        PracticeWordResponse response = new()
        {
            Id = randomWord.Id,
            EnglishWord = randomWord.EnglishWord ?? string.Empty,
            TurkishWord = randomWord.TurkishWord ?? string.Empty
        };

        return ServiceResult<PracticeWordResponse>.Success(response, HttpStatusCode.OK);
    }
    public async Task<List<string>> GetRandomDistractorsAsync(Guid userId, int count, long excludeWordId)
    {
        // Fetch all user words (from cache if possible)
        string cacheKey = CacheKeys.Words(userId);
        List<PracticeWordKey>? cachedWords = await cacheService.GetAsync<List<PracticeWordKey>>(cacheKey);

        List<PracticeWordKey> allWords;
        if (cachedWords != null && cachedWords.Count > 0)
        {
            allWords = cachedWords;
        }
        else
        {
            List<Word> words = await wordRepository.GetWordsByUserAsync(userId);
            allWords = words.Select(w => new PracticeWordKey
            {
                Id = w.Id,
                EnglishWord = w.EnglishWord,
                TurkishWord = w.TurkishWord
            }).ToList();

            if (allWords.Count > 0)
            {
                await cacheService.SetAsync(cacheKey, allWords, CacheDurations.Practice);
            }
        }

        // Filter valid distractors:
        // 1. Cannot be the target word (excludeWordId)
        // 2. Must have a valid Turkish word
        // In-memory filter is fast for typical vocabulary sizes (e.g. < 5000 words).
        List<string> candidates = allWords
            .Where(w => w.Id != excludeWordId && !string.IsNullOrWhiteSpace(w.TurkishWord))
            .Select(w => w.TurkishWord!)
            .Distinct() // Ensure unique options
            .ToList();

        if (candidates.Count == 0)
        {
            return [];
        }

        // Pick randoms efficiently
        List<string> distractors = [];
        int needed = count;

        // If we have fewer candidates than needed, return all of them
        if (candidates.Count <= needed)
        {
            return candidates;
        }

        // Shuffle picking
        while (distractors.Count < needed && candidates.Count > 0)
        {
            int index = Random.Shared.Next(candidates.Count);
            distractors.Add(candidates[index]);
            candidates.RemoveAt(index); // Remove to avoid duplicates
        }

        return distractors;
    }
    public async Task<ServiceResult<List<string>>> GetDistractorsAsync(Guid userId, int count, long excludeWordId)
    {
        // Limit count to prevent abuse
        if (count > 10) count = 10;
        if (count < 1) count = 1;
        List<string> distractors = await GetRandomDistractorsAsync(userId, count, excludeWordId);
        return ServiceResult<List<string>>.Success(distractors, HttpStatusCode.OK);
    }
    public async Task<ServiceResult<QuizResponse>> GetQuizAsync(Guid userId, long? excludeWordId = null)
    {
        ServiceResult<PracticeWordResponse> randomWordResult = await GetRandomWordAsync(userId, excludeWordId);
        if (!randomWordResult.IsSuccess || randomWordResult.Data == null)
        {
            return ServiceResult<QuizResponse>.Failure(randomWordResult.ErrorList ?? ["Kelime bulunamadı."], randomWordResult.StatusCode);
        }

        PracticeWordResponse question = randomWordResult.Data;

        // Get 3 distractors
        List<string> distractors = await GetRandomDistractorsAsync(userId, 3, question.Id);

        // It is possible we requested 3 but got fewer if the user has very few words.
        // The frontend should handle < 4 options or we can add filler "fake" answers if we really wanted to (but better not to fake it).

        List<string> options = [.. distractors];
        options.Add(question.TurkishWord);

        // Shuffle final options
        options = [.. options.OrderBy(x => Random.Shared.Next())];

        QuizResponse response = new()
        {
            Question = question,
            Options = options
        };

        return ServiceResult<QuizResponse>.Success(response, HttpStatusCode.OK);
    }
    public async Task<ServiceResult<bool>> CheckTranslationAndUpdateAsync(Guid userId, CheckTranslationRequest request)
    {
        // Use transaction to ensure data integrity
        using IDbContextTransaction transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            Word? word = await wordRepository.GetWordForUserTrackedAsync(request.WordId, userId);

            if (word == null)
            {
                return ServiceResult<bool>.Failure("Kelime bulunamadı.", HttpStatusCode.NotFound);
            }

            string? normalizedAnswer = request.Answer?.NormalizeTurkishWord();
            bool isCorrect = word.TurkishWord == normalizedAnswer;

            DateTime now = DateTime.UtcNow;
            word.IsLastAnswerCorrect = isCorrect;
            word.LastPracticeDate = now;

            // Add history record
            await practiceHistoryRepository.AddAsync(new PracticeHistory
            {
                UserId = userId,
                WordCount = 1,
                CorrectCount = isCorrect ? 1 : 0,
                WrongCount = isCorrect ? 0 : 1,
                PracticeDate = now
            });

            // Update User Streak
            AppUser? user = await userManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                DateTime today = now.Date;
                DateTime? lastUpdate = user.LastStreakUpdateDate?.Date;

                if (lastUpdate == null) // First time practice
                {
                    user.CurrentStreak = 1;
                    user.LastStreakUpdateDate = now;
                }
                else if (lastUpdate == today.AddDays(-1)) // Continued streak
                {
                    user.CurrentStreak++;
                    user.LastStreakUpdateDate = now;
                }
                else if (lastUpdate < today.AddDays(-1)) // Broken streak
                {
                    user.CurrentStreak = 1;
                    user.LastStreakUpdateDate = now;
                }
                // If already updated today, do nothing.

                await userManager.UpdateAsync(user);
            }

            if (isCorrect)
            {
                word.ConsecutiveCorrectCount++;
                word.ConsecutiveWrongCount = 0;
                word.TotalCorrectCount++;
            }
            else
            {
                word.ConsecutiveWrongCount++;
                word.ConsecutiveCorrectCount = 0;
                word.TotalWrongCount++;
            }

            wordRepository.Update(word);
            await unitOfWork.CommitAsync();
            await unitOfWork.CommitTransactionAsync();

            // Practice sonrası istatistik cache'ini invalidate et
            await cacheService.RemoveAsync(CacheKeys.UserStatistics(userId));

            return ServiceResult<bool>.Success(isCorrect, HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            logger.LogError(ex, "Error during CheckTranslationAndUpdateAsync for user {UserId}", userId);
            return ServiceResult<bool>.Failure("Bir hata oluştu.", HttpStatusCode.InternalServerError);
        }
    }
    public async Task<ServiceResult<PagedResult<WordResponse>>> GetPagedWordsAsync(Guid userId, string? search, int page, int pageSize)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize <= 0)
        {
            pageSize = 10;
        }

        (List<Word>? words, int totalCount) = await wordRepository.GetPagedWordsAsync(userId, search, page, pageSize);

        List<WordResponse> wordDtos = words.Select(w => new WordResponse
        {
            Id = w.Id,
            EnglishWord = w.EnglishWord,
            TurkishWord = w.TurkishWord,
            FavoriteId = w.Favorite?.Id,
            UnknowsId = w.Unknows?.Id
        }).ToList();

        PagedResult<WordResponse> pagedResult = new()
        {
            Items = wordDtos,
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return ServiceResult<PagedResult<WordResponse>>.Success(pagedResult, HttpStatusCode.OK);
    }
    public async Task<ServiceResult<List<WordLookupResponse>>> GetUserWordsAsync(Guid userId)
    {
        // Cache all user's words for dropdown usage; client will filter locally
        string cacheKey = CacheKeys.UserWordsDropdown(userId);
        List<WordLookupResponse>? cached = await cacheService.GetAsync<List<WordLookupResponse>>(cacheKey);
        if (cached != null && cached.Count > 0)
        {
            return ServiceResult<List<WordLookupResponse>>.Success(cached, HttpStatusCode.OK);
        }

        List<WordLookupResponse> words = await wordRepository
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.EnglishWord)
            .Select(x => new WordLookupResponse
            {
                Id = x.Id,
                EnglishWord = x.EnglishWord
            })
            .AsNoTracking()
            .ToListAsync();

        await cacheService.SetAsync(cacheKey, words, CacheDurations.Normal);
        return ServiceResult<List<WordLookupResponse>>.Success(words, HttpStatusCode.OK);
    }
    public async Task<ServiceResult<bool>> BulkUpdateStatsAsync(Guid userId, BulkUpdateStatsRequest request)
    {
        if (request.Results == null || request.Results.Count == 0)
        {
            return ServiceResult<bool>.Success(true, HttpStatusCode.OK);
        }

        // Use transaction to ensure data integrity across Words, History, and User tables
        using IDbContextTransaction transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            List<long> wordIds = request.Results.Select(r => (long)r.WordId).ToList();
            List<Word> words = await wordRepository
                .Where(x => wordIds.Contains(x.Id) && x.UserId == userId)
                .ToListAsync();

            int updatedCount = 0;
            int totalCorrect = 0;
            int totalWrong = 0;
            DateTime now = DateTime.UtcNow;

            foreach (PracticeResultItem result in request.Results)
            {
                Word? word = words.FirstOrDefault(w => w.Id == result.WordId);
                if (word == null) continue;

                word.IsLastAnswerCorrect = result.IsCorrect;
                word.LastPracticeDate = now;

                // Removed individual history record addition here

                if (result.IsCorrect)
                {
                    word.ConsecutiveCorrectCount++;
                    word.ConsecutiveWrongCount = 0;
                    word.TotalCorrectCount++;
                    totalCorrect++;
                }
                else
                {
                    word.ConsecutiveWrongCount++;
                    word.ConsecutiveCorrectCount = 0;
                    word.TotalWrongCount++;
                    totalWrong++;
                }

                wordRepository.Update(word);
                updatedCount++;
            }

            // Add ONE aggregated history record for the entire batch
            if (updatedCount > 0)
            {
                await practiceHistoryRepository.AddAsync(new PracticeHistory
                {
                    UserId = userId,
                    WordCount = updatedCount,
                    CorrectCount = totalCorrect,
                    WrongCount = totalWrong,
                    PracticeDate = now
                });
            }

            await unitOfWork.CommitAsync(); // Commit changes to words and history within the transaction

            // Update User Streak (Once for the bulk operation)
            if (updatedCount > 0)
            {
                AppUser? user = await userManager.FindByIdAsync(userId.ToString());
                if (user != null)
                {
                    DateTime today = now.Date;
                    DateTime? lastUpdate = user.LastStreakUpdateDate?.Date;

                    if (lastUpdate == null) // First time
                    {
                        user.CurrentStreak = 1;
                        user.LastStreakUpdateDate = now;
                    }
                    else if (lastUpdate == today.AddDays(-1)) // Streak continues
                    {
                        user.CurrentStreak++;
                        user.LastStreakUpdateDate = now;
                    }
                    else if (lastUpdate < today.AddDays(-1)) // Streak broken
                    {
                        user.CurrentStreak = 1;
                        user.LastStreakUpdateDate = now;
                    }

                    await userManager.UpdateAsync(user);
                }
            }

            await unitOfWork.CommitTransactionAsync(); // Commit the entire transaction

            logger.LogInformation("Practice completed for user {UserId}. Updated stats for {UpdatedCount} words. Correct: {Correct}, Wrong: {Wrong}", userId, updatedCount, totalCorrect, totalWrong);

            // Cache invalidation
            await cacheService.RemoveAsync(CacheKeys.Words(userId));
            await cacheService.RemoveAsync(CacheKeys.UserStatistics(userId)); // Practice sonrası istatistik cache'ini invalidate et
            foreach (long id in wordIds)
            {
                await cacheService.RemoveAsync(CacheKeys.Word(id, userId));
            }

            return ServiceResult<bool>.Success(true, HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            logger.LogError(ex, "Error during BulkUpdateStatsAsync for user {UserId}", userId);
            return ServiceResult<bool>.Failure("Bir hata oluştu.", HttpStatusCode.InternalServerError);
        }
    }
    public async Task<ServiceResult<PagedResult<AdminWordResponse>>> GetAdminPagedWordsAsync(string? search, int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize <= 0) pageSize = 10;

        (List<Word> words, int totalCount) = await wordRepository.GetAdminPagedWordsAsync(search, page, pageSize);

        List<AdminWordResponse> wordDtos = words.Select(w => new AdminWordResponse
        {
            Id = w.Id,
            EnglishWord = w.EnglishWord,
            TurkishWord = w.TurkishWord,
            UserId = w.UserId,
            UserName = w.User?.UserName,
            CreatedTime = w.CreatedTime
        }).ToList();

        PagedResult<AdminWordResponse> pagedResult = new()
        {
            Items = wordDtos,
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return ServiceResult<PagedResult<AdminWordResponse>>.Success(pagedResult, HttpStatusCode.OK);
    }
    public async Task<ServiceResult<AdminWordResponse>> AdminUpdateWordAsync(long wordId, UpdateWordRequest request)
    {
        Word? existingWord = await wordRepository.GetByIdAsync((int)wordId);
        if (existingWord == null)
        {
            logger.LogWarning("Admin update failed. Word not found: {WordId}", wordId);
            return ServiceResult<AdminWordResponse>.Failure("Kelime bulunamadı.", HttpStatusCode.NotFound);
        }

        request.EnglishWord = request.EnglishWord?.NormalizeEnglishWord()!;
        request.TurkishWord = request.TurkishWord?.NormalizeTurkishWord()!;

        if (string.IsNullOrWhiteSpace(request.EnglishWord))
        {
            return ServiceResult<AdminWordResponse>.Failure("İngilizce kelime boş olamaz.", HttpStatusCode.BadRequest);
        }

        // Admin can update any word, duplicate check might be tricky across users, 
        // but usually we check duplicates for the SAME user.
        // Let's check if the user already has this word (excluding the current one)
        if (existingWord.UserId.HasValue)
        {
            bool isDuplicate = await wordRepository.AnyAsync(x =>
               x.Id != wordId &&
               x.UserId == existingWord.UserId &&
               x.EnglishWord != null &&
               x.EnglishWord == request.EnglishWord);

            if (isDuplicate)
            {
                logger.LogWarning("Admin update failed. Duplicate word for user {UserId}: {EnglishWord}", existingWord.UserId, request.EnglishWord);
                return ServiceResult<AdminWordResponse>.Failure("Bu kullanıcıda bu kelime zaten mevcut.", HttpStatusCode.Conflict);
            }
        }

        existingWord.EnglishWord = request.EnglishWord;
        existingWord.TurkishWord = request.TurkishWord;

        wordRepository.Update(existingWord);
        await unitOfWork.CommitAsync();

        // Cache invalidation if user exists
        if (existingWord.UserId.HasValue)
        {
            Guid userId = existingWord.UserId.Value;
            await cacheService.RemoveAsync(CacheKeys.Word(wordId, userId));
            await cacheService.RemoveAsync(CacheKeys.Words(userId));
            await cacheService.RemoveAsync(CacheKeys.UserWordsDropdown(userId));
        }

        // We need to fetch the user again or assume it's loaded if we want to return UserName
        // But GetByIdAsync might not include User. 
        // For simplicity, we return the response without UserName or fetch it if needed.
        // Let's return basic info.

        AdminWordResponse response = new()
        {
            Id = existingWord.Id,
            EnglishWord = existingWord.EnglishWord,
            TurkishWord = existingWord.TurkishWord,
            UserId = existingWord.UserId,
            CreatedTime = existingWord.CreatedTime
        };

        logger.LogInformation("Admin update successful. Word ID: {WordId}", wordId);
        return ServiceResult<AdminWordResponse>.Success(response, HttpStatusCode.OK);
    }
    public async Task<ServiceResult> AdminDeleteWordAsync(long wordId)
    {
        Word? word = await wordRepository.GetByIdAsync((int)wordId);
        if (word == null)
        {
            logger.LogWarning("Admin delete failed. Word not found: {WordId}", wordId);
            return ServiceResult.Failure("Kelime bulunamadı.", HttpStatusCode.NotFound);
        }

        wordRepository.Remove(word);
        await unitOfWork.CommitAsync();

        // Cache invalidation
        if (word.UserId.HasValue)
        {
            Guid userId = word.UserId.Value;
            await cacheService.RemoveAsync(CacheKeys.Word(wordId, userId));
            await cacheService.RemoveAsync(CacheKeys.Words(userId));
            await cacheService.RemoveAsync(CacheKeys.UserWordsDropdown(userId));
            await cacheService.RemoveAsync(CacheKeys.Favorites(userId));
            await cacheService.RemoveAsync(CacheKeys.Unknows(userId));
        }

        logger.LogInformation("Admin delete successful. Word ID: {WordId}", wordId);
        return ServiceResult.Success(HttpStatusCode.NoContent);
    }
}
