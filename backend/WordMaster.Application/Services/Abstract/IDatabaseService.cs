using System.Threading.Tasks;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IDatabaseService
{
    Task<ServiceResult> ResetTableAsync(string tableName, string password, string userId, string? targetUserId = null);
}
