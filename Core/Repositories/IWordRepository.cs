using Core.Entity;

namespace Core.Repositories;

public interface IWordRepository : IGenericRepository<Word>
{
    Task<Word> getLastWord();
}
