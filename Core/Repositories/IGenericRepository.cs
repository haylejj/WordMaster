using System.Linq.Expressions;

namespace Core.Repositories;

public interface IGenericRepository<T> where T : class
{
    // Query Methods
    IQueryable<T> GetAll();
    IQueryable<T> GetAllAsTracking();
    IQueryable<T> Where(Expression<Func<T, bool>> predicate);
    Task<T?> GetByIdAsync(int id);
    Task<T?> GetByIdAsTrackingAsync(int id);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    Task<bool> AllAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    // Insert Methods
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);

    // Update Methods
    void Update(T entity);
    void UpdateRange(IEnumerable<T> entities);

    // Delete Methods
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
}
