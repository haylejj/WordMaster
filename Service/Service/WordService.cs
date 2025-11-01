using Core.Entity;
using Core.Repositories;
using Core.Service;
using Core.UnitOfWork;
using System.Linq.Expressions;

namespace Service.Service;

public class WordService : IWordService
{
    private readonly IGenericRepository<Word> _repository;
    private readonly IWordRepository _wordRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WordService(IGenericRepository<Word> repository, IUnitOfWork unitOfWork, IWordRepository wordRepository)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _wordRepository = wordRepository;
    }

    public IQueryable<Word> Where(Expression<Func<Word, bool>> predicate)
    {
        return _repository.Where(predicate);
    }

    public async Task<Word?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<bool> AnyAsync(Expression<Func<Word, bool>> predicate)
    {
        return await _repository.AnyAsync(predicate);
    }

    public async Task AddAsync(Word entity)
    {
        await _repository.AddAsync(entity);
        await _unitOfWork.CommitAsync();
    }

    public async Task UpdateAsync(Word entity)
    {
        _repository.Update(entity);
        await _unitOfWork.CommitAsync();
    }

    public async Task RemoveAsync(Word entity)
    {
        _repository.Remove(entity);
        await _unitOfWork.CommitAsync();
    }

    public async Task<Word> getLastWord()
    {
        return await _wordRepository.getLastWord();
    }
}
