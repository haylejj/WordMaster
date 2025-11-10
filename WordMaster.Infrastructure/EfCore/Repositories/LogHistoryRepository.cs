using WordMaster.Application.Persistence.Repositories;
using WordMaster.Domain.Entities;

namespace WordMaster.Infrastructure.EfCore.Repositories;

public class LogHistoryRepository(AppDbContext context) : GenericRepository<LogHistory>(context), ILogHistoryRepository
{
}

