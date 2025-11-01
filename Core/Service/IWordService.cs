using Core.Entity;
using System.Linq.Expressions;

namespace Core.Service
{
    public interface IWordService
    {
        IQueryable<Word> Where(Expression<Func<Word, bool>> predicate);
        Task<Word?> GetByIdAsync(int id);
        Task<bool> AnyAsync(Expression<Func<Word, bool>> predicate);
        Task AddAsync(Word entity);
        Task UpdateAsync(Word entity);
        Task RemoveAsync(Word entity);
        Task<Word> getLastWord();
    }
}
