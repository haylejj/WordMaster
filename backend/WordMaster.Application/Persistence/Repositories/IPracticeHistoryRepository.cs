using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Persistence.Repositories;

public interface IPracticeHistoryRepository : IGenericRepository<PracticeHistory>
{
    Task<List<PracticeHistory>> GetHistoryByUserAsync(Guid userId, DateTime startDate);
}
