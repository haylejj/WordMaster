using Core.Entity;
using System.Linq.Expressions;

namespace Core.Service
{
    public interface IUnknowsService
    {
        IQueryable<Unknows> Where(Expression<Func<Unknows, bool>> predicate);
        Task<Unknows?> GetByIdAsync(int id);
        Task<bool> AnyAsync(Expression<Func<Unknows, bool>> predicate);
        Task AddAsync(Unknows entity);
        Task UpdateAsync(Unknows entity);
        Task RemoveAsync(Unknows entity);
        Task<Unknows> GetLastUnknows();
    }
}
