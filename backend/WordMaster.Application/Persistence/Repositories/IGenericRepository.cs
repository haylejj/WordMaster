using System.Linq.Expressions;

namespace WordMaster.Application.Persistence.Repositories;

public interface IGenericRepository<T> where T : class
{

    IQueryable<T> GetAll();
    IQueryable<T> GetAllAsTracking();
    IQueryable<T> Where(Expression<Func<T, bool>> predicate);
    Task<T?> GetByIdAsync(int id);
    Task<T?> GetByIdAsTrackingAsync(int id);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void UpdateRange(IEnumerable<T> entities);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
}
