using Core.Entity;
using System.Linq.Expressions;

namespace Core.Service
{
    public interface IFavoriteService
    {
        IQueryable<Favorite> Where(Expression<Func<Favorite, bool>> predicate);
        Task<Favorite?> GetByIdAsync(int id);
        Task<bool> AnyAsync(Expression<Func<Favorite, bool>> predicate);
        Task AddAsync(Favorite entity);
        Task UpdateAsync(Favorite entity);
        Task RemoveAsync(Favorite entity);
        Task<Favorite> GetLastFavorite();
    }
}
