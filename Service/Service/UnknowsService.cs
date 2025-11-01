using Core.Entity;
using Core.Repositories;
using Core.Service;
using Core.UnitOfWork;
using System.Linq.Expressions;

namespace Service.Service;

public class UnknowsService(IGenericRepository<Unknows> repository, IUnitOfWork unitOfWork, IUnknowsRepository unknowsRepository) : IUnknowsService
{
    public IQueryable<Unknows> Where(Expression<Func<Unknows, bool>> predicate)
    {
        return repository.Where(predicate);
    }

    public async Task<Unknows?> GetByIdAsync(int id)
    {
        return await repository.GetByIdAsync(id);
    }

    public async Task<bool> AnyAsync(Expression<Func<Unknows, bool>> predicate)
    {
        return await repository.AnyAsync(predicate);
    }

    public async Task AddAsync(Unknows entity)
    {
        await repository.AddAsync(entity);
        await unitOfWork.CommitAsync();
    }

    public async Task UpdateAsync(Unknows entity)
    {
        repository.Update(entity);
        await unitOfWork.CommitAsync();
    }

    public async Task RemoveAsync(Unknows entity)
    {
        repository.Remove(entity);
        await unitOfWork.CommitAsync();
    }

    public async Task<Unknows> GetLastUnknows()
    {
        return await unknowsRepository.GetLastUnknows();
    }
}
