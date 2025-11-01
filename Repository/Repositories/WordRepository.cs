using Core.Entity;
using Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Repository.Repositories;

public class WordRepository : GenericRepository<Word>, IWordRepository
{
    public WordRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Word> getLastWord()
    {
        return await _context.Words.OrderByDescending(x => x.Id).FirstOrDefaultAsync();
    }
}
