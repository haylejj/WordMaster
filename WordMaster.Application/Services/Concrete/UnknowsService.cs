using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Concrete;

public class UnknowsService(IUnknowsRepository unknowsRepository, IUnitOfWork unitOfWork, IWordService wordService) : IUnknowsService
{
    private static readonly Random _random = new();
    private readonly IUnknowsRepository _unknowsRepository = unknowsRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IWordService _wordService = wordService;

    public async Task<Result> DeleteUnknowsAsync(int unknowsId, string userId)
    {
        var unknow = await _unknowsRepository.GetByIdForUserAsync(unknowsId, userId);
        if (unknow == null)
        {
            return Result.Failure("Bilinmeyen kelime bulunamadı veya size ait değil.");
        }

        _unknowsRepository.Remove(unknow);
        await _unitOfWork.CommitAsync();
        return Result.Success();
    }

    public async Task<Result<string>> GetRandomWordFromUnknowsAsync(string userId)
    {
        var unknows = await _unknowsRepository.GetUserUnknowsWithWordAsync(userId);
        if (unknows.Count == 0)
        {
            return Result<string>.Failure("Kayıt bulunamadı.");
        }

        int index = _random.Next(0, unknows.Count);
        return Result<string>.Success(unknows[index].Word?.EnglishWord ?? string.Empty);
    }

    public Task<Result<bool>> CheckTranslationAndUpdateAsync(string userId, string turkishWord, string englishWord)
    {
        return _wordService.CheckTranslationAndUpdateAsync(userId, turkishWord, englishWord);
    }

    public Task<List<Unknows>> GetUserUnknowsAsync(string userId)
    {
        return _unknowsRepository.GetUserUnknowsWithWordAsync(userId);
    }

    public async Task<Result<bool>> ToggleUnknowsAsync(int wordId, string userId)
    {
        var wordExists = await _wordService.GetWordForUserAsync(wordId, userId);
        if (!wordExists.IsSuccess || wordExists.Data == null)
        {
            return Result<bool>.Failure("Kelime bulunamadı.");
        }

        var existingUnknow = await _unknowsRepository.GetByWordForUserAsync(wordId, userId);
        if (existingUnknow == null)
        {
            var unknow = new Unknows { WordId = wordId, UserId = userId, CreatedTime = DateTime.Now };
            await _unknowsRepository.AddAsync(unknow);
            await _unitOfWork.CommitAsync();
            return Result<bool>.Success(true);
        }

        _unknowsRepository.Remove(existingUnknow);
        await _unitOfWork.CommitAsync();
        return Result<bool>.Success(false);
    }

    public async Task<Result<Unknows>> GetUnknowsWithWordAsync(int unknowsId, string userId)
    {
        var unknow = await _unknowsRepository.GetUnknowsWithWordAsync(unknowsId, userId);
        return unknow == null ? Result<Unknows>.Failure("Kayıt bulunamadı.") : Result<Unknows>.Success(unknow);
    }

    public async Task<Result<(List<Unknows> Unknows, int TotalCount)>> GetPagedUnknowsAsync(string userId, string? search, int page, int pageSize)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize <= 0)
        {
            pageSize = 10;
        }

        var (items, totalCount) = await _unknowsRepository.GetPagedUnknowsAsync(userId, search, page, pageSize);
        return Result<(List<Unknows> Unknows, int TotalCount)>.Success((items, totalCount));
    }
}

