using Core.Entity;
using Core.Extensions;
using Core.Repositories;
using Core.Service;
using Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Core.Results;

namespace Service.Service;

public class UnknowsService(IGenericRepository<Unknows> repository, IUnitOfWork unitOfWork, IWordService wordService) : IUnknowsService
{
    private static readonly Random _random = new();
    private readonly IWordService _wordService = wordService;
    public IQueryable<Unknows> Where(Expression<Func<Unknows, bool>> predicate) => repository.Where(predicate);

    public async Task<Result> DeleteUnknowsAsync(int unknowsId, string userId)
    {
        // Unknows'ı kullanıcıya ait mi kontrol et
        var unknow = await Where(x => x.Id == unknowsId && x.UserId == userId).FirstOrDefaultAsync();
        if (unknow == null)
        {
            return Result.Failure("Bilinmeyen kelime bulunamadı veya size ait değil.");
        }

        repository.Remove(unknow);
        await unitOfWork.CommitAsync();
        return Result.Success();
    }

    public async Task<Result<string>> GetRandomWordFromUnknowsAsync(string userId)
    {
        var unknows = await Where(x => x.UserId == userId)
            .Include(x => x.Word)
            .ToListAsync();

        if (unknows == null || unknows.Count == 0)
        {
            return Result<string>.Failure("Kayıt bulunamadı.");
        }

        int index = _random.Next(0, unknows.Count);
        return Result<string>.Success(unknows[index].Word?.EnglishWord ?? string.Empty);
    }

    public async Task<Result<bool>> CheckTranslationAndUpdateAsync(string userId, string turkishWord, string englishWord)
    {
        return await _wordService.CheckTranslationAndUpdateAsync(userId, turkishWord, englishWord);
    }

    public async Task<List<Unknows>> GetUserUnknowsAsync(string userId)
    {
        return await Where(x => x.UserId == userId).Include(x => x.Word).ToListAsync();
    }

    public async Task<Result<bool>> ToggleUnknowsAsync(int wordId, string userId)
    {
        var wordExists = await _wordService.GetWordForUserAsync(wordId, userId);
        if (!wordExists.IsSuccess || wordExists.Data == null)
        {
            return Result<bool>.Failure("Kelime bulunamadı.");
        }
        var existingUnknow = await Where(x => x.WordId == wordId && x.UserId == userId).FirstOrDefaultAsync();
        if (existingUnknow == null)
        {
            var unknow = new Unknows { WordId = wordId, UserId = userId, CreatedTime = DateTime.Now };
            await repository.AddAsync(unknow);
            await unitOfWork.CommitAsync();
            return Result<bool>.Success(true);
        }
        repository.Remove(existingUnknow);
        await unitOfWork.CommitAsync();
        return Result<bool>.Success(false);
    }

    public async Task<Result<Unknows>> GetUnknowsWithWordAsync(int unknowsId, string userId)
    {
        var unknow = await Where(x => x.Id == unknowsId && x.UserId == userId).Include(x => x.Word).FirstOrDefaultAsync();
        return unknow == null ? Result<Unknows>.Failure("Kayıt bulunamadı.") : Result<Unknows>.Success(unknow);
    }

    public async Task<Result<(List<Unknows> Unknows, int TotalCount)>> GetPagedUnknowsAsync(string userId, string? search, int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize <= 0) pageSize = 10;

        var query = Where(x => x.UserId == userId)
            .Include(x => x.Word)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Word!.EnglishWord!.Contains(search) || x.Word!.TurkishWord!.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(x => x.CreatedTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Result<(List<Unknows> Unknows, int TotalCount)>.Success((items, totalCount));
    }
}
