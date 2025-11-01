using Core.Entity;
using Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Repository.Repositories;

public class UnknowsRepository : GenericRepository<Unknows>, IUnknowsRepository
{
    public UnknowsRepository(AppDbContext context) : base(context)
    {
    }

    public Task<Unknows> GetLastUnknows()
    {
        return _context.Unknows.OrderByDescending(x => x.Id).FirstOrDefaultAsync();
    }
}
