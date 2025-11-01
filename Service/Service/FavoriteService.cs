using Core.Entity;
using Core.Repositories;
using Core.Service;
using Core.UnitOfWork;
using System.Linq.Expressions;

namespace Service.Service;

public class FavoriteService : IFavoriteService
{
    private readonly IGenericRepository<Favorite> _repository;
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FavoriteService(IGenericRepository<Favorite> repository, IUnitOfWork unitOfWork, IFavoriteRepository favoriteRepository)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _favoriteRepository = favoriteRepository;
    }

    public IQueryable<Favorite> Where(Expression<Func<Favorite, bool>> predicate)
    {
        return _repository.Where(predicate);
    }

    public async Task<Favorite?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<bool> AnyAsync(Expression<Func<Favorite, bool>> predicate)
    {
        return await _repository.AnyAsync(predicate);
    }

    public async Task AddAsync(Favorite entity)
    {
        await _repository.AddAsync(entity);
        await _unitOfWork.CommitAsync();
    }

    public async Task UpdateAsync(Favorite entity)
    {
        _repository.Update(entity);
        await _unitOfWork.CommitAsync();
    }

    public async Task RemoveAsync(Favorite entity)
    {
        _repository.Remove(entity);
        await _unitOfWork.CommitAsync();
    }

    public async Task<Favorite> GetLastFavorite()
    {
        return await _favoriteRepository.GetLastFavorite();
    }
}
